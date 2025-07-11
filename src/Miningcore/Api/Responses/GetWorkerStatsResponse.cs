namespace Miningcore.Api.Responses;

public class WorkerStats
{
    public string Miner { get; set; }
    public string Worker { get; set; }
    public double BestDifficulty { get; set; }
}

public class WorkerStatsResponse
{
    public WorkerStats[] WorkerStats { get; set; }
}
