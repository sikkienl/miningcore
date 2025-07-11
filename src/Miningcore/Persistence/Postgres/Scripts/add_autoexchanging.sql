ALTER TABLE miner_settings ADD COLUMN IF NOT EXISTS autoconversionenabled bool NULL;
ALTER TABLE miner_settings ADD COLUMN IF NOT EXISTS autoconversiondestination TEXT NULL;
ALTER TABLE miner_settings ADD COLUMN IF NOT EXISTS autoconversiondestinationaddress TEXT NULL;
