-- Estat d'inscripció, llista d'espera i sol·licituds web a curs_alumnes_cursets.
-- Per a BD ja existents (no hi ha migracions). Vegeu db/schema.sql.
-- MySQL 5.6 no té ADD COLUMN IF NOT EXISTS: si alguna columna ja existeix
-- (comproveu-ho amb SHOW COLUMNS FROM curs_alumnes_cursets), traieu-ne la línia.
-- Les files existents queden com a 'Admesa' (valor per defecte).

ALTER TABLE `curs_alumnes_cursets`
  ADD COLUMN `Estat` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Admesa',
  ADD COLUMN `OrdreLlistaEspera` int(11) DEFAULT NULL,
  ADD COLUMN `DataSollicitud` date DEFAULT NULL,
  ADD COLUMN `Origen` varchar(10) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD KEY `IX_curs_alumnes_cursets_CursetId_Estat` (`CursetId`,`Estat`);
