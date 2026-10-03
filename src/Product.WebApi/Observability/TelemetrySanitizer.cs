using System.Diagnostics;
using OpenTelemetry;

namespace Product.WebApi.Observability;

public sealed class TelemetrySanitizer : BaseProcessor<Activity>
{
    private static readonly HashSet<string> SensitiveTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "db.statement", "db.query.text", "db.connection_string", "db.user",
        "url.full", "url.path", "url.query", "http.url", "http.target",
        "exception.message", "exception.stacktrace"
    };

    public override void OnEnd(Activity activity)
    {
        if (activity.Source.Name == "Npgsql")
            activity.DisplayName = "postgresql.query";
        foreach (var tag in activity.TagObjects.ToArray())
            if (SensitiveTags.Contains(tag.Key) ||
                tag.Key.StartsWith("db.query.parameter.", StringComparison.OrdinalIgnoreCase) ||
                tag.Key.StartsWith("http.request.header.", StringComparison.OrdinalIgnoreCase) ||
                tag.Key.StartsWith("http.response.header.", StringComparison.OrdinalIgnoreCase))
                activity.SetTag(tag.Key, null);
        if (activity.Status == ActivityStatusCode.Error)
            activity.SetStatus(ActivityStatusCode.Error); // Preserve failure without exporting its raw description.
    }
}
