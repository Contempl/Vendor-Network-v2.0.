using System.Text.Json;
using System.Text.Json.Serialization;

namespace Product.Domain.Settings;

public static class CacheJsonSerializerSettings
{
    public static readonly JsonSerializerOptions Options = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve, 
    };
}