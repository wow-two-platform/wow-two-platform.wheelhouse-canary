-- Inverts 002-pings-created-at-index — drops the index; the rows stay.

DROP INDEX IF EXISTS ix_pings_created_at;
