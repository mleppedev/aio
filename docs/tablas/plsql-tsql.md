# Contexto y Propósito

## ¿Qué es?
PL/SQL (Oracle) y T-SQL (SQL Server) son extensiones procedimentales de SQL que permiten programar lógica compleja en la base de datos mediante procedimientos, funciones y triggers. Son fundamentales para entornos donde la lógica de negocio reside cerca de los datos.

## ¿Por qué?
Porque cada motor tiene particularidades de sintaxis, manejo de transacciones y paquetes. En mi experiencia, conocer ambos permitió diseñar sistemas portables y migraciones más rápidas entre Oracle y SQL Server en proyectos bancarios y de retail.

## ¿Para qué?
- **Comparar diferencias** clave en variables, cursores, control de flujo y paquetes.  
- **Diseñar migraciones** entre motores reduciendo riesgos.  
- **Optimizar lógica de negocio** con procedimientos almacenados robustos.  
- **Dominar transacciones** y manejo de errores según motor.  

## Valor agregado desde la experiencia
- Migraciones de **Oracle a SQL Server** en banca fueron exitosas al mapear excepciones y cursores correctamente.  
- **Paquetes en PL/SQL** facilitaron modularidad que replicamos con esquemas en T-SQL.  
- Conocer **MERGE/UPSERT** en ambos motores evitó duplicidad de lógica de inserción y actualización.  

# 🧩 PL/SQL vs T-SQL — Guía de Referencia

Comparativa práctica entre PL/SQL (Oracle) y T‑SQL (SQL Server) para escribir procedimientos, funciones, triggers y scripts portables.

---

## 📚 Visión general

| Aspecto       | PL/SQL (Oracle)                        | T‑SQL (SQL Server)              |
| ------------- | -------------------------------------- | ------------------------------- |
| Motor         | Oracle Database                        | Microsoft SQL Server            |
| Lenguaje      | Extensión procedimental de SQL         | Extensión procedimental de SQL  |
| Paquetes      | Sí (packages)                          | No (usar esquemas y módulos)    |
| Identidad     | `SEQUENCE` + `NEXTVAL`                 | `IDENTITY`, `SEQUENCE`          |
| Paginación    | `ROWNUM`, `ROW_NUMBER()`/`FETCH FIRST` | `OFFSET … FETCH`                |
| Variables     | `DECLARE … BEGIN … END;`               | `DECLARE @v TYPE;`              |
| Excepciones   | `EXCEPTION … WHEN`                     | `TRY … CATCH`                   |
| Transacciones | `COMMIT`/`ROLLBACK`                    | `COMMIT`/`ROLLBACK`             |
| Herramientas  | SQL\*Plus, SQLcl, SQL Developer        | SSMS, sqlcmd, Azure Data Studio |

---

## 🔤 Variables y tipos

### PL/SQL

```sql
DECLARE
  v_id NUMBER(10);
  v_name VARCHAR2(100);
  v_dt DATE := SYSDATE;
BEGIN
  v_id := 1;
  v_name := 'Alice';
END;/
```

### T‑SQL

```sql
DECLARE @id INT;
DECLARE @name NVARCHAR(100);
DECLARE @dt DATETIME2 = SYSDATETIME();

SET @id = 1;
SET @name = N'Alice';
```

---

## 🔁 Control de flujo

| Estructura | PL/SQL                               | T‑SQL                               |
| ---------- | ------------------------------------ | ----------------------------------- |
| IF         | `IF … THEN … ELSIF … ELSE … END IF;` | `IF … BEGIN … END ELSE BEGIN … END` |
| LOOP       | `LOOP … EXIT WHEN … END LOOP;`       | `WHILE … BEGIN … END`               |
| CASE       | `CASE WHEN … THEN … END;`            | `CASE WHEN … THEN … END`            |

---

## 🧮 Funciones y procedimientos

### PL/SQL

```sql
CREATE OR REPLACE PROCEDURE add_user(p_name IN VARCHAR2)
AS
BEGIN
  INSERT INTO users(name) VALUES(p_name);
END;
/

CREATE OR REPLACE FUNCTION user_count RETURN NUMBER AS
  v_cnt NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_cnt FROM users;
  RETURN v_cnt;
END;
/
```

### T‑SQL

```sql
CREATE OR ALTER PROCEDURE dbo.add_user @name NVARCHAR(100)
AS
BEGIN
  SET NOCOUNT ON;
  INSERT INTO dbo.users(name) VALUES(@name);
END;
GO

CREATE OR ALTER FUNCTION dbo.user_count()
RETURNS INT
AS
BEGIN
  RETURN (SELECT COUNT(*) FROM dbo.users);
END;
GO
```

---

## 🚨 Manejo de errores

### PL/SQL

```sql
BEGIN
  -- código
EXCEPTION
  WHEN NO_DATA_FOUND THEN NULL;
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE(SQLCODE || ' - ' || SQLERRM);
END;
/
```

### T‑SQL

```sql
BEGIN TRY
  -- código
END TRY
BEGIN CATCH
  SELECT ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg;
END CATCH;
```

---

## 🔒 Transacciones

| Operación | PL/SQL                          | T‑SQL                             |
| --------- | ------------------------------- | --------------------------------- |
| Begin     | Implícita al ejecutar bloque    | `BEGIN TRAN [name]`               |
| Commit    | `COMMIT;`                       | `COMMIT TRAN [name];`             |
| Rollback  | `ROLLBACK;`                     | `ROLLBACK TRAN [name];`           |
| Savepoint | `SAVEPOINT sp; ROLLBACK TO sp;` | `SAVE TRAN sp; ROLLBACK TRAN sp;` |

---

## 📦 Paquetes vs Esquemas

- PL/SQL: agrupa objetos en `PACKAGE`/`PACKAGE BODY`.
- T‑SQL: usa esquemas (`dbo`, `sales`, etc.) y módulos (procedures, functions).

```sql
-- PL/SQL
CREATE OR REPLACE PACKAGE user_api AS
  PROCEDURE add_user(p_name VARCHAR2);
  FUNCTION user_count RETURN NUMBER;
END user_api;
/

CREATE OR REPLACE PACKAGE BODY user_api AS
  PROCEDURE add_user(p_name VARCHAR2) AS
  BEGIN
    INSERT INTO users(name) VALUES(p_name);
  END;
  FUNCTION user_count RETURN NUMBER AS
    v NUMBER; BEGIN SELECT COUNT(*) INTO v FROM users; RETURN v; END;
END user_api;
/
```

---

## 📑 Cursores

### PL/SQL

```sql
DECLARE
  CURSOR c IS SELECT id, name FROM users;
  r c%ROWTYPE;
BEGIN
  OPEN c;
  LOOP
    FETCH c INTO r;
    EXIT WHEN c%NOTFOUND;
    NULL; -- procesar r
  END LOOP;
  CLOSE c;
END;
/
```

### T‑SQL

```sql
DECLARE c CURSOR FAST_FORWARD FOR SELECT id, name FROM dbo.users;
DECLARE @id INT, @name NVARCHAR(100);
OPEN c;
FETCH NEXT FROM c INTO @id, @name;
WHILE @@FETCH_STATUS = 0
BEGIN
  -- procesar
  FETCH NEXT FROM c INTO @id, @name;
END
CLOSE c; DEALLOCATE c;
```

---

## 🗃️ Temporales y variables tabla

| Caso           | PL/SQL / Oracle          | T‑SQL / SQL Server                 |
| -------------- | ------------------------ | ---------------------------------- |
| Tabla temporal | `GLOBAL TEMPORARY TABLE` | `#temp` (local), `##temp` (global) |
| Variable tabla | Colecciones (TABLE OF)   | `DECLARE @t TABLE (...)`           |

```sql
-- T‑SQL variable tabla
DECLARE @t TABLE(id INT PRIMARY KEY, name NVARCHAR(50));
INSERT INTO @t VALUES(1, N'A');
```

---

## 🔑 Identidad y secuencias

| Operación       | PL/SQL (Oracle)                   | T‑SQL (SQL Server)                |
| --------------- | --------------------------------- | --------------------------------- |
| Crear secuencia | `CREATE SEQUENCE s START WITH 1;` | `CREATE SEQUENCE s START WITH 1;` |
| Usar secuencia  | `s.NEXTVAL`                       | `NEXT VALUE FOR s`                |
| Identidad       | Emulada con secuencia/trigger     | `IDENTITY(1,1)`                   |

---

## 🧰 MERGE (UPSERT)

### PL/SQL

```sql
MERGE INTO target t
USING (SELECT :id id, :name name FROM dual) s
ON (t.id = s.id)
WHEN MATCHED THEN UPDATE SET t.name = s.name
WHEN NOT MATCHED THEN INSERT (id, name) VALUES (s.id, s.name);
```

### T‑SQL

```sql
MERGE dbo.target AS t
USING (SELECT @id AS id, @name AS name) AS s
ON (t.id = s.id)
WHEN MATCHED THEN UPDATE SET t.name = s.name
WHEN NOT MATCHED THEN INSERT (id, name) VALUES (s.id, s.name);
```

---

## 📄 Paginación

| Motor         | Sintaxis                                                  |
| ------------- | --------------------------------------------------------- |
| Oracle 12c+   | `... ORDER BY col FETCH FIRST 10 ROWS ONLY OFFSET 20`     |
| Oracle legacy | `ROW_NUMBER() OVER(ORDER BY col) BETWEEN …`               |
| SQL Server    | `... ORDER BY col OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY` |

---

## 🕒 Fechas y funciones comunes

| Intención         | Oracle                               | SQL Server                          |
| ----------------- | ------------------------------------ | ----------------------------------- |
| Fecha/hora actual | `SYSTIMESTAMP`                       | `SYSDATETIME()`                     |
| Suma días         | `dt + 7`                             | `DATEADD(DAY, 7, dt)`               |
| Diferencia días   | `dt2 - dt1`                          | `DATEDIFF(DAY, dt1, dt2)`           |
| Formateo          | `TO_CHAR(dt,'YYYY-MM-DD')`           | `FORMAT(dt,'yyyy-MM-dd')`/`CONVERT` |
| Parseo            | `TO_DATE('2025-09-01','YYYY-MM-DD')` | `CONVERT(DATE,'2025-09-01')`        |

---

## 🔤 Strings útiles

| Intención   | Oracle          | SQL Server                           |
| ----------- | --------------- | ------------------------------------ | ----- | ------------- |
| Concatenar  | `str1           |                                      | str2` | `str1 + str2` |
| Substring   | `SUBSTR(s,1,3)` | `SUBSTRING(s,1,3)`                   |
| Longitud    | `LENGTH(s)`     | `LEN(s)`                             |
| Trim        | `TRIM(s)`       | `LTRIM(RTRIM(s))` o `TRIM()` (2017+) |
| Upper/Lower | `UPPER/LOWER`   | `UPPER/LOWER`                        |

---

## 🔔 Triggers

### PL/SQL

```sql
CREATE OR REPLACE TRIGGER trg_bi_users
BEFORE INSERT ON users
FOR EACH ROW
BEGIN
  :NEW.created_at := SYSTIMESTAMP;
END;
/
```

### T‑SQL

```sql
CREATE OR ALTER TRIGGER dbo.trg_ai_users
ON dbo.users AFTER INSERT AS
BEGIN
  SET NOCOUNT ON;
  UPDATE u SET created_at = SYSDATETIME()
  FROM dbo.users u
  JOIN inserted i ON i.id = u.id;
END;
GO
```

---

## 🛠️ Rendimiento y buenas prácticas

- Usa tipos correctos y longitudes ajustadas (evita `NVARCHAR(MAX)`/`CLOB` sin necesidad).
- Indexa por columnas de filtrado y join; evita funciones sobre columnas en predicados.
- Prefiere `SET NOCOUNT ON` (T‑SQL) en procedimientos intensivos.
- En Oracle, considera `BULK COLLECT` y `FORALL` para cargas masivas.
- Evita cursores si puedes reescribir con set‑based queries.
- Usa `MERGE` para upserts atómicos; valida condiciones de carrera.

---

## 🧭 Mapa rápido de equivalencias

| Concepto       | Oracle                    | SQL Server                  |
| -------------- | ------------------------- | --------------------------- |
| TRUE/FALSE     | `1/0` (no boolean en SQL) | `BIT` (0/1)                 |
| Auto‑increment | Secuencia + trigger       | `IDENTITY` o `SEQUENCE`     |
| Limit          | `FETCH FIRST n ROWS`      | `TOP(n)` / `OFFSET … FETCH` |
| Comentarios    | `--` y `/* ... */`        | `--` y `/* ... */`          |

---

## 📎 Snippet: procedimiento con transacción y error

### PL/SQL

```sql
CREATE OR REPLACE PROCEDURE transfer(p_from NUMBER, p_to NUMBER, p_amt NUMBER) AS
BEGIN
  UPDATE accounts SET balance = balance - p_amt WHERE id = p_from;
  UPDATE accounts SET balance = balance + p_amt WHERE id = p_to;
  COMMIT;
EXCEPTION
  WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/
```

### T‑SQL

```sql
CREATE OR ALTER PROCEDURE dbo.transfer @from INT, @to INT, @amt DECIMAL(18,2)
AS
BEGIN
  SET XACT_ABORT ON;
  BEGIN TRY
    BEGIN TRAN;
      UPDATE dbo.accounts SET balance = balance - @amt WHERE id = @from;
      UPDATE dbo.accounts SET balance = balance + @amt WHERE id = @to;
    COMMIT TRAN;
  END TRY
  BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRAN;
    THROW;
  END CATCH
END;
GO
```

---

> Esta guía sirve como referencia rápida para migraciones y trabajo diario multi‑motor.
