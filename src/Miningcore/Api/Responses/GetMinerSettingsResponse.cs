namespace Miningcore.Api.Responses;

public class MinerSettings
{
    public decimal PaymentThreshold { get; set; }
    public bool AutoConversionEnabled { get; set; }
    public string AutoConversionDestination { get; set; }
    public string AutoConversionDestinationAddress { get; set; }

}
