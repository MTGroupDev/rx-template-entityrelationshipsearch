DECLARE
	@source_entity_id BIGINT = {1},
	@source_table_name NVARCHAR(128) = '{0}',
	@query NVARCHAR(MAX),
	@table_name NVARCHAR(128),
  @column_name NVARCHAR(128);

DROP TABLE IF EXISTS #tmp_result_table;

CREATE TABLE #tmp_result_table
(entity_type NVARCHAR(255),
entity_id BIGINT,
discriminator NVARCHAR(255),
entity_name NVARCHAR(255)
);

-- Поиск таблиц и колонок, где есть связи с сущностью
DECLARE db_cursor CURSOR FOR
    SELECT 
        OBJECT_NAME(fk.parent_object_id) AS table_name,
        COL_NAME(fc.parent_object_id, fc.parent_column_id) AS column_name
    FROM sys.foreign_keys AS fk
    INNER JOIN sys.foreign_key_columns AS fc 
        ON fk.object_id = fc.constraint_object_id
    WHERE OBJECT_NAME(fk.referenced_object_id) = @source_table_name;

OPEN db_cursor;
FETCH NEXT FROM db_cursor INTO @table_name, @column_name;

-- Для каждой таблицы ищем связи и выполняем запрос
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Проверка, есть ли в таблице колонка "name"
    IF EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = @table_name AND COLUMN_NAME = 'name'
    )
    BEGIN
        -- Если есть колонка "name", выполняем запрос с ней
        SET @query = N'
            INSERT INTO #tmp_result_table (entity_type, entity_id, discriminator, entity_name)
            SELECT ''' + @table_name + ''', id, discriminator, name 
            FROM ' + @table_name + ' 
            WHERE ' + @column_name + ' = @source_entity_id';
    END
    ELSE
    BEGIN
        -- Если нет колонки "name", выполняем запрос без неё
        SET @query = N'
            INSERT INTO #tmp_result_table (entity_type, entity_id, discriminator, entity_name)
            SELECT ''' + @table_name + ''', id, discriminator, NULL 
            FROM ' + @table_name + ' 
            WHERE ' + @column_name + ' = @source_entity_id';
    END

    -- Выполнение динамического запроса
    EXEC sp_executesql @query, N'@source_entity_id BIGINT', @source_entity_id;

    FETCH NEXT FROM db_cursor INTO @table_name, @column_name;
END

-- Закрытие и удаление курсора
CLOSE db_cursor;
DEALLOCATE db_cursor;

-- Поиск связей в задачах
SET @query = N'
    INSERT INTO #tmp_result_table (entity_type, entity_id, discriminator, entity_name)
    SELECT 
        ''sungero_wf_task'' AS entity_type,
        t.id AS entity_id,
        t.discriminator AS discriminator,
        t.subject AS entity_name
    FROM sungero_wf_attachment att
    JOIN sungero_wf_task t ON t.id = att.task
    WHERE att.attachmentid = @source_entity_id';

EXEC sp_executesql @query, N'@source_entity_id BIGINT', @source_entity_id;

-- Вывод результата после выполнения блока
SELECT entity_type, entity_id, COALESCE(discriminator, '') as discriminator, COALESCE(entity_name, '') as entity_name FROM #tmp_result_table;