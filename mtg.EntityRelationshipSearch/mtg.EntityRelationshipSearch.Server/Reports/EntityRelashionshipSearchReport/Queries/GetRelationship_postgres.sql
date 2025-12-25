DO $$ 
DECLARE 
    source_entity_id BIGINT := {1};  -- ID сущности (источник)
	  source_table_name TEXT := '{0}'; -- Наименование таблицы сущности (источник)
    rec RECORD;
    query TEXT;
BEGIN
    -- Создаётся временная таблица, если её нет (чтобы хранить результат)
    CREATE TEMP TABLE IF NOT EXISTS tmp_result_table (
        entity_type TEXT,
        entity_id INT,
        discriminator TEXT,
        entity_name TEXT
    ) ON COMMIT DROP;

    -- Поиск таблиц и колонок, где есть связи с сущностью
    FOR rec IN 
        WITH uses_tables AS (
            SELECT
                conrelid::regclass AS table_name,
                a.attname AS column_name
            FROM pg_constraint AS c
            JOIN pg_attribute AS a 
                ON a.attnum = ANY(c.conkey) 
                AND a.attrelid = c.conrelid
            WHERE confrelid = source_table_name::regclass
        ) 
        SELECT table_name, column_name FROM uses_tables
    LOOP
        -- Проверка, есть ли в таблице колонка "name"
        IF EXISTS (
            SELECT 1 
            FROM information_schema.columns 
            WHERE table_name = rec.table_name::text AND column_name = 'name'
        ) THEN
            -- SQL-запрос с учетом наличия "name"
            query := format(
                'INSERT INTO tmp_result_table (entity_type, entity_id, discriminator, entity_name) ' ||
                'SELECT %L, id, discriminator, name FROM %I WHERE %I = %s',
                rec.table_name, rec.table_name, rec.column_name, source_entity_id
            );
        ELSE
            -- SQL-запрос без "name", если такой колонки нет
            query := format(
                'INSERT INTO tmp_result_table (entity_type, entity_id, discriminator, entity_name) ' ||
                'SELECT %L, id, discriminator, NULL FROM %I WHERE %I = %s',
                rec.table_name, rec.table_name, rec.column_name, source_entity_id
            );
        END IF;

        -- Выполнение запроса
        EXECUTE query;
    END LOOP;
	-- Поиск связей в задачах
	INSERT INTO tmp_result_table (entity_type, entity_id, discriminator, entity_name)
	SELECT 
        'sungero_wf_task' AS entity_type,
        t.id AS entity_id,
        t.discriminator AS discriminator,
        t.subject AS entity_name
    FROM sungero_wf_attachment att
    JOIN sungero_wf_task t ON t.id = att.task
    WHERE att.attachmentid = source_entity_id;
	
END $$;

-- Вывод результата после выполнения блока
SELECT entity_type, entity_id, COALESCE(discriminator, '') as discriminator, COALESCE(entity_name, '') as entity_name FROM tmp_result_table;