"""Validate and smoke-test monitoring with disposable Compose resources.

Run from any directory after building src/product-api:latest and
src/product-seeder:latest. Requires Docker Compose and Python 3.10+.
"""

import argparse
import base64
import json
import math
import os
from pathlib import Path
import secrets
import subprocess
import time
import urllib.error
import urllib.parse
import urllib.request
import uuid

ROOT = Path(__file__).resolve().parents[2]
ARTIFACTS = ROOT / ".local" / "monitoring-ci"
PROJECT = "vendor-network-monitoring-ci"
CANARY = "monitoring-ci-private-query"


def compose(*arguments, capture=False, timeout=180, check=True):
    return subprocess.run(
        ["docker", "compose", "--env-file", str(ARTIFACTS / "environment.env"),
         "--project-name", PROJECT,
         "-f", "docker-compose.yml", "-f", "docker-compose.monitoring.yml",
         "-f", "scripts/ci/compose.monitoring-ci.yml", *arguments],
        cwd=ROOT, env=ENVIRONMENT, text=True, encoding="utf-8", errors="replace",
        capture_output=capture, timeout=timeout, check=check,
    )


def request(url, headers=None):
    req = urllib.request.Request(url, headers=headers or {})
    try:
        response = urllib.request.urlopen(req, timeout=5)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        return response.status, response.headers, response.read()


def get_json(url, headers=None):
    status, _, body = request(url, headers)
    if status != 200:
        raise RuntimeError(f"HTTP {status}: {url}")
    return json.loads(body)


def wait_for(description, probe, timeout=120):
    print(f"Waiting for {description}...", flush=True)
    deadline = time.monotonic() + timeout
    last_error = "not ready"
    while time.monotonic() < deadline:
        try:
            result = probe()
            if result:
                print(f"OK: {description}", flush=True)
                return result
        except (urllib.error.URLError, TimeoutError, OSError,
                json.JSONDecodeError, RuntimeError) as error:
            last_error = str(error)
        time.sleep(2)
    raise RuntimeError(f"Timed out waiting for {description}: {last_error}")


def base_url(service, port):
    address = compose("port", service, str(port), capture=True).stdout.strip()
    return f"http://127.0.0.1:{address.rsplit(':', 1)[1]}"


def objects(value):
    if isinstance(value, dict):
        yield value
        for child in value.values():
            yield from objects(child)
    elif isinstance(value, list):
        for child in value:
            yield from objects(child)


def hex_id(value):
    # Tempo versions can encode protobuf byte fields as hex or base64.
    try:
        return bytes.fromhex(value).hex()
    except ValueError:
        return base64.b64decode(value, validate=True).hex()


def trace_is_complete(grafana, headers, trace_id):
    trace = get_json(f"{grafana}/api/datasources/proxy/uid/tempo/api/traces/{trace_id}", headers)
    spans = [item for item in objects(trace) if "spanId" in item and "traceId" in item]
    if not any(span.get("kind") in (2, "2", "SPAN_KIND_SERVER") for span in spans):
        return False
    if not any(span.get("name") == "postgresql.query" for span in spans):
        return False
    if any(hex_id(span["traceId"]) != trace_id for span in spans):
        raise AssertionError("Tempo returned spans from a different trace")
    if CANARY in json.dumps(trace):
        raise AssertionError("Query-string canary leaked into the exported trace")
    forbidden = {"db.query.text", "db.statement", "db.connection_string",
                 "url.full", "url.query", "http.url", "http.target",
                 "exception.message", "exception.stacktrace"}
    if any(item.get("key") in forbidden for item in objects(trace)):
        raise AssertionError("Sensitive attributes leaked into the exported trace")
    return len(spans)


def query(prometheus, expression):
    response = get_json(f"{prometheus}/api/v1/query?" + urllib.parse.urlencode({"query": expression}))
    if response.get("status") != "success":
        raise RuntimeError(f"Prometheus query failed: {expression}")
    return response["data"]["result"]


def check_dashboard_metrics(prometheus, panels, api):
    # Keep producing observations across scrapes so histogram rates are meaningful.
    status, _, _ = request(f"{api}/Account/Register/User/2147483647")
    if status != 404:
        raise AssertionError(f"Expected missing invite (404); got HTTP {status}")
    missing = []
    for panel in panels:
        for target in panel.get("targets", []):
            if not target.get("expr"):
                continue
            results = query(prometheus, target["expr"])
            if not results or any(not math.isfinite(float(item["value"][1])) for item in results):
                missing.append(panel["title"])
    if missing:
        raise RuntimeError("Missing or non-finite dashboard metrics: " + ", ".join(missing))
    collector = query(prometheus, 'up{job="vendor-network"}')
    if not collector or any(float(item["value"][1]) != 1 for item in collector):
        raise RuntimeError("Collector scrape is unhealthy")
    return True


def smoke_test(pull_policy):
    dashboard_path = ROOT / "monitoring/grafana/dashboards/api.json"
    dashboard = json.loads(dashboard_path.read_text(encoding="utf-8-sig"))
    compose("config", "--quiet")
    print("OK: Compose and dashboard JSON", flush=True)
    compose("pull", "--policy", pull_policy, "db", "redis", "otel-collector", "prometheus", "tempo", "grafana", timeout=420)
    compose("run", "--rm", "--no-deps", "otel-collector", "validate", "--config=/etc/otelcol/config.yaml")
    compose("run", "--rm", "--no-deps", "--entrypoint", "promtool", "prometheus",
            "check", "config", "/etc/prometheus/prometheus.yaml")
    print("OK: Collector and Prometheus configuration", flush=True)
    compose("up", "-d", "--no-build", "--pull", "never",
            "api", "otel-collector", "prometheus", "tempo", "grafana")

    api = base_url("api", 80)
    grafana = base_url("grafana", 3000)
    prometheus = base_url("prometheus", 9090)
    authorization = base64.b64encode(
        f"admin:{ENVIRONMENT['GRAFANA_ADMIN_PASSWORD']}".encode()).decode()
    headers = {"Authorization": f"Basic {authorization}", "Accept": "application/json"}
    wait_for("API readiness", lambda: request(f"{api}/health/ready")[::2] == (200, b"Healthy"))
    wait_for("Grafana database", lambda: get_json(f"{grafana}/api/health").get("database") == "ok")
    provisioned = wait_for("provisioned Grafana dashboard", lambda:
        get_json(f"{grafana}/api/dashboards/uid/{dashboard['uid']}", headers))
    for uid in ("prometheus", "tempo"):
        get_json(f"{grafana}/api/datasources/uid/{uid}", headers)

    trace_id = uuid.uuid4().hex
    status, response_headers, _ = request(
        f"{api}/Account/Register/User/2147483647?token={CANARY}",
        {"traceparent": f"00-{trace_id}-{secrets.token_hex(8)}-01"})
    if status != 404 or response_headers.get("X-Trace-Id") != trace_id:
        raise AssertionError(f"Expected missing invite (404) with matching X-Trace-Id; got HTTP {status}")
    span_count = wait_for("HTTP and PostgreSQL spans in Tempo", lambda:
        trace_is_complete(grafana, headers, trace_id))
    wait_for("API request counter in Prometheus", lambda:
        any(float(item["value"][1]) >= 1 for item in query(prometheus,
            'http_server_request_duration_seconds_count{service_name="VendorNetwork.Api",http_response_status_code="404"}')))
    wait_for("all dashboard metrics", lambda:
        check_dashboard_metrics(prometheus, provisioned["dashboard"]["panels"], api))
    summary = {"traceId": trace_id, "spans": span_count,
               "dashboard": dashboard["uid"], "panels": len(provisioned["dashboard"]["panels"])}
    (ARTIFACTS / "result.json").write_text(json.dumps(summary, indent=2) + "\n", encoding="utf-8")
    print("Monitoring smoke test passed: " + json.dumps(summary), flush=True)


def collect_diagnostics():
    for name, arguments in (
        ("status.txt", ("ps", "--all")),
        ("compose.log", ("logs", "--no-color", "--tail", "100")),
    ):
        result = compose(*arguments, capture=True, check=False, timeout=30)
        (ARTIFACTS / name).write_text(result.stdout + result.stderr, encoding="utf-8")


def main():
    global ENVIRONMENT
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--pull-policy", choices=("always", "missing"), default="always",
                        help="Use missing locally to reuse cached service images; default: always.")
    arguments = parser.parse_args()
    existing_containers = subprocess.run(
        ["docker", "ps", "--all", "--quiet", "--filter",
         f"label=com.docker.compose.project={PROJECT}"],
        capture_output=True, text=True, check=True, timeout=30,
    ).stdout.strip()
    if existing_containers:
        raise RuntimeError(f"Compose project {PROJECT} already has containers; refusing to replace them")
    ARTIFACTS.mkdir(parents=True, exist_ok=True)
    (ARTIFACTS / "result.json").unlink(missing_ok=True)
    settings = {
        "POSTGRES_USER": "vendor_ci", "POSTGRES_DB": "vendor_network_ci",
        "POSTGRES_PASSWORD": secrets.token_urlsafe(32),
        "JWT_SECRET": secrets.token_urlsafe(48),
        "GRAFANA_ADMIN_USER": "admin", "GRAFANA_ADMIN_PASSWORD": secrets.token_urlsafe(32),
        "ASPNETCORE_ENVIRONMENT": "Development", "TRACE_SAMPLING_RATIO": "1.0",
        "COLLECTOR_ARCH": "amd64", "SEED_ADMIN_EMAIL": "", "SEED_ADMIN_PASSWORD": "",
        **{name: "0" for name in ("API_HOST_PORT", "POSTGRES_HOST_PORT", "REDIS_HOST_PORT",
                                 "GRAFANA_HOST_PORT", "PROMETHEUS_HOST_PORT",
                                 "OTLP_GRPC_HOST_PORT", "OTLP_HTTP_HOST_PORT")},
    }
    ENVIRONMENT = {**os.environ, **settings}
    (ARTIFACTS / "environment.env").write_text(
        "".join(f"{key}={value}\n" for key, value in settings.items()), encoding="utf-8")
    try:
        smoke_test(arguments.pull_policy)
    finally:
        try:
            collect_diagnostics()
        finally:
            compose("down", "--volumes", "--remove-orphans", timeout=90)
            (ARTIFACTS / "environment.env").unlink(missing_ok=True)


if __name__ == "__main__":
    main()
