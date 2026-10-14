-- 001-baseline — the ping log.
--
-- The canonical schema: EF maps over this and never generates it. One row per ping, written by
-- POST /api/pings and read back newest-first by GET /api/pings. `created_at` is stamped by the SDK audit
-- interceptor on insert.

CREATE TABLE pings (
    id         uuid        NOT NULL,
    note       text        NOT NULL,
    created_at timestamptz NOT NULL,

    CONSTRAINT pk_pings PRIMARY KEY (id)
);
