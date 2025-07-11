namespace Miningcore.Persistence.Model;

public class MinerWorkerStats
{
    public string PoolId { get; set; }
    public string Miner { get; set; }
    public string Worker { get; set; }
    public double BestDifficulty { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}
