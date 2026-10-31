using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace FabuRobotics.Shared;

public static class ObservabilityExtensions
{
    public static IHostApplicationBuilder AddObservability(this IHostApplicationBuilder builder, params string[] activitySourceNames)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation())
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                    options.Filter = context => !IsHealthEndpoint(context.Request.Path))
                .AddHttpClientInstrumentation(options =>
                    options.FilterHttpRequestMessage = request => !IsHealthEndpoint(request.RequestUri?.AbsolutePath))
                .AddSource(activitySourceNames)
                .SetSampler(new AlwaysOnSampler()))
            .UseOtlpExporter();

        return builder;
    }

    private static bool IsHealthEndpoint(string? path)
    {
        return path is "/health" or "/alive";
    }
}