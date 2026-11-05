CREATE VIEW COLUMNS AS
SELECT
    'master'  COLLATE NOCASE AS [TABLE_CATALOG],
    'main'    COLLATE NOCASE AS [TABLE_SCHEMA],
    m.name    COLLATE NOCASE AS [TABLE_NAME],
    p.name    COLLATE NOCASE AS [COLUMN_NAME],
    p.cid + 1                AS [ORDINAL_POSITION],
    p.dflt_value             AS [COLUMN_DEFAULT],
    CASE p."notnull" WHEN 1 THEN 'NO' ELSE 'YES' END AS [IS_NULLABLE],
    p.type    COLLATE NOCASE AS [DATA_TYPE]
FROM sqlite_schema m
    JOIN pragma_table_info(m.name) p
WHERE m.type = 'table'
    AND m.name NOT LIKE 'sqlite_%';