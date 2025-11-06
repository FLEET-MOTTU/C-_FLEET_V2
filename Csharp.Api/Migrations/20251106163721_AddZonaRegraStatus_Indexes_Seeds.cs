using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Csharp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddZonaRegraStatus_Indexes_Seeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------- ADD COLUMN Motos.ZonaId (idempotente) ----------
            migrationBuilder.Sql(@"
DECLARE v_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_count
  FROM user_tab_cols
  WHERE UPPER(table_name) = 'MOTOS' AND UPPER(column_name) = 'ZONAID';

  IF v_count = 0 THEN
    BEGIN
      EXECUTE IMMEDIATE 'ALTER TABLE ""Motos"" ADD ""ZonaId"" RAW(16)';
    EXCEPTION WHEN OTHERS THEN
      -- ORA-01430: a coluna já existe
      IF SQLCODE = -1430 THEN NULL; ELSE RAISE; END IF;
    END;
  END IF;
END;");

            // ---------- ADD COLUMN Beacons.ZonaId (idempotente) ----------
            migrationBuilder.Sql(@"
DECLARE v_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_count
  FROM user_tab_cols
  WHERE UPPER(table_name) = 'BEACONS' AND UPPER(column_name) = 'ZONAID';

  IF v_count = 0 THEN
    BEGIN
      EXECUTE IMMEDIATE 'ALTER TABLE ""Beacons"" ADD ""ZonaId"" RAW(16)';
    EXCEPTION WHEN OTHERS THEN
      IF SQLCODE = -1430 THEN NULL; ELSE RAISE; END IF;
    END;
  END IF;
END;");

            // ---------- MOTO_ZONA_HIST (Create se não existir) + FKs + Indexes ----------
            migrationBuilder.Sql(@"
DECLARE v_count NUMBER;
BEGIN
  -- Tabela
  SELECT COUNT(*) INTO v_count FROM user_tables WHERE UPPER(table_name) = 'MOTO_ZONA_HIST';
  IF v_count = 0 THEN
    EXECUTE IMMEDIATE 'CREATE TABLE ""MOTO_ZONA_HIST"" (
      ""Id"" RAW(16) NOT NULL,
      ""MotoId"" RAW(16) NOT NULL,
      ""ZonaId"" RAW(16) NOT NULL,
      ""FuncionarioId"" RAW(16),
      ""EntradaEm"" TIMESTAMP(7) NOT NULL,
      ""SaidaEm"" TIMESTAMP(7),
      CONSTRAINT PK_MOTO_ZONA_HIST PRIMARY KEY (""Id"")
    )';
  END IF;

  -- FK -> Motos(Id)
  SELECT COUNT(*) INTO v_count
  FROM user_constraints
  WHERE UPPER(table_name) = 'MOTO_ZONA_HIST'
    AND UPPER(constraint_name) = 'FK_MOTO_ZONA_HIST_MOTOS_MOTOID';
  IF v_count = 0 THEN
    BEGIN
      EXECUTE IMMEDIATE 'ALTER TABLE ""MOTO_ZONA_HIST""
        ADD CONSTRAINT FK_MOTO_ZONA_HIST_MOTOS_MotoId
        FOREIGN KEY (""MotoId"") REFERENCES ""Motos"" (""Id"") ON DELETE CASCADE';
    EXCEPTION WHEN OTHERS THEN
      IF SQLCODE IN (-2264, -2443) THEN NULL; ELSE RAISE; END IF;
    END;
  END IF;

  -- FK -> ZONAS_SYNC(ID)
  SELECT COUNT(*) INTO v_count
  FROM user_constraints
  WHERE UPPER(table_name) = 'MOTO_ZONA_HIST'
    AND UPPER(constraint_name) = 'FK_MOTO_ZONA_HIST_ZONAS_SYNC_ZONAID';
  IF v_count = 0 THEN
    BEGIN
      EXECUTE IMMEDIATE 'ALTER TABLE ""MOTO_ZONA_HIST""
        ADD CONSTRAINT FK_MOTO_ZONA_HIST_ZONAS_SYNC_ZonaId
        FOREIGN KEY (""ZonaId"") REFERENCES ""ZONAS_SYNC"" (""ID"") ON DELETE CASCADE';
    EXCEPTION WHEN OTHERS THEN
      IF SQLCODE IN (-2264, -2443) THEN NULL; ELSE RAISE; END IF;
    END;
  END IF;

  -- Índice (MotoId, EntradaEm)
  BEGIN
    EXECUTE IMMEDIATE 'CREATE INDEX IX_MOTO_ZONA_HIST_MotoId_EntradaEm ON ""MOTO_ZONA_HIST"" (""MotoId"", ""EntradaEm"")';
  EXCEPTION WHEN OTHERS THEN
    IF SQLCODE IN (-955, -1408) THEN NULL; ELSE RAISE; END IF;
  END;

  -- Índice (ZonaId)
  BEGIN
    EXECUTE IMMEDIATE 'CREATE INDEX IX_MOTO_ZONA_HIST_ZonaId ON ""MOTO_ZONA_HIST"" (""ZonaId"")';
  EXCEPTION WHEN OTHERS THEN
    IF SQLCODE IN (-955, -1408) THEN NULL; ELSE RAISE; END IF;
  END;
END;");

            // ---------- ZONA_REGRA_STATUS (Create se não existir) + FKs + Indexes ----------
            migrationBuilder.Sql(@"
DECLARE v_count NUMBER;
BEGIN
  -- Tabela
  SELECT COUNT(*) INTO v_count FROM user_tables WHERE UPPER(table_name) = 'ZONA_REGRA_STATUS';
  IF v_count = 0 THEN
    EXECUTE IMMEDIATE 'CREATE TABLE ""ZONA_REGRA_STATUS"" (
      ""Id"" RAW(16) NOT NULL,
      ""PateoId"" RAW(16) NOT NULL,
      ""StatusMoto"" NUMBER(10) NOT NULL,
      ""ZonaId"" RAW(16) NOT NULL,
      ""Prioridade"" NUMBER(10) NOT NULL,
      CONSTRAINT PK_ZONA_REGRA_STATUS PRIMARY KEY (""Id"")
    )';
  END IF;

  -- FK -> PATEOS_SYNC
  SELECT COUNT(*) INTO v_count
  FROM user_constraints
  WHERE UPPER(table_name) = 'ZONA_REGRA_STATUS'
    AND UPPER(constraint_name) = 'FK_ZONA_REGRA_STATUS_PATEOS_SYNC_PATEOID';
  IF v_count = 0 THEN
    BEGIN
      EXECUTE IMMEDIATE 'ALTER TABLE ""ZONA_REGRA_STATUS""
        ADD CONSTRAINT FK_ZONA_REGRA_STATUS_PATEOS_SYNC_PateoId
        FOREIGN KEY (""PateoId"") REFERENCES ""PATEOS_SYNC"" (""ID"") ON DELETE CASCADE';
    EXCEPTION WHEN OTHERS THEN
      IF SQLCODE IN (-2264, -2443) THEN NULL; ELSE RAISE; END IF;
    END;
  END IF;

  -- FK -> ZONAS_SYNC
  SELECT COUNT(*) INTO v_count
  FROM user_constraints
  WHERE UPPER(table_name) = 'ZONA_REGRA_STATUS'
    AND UPPER(constraint_name) = 'FK_ZONA_REGRA_STATUS_ZONAS_SYNC_ZONAID';
  IF v_count = 0 THEN
    BEGIN
      EXECUTE IMMEDIATE 'ALTER TABLE ""ZONA_REGRA_STATUS""
        ADD CONSTRAINT FK_ZONA_REGRA_STATUS_ZONAS_SYNC_ZonaId
        FOREIGN KEY (""ZonaId"") REFERENCES ""ZONAS_SYNC"" (""ID"") ON DELETE CASCADE';
    EXCEPTION WHEN OTHERS THEN
      IF SQLCODE IN (-2264, -2443) THEN NULL; ELSE RAISE; END IF;
    END;
  END IF;

  -- UNIQUE (PateoId, StatusMoto, Prioridade)
  BEGIN
    EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX IX_ZONA_REGRA_STATUS_Pateo_Status_Prioridade
                       ON ""ZONA_REGRA_STATUS"" (""PateoId"", ""StatusMoto"", ""Prioridade"")';
  EXCEPTION WHEN OTHERS THEN
    IF SQLCODE IN (-955, -1408) THEN NULL; ELSE RAISE; END IF;
  END;

  -- INDEX (ZonaId)
  BEGIN
    EXECUTE IMMEDIATE 'CREATE INDEX IX_ZONA_REGRA_STATUS_ZonaId ON ""ZONA_REGRA_STATUS"" (""ZonaId"")';
  EXCEPTION WHEN OTHERS THEN
    IF SQLCODE IN (-955, -1408) THEN NULL; ELSE RAISE; END IF;
  END;
END;");

            // ---------- Indexes simples em Motos/Beacons (ZonaId) ----------
            migrationBuilder.Sql(@"
BEGIN
  EXECUTE IMMEDIATE 'CREATE INDEX IX_Motos_ZonaId ON ""Motos"" (""ZonaId"")';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE IN (-955, -1408) THEN NULL; ELSE RAISE; END IF;
END;");

            migrationBuilder.Sql(@"
BEGIN
  EXECUTE IMMEDIATE 'CREATE INDEX IX_Beacons_ZonaId ON ""Beacons"" (""ZonaId"")';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE IN (-955, -1408) THEN NULL; ELSE RAISE; END IF;
END;");

            // ---------- Normalizações de dados (dinâmicas e case-safe) ----------
            // Motos.Placa -> UPPER
            migrationBuilder.Sql(@"
DECLARE v_tab VARCHAR2(128); v_col VARCHAR2(128);
BEGIN
  BEGIN
    SELECT table_name INTO v_tab FROM user_tables WHERE UPPER(table_name)='MOTOS';
    SELECT column_name INTO v_col FROM user_tab_cols
      WHERE UPPER(table_name)='MOTOS' AND UPPER(column_name)='PLACA';
    EXECUTE IMMEDIATE 'UPDATE ""'||v_tab||'"" SET ""'||v_col||'"" = UPPER(""'||v_col||'"" ) WHERE ""'||v_col||'"" IS NOT NULL';
  EXCEPTION WHEN NO_DATA_FOUND THEN NULL; END;
END;");

            // TagsBle.CodigoUnicoTag -> UPPER
            migrationBuilder.Sql(@"
DECLARE v_tab VARCHAR2(128); v_col VARCHAR2(128);
BEGIN
  BEGIN
    SELECT table_name INTO v_tab FROM user_tables WHERE UPPER(table_name)='TAGSBLE';
    SELECT column_name INTO v_col FROM user_tab_cols
      WHERE UPPER(table_name)='TAGSBLE' AND UPPER(column_name)='CODIGOUNICOTAG';
    EXECUTE IMMEDIATE 'UPDATE ""'||v_tab||'"" SET ""'||v_col||'"" = UPPER(""'||v_col||'"" )';
  EXCEPTION WHEN NO_DATA_FOUND THEN NULL; END;
END;");

            // Beacons.BeaconId -> UPPER
            migrationBuilder.Sql(@"
DECLARE v_tab VARCHAR2(128); v_col VARCHAR2(128);
BEGIN
  BEGIN
    SELECT table_name INTO v_tab FROM user_tables WHERE UPPER(table_name)='BEACONS';
    SELECT column_name INTO v_col FROM user_tab_cols
      WHERE UPPER(table_name)='BEACONS' AND UPPER(column_name)='BEACONID';
    EXECUTE IMMEDIATE 'UPDATE ""'||v_tab||'"" SET ""'||v_col||'"" = UPPER(""'||v_col||'"" )';
  EXCEPTION WHEN NO_DATA_FOUND THEN NULL; END;
END;");

            // Motos.DataCriacaoRegistro -> SYSTIMESTAMP se NULL
            migrationBuilder.Sql(@"
DECLARE v_tab VARCHAR2(128); v_col VARCHAR2(128);
BEGIN
  BEGIN
    SELECT table_name INTO v_tab FROM user_tables WHERE UPPER(table_name)='MOTOS';
    SELECT column_name INTO v_col FROM user_tab_cols
      WHERE UPPER(table_name)='MOTOS' AND UPPER(column_name)='DATACRIACAOREGISTRO';
    EXECUTE IMMEDIATE 'UPDATE ""'||v_tab||'"" SET ""'||v_col||'"" = SYSTIMESTAMP WHERE ""'||v_col||'"" IS NULL';
  EXCEPTION WHEN NO_DATA_FOUND THEN NULL; END;
END;");

            // ---------- Índices funcionais (UPPER) dinâmicos e idempotentes ----------
            // UX_MOTOS_PLACA_UPPER
            migrationBuilder.Sql(@"
DECLARE v_tab VARCHAR2(128); v_col VARCHAR2(128); v_count NUMBER;
BEGIN
  BEGIN
    SELECT table_name INTO v_tab FROM user_tables WHERE UPPER(table_name)='MOTOS';
    SELECT column_name INTO v_col FROM user_tab_cols
      WHERE UPPER(table_name)='MOTOS' AND UPPER(column_name)='PLACA';
    SELECT COUNT(*) INTO v_count FROM user_indexes WHERE UPPER(index_name)='UX_MOTOS_PLACA_UPPER';
    IF v_count = 0 THEN
      EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX UX_MOTOS_PLACA_UPPER ON ""'||v_tab||'"" (UPPER(""'||v_col||'"")) LOGGING';
    END IF;
  EXCEPTION WHEN NO_DATA_FOUND THEN NULL; END;
END;");

            // UX_TAGSBLE_CODIGO_UPPER
            migrationBuilder.Sql(@"
DECLARE v_tab VARCHAR2(128); v_col VARCHAR2(128); v_count NUMBER;
BEGIN
  BEGIN
    SELECT table_name INTO v_tab FROM user_tables WHERE UPPER(table_name)='TAGSBLE';
    SELECT column_name INTO v_col FROM user_tab_cols
      WHERE UPPER(table_name)='TAGSBLE' AND UPPER(column_name)='CODIGOUNICOTAG';
    SELECT COUNT(*) INTO v_count FROM user_indexes WHERE UPPER(index_name)='UX_TAGSBLE_CODIGO_UPPER';
    IF v_count = 0 THEN
      EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX UX_TAGSBLE_CODIGO_UPPER ON ""'||v_tab||'"" (UPPER(""'||v_col||'"")) LOGGING';
    END IF;
  EXCEPTION WHEN NO_DATA_FOUND THEN NULL; END;
END;");

            // UX_BEACONS_BEACONID_UPPER
            migrationBuilder.Sql(@"
DECLARE v_tab VARCHAR2(128); v_col VARCHAR2(128); v_count NUMBER;
BEGIN
  BEGIN
    SELECT table_name INTO v_tab FROM user_tables WHERE UPPER(table_name)='BEACONS';
    SELECT column_name INTO v_col FROM user_tab_cols
      WHERE UPPER(table_name)='BEACONS' AND UPPER(column_name)='BEACONID';
    SELECT COUNT(*) INTO v_count FROM user_indexes WHERE UPPER(index_name)='UX_BEACONS_BEACONID_UPPER';
    IF v_count = 0 THEN
      EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX UX_BEACONS_BEACONID_UPPER ON ""'||v_tab||'"" (UPPER(""'||v_col||'"")) LOGGING';
    END IF;
  EXCEPTION WHEN NO_DATA_FOUND THEN NULL; END;
END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ---------- Drop FKs ----------
            migrationBuilder.Sql(@"
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE ""Beacons"" DROP CONSTRAINT FK_BEACONS_ZONAS_SYNC_ZonaId';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE = -2443 THEN NULL; ELSE RAISE; END IF;
END;");

            migrationBuilder.Sql(@"
BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE ""Motos"" DROP CONSTRAINT FK_MOTOS_ZONAS_SYNC_ZonaId';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE = -2443 THEN NULL; ELSE RAISE; END IF;
END;");

            // ---------- Drop Indexes ----------
            migrationBuilder.Sql(@"
BEGIN
  EXECUTE IMMEDIATE 'DROP INDEX IX_MOTO_ZONA_HIST_MotoId_EntradaEm';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE IN (-1418, -942) THEN NULL; ELSE RAISE; END IF;
END;");

            migrationBuilder.Sql(@"
BEGIN
  EXECUTE IMMEDIATE 'DROP INDEX IX_MOTO_ZONA_HIST_ZonaId';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE IN (-1418, -942) THEN NULL; ELSE RAISE; END IF;
END;");

            migrationBuilder.Sql(@"
BEGIN
  EXECUTE IMMEDIATE 'DROP INDEX IX_ZONA_REGRA_STATUS_Pateo_Status_Prioridade';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE IN (-1418, -942) THEN NULL; ELSE RAISE; END IF;
END;");

            migrationBuilder.Sql(@"
BEGIN
  EXECUTE IMMEDIATE 'DROP INDEX IX_ZONA_REGRA_STATUS_ZonaId';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE IN (-1418, -942) THEN NULL; ELSE RAISE; END IF;
END;");

            migrationBuilder.Sql(@"
BEGIN
  EXECUTE IMMEDIATE 'DROP INDEX IX_Motos_ZonaId';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE IN (-1418, -942) THEN NULL; ELSE RAISE; END IF;
END;");

            migrationBuilder.Sql(@"
BEGIN
  EXECUTE IMMEDIATE 'DROP INDEX IX_Beacons_ZonaId';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE IN (-1418, -942) THEN NULL; ELSE RAISE; END IF;
END;");

            migrationBuilder.Sql(@"
BEGIN
  EXECUTE IMMEDIATE 'DROP INDEX UX_MOTOS_PLACA_UPPER';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE IN (-1418, -942) THEN NULL; ELSE RAISE; END IF;
END;");

            migrationBuilder.Sql(@"
BEGIN
  EXECUTE IMMEDIATE 'DROP INDEX UX_TAGSBLE_CODIGO_UPPER';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE IN (-1418, -942) THEN NULL; ELSE RAISE; END IF;
END;");

            migrationBuilder.Sql(@"
BEGIN
  EXECUTE IMMEDIATE 'DROP INDEX UX_BEACONS_BEACONID_UPPER';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE IN (-1418, -942) THEN NULL; ELSE RAISE; END IF;
END;");

            // ---------- Drop Tables ----------
            migrationBuilder.Sql(@"
BEGIN
  EXECUTE IMMEDIATE 'DROP TABLE ""ZONA_REGRA_STATUS"" CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE = -942 THEN NULL; ELSE RAISE; END IF;
END;");

            migrationBuilder.Sql(@"
BEGIN
  EXECUTE IMMEDIATE 'DROP TABLE ""MOTO_ZONA_HIST"" CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE = -942 THEN NULL; ELSE RAISE; END IF;
END;");

            // ---------- Drop Columns ----------
            migrationBuilder.Sql(@"
DECLARE v_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_count FROM user_tab_cols
  WHERE UPPER(table_name)='BEACONS' AND UPPER(column_name)='ZONAID';
  IF v_count = 1 THEN
    EXECUTE IMMEDIATE 'ALTER TABLE ""Beacons"" DROP COLUMN ""ZonaId""';
  END IF;
END;");

            migrationBuilder.Sql(@"
DECLARE v_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_count FROM user_tab_cols
  WHERE UPPER(table_name)='MOTOS' AND UPPER(column_name)='ZONAID';
  IF v_count = 1 THEN
    EXECUTE IMMEDIATE 'ALTER TABLE ""Motos"" DROP COLUMN ""ZonaId""';
  END IF;
END;");
        }
    }
}
