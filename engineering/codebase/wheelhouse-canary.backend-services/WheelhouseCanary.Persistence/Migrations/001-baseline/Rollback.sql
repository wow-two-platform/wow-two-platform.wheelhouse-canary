-- Inverts 001-baseline — drops the ping log with its rows. Dev/test recovery only; the log restarts empty.

DROP TABLE IF EXISTS pings;
