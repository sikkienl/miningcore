using CryptoExchange.Net.CommonObjects;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Miningcore.Blockchain.Alephium;
using Miningcore.Blockchain.Ergo;
using Miningcore.Configuration;
using Miningcore.Crypto.Hashing.Algorithms;
using NBitcoin;
using NBitcoin.Altcoins;
using Npgsql;
using Parlot.Fluent;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;
using static NBitcoin.BIP322.BIP322Signature;
using static System.Net.Mime.MediaTypeNames;

namespace Miningcore.Persistence.Postgres.Repositories
{
    public static class SystemRepository
    {
        public static void EnsureDBSchema(ClusterConfig config)
        {
            NpgsqlConnectionStringBuilder conBuilder = new NpgsqlConnectionStringBuilder();
            conBuilder.Host = config.Persistence.Postgres.Host;
            conBuilder.Username = config.Persistence.Postgres.User;
            conBuilder.Password = config.Persistence.Postgres.Password;
            conBuilder.Database = config.Persistence.Postgres.Database;
            using(var dataSource = NpgsqlDataSource.Create(conBuilder))
            {

                dataSource.CreateCommand("CREATE TABLE IF NOT EXISTS shares (poolid TEXT NOT NULL,blockheight BIGINT NOT NULL,difficulty DOUBLE PRECISION NOT NULL,networkdifficulty DOUBLE PRECISION NOT NULL,miner TEXT NOT NULL,worker TEXT NULL,useragent TEXT NULL,ipaddress TEXT NOT NULL,source TEXT NULL,created TIMESTAMPTZ NOT NULL);").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_SHARES_POOL_MINER on shares(poolid, miner);").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_SHARES_POOL_CREATED ON shares(poolid, created);").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_SHARES_POOL_MINER_DIFFICULTY on shares(poolid, miner, difficulty);").ExecuteNonQuery();
                
                dataSource.CreateCommand("CREATE TABLE IF NOT EXISTS blocks (id BIGSERIAL NOT NULL PRIMARY KEY,poolid TEXT NOT NULL,blockheight BIGINT NOT NULL,networkdifficulty DOUBLE PRECISION NOT NULL,status TEXT NOT NULL,type TEXT NULL,confirmationprogress FLOAT NOT NULL DEFAULT 0,effort FLOAT NULL,minereffort FLOAT NULL,transactionconfirmationdata TEXT NOT NULL,miner TEXT NULL,reward decimal(28, 12) NULL,source TEXT NULL,hash TEXT NULL,worker TEXT NULL,difficulty DOUBLE PRECISION NULL,created TIMESTAMPTZ NOT NULL);").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_BLOCKS_POOL_BLOCK_STATUS on blocks(poolid, blockheight, status);").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_BLOCKS_POOL_BLOCK_TYPE on blocks(poolid, blockheight, type);").ExecuteNonQuery();

                dataSource.CreateCommand("CREATE TABLE IF NOT EXISTS balances (poolid TEXT NOT NULL,address TEXT NOT NULL,amount decimal(28, 12) NOT NULL DEFAULT 0,created TIMESTAMPTZ NOT NULL,updated TIMESTAMPTZ NOT NULL,primary key(poolid, address)); ").ExecuteNonQuery();

                dataSource.CreateCommand("CREATE TABLE IF NOT EXISTS balance_changes (id BIGSERIAL NOT NULL PRIMARY KEY,poolid TEXT NOT NULL,address TEXT NOT NULL,amount decimal(28, 12) NOT NULL DEFAULT 0,usage TEXT NULL,tags text[] NULL,created TIMESTAMPTZ NOT NULL);").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_BALANCE_CHANGES_POOL_ADDRESS_CREATED on balance_changes(poolid, address, created desc);").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_BALANCE_CHANGES_POOL_TAGS on balance_changes USING gin (tags);").ExecuteNonQuery();

                dataSource.CreateCommand("CREATE TABLE IF NOT EXISTS miner_settings(poolid TEXT NOT NULL,address TEXT NOT NULL,paymentthreshold decimal(28, 12) NOT NULL,autoconversionenabled bool NULL,autoconversiondestination TEXT NULL,autoconversiondestinationaddress TEXT NULL,created TIMESTAMPTZ NOT NULL,updated TIMESTAMPTZ NOT NULL,primary key(poolid, address));").ExecuteNonQuery();

                dataSource.CreateCommand("CREATE TABLE IF NOT EXISTS payments(id BIGSERIAL NOT NULL PRIMARY KEY,poolid TEXT NOT NULL,coin TEXT NOT NULL,address TEXT NOT NULL,amount decimal(28, 12) NOT NULL,transactionconfirmationdata TEXT NOT NULL,created TIMESTAMPTZ NOT NULL);").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_PAYMENTS_POOL_COIN_WALLET on payments(poolid, coin, address);").ExecuteNonQuery();

                dataSource.CreateCommand("CREATE TABLE IF NOT EXISTS poolstats (id BIGSERIAL NOT NULL PRIMARY KEY,poolid TEXT NOT NULL,connectedminers INT NOT NULL DEFAULT 0,poolhashrate DOUBLE PRECISION NOT NULL DEFAULT 0,sharespersecond DOUBLE PRECISION NOT NULL DEFAULT 0,networkhashrate DOUBLE PRECISION NOT NULL DEFAULT 0,networkdifficulty DOUBLE PRECISION NOT NULL DEFAULT 0,lastnetworkblocktime TIMESTAMPTZ NULL,blockheight BIGINT NOT NULL DEFAULT 0,connectedpeers INT NOT NULL DEFAULT 0,created TIMESTAMPTZ NOT NULL);").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_POOLSTATS_POOL_CREATED on poolstats(poolid, created);").ExecuteNonQuery();

                dataSource.CreateCommand("CREATE TABLE IF NOT EXISTS minerstats (id BIGSERIAL NOT NULL PRIMARY KEY,poolid TEXT NOT NULL,miner TEXT NOT NULL,worker TEXT NOT NULL,hashrate DOUBLE PRECISION NOT NULL DEFAULT 0,sharespersecond DOUBLE PRECISION NOT NULL DEFAULT 0,created TIMESTAMPTZ NOT NULL);").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_MINERSTATS_POOL_CREATED on minerstats(poolid, created);").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_MINERSTATS_POOL_MINER_CREATED on minerstats(poolid, miner, created);").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_MINERSTATS_POOL_MINER_WORKER_CREATED_HASHRATE on minerstats(poolid,miner,worker,created desc,hashrate);").ExecuteNonQuery();

                dataSource.CreateCommand("CREATE TABLE IF NOT EXISTS workerstats (poolid TEXT NOT NULL,miner TEXT NOT NULL,worker TEXT NOT NULL,bestdifficulty DOUBLE PRECISION NOT NULL DEFAULT 0,created TIMESTAMPTZ NOT NULL,updated TIMESTAMPTZ NOT NULL,primary key(poolid, miner, worker));").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_WORKERSTATS_POOL_CREATED on workerstats(poolid, created);").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_WORKERSTATS_POOL_MINER_CREATED on workerstats(poolid, miner, created);").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_WORKERSTATS_POOL_MINER__WORKER_CREATED on workerstats(poolid, miner, worker, created);").ExecuteNonQuery();
                dataSource.CreateCommand("CREATE INDEX IF NOT EXISTS IDX_WORKERSTATS_POOL_MINER_WORKER_CREATED_BESTDIFFICULTY on workerstats(poolid,miner,worker,created desc,bestdifficulty);").ExecuteNonQuery();

                dataSource.CreateCommand("ALTER TABLE blocks ADD COLUMN IF NOT EXISTS worker TEXT NULL;").ExecuteNonQuery();
                dataSource.CreateCommand("ALTER TABLE blocks ADD COLUMN IF NOT EXISTS difficulty DOUBLE PRECISION NULL;").ExecuteNonQuery();
                dataSource.CreateCommand("ALTER TABLE blocks ADD COLUMN IF NOT EXISTS minereffort FLOAT NULL;").ExecuteNonQuery();
                dataSource.CreateCommand("ALTER TABLE miner_settings ADD COLUMN IF NOT EXISTS autoconversionenabled bool NULL;").ExecuteNonQuery();
                dataSource.CreateCommand("ALTER TABLE miner_settings ADD COLUMN IF NOT EXISTS autoconversiondestination TEXT NULL;").ExecuteNonQuery();
                dataSource.CreateCommand("ALTER TABLE miner_settings ADD COLUMN IF NOT EXISTS autoconversiondestinationaddress TEXT NULL;").ExecuteNonQuery();
            }
        }
    }
}
