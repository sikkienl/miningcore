using System.Data;
using AutoMapper;
using Dapper;
using Miningcore.Persistence.Model;
using Miningcore.Persistence.Repositories;

namespace Miningcore.Persistence.Postgres.Repositories;

public class MinerWorkerRepository : IMinerWorkerRepository
{
    public MinerWorkerRepository(IMapper mapper)
    {
        this.mapper = mapper;
    }

    private readonly IMapper mapper;

    public async Task<MinerWorkerStats> GetWorkerStatsAsync(IDbConnection con, IDbTransaction tx, string poolId, string miner, string worker)
    {
        const string query = @"SELECT * FROM workerstats WHERE poolid = @poolId AND miner = @miner AND worker = @worker";

        var entity = await con.QuerySingleOrDefaultAsync<Entities.MinerWorkerStats>(query, new {poolId, miner, worker}, tx);

        return mapper.Map<MinerWorkerStats>(entity);
    }

    public async Task<MinerWorkerStats[]> GetWorkerStatsAsync(IDbConnection con, IDbTransaction tx, string poolId, string miner)
    {
        const string query = @"SELECT * FROM workerstats WHERE poolid = @poolId AND miner = @miner";

        return (await con.QueryAsync<Entities.MinerWorkerStats>(new CommandDefinition(query, new { poolId, miner })))
            .Select(mapper.Map<MinerWorkerStats>)
            .ToArray();
    }

    public async Task<MinerWorkerStats[]> GetWorkerStatsAsync(IDbConnection con, IDbTransaction tx, string poolId)
    {
        const string query = @"SELECT * FROM workerstats WHERE poolid = @poolId";

        return (await con.QueryAsync<Entities.MinerWorkerStats>(new CommandDefinition(query, new { poolId })))
            .Select(mapper.Map<MinerWorkerStats>)
            .ToArray();
    }

    public Task UpdateWorkerStatsAsync(IDbConnection con, IDbTransaction tx, MinerWorkerStats settings)
    {
        const string query = @"INSERT INTO workerstats(poolid, miner, worker, bestdifficulty, created, updated)
            VALUES(@poolid, @miner, @worker, @bestdifficulty, now(), now())
            ON CONFLICT ON CONSTRAINT workerstats_pkey DO UPDATE
            SET bestdifficulty = @bestdifficulty, updated = now()
            WHERE workerstats.poolid = @poolid AND workerstats.miner = @miner AND workerstats.worker = @worker";

        return con.ExecuteAsync(query, settings, tx);
    }
}
