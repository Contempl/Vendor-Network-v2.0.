namespace Product.Domain.Settings;

public class RedisSettings
{
    public string Url { get; set; }

    public string InstanceName { get; set; }

    public RedisSettings(string url, string instanceName)
    {
        Url = url;
        InstanceName = instanceName;
    }
}