-- Autorització de tractament de dades a tercers.
-- Per a BD ja existents (no hi ha migracions). Vegeu db/schema.sql.
-- MySQL 5.6 no té ADD COLUMN IF NOT EXISTS: si alguna columna ja existeix
-- (comproveu-ho amb SHOW COLUMNS FROM tercers), traieu-ne la línia.
-- Les persones existents queden com a "no signada" (0).

ALTER TABLE `tercers`
  ADD COLUMN `AutoritzacioTractamentSignada` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'La persona ha signat l''autoritzacio de tractament de dades (es confirma en donar-la d''alta)',
  ADD COLUMN `DataAutoritzacio` datetime DEFAULT NULL;
