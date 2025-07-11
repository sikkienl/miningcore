namespace Miningcore.Persistence.Postgres.Entities;

public class MinerSettings
{
    public string PoolId { get; set; }
    public string Address { get; set; }
    public decimal PaymentThreshold { get; set; }
    public bool AutoConversionEnabled { get; set; }
    public string AutoConversionDestination { get; set; }
    public string AutoConversionDestinationAddress { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}
