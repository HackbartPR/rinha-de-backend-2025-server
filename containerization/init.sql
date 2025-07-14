CREATE UNLOGGED TABLE payments (
    id SERIAL PRIMARY KEY,
    processor SMALLINT NOT NULL,
    amount DECIMAL NOT NULL,
    requested_at TIMESTAMP NOT NULL
);

CREATE INDEX summeries_requested_at ON payments (requested_at);
