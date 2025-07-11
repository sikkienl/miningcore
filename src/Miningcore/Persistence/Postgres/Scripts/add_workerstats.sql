CREATE TABLE workerstats
(
    poolid TEXT NOT NULL,
	miner TEXT NOT NULL,
    worker TEXT NOT NULL,
	bestdifficulty DOUBLE PRECISION NOT NULL DEFAULT 0,
	created TIMESTAMPTZ NOT NULL,
	updated TIMESTAMPTZ NOT NULL,

	primary key(poolid, miner, worker)
);

CREATE INDEX IDX_WORKERSTATS_POOL_CREATED on workerstats(poolid, created);
CREATE INDEX IDX_WORKERSTATS_POOL_MINER_CREATED on workerstats(poolid, miner, created);
CREATE INDEX IDX_WORKERSTATS_POOL_MINER__WORKER_CREATED on workerstats(poolid, miner, worker, created);
CREATE INDEX IDX_WORKERSTATS_POOL_MINER_WORKER_CREATED_BESTDIFFICULTY on workerstats(poolid,miner,worker,created desc,bestdifficulty);

ALTER TABLE blocks ADD COLUMN IF NOT EXISTS worker TEXT NULL;
ALTER TABLE blocks ADD COLUMN IF NOT EXISTS difficulty DOUBLE PRECISION NULL;
