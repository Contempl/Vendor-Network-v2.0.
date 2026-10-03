using OpenTelemetry.Metrics;
using Microsoft.EntityFrameworkCore;
using Product.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Product.WebApi.Observability;

namespace Product.IntegrationTests.Tests;

public class ObservabilityTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task Health_IsAnonymousAndReturnsTraceId(string path)
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(response.Headers.GetValues("X-Trace-Id").Single()));
    }

    [Fact]
    public async Task CacheFailure_MakesReadinessUnhealthyButLivenessStaysHealthy()
    {
        using var failingFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDistributedCache>();
                services.AddSingleton<IDistributedCache, UnavailableCache>();
            }));
        using var client = failingFactory.CreateClient();
        using var ready = await client.GetAsync("/health/ready");
        using var live = await client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, ready.StatusCode);
        Assert.Equal("Unhealthy", await ready.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
        Assert.DoesNotContain("private-cache-error", await ready.Content.ReadAsStringAsync());
    }

    [Fact]
    public void Sanitizer_RemovesSecretsAndQueryTextButKeepsDiagnosticMetadata()
    {
        using var source = new ActivitySource("Npgsql");
        using var listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == "Npgsql",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(listener);
        using var activity = source.StartActivity("SELECT secret FROM users")!;
        activity.SetTag("db.query.text", "SELECT secret");
        activity.SetTag("db.connection_string", "Password=secret");
        activity.SetTag("url.full", "https://example.test/?token=secret");
        activity.SetTag("http.request.header.authorization", "Bearer secret");
        activity.SetTag("db.query.parameter.password", "secret");
        activity.SetTag("http.response.status_code", 500);
        activity.SetStatus(ActivityStatusCode.Error, "private-error");
        new TelemetrySanitizer().OnEnd(activity);
        Assert.Equal("postgresql.query", activity.DisplayName);
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Null(activity.StatusDescription);
        Assert.Single(activity.TagObjects);
        Assert.Equal(500, activity.GetTagItem("http.response.status_code"));
    }


    [Fact]
    public async Task MissingCollector_DoesNotBreakRequests_AndDatabaseSpansAreSanitized()
    {
        var exported = new System.Collections.Concurrent.ConcurrentQueue<Activity>();
        using var telemetryFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Observability:Enabled", "true");
            builder.UseSetting("Observability:TraceSamplingRatio", "1");
            builder.ConfigureTestServices(services =>
            {
                services.Configure<OpenTelemetry.Exporter.OtlpExporterOptions>(options =>
                {
                    options.Endpoint = new Uri("http://127.0.0.1:1");
                    options.TimeoutMilliseconds = 250;
                });
                services.AddOpenTelemetry().WithTracing(tracing =>
                    tracing.AddProcessor(new OpenTelemetry.SimpleActivityExportProcessor(
                        new ActivityExporter(exported))));
            });
        });
        using var client = telemetryFactory.CreateClient();
        client.DefaultRequestHeaders.Add("traceparent", "00-11111111111111111111111111111111-2222222222222222-01");
        Assert.True(telemetryFactory.Services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>()
            .GetValue<bool>("Observability:Enabled"));
        using var response = await client.GetAsync("/swagger/v1/swagger.json?token=telemetry-canary");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(response.Headers.GetValues("X-Trace-Id").Single()));
        using var operation = new Activity("observability-test").SetParentId("00-" + Guid.NewGuid().ToString("N") + "-5555555555555555-01").Start();
        using var scope = telemetryFactory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.ExecuteSqlRawAsync("SELECT 'telemetry-canary'");
        var span = Assert.Single(exported, span => span.Source.Name == "Npgsql" && span.TraceId == operation.TraceId);
        Assert.Equal("postgresql.query", span.DisplayName);
        Assert.DoesNotContain(span.TagObjects, tag => tag.Value?.ToString()?.Contains("telemetry-canary") == true);
        Assert.DoesNotContain(span.TagObjects, tag => tag.Key is "db.query.text" or "db.statement" or "db.connection_string");
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DisabledTelemetry_DoesNotExportDatabaseSpansOrMetrics(bool overrideOptions)
    {
        var spans = new System.Collections.Concurrent.ConcurrentQueue<Activity>();
        var metrics = new System.Collections.Concurrent.ConcurrentQueue<OpenTelemetry.Metrics.Metric>();
        using var disabledFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Observability:Enabled", overrideOptions ? "true" : "false");
            builder.ConfigureTestServices(services =>
            {
                if (overrideOptions)
                    services.PostConfigure<ObservabilityOptions>(options => options.Enabled = false);
                services.AddOpenTelemetry()
                    .WithTracing(tracing => tracing.AddProcessor(new SimpleActivityExportProcessor(new ActivityExporter(spans))))
                    .WithMetrics(meter => meter.AddReader(new OpenTelemetry.Metrics.PeriodicExportingMetricReader(new MetricExporter(metrics))));
            });
        });
        using var client = disabledFactory.CreateClient();
        using var response = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = disabledFactory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.ExecuteSqlRawAsync("SELECT 1");
        disabledFactory.Services.GetRequiredService<OpenTelemetry.Metrics.MeterProvider>().ForceFlush(1000);
        Assert.Empty(spans);
        Assert.Empty(metrics);
    }

    [Theory]
    [InlineData("-0.1")]
    [InlineData("1.1")]
    [InlineData("NaN")]
    public void InvalidSamplingRatio_FailsStartup(string ratio)
    {
        using var invalidFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Observability:Enabled", "true");
            builder.UseSetting("Observability:TraceSamplingRatio", ratio);
        });
        var error = Assert.Throws<OptionsValidationException>(() => invalidFactory.CreateClient());
        Assert.Contains("TraceSamplingRatio must be between 0 and 1", error.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ZeroRootSampling_RespectsParentSamplingDecision(bool sampled)
    {
        var exported = new System.Collections.Concurrent.ConcurrentQueue<Activity>();
        using var samplingFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Observability:Enabled", "true");
            builder.UseSetting("Observability:TraceSamplingRatio", "0");
            builder.ConfigureTestServices(services => services.AddOpenTelemetry().WithTracing(tracing =>
                tracing.AddProcessor(new SimpleActivityExportProcessor(new ActivityExporter(exported)))));
        });
        using var client = samplingFactory.CreateClient();
        using var parent = new Activity("upstream-request")
            .SetParentId("00-33333333333333333333333333333333-4444444444444444-" + (sampled ? "01" : "00"))
            .Start();
        using var scope = samplingFactory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.ExecuteSqlRawAsync("SELECT 1");
        if (sampled) Assert.Single(exported, span => span.Source.Name == "Npgsql" && span.TraceId == parent.TraceId);
        else Assert.DoesNotContain(exported, span => span.TraceId == parent.TraceId);
    }

    [Fact]
    public void ServiceIdentity_UsesOptionsForBothProviders()
    {
        using var configuredFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Observability:ServiceName", "configured-service");
            builder.ConfigureTestServices(services =>
                services.PostConfigure<ObservabilityOptions>(options => options.ServiceName += "-override"));
        });
        using var client = configuredFactory.CreateClient();
        var options = configuredFactory.Services.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
        Assert.Equal("configured-service-override", options.ServiceName);
        var traceResource = configuredFactory.Services.GetRequiredService<TracerProvider>()
            .GetResource().Attributes.ToDictionary(attribute => attribute.Key, attribute => attribute.Value);
        var metricResource = configuredFactory.Services.GetRequiredService<MeterProvider>()
            .GetResource().Attributes.ToDictionary(attribute => attribute.Key, attribute => attribute.Value);
        Assert.Equal(options.ServiceName, traceResource["service.name"]);
        Assert.Equal(options.ServiceName, metricResource["service.name"]);
        Assert.Equal(traceResource["service.instance.id"], metricResource["service.instance.id"]);
        Assert.Equal("Development", traceResource["deployment.environment.name"]);
        Assert.Equal("Development", metricResource["deployment.environment.name"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyServiceName_FailsStartup(string serviceName)
    {
        using var invalidFactory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("Observability:ServiceName", serviceName));
        var error = Assert.Throws<OptionsValidationException>(() => invalidFactory.CreateClient());
        Assert.Contains("ServiceName must not be empty", error.Message);
    }

    private sealed class MetricExporter(System.Collections.Concurrent.ConcurrentQueue<OpenTelemetry.Metrics.Metric> metrics)
        : BaseExporter<OpenTelemetry.Metrics.Metric>
    {
        public override ExportResult Export(in Batch<OpenTelemetry.Metrics.Metric> batch)
        {
            foreach (var metric in batch) metrics.Enqueue(metric);
            return ExportResult.Success;
        }
    }
    private sealed class ActivityExporter(System.Collections.Concurrent.ConcurrentQueue<Activity> activities)
        : OpenTelemetry.BaseExporter<Activity>
    {
        public override OpenTelemetry.ExportResult Export(in OpenTelemetry.Batch<Activity> batch)
        {
            foreach (var activity in batch) activities.Enqueue(activity);
            return OpenTelemetry.ExportResult.Success;
        }
    }

    private sealed class UnavailableCache : IDistributedCache
    {
        public byte[]? Get(string key) => throw new InvalidOperationException("private-cache-error");
        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) =>
            Task.FromException<byte[]?>(new InvalidOperationException("private-cache-error"));
        public void Refresh(string key) => throw new NotSupportedException();
        public Task RefreshAsync(string key, CancellationToken token = default) => throw new NotSupportedException();
        public void Remove(string key) => throw new NotSupportedException();
        public Task RemoveAsync(string key, CancellationToken token = default) => throw new NotSupportedException();
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => throw new NotSupportedException();
        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => throw new NotSupportedException();
    }
}
