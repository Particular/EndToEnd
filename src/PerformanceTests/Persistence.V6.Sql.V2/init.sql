IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'V6')
BEGIN
	EXEC('CREATE SCHEMA V6')
END

--Truncate relevant tables

DECLARE @table_name SYSNAME;
DECLARE @cmd NVARCHAR(MAX);
DECLARE table_cursor CURSOR FOR SELECT '['+s.name+'].['+t.name+']' FROM sys.tables t JOIN sys.schemas s ON t.schema_id=s.schema_id AND s.name='V6'

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @table_name;

WHILE @@FETCH_STATUS = 0 BEGIN
  SELECT @cmd = 'TRUNCATE TABLE '+@table_name;
  EXEC (@cmd);
  FETCH NEXT FROM table_cursor INTO @table_name;
END

CLOSE table_cursor;
DEALLOCATE table_cursor;
