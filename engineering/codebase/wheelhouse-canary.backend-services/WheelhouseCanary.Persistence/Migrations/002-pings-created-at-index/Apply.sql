-- 002-pings-created-at-index — serves the newest-first read of the latest pings.
-- A second, additive migration, so a deployment that upgrades from 001 has a real pending step to apply.

CREATE INDEX IF NOT EXISTS ix_pings_created_at ON pings (created_at DESC);
