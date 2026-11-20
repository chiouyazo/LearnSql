CREATE VIEW COLUMNS AS
WITH RawColumns AS (
    SELECT
        m.name AS TableName,
        p.cid AS Cid,
        p.name AS ColumnName,
        CASE WHEN p."notnull" = 1 OR p.pk > 0 THEN 1 ELSE 0 END AS NullFlag,
        p.dflt_value AS DefaultValue,
        lower(trim(p.type)) AS DeclaredType
    FROM sqlite_schema m
        JOIN pragma_table_xinfo(m.name) p
    WHERE m.type IN ('table', 'view')
        AND m.name NOT LIKE 'sqlite\_%' ESCAPE '\'
        AND m.name NOT IN ('TABLES', 'COLUMNS')
        AND p.hidden IN (0, 2, 3)
),
SplitType AS (
    SELECT
        *,
        CASE
            WHEN instr(DeclaredType, '(') > 0
                THEN trim(substr(DeclaredType, 1, instr(DeclaredType, '(') - 1))
            WHEN DeclaredType LIKE '% max'
                THEN trim(substr(DeclaredType, 1, length(DeclaredType) - 4))
            ELSE DeclaredType END AS BaseType,
        CASE
            WHEN instr(DeclaredType, '(') > 0
                THEN trim(substr(DeclaredType, instr(DeclaredType, '(') + 1,
                    instr(DeclaredType, ')') - instr(DeclaredType, '(') - 1))
            WHEN DeclaredType LIKE '% max' THEN 'max'
            ELSE NULL END AS TypeArgs
    FROM RawColumns
),
ParsedType AS (
    SELECT
        *,
        CASE
            WHEN TypeArgs IS NULL THEN NULL
            WHEN TypeArgs = 'max' THEN -1
            WHEN instr(TypeArgs, ',') > 0 THEN CAST(trim(substr(TypeArgs, 1, instr(TypeArgs, ',') - 1)) AS INTEGER)
            ELSE CAST(TypeArgs AS INTEGER) END AS FirstArg,
        CASE
            WHEN TypeArgs IS NOT NULL AND instr(TypeArgs, ',') > 0
                THEN CAST(trim(substr(TypeArgs, instr(TypeArgs, ',') + 1)) AS INTEGER)
            ELSE NULL END AS SecondArg,
        CASE BaseType
            WHEN 'integer' THEN 'int'
            WHEN 'real' THEN 'float'
            WHEN 'double' THEN 'float'
            WHEN '' THEN NULL
            ELSE BaseType END AS DataType
    FROM SplitType
)
SELECT
    'LearnSql' COLLATE NOCASE AS [TABLE_CATALOG],
    'dbo' COLLATE NOCASE AS [TABLE_SCHEMA],
    TableName COLLATE NOCASE AS [TABLE_NAME],
    ColumnName COLLATE NOCASE AS [COLUMN_NAME],
    Cid + 1 AS [ORDINAL_POSITION],
    DefaultValue AS [COLUMN_DEFAULT],
    CASE NullFlag WHEN 1 THEN 'NO' ELSE 'YES' END AS [IS_NULLABLE],
    DataType COLLATE NOCASE AS [DATA_TYPE],
    CASE WHEN DataType IN ('char', 'nchar', 'varchar', 'nvarchar', 'binary', 'varbinary')
        THEN COALESCE(FirstArg, 1) ELSE NULL END AS [CHARACTER_MAXIMUM_LENGTH],
    CASE DataType
        WHEN 'bit' THEN 1
        WHEN 'tinyint' THEN 3
        WHEN 'smallint' THEN 5
        WHEN 'int' THEN 10
        WHEN 'bigint' THEN 19
        WHEN 'float' THEN 53
        WHEN 'decimal' THEN COALESCE(FirstArg, 18)
        WHEN 'numeric' THEN COALESCE(FirstArg, 18)
        ELSE NULL END AS [NUMERIC_PRECISION],
    CASE DataType
        WHEN 'bit' THEN 0
        WHEN 'tinyint' THEN 0
        WHEN 'smallint' THEN 0
        WHEN 'int' THEN 0
        WHEN 'bigint' THEN 0
        WHEN 'decimal' THEN COALESCE(SecondArg, 0)
        WHEN 'numeric' THEN COALESCE(SecondArg, 0)
        ELSE NULL END AS [NUMERIC_SCALE],
    CASE DataType
        WHEN 'date' THEN 0
        WHEN 'datetime' THEN 3
        WHEN 'datetime2' THEN COALESCE(FirstArg, 7)
        WHEN 'time' THEN COALESCE(FirstArg, 7)
        ELSE NULL END AS [DATETIME_PRECISION]
FROM ParsedType;
