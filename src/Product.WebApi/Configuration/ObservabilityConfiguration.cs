using System.Diagnostics;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Product.WebApi.Observability;

namespace Product.WebApi.Configuration;

public static class ObservabilityConfiguration
{
    public static void AddObservability(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<ObservabilityOptions>()
            .BindConfiguration(ObservabilityOptions.SectionName)
            .Validate(options => !options.Enabled ||
                (double.IsFinite(options.TraceSamplingRatio) && options.TraceSamplingRatio is >= 0 and <= 1),
                "Observability:TraceSamplingRatio must be between 0 and 1.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ServiceName),
                "Observability:ServiceName must not be empty.")
            .ValidateOnStart();

        builder.Logging.ClearProviders();
        builder.Logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.UseUtcTimestamp = true;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        });
        builder.Logging.Configure(options => options.ActivityTrackingOptions =
            ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
        builder.Logging.AddFilter("Npgsql.Command", LogLevel.None);

        builder.Services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgresql", tags: ["ready"], timeout: TimeSpan.FromSeconds(3))
            .AddCheck<CacheHealthCheck>("redis", tags: ["ready"], timeout: TimeSpan.FromSeconds(3));

        var serviceInstanceId = Guid.NewGuid().ToString();
        var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString();

        // Register SDK dependencies before building DI; resolve options when providers start.
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddAttributes([new("deployment.environment.name", builder.Environment.EnvironmentName)]))
            .WithTracing(traces => traces
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = false;
                    options.Filter = context => !context.Request.Path.StartsWithSegments("/health");
                })
                .AddHttpClientInstrumentation(options => options.RecordException = false)
                .AddSource("Npgsql")
                .AddProcessor(new TelemetrySanitizer())
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("Npgsql")
                .AddOtlpExporter());

        builder.Services.ConfigureOpenTelemetryTracerProvider((services, traces) =>
        {
            var options = services.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
            traces.ConfigureResource(resource => resource.AddService(options.ServiceName,
                serviceVersion: serviceVersion, serviceInstanceId: serviceInstanceId));
            if (!options.Enabled)
            {
                traces.SetSampler(new AlwaysOffSampler());
                return;
            }
            traces.SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(options.TraceSamplingRatio)));
        });
        builder.Services.ConfigureOpenTelemetryMeterProvider((services, metrics) =>
        {
            var options = services.GetRequiredService<IOptions<ObservabilityOptions>>().Value;
            metrics.ConfigureResource(resource => resource.AddService(options.ServiceName,
                serviceVersion: serviceVersion, serviceInstanceId: serviceInstanceId));
            if (!options.Enabled)
            {
                metrics.AddView("*", MetricStreamConfiguration.Drop);
                return;
            }
            metrics.AddView("http.server.request.duration", new ExplicitBucketHistogramConfiguration
            {
                Boundaries = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10]
            });
        });
    }
}
