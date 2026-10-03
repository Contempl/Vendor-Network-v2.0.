namespace Product.WebApi.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public bool Enabled { get; set; }
    public double TraceSamplingRatio { get; set; } = 0.1;
    public string ServiceName { get; set; } = "VendorNetwork.Api";
}
