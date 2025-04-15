DECLARE @sql NVARCHAR(MAX) = '';

-- tables without dependencies
SELECT @sql = @sql + 'DROP TABLE IF EXISTS ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';' + CHAR(13)
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE NOT EXISTS (
    SELECT 1 
    FROM sys.foreign_keys fk 
    WHERE fk.referenced_object_id = t.object_id
);

-- tables with dependencies
SELECT @sql = @sql + 'DROP TABLE IF EXISTS ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';' + CHAR(13)
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE EXISTS (
    SELECT 1 
    FROM sys.foreign_keys fk 
    WHERE fk.referenced_object_id = t.object_id
);

EXEC sp_executesql @sql;
