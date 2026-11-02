-- Machine-readable PostgreSQL catalog. Excludes EF's migration bookkeeping only.
WITH tables AS (
    SELECT c.oid, c.relname FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
    WHERE n.nspname = 'public' AND c.relkind IN ('r', 'p') AND c.relname <> '__EFMigrationsHistory'
)
SELECT jsonb_build_object(
    'tables', (SELECT coalesce(jsonb_agg(relname ORDER BY relname), '[]') FROM tables),
    'columns', (
        SELECT coalesce(jsonb_agg(jsonb_build_object(
            'table', t.relname, 'name', a.attname, 'type', format_type(a.atttypid, a.atttypmod),
            'notNull', a.attnotnull, 'default', pg_get_expr(d.adbin, d.adrelid),
            'identity', a.attidentity, 'generated', a.attgenerated
        ) ORDER BY t.relname, a.attname), '[]')
        FROM tables t JOIN pg_attribute a ON a.attrelid = t.oid
        LEFT JOIN pg_attrdef d ON d.adrelid = a.attrelid AND d.adnum = a.attnum
        WHERE a.attnum > 0 AND NOT a.attisdropped
    ),
    'constraints', (
        SELECT coalesce(jsonb_agg(jsonb_build_object(
            'table', t.relname, 'name', c.conname, 'type', c.contype,
            'definition', pg_get_constraintdef(c.oid, true),
            'deferrable', c.condeferrable, 'deferred', c.condeferred, 'validated', c.convalidated
        ) ORDER BY t.relname, c.conname), '[]')
        FROM tables t JOIN pg_constraint c ON c.conrelid = t.oid
        -- PostgreSQL 18 exposes NOT NULL constraints in pg_constraint; columns already cover them.
        WHERE c.contype <> 'n'
    ),
    'indexes', (
        SELECT coalesce(jsonb_agg(jsonb_build_object(
            'table', tablename, 'name', indexname, 'definition', indexdef
        ) ORDER BY tablename, indexname), '[]') FROM pg_indexes
        WHERE schemaname = 'public' AND tablename <> '__EFMigrationsHistory'
    ),
    'sequences', (
        SELECT coalesce(jsonb_agg(jsonb_build_object(
            'name', sequencename, 'type', data_type::text, 'start', start_value::text,
            'min', min_value::text, 'max', max_value::text, 'increment', increment_by::text,
            'cycle', cycle, 'cache', cache_size::text
        ) ORDER BY sequencename), '[]') FROM pg_sequences WHERE schemaname = 'public'
    ),
    'functions', (
        SELECT coalesce(jsonb_agg(jsonb_build_object(
            'name', p.proname, 'arguments', pg_get_function_identity_arguments(p.oid),
            'returns', pg_get_function_result(p.oid), 'language', l.lanname, 'body', replace(p.prosrc, E'\r\n', E'\n'),
            'volatility', p.provolatile, 'strict', p.proisstrict, 'securityDefiner', p.prosecdef
        ) ORDER BY p.proname), '[]') FROM pg_proc p
        JOIN pg_namespace n ON n.oid = p.pronamespace JOIN pg_language l ON l.oid = p.prolang
        WHERE n.nspname = 'public'
    ),
    'triggers', (
        SELECT coalesce(jsonb_agg(jsonb_build_object(
            'table', tab.relname, 'name', t.tgname, 'definition', pg_get_triggerdef(t.oid, true),
            'enabled', t.tgenabled
        ) ORDER BY tab.relname, t.tgname), '[]') FROM pg_trigger t JOIN tables tab ON tab.oid = t.tgrelid
        WHERE NOT t.tgisinternal
    ),
    'modelSeeds', jsonb_build_object(
        'ConstraintTypes', (SELECT coalesce(jsonb_agg(to_jsonb(t) ORDER BY "Id"), '[]') FROM "ConstraintTypes" t),
        'TicketSections', (SELECT coalesce(jsonb_agg(to_jsonb(t) ORDER BY "Id"), '[]') FROM "TicketSections" t),
        'TicketTypes', (SELECT coalesce(jsonb_agg(to_jsonb(t) ORDER BY "Id"), '[]') FROM "TicketTypes" t)
    )
);
