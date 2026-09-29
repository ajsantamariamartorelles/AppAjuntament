-- ============================================================================
-- Estructura de la base de dades (MySQL 5.6) + catàleg inicial
-- Generat el 2026-09-28 a partir d'una còpia neta de producció.
-- Conté NOMÉS estructura i dades de catàleg (sense cap dada personal ni
-- pròpia d'un ajuntament concret, tret d'`arees` i `anys`, que són
-- categories administratives d'exemple: qui reutilitzi l'aplicació les pot
-- buidar i refer-les a mida).
-- Vegeu AGENTS.md, regla 6: aquest fitxer viatja amb tot canvi de model.
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Dumping database structure for gestorsubvencions
CREATE DATABASE IF NOT EXISTS `gestorsubvencions` /*!40100 DEFAULT CHARACTER SET utf8mb4 */;
USE `gestorsubvencions`;
-- Dumping structure for table gestorsubvencions.anys
CREATE TABLE IF NOT EXISTS `anys` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Any` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Any` (`Any`),
  UNIQUE KEY `uq_any` (`Any`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=latin1 COMMENT='Taula que conté els anys de subvencions';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.arees
CREATE TABLE IF NOT EXISTS `arees` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(255) NOT NULL,
  `EnsId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_arees_ens` (`EnsId`),
  CONSTRAINT `FK_Arees_Ens` FOREIGN KEY (`EnsId`) REFERENCES `ens` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=45 DEFAULT CHARSET=latin1 COMMENT='Taula que conté les àrees dels organismes';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.armes_armes
CREATE TABLE IF NOT EXISTS `armes_armes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT COMMENT 'Clau primària',
  `NumSerie` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Número de sèrie de l''arma',
  `Marca` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Marca fabricant',
  `Model` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Model',
  `Calibre` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Calibre',
  `TipusArmaId` int(11) NOT NULL COMMENT 'FK → Aux_TipusArma(Id)',
  `TercerId` int(11) DEFAULT NULL COMMENT 'Titular actual FK → Tercers(Id)',
  `EnsId` int(11) NOT NULL COMMENT 'Ens propietari FK → ens(Id)',
  `DataAlta` date DEFAULT NULL COMMENT 'Data d''alta al registre',
  `DataBaixa` date DEFAULT NULL COMMENT 'Data de baixa (NULL = activa)',
  `MotiuBaixa` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Observacions` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` int(11) DEFAULT NULL COMMENT 'FK → usuaris(Id)',
  `updated_by` int(11) DEFAULT NULL COMMENT 'FK → usuaris(Id)',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_arma_numserie` (`NumSerie`),
  KEY `idx_arma_tipus` (`TipusArmaId`),
  KEY `idx_arma_tercer` (`TercerId`),
  KEY `idx_arma_ens` (`EnsId`),
  KEY `idx_arma_created_by` (`created_by`),
  KEY `idx_arma_updated_by` (`updated_by`),
  CONSTRAINT `fk_arma_created_by` FOREIGN KEY (`created_by`) REFERENCES `usuaris` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_arma_ens` FOREIGN KEY (`EnsId`) REFERENCES `ens` (`Id`),
  CONSTRAINT `fk_arma_tercer` FOREIGN KEY (`TercerId`) REFERENCES `tercers` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_arma_tipusarma` FOREIGN KEY (`TipusArmaId`) REFERENCES `aux_tipusarma` (`Id`),
  CONSTRAINT `fk_arma_updated_by` FOREIGN KEY (`updated_by`) REFERENCES `usuaris` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.armes_inspeccions
CREATE TABLE IF NOT EXISTS `armes_inspeccions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ArmaId` int(11) NOT NULL COMMENT 'FK → ARMES_armes(Id)',
  `Data` date NOT NULL COMMENT 'Data de la inspecció',
  `ResultatId` int(11) NOT NULL COMMENT 'FK → Aux_ResultatInspeccio(Id)',
  `Observacions` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` int(11) DEFAULT NULL COMMENT 'FK → usuaris(Id)',
  `updated_by` int(11) DEFAULT NULL COMMENT 'FK → usuaris(Id)',
  PRIMARY KEY (`Id`),
  KEY `idx_inspec_arma` (`ArmaId`),
  KEY `idx_inspec_resultat` (`ResultatId`),
  KEY `idx_inspec_data` (`Data`),
  KEY `idx_inspec_created_by` (`created_by`),
  KEY `idx_inspec_updated_by` (`updated_by`),
  CONSTRAINT `fk_inspec_arma` FOREIGN KEY (`ArmaId`) REFERENCES `armes_armes` (`Id`),
  CONSTRAINT `fk_inspec_created_by` FOREIGN KEY (`created_by`) REFERENCES `usuaris` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_inspec_resultat` FOREIGN KEY (`ResultatId`) REFERENCES `aux_resultatinspeccio` (`Id`),
  CONSTRAINT `fk_inspec_updated_by` FOREIGN KEY (`updated_by`) REFERENCES `usuaris` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.armes_licencies
CREATE TABLE IF NOT EXISTS `armes_licencies` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ArmaId` int(11) NOT NULL COMMENT 'FK → ARMES_armes(Id)',
  `TercerId` int(11) NOT NULL COMMENT 'Titular llicència FK → Tercers(Id)',
  `TipusLicenciaId` int(11) NOT NULL COMMENT 'FK → Aux_TipusLicencia(Id)',
  `EstatLicenciaId` int(11) NOT NULL COMMENT 'FK → Aux_EstatLicencia(Id)',
  `NumLicencia` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Número de llicència expedida',
  `DataExpedicio` date DEFAULT NULL COMMENT 'Data d''expedició',
  `DataCaducitat` date DEFAULT NULL COMMENT 'Data de caducitat',
  `Observacions` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` int(11) DEFAULT NULL COMMENT 'FK → usuaris(Id)',
  `updated_by` int(11) DEFAULT NULL COMMENT 'FK → usuaris(Id)',
  PRIMARY KEY (`Id`),
  KEY `idx_llicencia_arma` (`ArmaId`),
  KEY `idx_llicencia_tercer` (`TercerId`),
  KEY `idx_llicencia_tipus` (`TipusLicenciaId`),
  KEY `idx_llicencia_estat` (`EstatLicenciaId`),
  KEY `idx_llicencia_caducitat` (`DataCaducitat`),
  KEY `idx_llicencia_created_by` (`created_by`),
  KEY `idx_llicencia_updated_by` (`updated_by`),
  CONSTRAINT `fk_llicencia_arma` FOREIGN KEY (`ArmaId`) REFERENCES `armes_armes` (`Id`),
  CONSTRAINT `fk_llicencia_created_by` FOREIGN KEY (`created_by`) REFERENCES `usuaris` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_llicencia_estatllic` FOREIGN KEY (`EstatLicenciaId`) REFERENCES `aux_estatlicencia` (`Id`),
  CONSTRAINT `fk_llicencia_tercer` FOREIGN KEY (`TercerId`) REFERENCES `tercers` (`Id`),
  CONSTRAINT `fk_llicencia_tipusllic` FOREIGN KEY (`TipusLicenciaId`) REFERENCES `aux_tipuslicencia` (`Id`),
  CONSTRAINT `fk_llicencia_updated_by` FOREIGN KEY (`updated_by`) REFERENCES `usuaris` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.audit_logs
CREATE TABLE IF NOT EXISTS `audit_logs` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `table_name` varchar(255) NOT NULL,
  `record_id` varchar(255) NOT NULL,
  `action` enum('INSERT','UPDATE','DELETE') NOT NULL,
  `old_values` text,
  `new_values` text,
  `user_id` varchar(255) DEFAULT NULL,
  `timestamp` datetime DEFAULT CURRENT_TIMESTAMP,
  `ip_address` varchar(45) DEFAULT NULL,
  `session_id` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_table_record` (`table_name`(191),`record_id`(191)),
  KEY `idx_timestamp` (`timestamp`)
) ENGINE=InnoDB AUTO_INCREMENT=8820536 DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.aux_estatlicencia
CREATE TABLE IF NOT EXISTS `aux_estatlicencia` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'p.ex. Vigent, Caducada, Revocada, Suspesa',
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_estatlicencia_nom` (`Nom`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.aux_resultatinspeccio
CREATE TABLE IF NOT EXISTS `aux_resultatinspeccio` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'p.ex. Favorable, Desfavorable, Pendent',
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_resultatinspeccio_nom` (`Nom`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.aux_tipusarma
CREATE TABLE IF NOT EXISTS `aux_tipusarma` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'p.ex. Pistola, Escopeta, Carabina',
  `Descripcio` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_tipusarma_nom` (`Nom`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.aux_tipusdocument
CREATE TABLE IF NOT EXISTS `aux_tipusdocument` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'p.ex. DNI, Llicència, Acord de voluntariat',
  `Descripcio` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_tipusdocument_nom` (`Nom`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.aux_tipuslicencia
CREATE TABLE IF NOT EXISTS `aux_tipuslicencia` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'p.ex. Tipus A, Tipus B, Llicència de caça',
  `Descripcio` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_tipuslicencia_nom` (`Nom`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.aux_tramit
CREATE TABLE IF NOT EXISTS `aux_tramit` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Codi` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Codi funcional únic (p.ex. TERCER_ALTA)',
  `Nom` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Nom descriptiu del tràmit',
  `Descripcio` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_auxtramit_codi` (`Codi`),
  UNIQUE KEY `uq_auxtramit_nom` (`Nom`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.comarques
CREATE TABLE IF NOT EXISTS `comarques` (
  `Codi` int(11) NOT NULL,
  `Nom` varchar(255) NOT NULL COMMENT 'Nom de la comarca',
  `Coordenades` varchar(255) DEFAULT NULL COMMENT 'Coordenades geogràfiques (latitud,longitud)',
  `NomDBPedia` varchar(255) DEFAULT NULL COMMENT 'Nom de DBPedia',
  `CCAdrecaCompleta` varchar(500) DEFAULT NULL COMMENT 'Adreça completa del Consell Comarcal',
  `CCAdreca` varchar(255) DEFAULT NULL COMMENT 'Adreça del Consell Comarcal',
  `CCCodiPostal` varchar(10) DEFAULT NULL COMMENT 'Codi postal del Consell Comarcal',
  `CCEmail` text COMMENT 'Emails del Consell Comarcal (JSON array)',
  `CCFax` varchar(20) DEFAULT NULL COMMENT 'Fax del Consell Comarcal',
  `CCWeb` varchar(255) DEFAULT NULL COMMENT 'Web del Consell Comarcal',
  PRIMARY KEY (`Codi`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Taula que conté les comarques';

-- Data exporting was unselected.
-- Dumping structure for table gestorsubvencions.conv_convenis_auxiliars
CREATE TABLE IF NOT EXISTS `conv_convenis_auxiliars` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ConveniId` int(11) NOT NULL,
  `Notes` varchar(2000) DEFAULT NULL,
  `Attachments` varchar(500) DEFAULT NULL,
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.conv_convenis_locals
CREATE TABLE IF NOT EXISTS `conv_convenis_locals` (
  `_Id` int(11) NOT NULL AUTO_INCREMENT,
  `ANY_SIGNATURA` int(11) DEFAULT NULL,
  `MATERIA` varchar(255) DEFAULT NULL,
  `CODI_SECCIO` int(11) DEFAULT NULL,
  `SECCIO` varchar(255) DEFAULT NULL,
  `NUMERO_CONVENI_DEFINITIU` varchar(255) DEFAULT NULL,
  `TITOL_CONVENI` varchar(255) DEFAULT NULL,
  `CONVENIS_RELACIONATS` varchar(255) DEFAULT NULL,
  `DATA_SIGNATURA` datetime DEFAULT NULL,
  `DATA_VIGENCIA` datetime DEFAULT NULL,
  `DURADA` varchar(255) DEFAULT NULL,
  `VIGENT` varchar(255) DEFAULT NULL,
  `PRORROGABLE` varchar(255) DEFAULT NULL,
  `OBJECTE` text,
  `DRETS_I_OBLIGACIONS` text,
  `COMPLIMENT_I_EXECUCIO` text,
  `ORGANISMES_SIGNANTS_GENERALITAT` varchar(255) DEFAULT NULL,
  `ORGANISMES_SIGNANTS_ENS_LOCALS` varchar(255) DEFAULT NULL,
  `CODI_ENS` bigint(20) DEFAULT NULL,
  `ALTRES_ORGANISMES_SIGNANTS` varchar(255) DEFAULT NULL,
  `APORTACIONS_PREVISTES_GENERALITAT` decimal(18,2) DEFAULT NULL,
  `APORTACIONS_PREVISTES_ENS_LOCALS` decimal(18,2) DEFAULT NULL,
  `APORTACIONS_PREVISTES_ALTRES_ORGANISMES` decimal(18,2) DEFAULT NULL,
  `TOTAL_APORTACIONS_PREVISTES` decimal(18,2) DEFAULT NULL,
  `PDF_CONVENI` varchar(255) DEFAULT NULL,
  `ALTRES_DOCUMENTS` varchar(255) DEFAULT NULL,
  `Notes` varchar(2000) DEFAULT NULL,
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `Discriminator` varchar(50) DEFAULT 'ConvenisLocals',
  `IsLocal` tinyint(1) DEFAULT '1',
  `ProrroguesPermeses` int(11) DEFAULT NULL,
  PRIMARY KEY (`_Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.curs_alumnes
CREATE TABLE IF NOT EXISTS `curs_alumnes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `TercerId` int(11) NOT NULL,
  `Notes` varchar(1000) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  `Empadronat` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Empadronat/a al municipi: determina el preu per sessió',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_curs_alumnes_tercer` (`TercerId`),
  KEY `idx_curs_alumnes_actiu` (`Actiu`),
  CONSTRAINT `fk_curs_alumnes_tercer` FOREIGN KEY (`TercerId`) REFERENCES `tercers` (`Id`) ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=65 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Alumnes de cursets (dades específiques; identitat i contacte a tercers)';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.curs_alumnes_cursets
CREATE TABLE IF NOT EXISTS `curs_alumnes_cursets` (
  `AlumneId` int(11) NOT NULL,
  `CursetId` int(11) NOT NULL,
  `DataAlta` date NOT NULL,
  `DataBaixa` date DEFAULT NULL COMMENT 'Delimita quines sessions se li facturen; NULL = encara hi és',
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  `Estat` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Admesa',
  `OrdreLlistaEspera` int(11) DEFAULT NULL,
  `DataSollicitud` date DEFAULT NULL,
  `Origen` varchar(10) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`AlumneId`,`CursetId`),
  KEY `idx_curs_alumnes_cursets_curset` (`CursetId`),
  KEY `IX_curs_alumnes_cursets_CursetId_Estat` (`CursetId`,`Estat`),
  CONSTRAINT `fk_curs_alumnes_cursets_alumne` FOREIGN KEY (`AlumneId`) REFERENCES `curs_alumnes` (`Id`) ON DELETE CASCADE ON UPDATE NO ACTION,
  CONSTRAINT `fk_curs_alumnes_cursets_curset` FOREIGN KEY (`CursetId`) REFERENCES `curs_cursets` (`Id`) ON DELETE CASCADE ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Inscripció d''alumnes a cursets (N:M)';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.curs_assistencies
CREATE TABLE IF NOT EXISTS `curs_assistencies` (
  `SessioId` int(11) NOT NULL,
  `AlumneId` int(11) NOT NULL,
  `Present` tinyint(1) NOT NULL DEFAULT '1',
  `Nota` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`SessioId`,`AlumneId`),
  KEY `idx_curs_assistencies_alumne` (`AlumneId`),
  CONSTRAINT `fk_curs_assistencies_alumne` FOREIGN KEY (`AlumneId`) REFERENCES `curs_alumnes` (`Id`) ON UPDATE NO ACTION,
  CONSTRAINT `fk_curs_assistencies_sessio` FOREIGN KEY (`SessioId`) REFERENCES `curs_sessions` (`Id`) ON DELETE CASCADE ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Assistència de cada alumne a cada sessió';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.curs_cursets
CREATE TABLE IF NOT EXISTS `curs_cursets` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `TipusCursetId` int(11) NOT NULL,
  `ProfessoraId` int(11) NOT NULL,
  `DiaSetmana` tinyint(4) DEFAULT NULL COMMENT '0=Diumenge..6=Dissabte (System.DayOfWeek)',
  `HoraInici` time DEFAULT NULL,
  `HoraFi` time DEFAULT NULL,
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  `PreuPerSessioEmpadronat` decimal(8,2) NOT NULL DEFAULT '0.00' COMMENT 'Preu per classe impartida (no per assistència), alumnes empadronades',
  `PreuPerSessioNoEmpadronat` decimal(8,2) NOT NULL DEFAULT '0.00' COMMENT 'Preu per classe impartida, alumnes NO empadronades',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` datetime DEFAULT NULL,
  `RegidorId` int(11) DEFAULT NULL,
  `CodiLiquidacio` varchar(16) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CadenciaLiquidacio` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `OrdenancaLiquidacio` varchar(400) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `MaxPlaces` int(11) DEFAULT NULL,
  `InscripcioInici` date DEFAULT NULL,
  `InscripcioFi` date DEFAULT NULL,
  `UrlInscripcio` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `SorteigNumero` int(11) DEFAULT NULL,
  `SorteigData` date DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_curs_cursets_tipus` (`TipusCursetId`),
  KEY `idx_curs_cursets_professora` (`ProfessoraId`),
  KEY `idx_curs_cursets_actiu` (`Actiu`),
  KEY `idx_curs_cursets_regidor` (`RegidorId`),
  CONSTRAINT `fk_curs_cursets_professora` FOREIGN KEY (`ProfessoraId`) REFERENCES `usuaris` (`Id`) ON UPDATE NO ACTION,
  CONSTRAINT `fk_curs_cursets_regidor` FOREIGN KEY (`RegidorId`) REFERENCES `regidors` (`Id`) ON DELETE SET NULL ON UPDATE NO ACTION,
  CONSTRAINT `fk_curs_cursets_tipus` FOREIGN KEY (`TipusCursetId`) REFERENCES `curs_tipus_cursets` (`Id`) ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cursets (p.ex. Pilates - Dilluns matí)';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.curs_liquidacions
CREATE TABLE IF NOT EXISTS `curs_liquidacions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Numero` varchar(40) NOT NULL,
  `Serie` int(11) NOT NULL,
  `RemesaId` int(11) NOT NULL,
  `AlumneId` int(11) NOT NULL,
  `CursetId` int(11) NOT NULL,
  `CodiServei` varchar(16) DEFAULT NULL,
  `CodiPeriode` varchar(16) DEFAULT NULL,
  `CodiCurset` varchar(16) DEFAULT NULL,
  `PeriodeInici` date DEFAULT NULL,
  `PeriodeFi` date DEFAULT NULL,
  `PeriodeEtiqueta` varchar(60) DEFAULT NULL,
  `NumSessions` int(11) NOT NULL DEFAULT '0',
  `PreuPerSessio` decimal(10,2) NOT NULL DEFAULT '0.00',
  `Import` decimal(10,2) NOT NULL DEFAULT '0.00',
  `Empadronat` tinyint(1) NOT NULL DEFAULT '0',
  `Estat` varchar(20) NOT NULL DEFAULT 'Emesa',
  `DataEmissio` datetime DEFAULT NULL,
  `DataCobrament` datetime DEFAULT NULL,
  `DataAnullacio` datetime DEFAULT NULL,
  `MotiuAnullacio` varchar(255) DEFAULT NULL,
  `AlumneNomComplet` varchar(200) DEFAULT NULL,
  `AlumneNif` varchar(20) DEFAULT NULL,
  `AlumneAdreca` varchar(300) DEFAULT NULL,
  `CursetTitol` varchar(200) DEFAULT NULL,
  `ConcepteText` varchar(300) DEFAULT NULL,
  `OrdenancaText` varchar(400) DEFAULT NULL,
  `DatesHorariText` varchar(200) DEFAULT NULL,
  `DetallSessionsJson` text,
  `CreatedAt` datetime DEFAULT NULL,
  `UpdatedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `ux_curs_liquidacions_numero` (`Numero`),
  KEY `idx_curs_liquidacions_remesa` (`RemesaId`),
  KEY `idx_curs_liquidacions_alumne` (`AlumneId`),
  KEY `idx_curs_liquidacions_curset` (`CursetId`),
  KEY `idx_curs_liquidacions_periode_serie` (`CodiPeriode`,`Serie`),
  KEY `idx_curs_liquidacions_estat` (`Estat`),
  CONSTRAINT `fk_curs_liquidacions_alumne` FOREIGN KEY (`AlumneId`) REFERENCES `curs_alumnes` (`Id`),
  CONSTRAINT `fk_curs_liquidacions_curset` FOREIGN KEY (`CursetId`) REFERENCES `curs_cursets` (`Id`),
  CONSTRAINT `fk_curs_liquidacions_remesa` FOREIGN KEY (`RemesaId`) REFERENCES `curs_liquidacions_remeses` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.curs_liquidacions_comptador
CREATE TABLE IF NOT EXISTS `curs_liquidacions_comptador` (
  `Clau` varchar(24) NOT NULL,
  `Ultim` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Clau`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.curs_liquidacions_config
CREATE TABLE IF NOT EXISTS `curs_liquidacions_config` (
  `Id` int(11) NOT NULL,
  `ExpedientPatro` varchar(120) DEFAULT NULL,
  `OrdenancaTarifa` text,
  `ConceptePatro` varchar(300) DEFAULT NULL,
  `EntitatsColaboradoresText` text,
  `MitjansPagamentText` text,
  `OficinaCobratoriaText` text,
  `TextRecursos` text,
  `TextImportant` text,
  `TextTerminis` text,
  `CapcaleraMunicipi` varchar(200) DEFAULT NULL,
  `UpdatedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.curs_liquidacions_remeses
CREATE TABLE IF NOT EXISTS `curs_liquidacions_remeses` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Descripcio` varchar(200) DEFAULT NULL,
  `CadenciaTipus` varchar(20) DEFAULT NULL,
  `CodiPeriode` varchar(16) DEFAULT NULL,
  `PeriodeEtiqueta` varchar(60) DEFAULT NULL,
  `PeriodeInici` date DEFAULT NULL,
  `PeriodeFi` date DEFAULT NULL,
  `Expedient` varchar(60) DEFAULT NULL,
  `DataCreacio` datetime DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `NumLiquidacions` int(11) NOT NULL DEFAULT '0',
  `ImportTotal` decimal(12,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.curs_sessions
CREATE TABLE IF NOT EXISTS `curs_sessions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `CursetId` int(11) NOT NULL,
  `ProfessoraId` int(11) NOT NULL,
  `Data` date NOT NULL,
  `Estat` tinyint(4) NOT NULL DEFAULT '1' COMMENT '1=Oberta, 2=Tancada',
  `NotaSessio` varchar(1000) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ClosedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_curs_sessions_curset_data` (`CursetId`,`Data`),
  KEY `idx_curs_sessions_professora` (`ProfessoraId`),
  CONSTRAINT `fk_curs_sessions_curset` FOREIGN KEY (`CursetId`) REFERENCES `curs_cursets` (`Id`) ON UPDATE NO ACTION,
  CONSTRAINT `fk_curs_sessions_professora` FOREIGN KEY (`ProfessoraId`) REFERENCES `usuaris` (`Id`) ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Sessions (classes concretes) de cada curset';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.curs_tipus_cursets
CREATE TABLE IF NOT EXISTS `curs_tipus_cursets` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  `CodiLiquidacio` varchar(16) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_curs_tipus_cursets_nom` (`Nom`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Temàtiques de curset (Pilates, Ioga, Zumba...)';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.dades_geografiques
CREATE TABLE IF NOT EXISTS `dades_geografiques` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `NombreHabitants` int(11) DEFAULT NULL COMMENT 'Nombre d''habitants',
  `Extensio` decimal(10,2) DEFAULT NULL COMMENT 'Extensió en km²',
  `Altitud` int(11) DEFAULT NULL COMMENT 'Altitud en metres',
  `Localitzacio` point DEFAULT NULL COMMENT 'Coordenades geogràfiques (latitud i longitud)',
  `CentreMunicipal` point DEFAULT NULL COMMENT 'Coordenades del centre municipal (latitud i longitud)',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COMMENT='Taula que conté les dades geogràfiques';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.documents
CREATE TABLE IF NOT EXISTS `documents` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EntityType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Tipos entitat: Voluntari,Arma,LicenciaArma,Subvencio,Tercer,...',
  `EntityId` int(11) NOT NULL COMMENT 'Id de lentitat referenciada',
  `TipusDocumentId` int(11) DEFAULT NULL COMMENT 'FK → Aux_TipusDocument(Id), NULL per tipus personalitzats',
  `TipusPersonalitzat` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Tipus personalitzat quan s''escull "Altres"',
  `Nom` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Nom visible del document',
  `PathMinio` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Ruta a MinIO (bucket/objectKey)',
  `MimeType` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'p.ex. application/pdf, image/jpeg',
  `MidaBytes` int(11) DEFAULT NULL COMMENT 'Mida en bytes',
  `DataDocument` date DEFAULT NULL COMMENT 'Data del document (p.ex. data d''expedició)',
  `Observacions` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` int(11) DEFAULT NULL COMMENT 'FK → usuaris(Id)',
  `updated_by` int(11) DEFAULT NULL COMMENT 'FK → usuaris(Id)',
  PRIMARY KEY (`Id`),
  KEY `idx_doc_tipus` (`TipusDocumentId`),
  KEY `idx_doc_created_by` (`created_by`),
  KEY `idx_doc_updated_by` (`updated_by`),
  KEY `idx_doc_entity` (`EntityType`,`EntityId`),
  CONSTRAINT `fk_doc_created_by` FOREIGN KEY (`created_by`) REFERENCES `usuaris` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_doc_tipusdocument` FOREIGN KEY (`TipusDocumentId`) REFERENCES `aux_tipusdocument` (`Id`),
  CONSTRAINT `fk_doc_updated_by` FOREIGN KEY (`updated_by`) REFERENCES `usuaris` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.ens
CREATE TABLE IF NOT EXISTS `ens` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(255) NOT NULL,
  `CodiEns` varchar(50) NOT NULL DEFAULT '',
  `CIF` varchar(20) DEFAULT NULL,
  `NomCurt` varchar(255) DEFAULT NULL,
  `Article` varchar(255) DEFAULT NULL,
  `Transliterat` varchar(255) DEFAULT NULL,
  `CurtTransliterat` varchar(255) DEFAULT NULL,
  `AdrecaCompleta` varchar(255) DEFAULT NULL,
  `INE6` varchar(10) DEFAULT NULL,
  `NomDBPedia` varchar(255) DEFAULT NULL,
  `ComarcaId` int(11) DEFAULT NULL COMMENT 'Referència a la taula comarques',
  `ProvinciaId` varchar(10) CHARACTER SET utf8mb4 DEFAULT NULL,
  `ContacteId` int(11) DEFAULT NULL COMMENT 'Referència a la taula contactes',
  `ImatgeId` int(11) DEFAULT NULL COMMENT 'Referència a la taula imatges',
  `DadesGeografiquesId` int(11) DEFAULT NULL COMMENT 'Referència a la taula dades_geografiques',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `CodiEns` (`CodiEns`),
  KEY `idx_ens_nom` (`Nom`),
  KEY `fk_ens_comarques` (`ComarcaId`),
  KEY `fk_ens_contactes` (`ContacteId`), -- NOTA: la FK cap a `contactes` s'ha tret perque aquesta taula ja no hi es en aquest fitxer (dades reals: SUBV_contactes)
  KEY `fk_ens_imatges` (`ImatgeId`),
  KEY `fk_ens_dades_geografiques` (`DadesGeografiquesId`),
  KEY `fk_ens_provincies` (`ProvinciaId`),
  CONSTRAINT `fk_ens_dades_geografiques` FOREIGN KEY (`DadesGeografiquesId`) REFERENCES `dades_geografiques` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_ens_imatges` FOREIGN KEY (`ImatgeId`) REFERENCES `imatges` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_ens_provincies` FOREIGN KEY (`ProvinciaId`) REFERENCES `provincies` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=45 DEFAULT CHARSET=latin1 COMMENT='Taula que conté els ens';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.entitats
CREATE TABLE IF NOT EXISTS `entitats` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(255) NOT NULL,
  `LogoURL` varchar(255) DEFAULT NULL,
  `ColorCorporatiu` varchar(50) DEFAULT NULL,
  `PermetAutenticacioExterna` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COMMENT='Taula que conté les entitats';

-- Data exporting was unselected.
-- Dumping structure for table gestorsubvencions.imatges
CREATE TABLE IF NOT EXISTS `imatges` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EscutURL` varchar(255) DEFAULT NULL COMMENT 'URL de l''escut',
  `BanderaURL` varchar(255) DEFAULT NULL COMMENT 'URL de la bandera',
  `VistaURL` varchar(255) DEFAULT NULL COMMENT 'URL de la vista',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COMMENT='Taula que conté les URLs de les imatges';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.ingressos_externs_fons_cooperacio
CREATE TABLE IF NOT EXISTS `ingressos_externs_fons_cooperacio` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `font` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'FCLC',
  `codi_ine6` varchar(10) COLLATE utf8mb4_unicode_ci NOT NULL,
  `municipi` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `comarca` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `exercici` int(11) NOT NULL,
  `import` decimal(18,2) NOT NULL,
  `import_complementari` decimal(18,2) DEFAULT NULL,
  `poblacio` int(11) DEFAULT NULL,
  `sincronitzat_utc` datetime NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ux_ingressos_externs_fclc` (`font`,`codi_ine6`,`exercici`),
  KEY `ix_ingressos_externs_fclc_exercici` (`font`,`exercici`)
) ENGINE=InnoDB AUTO_INCREMENT=8819059 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.ingressos_externs_pendents_pagament
CREATE TABLE IF NOT EXISTS `ingressos_externs_pendents_pagament` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `tipus_ajut` varchar(60) COLLATE utf8mb4_unicode_ci NOT NULL,
  `exercici` int(11) NOT NULL,
  `import` decimal(18,2) NOT NULL,
  `retingut` tinyint(1) NOT NULL DEFAULT '0',
  `data_publicacio` date DEFAULT NULL,
  `sincronitzat_utc` datetime NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ux_ingressos_pendents_ajut_exercici` (`tipus_ajut`,`exercici`)
) ENGINE=InnoDB AUTO_INCREMENT=8911 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.logs
CREATE TABLE IF NOT EXISTS `logs` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `timestamp` datetime DEFAULT CURRENT_TIMESTAMP,
  `level` enum('Debug','Information','Warning','Error','Fatal') NOT NULL,
  `message` text NOT NULL,
  `exception` text,
  `user_id` varchar(255) DEFAULT NULL,
  `action` varchar(255) DEFAULT NULL,
  `ip_address` varchar(45) DEFAULT NULL,
  `session_id` varchar(255) DEFAULT NULL,
  `additional_data` text,
  PRIMARY KEY (`id`),
  KEY `idx_timestamp` (`timestamp`),
  KEY `idx_level` (`level`),
  KEY `idx_user_id` (`user_id`(191))
) ENGINE=InnoDB AUTO_INCREMENT=41 DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for procedure gestorsubvencions.MigratePagaments
DELIMITER //
//
DELIMITER ;

-- Dumping structure for table gestorsubvencions.municipis
CREATE TABLE IF NOT EXISTS `municipis` (
  `ine` varchar(10) NOT NULL,
  `municipi_nom` varchar(255) NOT NULL,
  `municipi_nom_curt` varchar(255) DEFAULT NULL,
  `municipi_article` varchar(50) DEFAULT NULL,
  `municipi_transliterat` varchar(255) DEFAULT NULL,
  `municipi_curt_transliterat` varchar(255) DEFAULT NULL,
  `centre_municipal` varchar(50) DEFAULT NULL,
  `comarca_codi` int(11) DEFAULT NULL,
  `provincia_codi` varchar(10) DEFAULT NULL,
  `adreca_completa` varchar(255) DEFAULT NULL,
  `adreca` varchar(255) DEFAULT NULL,
  `codi_postal` varchar(10) DEFAULT NULL,
  `localitzacio` varchar(50) DEFAULT NULL,
  `telefon_contacte` varchar(20) DEFAULT NULL,
  `fax` varchar(20) DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  `url_general` varchar(255) DEFAULT NULL,
  `cif` varchar(20) DEFAULT NULL,
  `municipi_escut` varchar(255) DEFAULT NULL,
  `municipi_bandera` varchar(255) DEFAULT NULL,
  `municipi_vista` varchar(255) DEFAULT NULL,
  `ine6` varchar(10) DEFAULT NULL,
  `nom_dbpedia` varchar(255) DEFAULT NULL,
  `nombre_habitants` int(11) DEFAULT NULL,
  `extensio` decimal(6,2) DEFAULT NULL,
  `altitud` int(11) DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`ine`),
  KEY `comarca_codi` (`comarca_codi`),
  KEY `municipis_ibfk_2` (`provincia_codi`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.
-- Dumping structure for table gestorsubvencions.patr_arxius_patrimoni
CREATE TABLE IF NOT EXISTS `patr_arxius_patrimoni` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `patrimoni_id` int(11) DEFAULT NULL,
  `tipus_arxiu_id` int(11) NOT NULL,
  `nom_original` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `nom_sistema` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `path_minio` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `bucket_minio` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `url_minio` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `mime_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `mida_bytes` bigint(20) DEFAULT NULL,
  `hash_md5` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `es_imatge` tinyint(1) DEFAULT '0',
  `es_principal` tinyint(1) DEFAULT '0',
  `es_portada` tinyint(1) DEFAULT '0',
  `ordre_visualitzacio` int(11) DEFAULT '0',
  `colleccio_id` int(11) DEFAULT NULL,
  `localitzacio_id` int(11) DEFAULT NULL,
  `data_fotografia` date DEFAULT NULL,
  `titol` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `descripcio` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `autor` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `data_captura` date DEFAULT NULL,
  `visible_public` tinyint(1) DEFAULT '1',
  `descarregable` tinyint(1) DEFAULT '0',
  `actiu` tinyint(1) DEFAULT '1',
  `uploaded_by` int(11) DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `amplada_px` int(11) DEFAULT NULL,
  `altura_px` int(11) DEFAULT NULL,
  `resolucio_dpi` int(11) DEFAULT NULL,
  `drets_autor` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_patrimoni` (`patrimoni_id`),
  KEY `idx_tipus` (`tipus_arxiu_id`),
  KEY `idx_es_imatge` (`es_imatge`),
  KEY `idx_visible_public` (`visible_public`),
  KEY `idx_actiu` (`actiu`),
  KEY `fk_arxius_colleccio` (`colleccio_id`),
  KEY `fk_arxius_localitzacio` (`localitzacio_id`),
  CONSTRAINT `fk_arxius_colleccio` FOREIGN KEY (`colleccio_id`) REFERENCES `patr_coleccions` (`id`) ON DELETE SET NULL,
  CONSTRAINT `fk_arxius_localitzacio` FOREIGN KEY (`localitzacio_id`) REFERENCES `patr_localitzacions` (`id`) ON DELETE SET NULL,
  CONSTRAINT `fk_arxius_patrimoni` FOREIGN KEY (`patrimoni_id`) REFERENCES `patr_patrimonis` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_arxius_tipus` FOREIGN KEY (`tipus_arxiu_id`) REFERENCES `patr_tipus_arxius` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=516 DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.patr_arxius_tags
CREATE TABLE IF NOT EXISTS `patr_arxius_tags` (
  `arxiu_id` int(11) NOT NULL,
  `tag_id` int(11) NOT NULL,
  PRIMARY KEY (`arxiu_id`,`tag_id`),
  KEY `fk_arxius_tags_tag` (`tag_id`),
  CONSTRAINT `fk_arxius_tags_arxiu` FOREIGN KEY (`arxiu_id`) REFERENCES `patr_arxius_patrimoni` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_arxius_tags_tag` FOREIGN KEY (`tag_id`) REFERENCES `tags` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.patr_coleccions
CREATE TABLE IF NOT EXISTS `patr_coleccions` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nom` varchar(200) NOT NULL,
  `descripcio` text,
  `autor` varchar(200) DEFAULT NULL,
  `any_inici` smallint(6) DEFAULT NULL,
  `any_fi` smallint(6) DEFAULT NULL,
  `actiu` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.patr_estats_conservacio
CREATE TABLE IF NOT EXISTS `patr_estats_conservacio` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nom` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `descripcio` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `color` varchar(7) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT '#28A745',
  `prioritat_intervencio` int(11) DEFAULT '1',
  `actiu` tinyint(1) DEFAULT '1',
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_actiu` (`actiu`),
  KEY `idx_prioritat` (`prioritat_intervencio`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.patr_inspeccions
CREATE TABLE IF NOT EXISTS `patr_inspeccions` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `patrimoni_id` int(11) NOT NULL,
  `data_inspeccio` date NOT NULL,
  `inspector` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `tipus_inspeccio` enum('ordinÃ ria','extraordinÃ ria','urgent','seguiment') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'ordinÃ ria',
  `estat_general` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `estat_estructura` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `estat_cobertes` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `estat_facades` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `estat_interiors` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `observacions` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `problemes_detectats` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `mesures_urgents` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `recomanacions` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `puntuacio_global` int(11) DEFAULT NULL,
  `necessita_intervencio` tinyint(1) DEFAULT '0',
  `prioritat_intervencio` int(11) DEFAULT '1',
  `cost_estimat_intervencio` decimal(12,2) DEFAULT NULL,
  `actiu` tinyint(1) DEFAULT '1',
  `created_by` int(11) DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_patrimoni` (`patrimoni_id`),
  KEY `idx_data` (`data_inspeccio`),
  KEY `idx_necessita_intervencio` (`necessita_intervencio`),
  KEY `idx_actiu` (`actiu`),
  CONSTRAINT `fk_inspeccions_patrimoni` FOREIGN KEY (`patrimoni_id`) REFERENCES `patr_patrimonis` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.patr_intervencions
CREATE TABLE IF NOT EXISTS `patr_intervencions` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `patrimoni_id` int(11) NOT NULL,
  `inspeccio_id` int(11) DEFAULT NULL,
  `tipus_intervencio` enum('restauraciÃ³','conservaciÃ³','rehabilitaciÃ³','manteniment','altra') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'manteniment',
  `descripcio` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `objectius` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `data_planificada_inici` date DEFAULT NULL,
  `data_planificada_fi` date DEFAULT NULL,
  `data_real_inici` date DEFAULT NULL,
  `data_real_fi` date DEFAULT NULL,
  `estat_intervencio` enum('planificada','en_curs','finalitzada','suspesa','cancelÂ·lada') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'planificada',
  `empresa_contractista` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `responsable_tecnic` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `pressupost_aprovat` decimal(12,2) DEFAULT NULL,
  `cost_real` decimal(12,2) DEFAULT NULL,
  `font_financament` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `memoria_intervencio` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `resultats` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `actiu` tinyint(1) DEFAULT '1',
  `created_by` int(11) DEFAULT NULL,
  `updated_by` int(11) DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_patrimoni` (`patrimoni_id`),
  KEY `idx_inspeccio` (`inspeccio_id`),
  KEY `idx_estat` (`estat_intervencio`),
  KEY `idx_dates` (`data_planificada_inici`,`data_planificada_fi`),
  KEY `idx_actiu` (`actiu`),
  CONSTRAINT `fk_intervencions_inspeccio` FOREIGN KEY (`inspeccio_id`) REFERENCES `patr_inspeccions` (`id`) ON DELETE SET NULL,
  CONSTRAINT `fk_intervencions_patrimoni` FOREIGN KEY (`patrimoni_id`) REFERENCES `patr_patrimonis` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.patr_localitzacions
CREATE TABLE IF NOT EXISTS `patr_localitzacions` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nom` varchar(200) NOT NULL,
  `descripcio` text,
  `latitud` decimal(10,7) DEFAULT NULL,
  `longitud` decimal(10,7) DEFAULT NULL,
  `actiu` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.patr_notes_patrimoni
CREATE TABLE IF NOT EXISTS `patr_notes_patrimoni` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `patrimoni_id` int(11) NOT NULL,
  `tipus_nota_id` int(11) NOT NULL,
  `titol` varchar(200) DEFAULT NULL,
  `contingut` longtext NOT NULL,
  `visible_public` tinyint(1) DEFAULT '0',
  `actiu` tinyint(1) DEFAULT '1',
  `created_by` int(11) DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `patrimoni_id` (`patrimoni_id`),
  KEY `tipus_nota_id` (`tipus_nota_id`),
  CONSTRAINT `patr_notes_patrimoni_ibfk_1` FOREIGN KEY (`patrimoni_id`) REFERENCES `patr_patrimonis` (`id`) ON DELETE CASCADE,
  CONSTRAINT `patr_notes_patrimoni_ibfk_2` FOREIGN KEY (`tipus_nota_id`) REFERENCES `patr_tipus_notes` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=125 DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.patr_patrimonis
CREATE TABLE IF NOT EXISTS `patr_patrimonis` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `codi_inventari` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `nom` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `nom_alternatiu` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `tipus_patrimoni_id` int(11) NOT NULL,
  `subtipus` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `descripcio_breu` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `descripcio_detallada` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `historia` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `ubicacio_id` int(11) DEFAULT NULL,
  `adreca_completa` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `municipi` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `comarca` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `comarca_id` int(11) DEFAULT NULL,
  `provincia` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `provincia_id` int(11) DEFAULT NULL,
  `numero_policia` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `latitud` decimal(10,8) DEFAULT NULL,
  `longitud` decimal(11,8) DEFAULT NULL,
  `utm_x` double DEFAULT NULL,
  `utm_y` double DEFAULT NULL,
  `referencia_cadastral` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `any_construccio` year(4) DEFAULT NULL,
  `periode_historic` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `estil_arquitectonic` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `materials_principals` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `dimensions` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `estat_conservacio_id` int(11) DEFAULT NULL,
  `observacions_estat` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `data_ultima_inspeccio` date DEFAULT NULL,
  `proteccio_legal` enum('cap','local','autonomica','nacional','internacional') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'cap',
  `numero_expedient` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `data_declaracio` date DEFAULT NULL,
  `organisme_proteccio` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `us_actual` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `us_original` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `propietari_actual` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `regim_propietat` enum('pÃºblic','privat','mixte','desconegut') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'desconegut',
  `valor_historic` int(11) DEFAULT '1',
  `valor_artistic` int(11) DEFAULT '1',
  `valor_arquitectonic` int(11) DEFAULT '1',
  `valor_social` int(11) DEFAULT '1',
  `valor_total` decimal(8,2) DEFAULT '1.00',
  `accessible_public` tinyint(1) DEFAULT '0',
  `horari_visites` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `preu_entrada` decimal(8,2) DEFAULT NULL,
  `actiu` tinyint(1) DEFAULT '1',
  `notes_internes` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `created_by` int(11) DEFAULT NULL,
  `updated_by` int(11) DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `coordenades` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `codi_inventari` (`codi_inventari`),
  KEY `idx_codi_inventari` (`codi_inventari`),
  KEY `idx_nom` (`nom`(191)),
  KEY `idx_tipus` (`tipus_patrimoni_id`),
  KEY `idx_ubicacio` (`ubicacio_id`),
  KEY `idx_estat` (`estat_conservacio_id`),
  KEY `idx_proteccio` (`proteccio_legal`),
  KEY `idx_valor_total` (`valor_total`),
  KEY `idx_actiu` (`actiu`),
  KEY `fk_patrimonis_comarca` (`comarca_id`),
  FULLTEXT KEY `ft_cerca_general` (`nom`,`nom_alternatiu`,`descripcio_breu`),
  CONSTRAINT `fk_patrimonis_comarca` FOREIGN KEY (`comarca_id`) REFERENCES `comarques` (`Codi`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_patrimonis_estat` FOREIGN KEY (`estat_conservacio_id`) REFERENCES `patr_estats_conservacio` (`id`),
  CONSTRAINT `fk_patrimonis_tipus` FOREIGN KEY (`tipus_patrimoni_id`) REFERENCES `patr_tipus_patrimoni` (`id`),
  CONSTRAINT `fk_patrimonis_ubicacio` FOREIGN KEY (`ubicacio_id`) REFERENCES `patr_ubicacions` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=147 DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.patr_subtipus_patrimoni
CREATE TABLE IF NOT EXISTS `patr_subtipus_patrimoni` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `tipus_patrimoni_id` int(11) NOT NULL,
  `nom` varchar(100) NOT NULL,
  `descripcio` text,
  `actiu` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`id`),
  KEY `tipus_patrimoni_id` (`tipus_patrimoni_id`),
  CONSTRAINT `PATR_subtipus_patrimoni_ibfk_1` FOREIGN KEY (`tipus_patrimoni_id`) REFERENCES `patr_tipus_patrimoni` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.patr_tipus_arxius
CREATE TABLE IF NOT EXISTS `patr_tipus_arxius` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nom` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `extensions_permeses` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `mida_maxima_mb` int(11) DEFAULT '10',
  `descripcio` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `actiu` tinyint(1) DEFAULT '1',
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_actiu` (`actiu`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.patr_tipus_notes
CREATE TABLE IF NOT EXISTS `patr_tipus_notes` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nom` varchar(100) NOT NULL,
  `actiu` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.patr_tipus_patrimoni
CREATE TABLE IF NOT EXISTS `patr_tipus_patrimoni` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nom` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `descripcio` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `color` varchar(7) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT '#007BFF',
  `icona` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'fa-building',
  `actiu` tinyint(1) DEFAULT '1',
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_actiu` (`actiu`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.patr_ubicacions
CREATE TABLE IF NOT EXISTS `patr_ubicacions` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nom` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `tipus` enum('districte','barri','zona','altra') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'barri',
  `parent_id` int(11) DEFAULT NULL,
  `coordenades_center` point DEFAULT NULL,
  `actiu` tinyint(1) DEFAULT '1',
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `comarca_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_actiu` (`actiu`),
  KEY `idx_tipus` (`tipus`),
  KEY `fk_ubicacions_parent` (`parent_id`),
  CONSTRAINT `fk_ubicacions_parent` FOREIGN KEY (`parent_id`) REFERENCES `patr_ubicacions` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.patrimoniidentificador
CREATE TABLE IF NOT EXISTS `patrimoniidentificador` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `patrimoni_id` int(11) NOT NULL,
  `tipus` varchar(50) NOT NULL,
  `valor` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_patrimoni_id` (`patrimoni_id`),
  CONSTRAINT `fk_patrimoniidentificador_patrimoni` FOREIGN KEY (`patrimoni_id`) REFERENCES `patr_patrimonis` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=293 DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.provincies
CREATE TABLE IF NOT EXISTS `provincies` (
  `id` varchar(10) NOT NULL,
  `Codi` varchar(10) NOT NULL COMMENT 'Codi de la província',
  `Nom` varchar(255) NOT NULL COMMENT 'Nom de la província',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_provincies_codi` (`Codi`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Taula que conté les províncies';

-- Data exporting was unselected.

-- Dumping structure for event gestorsubvencions.purge_old_logs
DELIMITER //
CREATE DEFINER=`santamaria0`@`%` EVENT `purge_old_logs` ON SCHEDULE EVERY 1 DAY STARTS '2026-01-24 02:00:00' ON COMPLETION NOT PRESERVE ENABLE DO DELETE FROM logs WHERE timestamp < DATE_SUB(NOW(), INTERVAL 30 DAY)//
DELIMITER ;

-- Dumping structure for table gestorsubvencions.regidors
CREATE TABLE IF NOT EXISTS `regidors` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(255) DEFAULT NULL,
  `Carrec` varchar(255) DEFAULT NULL,
  `Partit` varchar(255) DEFAULT NULL,
  `Area` varchar(255) DEFAULT NULL,
  `DataNomenament` date DEFAULT NULL,
  `Email` varchar(255) DEFAULT NULL,
  `Orde` int(11) DEFAULT NULL,
  `CodiEns` varchar(50) DEFAULT NULL,
  `NomEns` varchar(255) DEFAULT NULL,
  `EnsId` int(11) DEFAULT NULL,
  `SexeId` int(11) DEFAULT NULL COMMENT 'Referència a la taula sexes',
  `Sigles` varchar(255) DEFAULT NULL,
  `ExternId` varchar(100) DEFAULT NULL,
  `NomComplet` varchar(400) DEFAULT NULL,
  `Cognom1` varchar(200) DEFAULT NULL,
  `Cognom2` varchar(200) DEFAULT NULL,
  `PasswordHash` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_regidors_nom` (`Nom`),
  KEY `idx_regidors_ibfk_1` (`EnsId`),
  KEY `fk_regidors_sexes` (`SexeId`),
  CONSTRAINT `fk_regidors_ens` FOREIGN KEY (`EnsId`) REFERENCES `ens` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `fk_regidors_sexes` FOREIGN KEY (`SexeId`) REFERENCES `sexes` (`Id`),
  CONSTRAINT `regidors_ibfk_1` FOREIGN KEY (`EnsId`) REFERENCES `ens` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=latin1 COMMENT='Taula que conté els regidors';

-- Data exporting was unselected.
-- Dumping structure for table gestorsubvencions.rel_tramittipusdocument
CREATE TABLE IF NOT EXISTS `rel_tramittipusdocument` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `TramitId` int(11) NOT NULL COMMENT 'FK -> Aux_Tramit(Id)',
  `TipusDocumentId` int(11) NOT NULL COMMENT 'FK -> Aux_TipusDocument(Id)',
  `EsObligatori` tinyint(1) NOT NULL DEFAULT '0' COMMENT '1 = obligatori per al tràmit',
  `Ordre` int(11) NOT NULL DEFAULT '100' COMMENT 'Ordre de visualització',
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_rel_tramit_tipusdocument` (`TramitId`,`TipusDocumentId`),
  KEY `idx_rel_tramit` (`TramitId`),
  KEY `idx_rel_tipusdocument` (`TipusDocumentId`),
  KEY `idx_rel_ordre` (`TramitId`,`Ordre`),
  CONSTRAINT `fk_rel_tipusdocument` FOREIGN KEY (`TipusDocumentId`) REFERENCES `aux_tipusdocument` (`Id`) ON UPDATE CASCADE,
  CONSTRAINT `fk_rel_tramit` FOREIGN KEY (`TramitId`) REFERENCES `aux_tramit` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Relació N:M entre tràmits i tipus de document';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.rols
CREATE TABLE IF NOT EXISTS `rols` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(255) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1 COMMENT='Taula que conté els rols dels usuaris';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.sexes
CREATE TABLE IF NOT EXISTS `sexes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Codi` char(1) NOT NULL COMMENT 'Codi del sexe (M, F, A, ...)',
  `Descripcio` varchar(50) NOT NULL COMMENT 'Descripció del sexe',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_sexes_codi` (`Codi`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COMMENT='Taula que conté els sexes';

-- Data exporting was unselected.
-- Dumping structure for table gestorsubvencions.subv_contactes
CREATE TABLE IF NOT EXISTS `subv_contactes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Adreca` varchar(255) DEFAULT NULL COMMENT 'Adreça completa',
  `CodiPostal` varchar(10) DEFAULT NULL COMMENT 'Codi postal',
  `Telefon` varchar(20) DEFAULT NULL COMMENT 'Telèfon de contacte',
  `Email` varchar(255) DEFAULT NULL COMMENT 'Adreça electrònica',
  `Web` varchar(255) DEFAULT NULL COMMENT 'Lloc web',
  `TelefonContacte` varchar(20) DEFAULT NULL COMMENT 'Telèfon de contacte addicional',
  `Fax` varchar(20) DEFAULT NULL COMMENT 'Fax',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COMMENT='Taula que conté les dades de contacte';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.subv_estats
CREATE TABLE IF NOT EXISTS `subv_estats` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(255) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Nom` (`Nom`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=latin1 COMMENT='Taula que conté els estats de les subvencions';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.subv_expedients
CREATE TABLE IF NOT EXISTS `subv_expedients` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SubvencioId` int(11) NOT NULL,
  `NumeroExpedient` varchar(20) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_expedients_SubvencioId` (`SubvencioId`)
) ENGINE=InnoDB AUTO_INCREMENT=640 DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.subv_pagaments
CREATE TABLE IF NOT EXISTS `subv_pagaments` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SubvencioId` int(11) NOT NULL,
  `Amount` decimal(18,2) DEFAULT NULL,
  `Currency` varchar(10) COLLATE utf8mb4_unicode_ci DEFAULT 'EUR',
  `Date` datetime DEFAULT NULL,
  `Comment` text COLLATE utf8mb4_unicode_ci,
  `PaymentPercentage` int(11) DEFAULT NULL,
  `IsNoPayment` tinyint(1) NOT NULL DEFAULT '0',
  `ServiceDescription` text COLLATE utf8mb4_unicode_ci,
  `IsGrantDate` tinyint(1) NOT NULL DEFAULT '0',
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `fk_pagaments_subvencio` (`SubvencioId`)
) ENGINE=InnoDB AUTO_INCREMENT=142 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Denormalized payment information from subvencions.Pagament';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.subv_regidorssubvencions
CREATE TABLE IF NOT EXISTS `subv_regidorssubvencions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SubvencioId` int(11) NOT NULL,
  `RegidorId` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `SubvencioId` (`SubvencioId`),
  KEY `RegidorId` (`RegidorId`)
) ENGINE=InnoDB AUTO_INCREMENT=65 DEFAULT CHARSET=latin1;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.subv_subvencions
CREATE TABLE IF NOT EXISTS `subv_subvencions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Subvencio` varchar(255) NOT NULL,
  `Actuacio` varchar(255) DEFAULT NULL,
  `ImportAtorgat` decimal(18,2) DEFAULT NULL,
  `Pagament` varchar(255) DEFAULT NULL,
  `ImportTotalActuacio` decimal(18,2) DEFAULT NULL,
  `Obligacions` varchar(255) DEFAULT NULL,
  `PersonaContacte` varchar(255) DEFAULT NULL,
  `Terminis` varchar(255) DEFAULT NULL,
  `Justificacio` varchar(255) DEFAULT NULL,
  `Partida` varchar(255) DEFAULT NULL,
  `JustificacioVerificada` varchar(255) DEFAULT NULL,
  `ImportsRetornar` decimal(18,2) DEFAULT NULL,
  `EntitatId` int(11) DEFAULT NULL,
  `AnyId` int(11) DEFAULT NULL,
  `AreaId` int(11) DEFAULT NULL,
  `EstatId` int(11) DEFAULT NULL,
  `EnsId` int(11) DEFAULT NULL,
  `FontResponsable` char(1) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_subvencions_anyid` (`AnyId`),
  KEY `idx_subvencions_areaid` (`AreaId`),
  KEY `idx_subvencions_estatid` (`EstatId`),
  KEY `idx_subvencions_ensid` (`EnsId`),
  KEY `idx_subvencions_subvencio` (`Subvencio`),
  KEY `idx_subvencions_ibfk_6` (`EnsId`),
  KEY `idx_subvencions_ibfk_1` (`EntitatId`),
  KEY `idx_subvencions_ibfk_4` (`EstatId`),
  KEY `idx_subvencions_areaid_anyid` (`AreaId`,`AnyId`)
) ENGINE=InnoDB AUTO_INCREMENT=70 DEFAULT CHARSET=latin1 COMMENT='Taula que conté les subvencions';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.subv_terminis
CREATE TABLE IF NOT EXISTS `subv_terminis` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Descripcio` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.subv_terminis_subvencions
CREATE TABLE IF NOT EXISTS `subv_terminis_subvencions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SubvencioId` int(11) NOT NULL,
  `TerminiId` int(11) NOT NULL,
  `DataInici` datetime DEFAULT NULL,
  `DataFi` datetime DEFAULT NULL,
  `Notes` varchar(1000) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `IX_TerminisSubvencions_SubvencioId` (`SubvencioId`),
  KEY `IX_TerminisSubvencions_TerminiId` (`TerminiId`)
) ENGINE=InnoDB AUTO_INCREMENT=49 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Data exporting was unselected.
-- Dumping structure for table gestorsubvencions.tags
CREATE TABLE IF NOT EXISTS `tags` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nom` varchar(100) NOT NULL,
  `color` varchar(7) NOT NULL DEFAULT '#6C757D',
  `actiu` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_tags_nom` (`nom`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.
-- Dumping structure for table gestorsubvencions.tercers
CREATE TABLE IF NOT EXISTS `tercers` (
  `Id` int(11) NOT NULL AUTO_INCREMENT COMMENT 'Clau primÃ ria',
  `Nom` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Nom',
  `Cognoms` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Cognoms',
  `DNI` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'DNI / NIE / Passaport',
  `DataNaixement` date DEFAULT NULL COMMENT 'Data de naixement',
  `SexeId` int(11) DEFAULT NULL COMMENT 'FK â†’ sexes(Id)',
  `Email` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Correu electrÃ²nic principal',
  `Telefon` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'TelÃ¨fon principal',
  `Adreca` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'AdreÃ§a',
  `CodiPostal` varchar(10) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Codi postal',
  `Poblacio` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'PoblaciÃ³ (text lliure, si no es vincula al cens)',
  `municipi_ine` varchar(10) CHARACTER SET utf8 DEFAULT NULL COMMENT 'FK â†’ municipis(ine)',
  `provincia_id` varchar(10) CHARACTER SET utf8 DEFAULT NULL COMMENT 'FK â†’ provincies(Id)',
  `comarca_codi` int(11) DEFAULT NULL COMMENT 'FK â†’ comarques(Codi)',
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` int(11) DEFAULT NULL COMMENT 'FK â†’ usuaris(Id)',
  `updated_by` int(11) DEFAULT NULL COMMENT 'FK â†’ usuaris(Id)',
  `AutoritzacioTractamentSignada` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'La persona ha signat l''autoritzacio de tractament de dades (es confirma en donar-la d''alta)',
  `DataAutoritzacio` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_tercer_dni` (`DNI`),
  KEY `idx_tercer_nom` (`Nom`(100)),
  KEY `idx_tercer_email` (`Email`(100)),
  KEY `idx_tercer_municipi` (`municipi_ine`),
  KEY `idx_tercer_provincia` (`provincia_id`),
  KEY `idx_tercer_comarca` (`comarca_codi`),
  KEY `idx_tercer_sexe` (`SexeId`),
  KEY `idx_tercer_created_by` (`created_by`),
  KEY `idx_tercer_updated_by` (`updated_by`)
) ENGINE=InnoDB AUTO_INCREMENT=66 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Taula genÃ¨rica de tercers (persones fÃ­siques)';

-- Data exporting was unselected.
-- Dumping structure for table gestorsubvencions.usuaris
CREATE TABLE IF NOT EXISTS `usuaris` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(255) NOT NULL,
  `Email` varchar(255) NOT NULL,
  `PasswordHash` varchar(255) NOT NULL COMMENT 'Contrasenya encriptada',
  `EnsId` int(11) DEFAULT NULL,
  `data_creacio` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Email` (`Email`),
  KEY `idx_usuaris_ibfk_1` (`EnsId`) USING BTREE,
  CONSTRAINT `FK_Ens` FOREIGN KEY (`EnsId`) REFERENCES `ens` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=latin1 COMMENT='Taula que conté els usuaris';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.usuarisrols
CREATE TABLE IF NOT EXISTS `usuarisrols` (
  `UsuariId` int(11) NOT NULL DEFAULT '0',
  `RolId` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`UsuariId`,`RolId`),
  KEY `idx_usuarisrols_ibfk_2` (`RolId`),
  KEY `idx_usuarisrols_ibfk_1` (`UsuariId`),
  CONSTRAINT `usuarisrols_ibfk_1` FOREIGN KEY (`UsuariId`) REFERENCES `usuaris` (`Id`),
  CONSTRAINT `usuarisrols_ibfk_2` FOREIGN KEY (`RolId`) REFERENCES `rols` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COMMENT='Taula que relaciona usuaris amb rols';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.volu_activitats
CREATE TABLE IF NOT EXISTS `volu_activitats` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Descripcio` text COLLATE utf8mb4_unicode_ci,
  `DataInici` date NOT NULL,
  `DataFi` date DEFAULT NULL,
  `Ubicacio` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Activa` tinyint(1) NOT NULL DEFAULT '1',
  `DataCreacio` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `EnsId` int(11) NOT NULL,
  `AreaId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_activitat_nom` (`Nom`(100)),
  KEY `idx_activitat_dates` (`DataInici`,`DataFi`),
  KEY `idx_activitat_activa` (`Activa`),
  KEY `fk_activitat_ens` (`EnsId`),
  KEY `fk_activitat_area` (`AreaId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Taula d''activitats per als voluntaris';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.volu_activitats_voluntaris
CREATE TABLE IF NOT EXISTS `volu_activitats_voluntaris` (
  `ActivitatId` int(11) NOT NULL,
  `VoluntariId` int(11) NOT NULL,
  `DataInscripcio` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Observacions` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`ActivitatId`,`VoluntariId`),
  KEY `fk_activitat_voluntari_voluntari` (`VoluntariId`),
  KEY `idx_activitat_voluntari_actiu` (`Actiu`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Relació entre activitats i voluntaris';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.volu_ambits_voluntaris
CREATE TABLE IF NOT EXISTS `volu_ambits_voluntaris` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(191) NOT NULL,
  `Descripcio` varchar(500) DEFAULT NULL,
  `PotSerEnllacAjuntament` tinyint(1) NOT NULL,
  `PotSerCoordinador` tinyint(1) NOT NULL,
  `Actiu` tinyint(1) NOT NULL,
  `DataCreacio` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.volu_estats_voluntaris
CREATE TABLE IF NOT EXISTS `volu_estats_voluntaris` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(191) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Descripcio` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Color` varchar(7) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Color hexadecimal per a la visualització',
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  `Ordre` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_estat_nom` (`Nom`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Taula que conté els estats dels voluntaris';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.volu_formacions
CREATE TABLE IF NOT EXISTS `volu_formacions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Descripcio` text COLLATE utf8mb4_unicode_ci,
  `DataInici` date NOT NULL,
  `DataFi` date DEFAULT NULL,
  `DuradaHores` int(11) DEFAULT NULL,
  `Formador` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Ubicacio` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Activa` tinyint(1) NOT NULL DEFAULT '1',
  `DataCreacio` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `EnsId` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_formacio_nom` (`Nom`(100)),
  KEY `idx_formacio_dates` (`DataInici`,`DataFi`),
  KEY `idx_formacio_activa` (`Activa`),
  KEY `fk_formacio_ens` (`EnsId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Taula de formacions per als voluntaris';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.volu_formacions_voluntaris
CREATE TABLE IF NOT EXISTS `volu_formacions_voluntaris` (
  `FormacioId` int(11) NOT NULL,
  `VoluntariId` int(11) NOT NULL,
  `DataInscripcio` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `DataCompletat` timestamp NULL DEFAULT NULL,
  `Superat` tinyint(1) NOT NULL DEFAULT '0',
  `Observacions` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`FormacioId`,`VoluntariId`),
  KEY `fk_formacio_voluntari_voluntari` (`VoluntariId`),
  KEY `idx_formacio_voluntari_actiu` (`Actiu`),
  KEY `idx_formacio_voluntari_superat` (`Superat`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Relació entre formacions i voluntaris';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.volu_subambits_voluntaris
CREATE TABLE IF NOT EXISTS `volu_subambits_voluntaris` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(191) NOT NULL,
  `Descripcio` varchar(500) DEFAULT NULL,
  `AmbitVoluntariId` int(11) NOT NULL,
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  `DataCreacio` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `FK_subambits_voluntaris_ambits_voluntaris_AmbitVoluntariId` (`AmbitVoluntariId`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.volu_tipologies_voluntaris
CREATE TABLE IF NOT EXISTS `volu_tipologies_voluntaris` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nom` varchar(191) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Descripcio` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `PotSerEnllacAjuntament` tinyint(1) NOT NULL DEFAULT '0',
  `PotSerCoordinador` tinyint(1) NOT NULL DEFAULT '0',
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  `DataCreacio` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_tipologia_nom` (`Nom`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Taula que conté les tipologies de voluntaris';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.volu_voluntaris
CREATE TABLE IF NOT EXISTS `volu_voluntaris` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `TercerId` int(11) DEFAULT NULL COMMENT 'FK → Tercers(Id)',
  `TelefonEmergencia` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `DataIncorporacio` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `DataBaixa` timestamp NULL DEFAULT NULL,
  `MotiuBaixa` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Habilitats` text COLLATE utf8mb4_unicode_ci,
  `Disponibilitat` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Observacions` text COLLATE utf8mb4_unicode_ci,
  `AcordSignat` tinyint(1) NOT NULL DEFAULT '0',
  `DataSignaturaAcord` timestamp NULL DEFAULT NULL,
  `RutaAcordSignat` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Actiu` tinyint(1) NOT NULL DEFAULT '1',
  `TipologiaVoluntariId` int(11) NOT NULL,
  `EstatVoluntariId` int(11) NOT NULL,
  `EnsId` int(11) NOT NULL,
  `EnllacAjuntamentId` int(11) DEFAULT NULL COMMENT 'Voluntari que actua com a enllaç amb l''ajuntament',
  `CoordinadorId` int(11) DEFAULT NULL COMMENT 'Voluntari coordinador',
  PRIMARY KEY (`Id`),
  KEY `idx_voluntari_actiu` (`Actiu`),
  KEY `fk_voluntari_tipologia` (`TipologiaVoluntariId`),
  KEY `fk_voluntari_estat` (`EstatVoluntariId`),
  KEY `fk_voluntari_ens` (`EnsId`),
  KEY `fk_voluntari_enllac` (`EnllacAjuntamentId`),
  KEY `fk_voluntari_coordinador` (`CoordinadorId`),
  KEY `idx_voluntari_tercer` (`TercerId`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Taula principal dels voluntaris';

-- Data exporting was unselected.

-- Dumping structure for table gestorsubvencions.volu_voluntaris_ambits
CREATE TABLE IF NOT EXISTS `volu_voluntaris_ambits` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `VoluntariId` int(11) NOT NULL,
  `AmbitVoluntariId` int(11) NOT NULL,
  `DataAssignacio` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `SubambitVoluntariId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_voluntaris_ambits_voluntaris_VoluntariId` (`VoluntariId`),
  KEY `FK_voluntaris_ambits_ambits_voluntaris_AmbitVoluntariId` (`AmbitVoluntariId`),
  KEY `FK_voluntaris_ambits_subambits_voluntaris_SubambitVoluntariId` (`SubambitVoluntariId`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4;

-- Dumping structure for table gestorsubvencions.acceptacions_termes
-- Acceptació dels termes i condicions i la política de privacitat (Web +
-- app de Cursets). Vegeu Models/Base/AcceptacioTermes/AcceptacioTermes.cs
-- i Services/AcceptacioTermesService.cs.
CREATE TABLE IF NOT EXISTS `acceptacions_termes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Email` varchar(255) NOT NULL,
  `VersioAcceptada` int(11) NOT NULL,
  `DataAcceptacio` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_acceptacions_termes_Email` (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Acceptació dels termes i condicions i la política de privacitat, per correu';

-- Data exporting was unselected.
/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;


-- ============================================================================
-- Dades de catàleg (sense dades personals)
-- ============================================================================

INSERT INTO `anys` (`Id`, `Any`) VALUES
	(1, 2023),
	(2, 2024),
	(3, 2025),
	(5, 2026);

INSERT INTO `arees` (`Id`, `Nom`, `EnsId`) VALUES
	(2, 'Esports', 2),
	(4, 'Urbanisme', 5),
	(5, 'Urbanisme', 6),
	(6, 'Urbanisme', 4),
	(7, 'infraestructures viàries', 6),
	(8, 'Equipaments esportius', 6),
	(9, 'Esports', 6),
	(10, 'Educació', 6),
	(11, 'Serveis Socials', 6),
	(12, 'Joventut', 6),
	(13, 'Joventut', 3),
	(14, 'Personal', 6),
	(15, 'Digital DIBA - TIC', 6),
	(16, 'Personal', 5),
	(17, 'Salut pública', 6),
	(18, 'Feminisme i igualtat', 6),
	(19, 'Feminisme i igualtat', 8),
	(20, 'Serveis Socials', 9),
	(21, 'Serveis Socials', 3),
	(22, 'Feminisme i igualtat', 11),
	(23, 'Turisme', 6),
	(24, 'Comerç i consum', 6),
	(25, 'Cultura', 5),
	(26, 'Cultura', 6),
	(27, 'Sanitat', 6),
	(28, 'Medi ambient', 6),
	(29, 'Territori i parcs', 6),
	(30, 'ACA', 5),
	(31, 'PGI / medi ambient', 6),
	(32, 'Governs digitals', 6),
	(33, 'Serveis cooperació local', 6),
	(34, 'Jutjats de Pau', 5),
	(35, 'Àrea de Presidència', 6),
	(36, 'Oficina Tècnica de Cartografia i SIG Local', 6),
	(43, 'Urbanisme  Urbanisme', 7),
	(44, 'Serveis Socials  Serveis Socials  Serveis Socials', 3);

INSERT INTO `comarques` (`Codi`, `Nom`, `Coordenades`, `NomDBPedia`, `CCAdrecaCompleta`, `CCAdreca`, `CCCodiPostal`, `CCEmail`, `CCFax`, `CCWeb`) VALUES
	(3, 'Alt Penedès', '41.3459815,1.6962582', 'Alt_Penedès', 'Hermenegild Clascar, 1-3, 08720 Alt Penedès', 'Hermenegild Clascar, 1-3', '08720', '["ccapenedes@ccapenedes.cat"]', '938170160', 'https://www.ccapenedes.com'),
	(6, 'Anoia', '41.5780054,1.6178448', 'Anoia', 'Plaça Sant Miquel, 5, 08700 Anoia', 'Plaça Sant Miquel, 5', '08700', '["consell.anoia@anoia.cat"]', '938050611', 'https://www.anoia.cat'),
	(7, 'Bages', '41.7246566,1.8218719', 'Bages', 'Muralla de St. Domènec, 24, 08241 Bages', 'Muralla de St. Domènec, 24', '08241', '["consell@ccbages.cat"]', '936930351', 'https://www.ccbages.cat'),
	(11, 'Baix Llobregat', '41.3979911,2.0518154', 'Baix_Llobregat', 'Ctra. Nacional 340, km 1248, 08980 Baix Llobregat', 'Ctra. Nacional 340, km 1248', '08980', '["consellcomarcal@elbaixllobregat.cat"]', '936851868', 'https://www.elbaixllobregat.cat'),
	(13, 'Barcelonès', '41.3769483,2.1713145', 'Barcelonès', 'Barcelonès', '', '', '[]', '', ''),
	(14, 'Berguedà', '42.0993147,1.8470237', 'Berguedà', 'Barcelona, 49, 3r, 08600 Berguedà', 'Barcelona, 49, 3r', '08600', '["ccbergueda@ccbergueda.cat"]', '938220955', 'https://www.bergueda.cat'),
	(17, 'Garraf', '41.2233955,1.7141506576547', 'Garraf', 'Pl. Beatriu de Claramunt, 5-8, 08800 Garraf', 'Pl. Beatriu de Claramunt, 5-8', '08800', '["ccgarraf@ccgarraf.cat"]', '938100055', 'https://www.ccgarraf.cat'),
	(21, 'Maresme', '41.5333744,2.4449591', 'Maresme', 'Pl. Miquel Biada, 1, 08302 Maresme', 'Pl. Miquel Biada, 1', '08302', '["maresme@ccmaresme.cat"]', '937573112', 'https://www.ccmaresme.cat'),
	(24, 'Osona', '41.9273762,2.2531929', 'Osona', 'C. Historiador Ramon d\'Abadal, 5 (Edif.Sucre), 08500 Osona', 'C. Historiador Ramon d\'Abadal, 5 (Edif.Sucre)', '08500', '["mcastellp@ccosona.cat"]', '938895632', 'https://www.ccosona.net'),
	(34, 'Selva', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
	(40, 'Vallès Occidental', '41.4711462,2.1793763', 'Vallès_Occidental', 'Ctra. Nacional 150, km 15, 08227 Vallès Occidental', 'Ctra. Nacional 150, km 15', '08227', '["ccvoc@ccvoc.cat"]', '937271969', 'https://www.ccvoc.cat'),
	(41, 'Vallès Oriental', '41.606033,2.2862003', 'Vallès_Oriental', 'Miquel Ricomà, 46, 08401 Vallès Oriental', 'Miquel Ricomà, 46', '08401', '["ccvo@vallesoriental.cat"]', '938790444', 'https://www.vallesoriental.cat'),
	(42, 'Moianès', '41.81224,2.09755', 'Moianès', 'carrer Joies, 11-13, 08180 Moianès', 'carrer Joies, 11-13', '08180', '["ccmn.consell@ccmoianes.cat"]', '938207624', 'https://www.ccmoianes.cat'),
	(43, 'Lluçanès', '42.02079175,2.1079552842305067', 'Lluçanès', 'Lluçanès', '', '', '[]', '', '');

INSERT INTO `curs_tipus_cursets` (`Id`, `Nom`, `Actiu`, `CodiLiquidacio`) VALUES
	(1, 'Pilates', 1, 'PTES'),
	(2, 'Gent Gran', 1, NULL);

-- (dades preses de la taula antiga `estats`, ja consolidada a `SUBV_estats`)
INSERT INTO `SUBV_estats` (`Id`, `Nom`) VALUES
	(1, 'Pendent'),
	(2, 'Aprovada'),
	(3, 'Denegada'),
	(7, 'Justificada'),
	(8, 'A retornar'),
	(9, 'Pendents tècnics'),
	(10, 'Finalitzada');

INSERT INTO `municipis` (`ine`, `municipi_nom`, `municipi_nom_curt`, `municipi_article`, `municipi_transliterat`, `municipi_curt_transliterat`, `centre_municipal`, `comarca_codi`, `provincia_codi`, `adreca_completa`, `adreca`, `codi_postal`, `localitzacio`, `telefon_contacte`, `fax`, `email`, `url_general`, `cif`, `municipi_escut`, `municipi_bandera`, `municipi_vista`, `ine6`, `nom_dbpedia`, `nombre_habitants`, `extensio`, `altitud`, `created_at`, `updated_at`) VALUES
	('08001', 'Abrera', 'Abrera', '', 'abrera', 'abrera', '41.5162832,1.9017313', 11, '8', NULL, 'Plaça de la Constitució, 1', '08630', '41.5162832,1.9017313', '937700325', '937702612', 'informacio@abrera.cat', 'https://www.ajuntamentabrera.cat', 'P0800100J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08001.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08001.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08001.jpg', '080018', 'Abrera', 13227, 19.94, 105, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08002', 'Aguilar de Segarra', 'Aguilar de Segarra', '', 'aguilar_de_segarra', 'aguilar_de_segarra', '41.7314994,1.6331130', 7, '8', NULL, 'Carrer del Raval, s/n', '08289', '41.7388358,1.6251278', '938366080', '938366080', 'aguilar@aguilardesegarra.cat', 'https://www.aguilardesegarra.cat', 'P0800200H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08002.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08002.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08002.jpg', '080023', 'Aguilar_de_Segarra', 292, 43.32, 641, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08003', 'Alella', 'Alella', '', 'alella', 'alella', '41.4955603,2.2935325', 21, '8', NULL, 'Plaça de l\'Ajuntament, 1', '08328', '41.4936152,2.2951838', '935552339', '935400328', 'alella.ajuntament@alella.cat', 'https://www.alella.cat', 'P0800300F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08003.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08003.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08003.jpg', '080039', 'Alella', 10262, 9.58, 90, '2026-03-05 13:12:27', '2026-07-30 01:30:12'),
	('08004', 'Alpens', 'Alpens', '', 'alpens', 'alpens', '42.1193140,2.1012294', 43, '8', NULL, 'Plaça Major, 15', '08587', '42.1193140,2.1012294', '938578075', '938578032', 'alpens@alpens.cat', 'https://www.alpens.cat', 'P0800400D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08004.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08004.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08004.jpg', '080044', 'Alpens', 267, 13.80, 855, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08005', 'L\'Ametlla del Vallès', 'Ametlla del Vallès', 'L\'', 'lametlla_del_valles', 'ametlla_del_valles', '41.669334,2.2614527', 41, '8', NULL, 'Pl. Ajuntament, 1', '08480', '41.669334,2.2614527', '938432501', '938432313', 'ametlla@ametlla.cat', 'https://www.ametlla.cat', 'P0800500A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08005.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08005.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08005.jpg', '080057', 'L\'Ametlla_del_Vallès', 9474, 14.24, 321, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08006', 'Arenys de Mar', 'Arenys de Mar', '', 'arenys_de_mar', 'arenys_de_mar', '41.5797031,2.5491562', 21, '8', NULL, 'Riera Bisbe Pol, 8', '08350', '41.5797031,2.5491562', '937959900', '937957031', 'alcaldia@arenysdemar.cat', 'https://www.arenysdemar.cat', 'P0800600I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08006.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08006.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08006.jpg', '080060', 'Arenys_de_Mar', 17042, 6.75, 10, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08007', 'Arenys de Munt', 'Arenys de Munt', '', 'arenys_de_munt', 'arenys_de_munt', '41.6095429,2.5400513', 21, '8', NULL, 'Rbla. F. Macià, 59', '08358', '41.6095429,2.5400513', '937937980', '937950630', 'ajuntament@arenysdemunt.cat', 'https://www.arenysdemunt.cat', 'P0800700G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08007.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08007.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08007.jpg', '080076', 'Arenys_de_Munt', 9558, 21.29, 121, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08008', 'Argençola', 'Argençola', '', 'argencola', 'argencola', '41.597963,1.443127', 6, '8', NULL, 'Plaça Lluís Maria Xirinacs, s/n', '08717', '41.597963,1.443127', '938092000', '938092000', 'argensola@argencola.cat', 'https://www.argencola.cat', 'P0800800E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08008.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08008.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08008.jpg', '080082', 'Argençola', 245, 47.09, 768, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08009', 'Argentona', 'Argentona', '', 'argentona', 'argentona', '41.5554326,2.4002787', 21, '8', NULL, 'Carrer Gran, 59', '08310', '41.5559514,2.4005651', '937974900', '937970800', 'argentona@argentona.cat', 'https://argentona.cat', 'P0800900C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08009.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08009.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08009.jpg', '080095', 'Argentona', 12891, 25.40, 88, '2026-03-05 13:12:27', '2026-06-24 17:05:23'),
	('08010', 'Artés', 'Artés', '', 'artes', 'artes', '41.4837582,2.1867126', 7, '8', NULL, 'Barquera, 41', '08271', '41.4837582,2.1867126', '938305001', '938202049', 'artes@artes.cat', 'https://www.artes.cat', 'P0801000A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08010.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08010.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08010.jpg', '080109', 'Artés', 6225, 17.87, 223, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08011', 'Avià', 'Avià', '', 'avia', 'avia', '41.9596845,1.8978209', 14, '8', NULL, 'Av. Pau Casals, 22', '08610', '42.0782531,1.8201273', '938230000', '938231002', 'avia.ajuntament@avia.cat', 'https://www.avia.cat', 'P0801100I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08011.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08011.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08011.jpg', '080116', 'Avià', 2267, 27.23, 693, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08012', 'Avinyó', 'Avinyó', '', 'avinyo', 'avinyo', '41.8636947,1.9712992', 7, '8', NULL, 'Plaça Major, 11', '08279', '41.8636947,1.9712992', '938387700', '938387552', 'avinyo@avinyo.cat', 'https://www.avinyo.cat', 'P0801200G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08012.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08012.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08012.jpg', '080121', 'Avinyó', 2322, 63.23, 353, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08013', 'Avinyonet del Penedès', 'Avinyonet del Penedès', '', 'avinyonet_del_penedes', 'avinyonet_del_penedes', '41.3610844,1.7792795', 3, '8', NULL, 'Pl. Vila, 1', '08793', '41.3610844,1.7792795', '938970000', '938970667', 'avinyonet@avinyonet.org', 'https://www.avinyonet.org', 'P0801300E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08013.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08013.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08013.jpg', '080137', 'Avinyonet_del_Penedès', 1782, 29.13, 331, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08014', 'Aiguafreda', 'Aiguafreda', '', 'aiguafreda', 'aiguafreda', '41.7692525,2.2517162', 24, '8', NULL, 'Pl. Ajuntament, 1', '08591', '41.7688138,2.2495073', '938442253', '938442185', 'aiguafreda@aiguafreda.cat', 'https://www.aiguafreda.cat', 'P0801400C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08014.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08014.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08014.jpg', '080142', 'Aiguafreda', 2569, 7.90, 404, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08015', 'Badalona', 'Badalona', '', 'badalona', 'badalona', '41.4493539,2.2482540', 13, '8', NULL, 'Pl. de la Vila, 1', '08911', '41.4493539,2.2482540', '934832600', '933840478', 'correu@badalona.cat', 'https://ajuntament.badalona.cat/', 'P0801500J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08015.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08015.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08015.jpg', '080155', 'Badalona', 231542, 21.18, 6, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08016', 'Bagà', 'Bagà', '', 'baga', 'baga', '41.4028073,2.1532573', 14, '8', NULL, 'Plaça de Catalunya, 7 bis', '08695', '42.2526761,1.8617328', '938244013', '938244502', 'baga@baga.cat', 'https://www.baga.cat', 'P0801600H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08016.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08016.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08016.jpg', '080168', 'Bagà', 2161, 43.13, 785, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08017', 'Balenyà', 'Balenyà', '', 'balenya', 'balenya', '41.8151185,2.2331607', 24, '8', NULL, 'Carrer de la Pista, 2', '08550', '41.8151185,2.2331607', '938898385', '938898072', 'balenya@diba.cat', 'https://www.balenya.cat', 'P0801700F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08017.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08017.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08017.jpg', '080174', 'Balenyà', 4021, 17.37, 587, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08018', 'Balsareny', 'Balsareny', '', 'balsareny', 'balsareny', '41.8631839,1.8755997', 7, '8', NULL, 'Plaça de l\'Ajuntament, 2', '08660', '41.8631135,1.8767925', '938396100', '938200422', 'balsareny@balsareny.cat', 'https://www.balsareny.cat', 'P0801800D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08018.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08018.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08018.jpg', '080180', 'Balsareny', 3380, 36.91, 327, '2026-03-05 13:12:27', '2026-09-12 10:14:37'),
	('08019', 'Barcelona', 'Barcelona', '', 'barcelona', 'barcelona', '41.3825919,2.177333', 13, '8', NULL, 'Pl. St. Jaume, 1', '08002', '41.3825919,2.177333', '934027000', '933170139', '', 'https://www.barcelona.cat', 'P0801900B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08019.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08019.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08019.jpg', '080193', 'Barcelona', 1731649, 101.35, 4, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08020', 'Begues', 'Begues', '', 'begues', 'begues', '41.3328119,1.9254680', 11, '8', NULL, 'Avinguda de Torres Vilaró, 4', '08859', '41.3328119,1.9254680', '936390538', '936390018', 'begues@begues.cat', 'https://www.begues.cat', 'P0802000J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08020.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08020.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08020.jpg', '080207', 'Begues', 7561, 50.44, 399, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08021', 'Bellprat', 'Bellprat', '', 'bellprat', 'bellprat', '41.5174829,1.4335846', 6, '8', NULL, 'Casa Consistorial', '43421', '41.5174829,1.4335846', '977881240', '977881240', 'bellprat@bellprat.cat', 'https://www.bellprat.cat', 'P0802100H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08021.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08021.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08021.jpg', '080214', 'Bellprat', 69, 30.97, 653, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08022', 'Berga', 'Berga', '', 'berga', 'berga', '42.1011456,1.8454758', 14, '8', NULL, 'Plaça de Sant Pere, 1', '08600', '42.1042317,1.8459226', '938214333', '938211787', 'berga@ajberga.cat', 'https://www.ajberga.cat', 'P0802200F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08022.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08022.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08022.jpg', '080229', 'Berga', 17473, 22.57, 704, '2026-03-05 13:12:27', '2026-07-18 13:26:47'),
	('08023', 'Bigues i Riells del Fai', 'Bigues i Riells del Fai', '', 'bigues_i_riells_del_fai', 'bigues_i_riells_del_fai', '41.6865341,2.2074151', 41, '8', NULL, 'Avinguda Prat de la Riba, 167', '08415', '41.6792691,2.2082488', '938656225', '938656375', 'ajuntament@biguesiriells.cat', 'https://www.biguesiriells.cat', 'P0802300D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08023.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08023.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08023.jpg', '080235', 'Bigues_i_Riells_del_Fai', 10139, 28.64, 280, '2026-03-05 13:12:27', '2026-07-18 13:26:47'),
	('08024', 'Borredà', 'Borredà', '', 'borreda', 'borreda', '42.13574,1.99413', 14, '8', NULL, 'Pl. Ajuntament, s/n', '08619', '42.13574,1.99413', '938239151', '938239223', 'borreda@borreda.cat', 'https://www.borreda.net', 'P0802400B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08024.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08024.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08024.jpg', '080240', 'Borredà', 433, 43.45, 854, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08025', 'El Bruc', 'Bruc', 'El', 'el_bruc', 'bruc', '41.5792357,1.7812982', 6, '8', NULL, 'Bruc del Mig, 55', '08294', '41.5792357,1.7812982', '937710006', '937710450', 'bruc@bruc.cat', 'https://www.bruc.cat', 'P0802500I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08025.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08025.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08025.jpg', '080253', 'El_Bruc', 2290, 47.20, 452, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08026', 'El Brull', 'Brull', 'El', 'el_brull', 'brull', '41.8168099,2.3052283', 24, '8', NULL, 'Plaça de l\'Ajuntament, s/n', '08559', '41.8168099,2.3052283', '938840041', '938841054', 'brull@elbrull.cat', 'https://www.elbrull.cat', 'P0802600G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08026.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08026.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08026.jpg', '080266', 'El_Brull', 296, 41.03, 843, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08027', 'Les Cabanyes', 'Cabanyes', 'Les', 'les_cabanyes', 'cabanyes', '41.371674,1.689535', 3, '8', NULL, 'Carrer Bisbe Torres i Bages, 6', '08794', '41.371674,1.689535', '938921048', '932220273', 'cabanyes@lescabanyes.cat', 'https://www.lescabanyes.cat', 'P0802700E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08027.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08027.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08027.jpg', '080272', 'Les_Cabanyes', 1060, 1.15, 252, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08028', 'Cabrera d\'Anoia', 'Cabrera d\'Anoia', '', 'cabrera_danoia', 'cabrera_danoia', '41.4646648,1.7274310', 6, '8', NULL, 'Pl. Canaletes, 1', '08718', '41.4777773,1.7039024', '937749017', '937749059', 'cabrerai@cabreradanoia.cat', 'https://www.cabreradanoia.cat/', 'P0802800C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08028.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08028.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08028.jpg', '080288', 'Cabrera_d\'Anoia', 1772, 17.00, 347, '2026-03-05 13:12:27', '2026-08-12 09:51:07'),
	('08029', 'Cabrera de Mar', 'Cabrera de Mar', '', 'cabrera_de_mar', 'cabrera_de_mar', '41.5276986,2.3925117', 21, '8', NULL, 'Plaça de l\' Ajuntament, 5', '08349', '41.5259241,2.3933497', '937590091', '937500284', 'info@cabrerademar.cat', 'https://www.cabrerademar.cat', 'P0802900A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08029.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08029.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08029.jpg', '080291', 'Cabrera_de_Mar', 5045, 8.98, 104, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08030', 'Cabrils', 'Cabrils', '', 'cabrils', 'cabrils', '41.5265049,2.3670773', 21, '8', NULL, 'Carrer de Domènec Carles, 1', '08348', '41.5265049,2.3670773', '937539660', '937507030', 'ajuntament@cabrils.cat', 'https://www.cabrils.cat', 'P0803000I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08030.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08030.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08030.jpg', '080305', 'Cabrils', 7765, 7.05, 147, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08031', 'Calaf', 'Calaf', '', 'calaf', 'calaf', '41.7323852,1.5150275', 6, '8', NULL, 'Plaça Gran, 2', '08280', '41.7340956,1.5111315', '938698512', '938680462', 'calaf@calaf.cat', 'https://www.calaf.cat', 'P0803100G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08031.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08031.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08031.jpg', '080312', 'Calaf', 3644, 9.22, 680, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08032', 'Caldes d\'Estrac', 'Caldes d\'Estrac', '', 'caldes_destrac', 'caldes_destrac', '41.5719788,2.5277607', 21, '8', NULL, 'Pl. de la Vila, s/n', '08393', '41.5719788,2.5277607', '937910005', '937910503', 'alcaldia@caldetes.cat', 'https://www.caldetes.cat', 'P0803200E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08032.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08032.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08032.jpg', '080327', 'Caldes_d\'Estrac', 3356, 0.88, 33, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08033', 'Caldes de Montbui', 'Caldes de Montbui', '', 'caldes_de_montbui', 'caldes_de_montbui', '41.6344802,2.1626124', 41, '8', NULL, 'Pl. de la Font de Lleó, 11', '08140', '41.6344802,2.1626124', '938655656', '938655657', 'caldesm@caldesdemontbui.cat', 'https://www.caldesdemontbui.cat', 'P0803300C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08033.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08033.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08033.jpg', '080333', 'Caldes_de_Montbui', 18567, 37.45, 203, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08034', 'Calders', 'Calders', '', 'calders', 'calders', '41.7892144,1.9916386', 42, '8', NULL, 'Plaça Major, 1', '08275', '41.7893336,1.9918661', '938309000', '938309229', 'calders@calders.cat', 'https://www.calders.cat', 'P0803400A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08034.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08034.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08034.jpg', '080348', 'Calders', 1122, 33.09, 552, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08035', 'Calella', 'Calella', '', 'calella', 'calella', '41.6132925,2.6576102', 21, '8', NULL, 'Pl. de l\'ajuntament, 9', '08370', '41.6132755,2.6569794', '937663030', '937660576', 'calella@calella.cat', 'https://www.calella.cat', 'P0803500H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08035.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08035.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08035.jpg', '080351', 'Calella', 20864, 8.00, 5, '2026-03-05 13:12:27', '2026-06-19 22:42:01'),
	('08036', 'Calonge de Segarra', 'Calonge de Segarra', '', 'calonge_de_segarra', 'calonge_de_segarra', '41.7642502,1.4820028', 6, '8', NULL, 'Escoles de Dusfort, s/n', '08281', '41.7642502,1.4820028', '938680409', '938681234', 'calonge@diba.cat', 'https://www.calongesegarra.cat', 'P0803600F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08036.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08036.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08036.jpg', '080364', 'Calonge_de_Segarra', 179, 37.15, 643, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08037', 'Calldetenes', 'Calldetenes', '', 'calldetenes', 'calldetenes', '41.9257651,2.2834318', 24, '8', NULL, '11 de setembre, s/n', '08506', '41.9243209,2.2844315', '938863105', '938891320', 'calldetenes@calldetenes.cat', 'https://www.calldetenes.cat', 'P0822400H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08037.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08037.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08037.jpg', '080370', 'Calldetenes', 2733, 5.80, 489, '2026-03-05 13:12:27', '2026-06-16 16:43:19'),
	('08038', 'Callús', 'Callús', '', 'callus', 'callus', '41.78117575439211,1.7839580473592875', 7, '8', NULL, 'Pl. Major, 1', '08262', '41.78117575439211,1.7839580473592875', '936930000', '938360301', 'callus@callus.cat', 'https://www.callus.cat', 'P0803700D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08038.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08038.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08038.jpg', '080386', 'Callús', 2180, 12.50, 260, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08039', 'Campins', 'Campins', '', 'campins', 'campins', '41.7271582,2.4707423', 41, '8', NULL, 'Pl. Vila, s/n', '08472', '41.7245533,2.4635884', '938475030', '938475030', 'ajuntament@campins.cat', 'https://www.campins.cat', 'P0803800B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08039.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08039.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08039.jpg', '080399', 'Campins', 586, 7.29, 321, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08040', 'Canet de Mar', 'Canet de Mar', '', 'canet_de_mar', 'canet_de_mar', '41.5900933,2.5777998', 21, '8', NULL, 'Carrer Ample, 11-13', '08360', '41.5884151,2.5825097', '937943940', '937941231', 'canet.oac@canetdemar.cat', 'https://www.canetdemar.cat', 'P0803900J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08040.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08040.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08040.jpg', '080403', 'Canet_de_Mar', 15198, 5.56, 15, '2026-03-05 13:12:27', '2026-09-12 10:14:37'),
	('08041', 'Canovelles', 'Canovelles', '', 'canovelles', 'canovelles', '41.6173948,2.2828800', 41, '8', NULL, 'Plaça de l\'Ajuntament, 1', '08420', '41.6220658,2.2772325', '938464555', '938465302', 'ajuntament@canovelles.cat', 'https://www.canovelles.cat', 'P0804000H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08041.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08041.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08041.jpg', '080410', 'Canovelles', 17473, 6.66, 175, '2026-03-05 13:12:27', '2026-06-21 03:13:11'),
	('08042', 'Cànoves i Samalús', 'Cànoves i Samalús', '', 'canoves_i_samalus', 'canoves_i_samalus', '41.6942822,2.3493736', 41, '8', NULL, 'Can Casademunt, s/n', '08445', '41.6942822,2.3493736', '938710018', '938434145', 'canovesisamalus@canovesisamalus.cat', 'https://www.canovesisamalus.cat', 'P0804100F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08042.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08042.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08042.jpg', '080425', 'Cànoves_i_Samalús', 3399, 29.19, 346, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08043', 'Canyelles', 'Canyelles', '', 'canyelles', 'canyelles', '41.2858273,1.7222743', 17, '8', NULL, 'Plaça de l\'11 de setembre, s/n', '08811', '41.2858273,1.7222743', '938973011', '938188130', 'canyelles@canyelles.cat', 'https://www.canyelles.cat', 'P0804200D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08043.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08043.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08043.jpg', '080431', 'Canyelles', 5505, 14.23, 142, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08044', 'Capellades', 'Capellades', '', 'capellades', 'capellades', '41.5300000,1.6847870', 6, '8', NULL, 'C/ Ramon Godó, núm. 9', '08786', '41.5300000,1.6847870', '938011001', '938013969', 'capellades@capellades.cat', 'https://www.capellades.cat', 'P0804300B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08044.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08044.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08044.jpg', '080446', 'Capellades', 5608, 2.94, 317, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08045', 'Capolat', 'Capolat', '', 'capolat', 'capolat', '42.0776191,1.7528670', 14, '8', NULL, 'Casa consistorial, s/n', '08617', '42.0776191,1.7528670', '938215040', '938215040', 'capolat@capolat.cat', 'https://www.capolat.cat', 'P0804400J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08045.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08045.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08045.jpg', '080459', 'Capolat', 93, 34.13, 1279, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08046', 'Cardedeu', 'Cardedeu', '', 'cardedeu', 'cardedeu', '41.6385167,2.3558408', 41, '8', NULL, 'Pl. St. Joan, 1', '08440', '41.6380346,2.3544621', '938444004', '938711477', 'cardedeu@cardedeu.cat', 'https://www.cardedeu.cat', 'P0804500G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08046.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08046.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08046.jpg', '080462', 'Cardedeu', 19046, 12.10, 193, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08047', 'Cardona', 'Cardona', '', 'cardona', 'cardona', '41.9142758,1.6813300', 7, '8', NULL, 'Plaça de la Fira, 1', '08261', '41.9130188,1.6807660', '938691000', '938692901', 'cardona@cardona.cat', 'https://www.cardona.cat', 'P0804600E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08047.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08047.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08047.jpg', '080478', 'Cardona', 4553, 66.70, 506, '2026-03-05 13:12:27', '2026-06-19 22:42:01'),
	('08048', 'Carme', 'Carme', '', 'carme', 'carme', '41.5319058,1.6198579', 6, '8', NULL, 'Avinguda de Catalunya, 2', '08787', '41.5309916,1.6230250', '938080051', '938080368', 'carme@carme.cat', 'https://www.carme.cat', 'P0804700C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08048.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08048.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08048.jpg', '080484', 'Carme', 847, 11.68, 351, '2026-03-05 13:12:27', '2026-06-19 22:42:01'),
	('08049', 'Casserres', 'Casserres', '', 'casserres', 'casserres', '42.0138199,1.8430450', 14, '8', NULL, 'Escodines, 14', '08693', '42.0140144,1.8394710', '938234000', '938234336', 'casserres@casserres.cat', 'https://www.casserres.cat', 'P0804800A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08049.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08049.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08049.jpg', '080497', 'Casserres', 1660, 29.46, 617, '2026-03-05 13:12:27', '2026-08-28 11:11:13'),
	('08050', 'Castellar del Riu', 'Castellar del Riu', '', 'castellar_del_riu', 'castellar_del_riu', '42.1273226,1.7549529', 14, '8', NULL, 'Carretera de Berga als Rasos de Peguera, Km. 2,5', '08618', '42.1273226,1.7549529', '938212775', '938221343', 'castellarr@castellardelriu.cat', 'https://www.castellardelriu.cat', 'P0804900I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08050.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08050.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08050.jpg', '080500', 'Castellar_del_Riu', 151, 32.74, 1234, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08051', 'Castellar del Vallès', 'Castellar del Vallès', '', 'castellar_del_valles', 'castellar_del_valles', '41.618539600627095,2.087839689571022', 40, '8', NULL, 'Passeig de Tolrà, 1', '08211', '41.618539600627095,2.087839689571022', '937144040', '937144093', 'ajuntament@castellarvalles.cat', 'https://www.castellarvalles.cat', 'P0805000G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08051.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08051.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08051.jpg', '080517', 'Castellar_del_Vallès', 25422, 44.91, 331, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08052', 'Castellar de n\'Hug', 'Castellar de n\'Hug', '', 'castellar_de_nhug', 'castellar_de_nhug', '42.2853423,2.0202452', 14, '8', NULL, 'Pl. Ajuntament, s/n', '08696', '42.2823076,2.0168630', '938257077', '938257066', 'castellarh@ajcastellardenhug.cat', 'https://www.ajcastellardenhug.cat', 'P0805100E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08052.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08052.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08052.jpg', '080522', 'Castellar_de_n\'Hug', 166, 47.12, 1395, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08053', 'Castellbell i el Vilar', 'Castellbell i el Vilar', '', 'castellbell_i_el_vilar', 'castellbell_i_el_vilar', '41.62925928123154,1.8610616391505155', 7, '8', NULL, 'Joaquim Borràs, 40', '08296', '41.62925928123154,1.8610616391505155', '938340350', '938282122', 'castellbell@castellbellielvilar.cat', 'https://www.castellbellielvilar.cat', 'P0805200C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08053.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08053.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08053.jpg', '080538', 'Castellbell_i_el_Vilar', 4159, 28.47, 188, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08054', 'Castellbisbal', 'Castellbisbal', '', 'castellbisbal', 'castellbisbal', '41.476470934156794,1.9828663517609801', 40, '8', NULL, 'Avinguda de Pau Casals, 9', '08755', '41.476470934156794,1.9828663517609801', '937720225', '937721307', 'bustia@castellbisbal.cat', 'https://www.castellbisbal.cat', 'P0805300A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08054.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08054.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08054.jpg', '080543', 'Castellbisbal', 13061, 31.03, 132, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08055', 'Castellcir', 'Castellcir', '', 'castellcir', 'castellcir', '41.7615048,2.1507238', 42, '8', NULL, 'Pl. Era, 5', '08183', '41.7604834,2.1496640', '938668151', '938668151', 'castellcir@castellcir.cat', 'https://www.castellcir.cat', 'P0805400I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08055.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08055.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08055.jpg', '080556', 'Castellcir', 804, 34.18, 693, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08056', 'Castelldefels', 'Castelldefels', '', 'castelldefels', 'castelldefels', '41.2861022,1.9824173', 11, '8', NULL, 'Plaça de l\'Església, 1', '08860', '41.2805391,1.9770465', '936651150', '936657714', 'ajuntament@castelldefels.org', 'https://www.castelldefels.org', 'P0805500F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08056.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08056.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08056.jpg', '080569', 'Castelldefels', 70057, 12.87, 3, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08057', 'Castell de l\'Areny', 'Castell de l\'Areny', '', 'castell_de_lareny', 'castell_de_lareny', '42.1730606,1.9429462', 14, '8', NULL, 'Pl. Ajuntament, s/n', '08604', '42.1730606,1.9429462', '938238025', '938238230', 'castell@castelldelareny.cat', 'https://www.castelldelareny.cat', 'P0805600D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08057.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08057.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08057.jpg', '080575', 'Castell_de_l\'Areny', 68, 24.35, 954, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08058', 'Castellet i la Gornal', 'Castellet i la Gornal', '', 'castellet_i_la_gornal', 'castellet_i_la_gornal', '41.2532183,1.5917346', 3, '8', NULL, 'Carrer de Rosselló, 19-23', '08729', '41.2532183,1.5917346', '977670326', '977670277', 'castellet@castelletilagornal.cat', 'https://www.castelletilagornal.cat/', 'P0805700B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08058.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08058.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08058.jpg', '080581', 'Castellet_i_la_Gornal', 2776, 47.48, 159, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08059', 'Castellfollit del Boix', 'Castellfollit del Boix', '', 'castellfollit_del_boix', 'castellfollit_del_boix', '41.6657331,1.6847457', 7, '8', NULL, 'Pl. Ajuntament, s/n', '08255', '41.6667439,1.6834410', '938356033', '938356103', 'castellfollitb@castellfollitdelboix.cat', 'https://castellfollitdelboix.cat', 'P0805800J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08059.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08059.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08059.jpg', '080594', 'Castellfollit_del_Boix', 467, 58.90, 702, '2026-03-05 13:12:27', '2026-08-28 11:11:13'),
	('08060', 'Castellfollit de Riubregós', 'Castellfollit de Riubregós', '', 'castellfollit_de_riubregos', 'castellfollit_de_riubregos', '41.7759129,1.4374393', 6, '8', NULL, 'Carrer Major, 10', '08283', '41.7759129,1.4374393', '938693031', '938693125', 'castellfollitr@riubregos.cat', 'https://www.riubregos.cat', 'P0805900H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08060.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08060.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08060.jpg', '080608', 'Castellfollit_de_Riubregós', 153, 26.21, 467, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08061', 'Castellgalí', 'Castellgalí', '', 'castellgali', 'castellgali', '41.6745463,1.8418971', 7, '8', NULL, 'Avinguda de Montserrat, s/n', '08297', '41.6745463,1.8418971', '938330021', '938331121', 'castellgali@castellgali.cat', 'https://www.castellgali.cat', 'P0806000F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08061.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08061.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08061.jpg', '080615', 'Castellgalí', 2413, 17.21, 266, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08062', 'Castellnou de Bages', 'Castellnou de Bages', '', 'castellnou_de_bages', 'castellnou_de_bages', '41.8348967,1.8365127', 7, '8', NULL, 'Plaça de l\'Església, s/n', '08251', '41.8341525,1.8370902', '938272091', '938320509', 'castellnou@castellnoudebages.cat', 'https://www.castellnoudebages.cat', 'P0806100D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08062.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08062.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08062.jpg', '080620', 'Castellnou_de_Bages', 1473, 29.16, 469, '2026-03-05 13:12:27', '2026-08-28 11:11:13'),
	('08063', 'Castellolí', 'Castellolí', '', 'castelloli', 'castelloli', '41.5974269,1.6980859', 6, '8', NULL, 'Avinguda de la Unió, 60', '08719', '41.5974269,1.6980859', '938084000', '938083111', 'castelloli@castelloli.cat', 'https://www.castelloli.cat', 'P0806200B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08063.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08063.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08063.jpg', '080636', 'Castellolí', 669, 25.28, 415, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08064', 'Castellterçol', 'Castellterçol', '', 'castelltercol', 'castelltercol', '41.7513540,2.1205596', 42, '8', NULL, 'Plaça Vella, 3', '08183', '41.7513540,2.1205596', '938666188', '938666268', 'castelltersol@castelltersol.cat', 'https://www.castelltersol.cat', 'P0806300J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08064.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08064.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08064.jpg', '080641', 'Castellterçol', 2754, 31.90, 726, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08065', 'Castellví de la Marca', 'Castellví de la Marca', '', 'castellvi_de_la_marca', 'castellvi_de_la_marca', '41.3267394,1.6184455', 3, '8', NULL, 'Av. Catalunya, 6', '08732', '41.3267394,1.6184455', '938918077', '938918126', 'marca@castellvidelamarca.cat', 'https://www.castellvidelamarca.cat', 'P0806400H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08065.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08065.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08065.jpg', '080654', 'Castellví_de_la_Marca', 1721, 28.40, 313, '2026-03-05 13:12:27', '2026-07-29 00:52:58'),
	('08066', 'Castellví de Rosanes', 'Castellví de Rosanes', '', 'castellvi_de_rosanes', 'castellvi_de_rosanes', '41.4494938,1.8995013', 11, '8', NULL, 'Carrer Sant Antoni, 1', '08769', '41.4494938,1.8995013', '937751942', '937740684', 'info@castellviderosanes.cat', 'https://www.castellviderosanes.cat', 'P0806500E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08066.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08066.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08066.jpg', '080667', 'Castellví_de_Rosanes', 2171, 16.38, 98, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08067', 'Centelles', 'Centelles', '', 'centelles', 'centelles', '41.7974747,2.2190528', 24, '8', NULL, 'Nou, 15-17', '08540', '41.8009632,2.2150745', '938810375', '938812094', 'centelles@centelles.cat', 'https://www.centelles.cat', 'P0806600C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08067.png', '', 'https://media.diba.cat/diba/municipis/img/vistes/vista08067.jpg', '080673', 'Centelles', 7864, 15.18, 496, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08068', 'Cervelló', 'Cervelló', '', 'cervello', 'cervello', '41.3960866,1.9589440', 11, '8', NULL, 'Carrer Major, 146-148', '08758', '41.3960866,1.9589440', '936600070', '936600659', 'ajuntament@cervello.cat', 'https://www.cervello.cat', 'P0806700A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08068.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08068.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08068.jpg', '080689', 'Cervelló', 9743, 24.10, 122, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08069', 'Collbató', 'Collbató', '', 'collbato', 'collbato', '41.5673977,1.8281993', 11, '8', NULL, 'Bonavista, 2', '08293', '41.5673977,1.8281993', '937770100', '937770650', 'collbato@collbato.cat', 'https://www.collbato.cat', 'P0806800I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08069.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08069.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08069.jpg', '080692', 'Collbató', 4828, 18.07, 388, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08070', 'Collsuspina', 'Collsuspina', '', 'collsuspina', 'collsuspina', '41.8256311,2.1752516', 42, '8', NULL, 'Plaça Major, 3', '08178', '41.8256311,2.1752516', '938300376', '938208330', 'collsuspina@collsuspina.cat', 'https://www.collsuspina.cat', 'P0806900G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08070.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08070.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08070.jpg', '080706', 'Collsuspina', 391, 15.06, 901, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08071', 'Copons', 'Copons', '', 'copons', 'copons', '41.6363657,1.5179333', 6, '8', NULL, 'Carrer d\'Angel Guimerà, 8', '08289', '41.6363657,1.5179333', '938090000', '938090013', 'copons@copons.cat', 'https://www.copons.cat', 'P0807000E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08071.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08071.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08071.jpg', '080713', 'Copons', 352, 18.66, 432, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08072', 'Corbera de Llobregat', 'Corbera de Llobregat', '', 'corbera_de_llobregat', 'corbera_de_llobregat', '41.4163285,1.9278704', 11, '8', NULL, 'Carrer de La Pau, 5', '08757', '41.4163285,1.9278704', '936500211', '936500662', 'ajuntament@corberadellobregat.cat', 'https://www.corberadellobregat.cat', 'P0807100C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08072.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08072.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08072.jpg', '080728', 'Corbera_de_Llobregat', 16010, 18.41, 342, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08073', 'Cornellà de Llobregat', 'Cornellà de Llobregat', '', 'cornella_de_llobregat', 'cornella_de_llobregat', '41.3538698,2.0754445', 11, '8', NULL, 'Mossèn Cinto Verdaguer, s/n', '08940', '41.3538698,2.0754445', '933770212', '933778900', 'informacio@aj-cornella.cat', 'https://www.cornella.cat', 'P0807200A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08073.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08073.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08073.jpg', '080734', 'Cornellà_de_Llobregat', 92237, 6.99, 27, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08074', 'Cubelles', 'Cubelles', '', 'cubelles', 'cubelles', '41.2083363,1.6730541', 17, '8', NULL, 'Pl. Vila, 1', '08880', '41.2081837,1.6723725', '938950300', '938952729', 'cubelles@cubelles.cat', 'https://www.cubelles.cat', 'P0807300I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08074.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08074.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08074.jpg', '080749', 'Cubelles', 17673, 13.49, 12, '2026-03-05 13:12:27', '2026-09-27 03:53:34'),
	('08075', 'Dosrius', 'Dosrius', '', 'dosrius', 'dosrius', '41.5943470,2.4063166', 21, '8', NULL, 'St. Antoni, 1', '08319', '41.5946810,2.4060692', '937918014', '937919080', 'dosrius@dosrius.cat', 'https://www.dosrius.cat', 'P0807400G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08075.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08075.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08075.jpg', '080752', 'Dosrius', 6287, 40.73, 147, '2026-03-05 13:12:27', '2026-09-12 10:14:37'),
	('08076', 'Esparreguera', 'Esparreguera', '', 'esparreguera', 'esparreguera', '41.5389434,1.8705935', 11, '8', NULL, 'Pl. Ajuntament, 1', '08292', '41.5390297,1.8707239', '937771801', '937775904', 'esparreguera@esparreguera.cat', 'https://www.esparreguera.cat', 'P0807500D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08076.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08076.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08076.jpg', '080765', 'Esparreguera', 22665, 27.40, 187, '2026-03-05 13:12:27', '2026-09-12 10:14:37'),
	('08077', 'Esplugues de Llobregat', 'Esplugues de Llobregat', '', 'esplugues_de_llobregat', 'esplugues_de_llobregat', '41.3778094,2.0886257', 11, '8', NULL, 'Pl. Sta. Magdalena, 5-6', '08950', '41.3776689,2.0885657', '933713350', '933722910', 'ajuntament@esplugues.cat', 'https://www.esplugues.cat', 'P0807600B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08077.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08077.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08077.jpg', '080771', 'Esplugues_de_Llobregat', 48221, 4.60, 110, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08078', 'L\'Espunyola', 'Espunyola', 'L\'', 'lespunyola', 'espunyola', '42.0530138,1.7698522', 14, '8', NULL, 'Ctra. Solsona a Berga, s/n', '08614', '42.0530138,1.7698522', '938231055', '938231294', 'espunyola@espunyola.cat', 'https://www.espunyola.cat', 'P0807700J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08078.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08078.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08078.jpg', '080787', 'L\'Espunyola', 260, 35.46, 759, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08079', 'L\'Estany', 'Estany', 'L\'', 'lestany', 'estany', '41.8694083,2.108911', 42, '8', NULL, 'Doctor Vilardell, 1', '08148', '41.8694083,2.108911', '938303000', '938303251', 'estany@estany.cat', 'https://www.estany.cat', 'P0807800H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08079.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08079.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08079.jpg', '080790', 'L\'Estany', 390, 10.25, 870, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08080', 'Fígols', 'Fígols', '', 'figols', 'figols', '42.1809722,1.834568', 14, '8', NULL, 'Pl. Església, s/n', '08698', '42.1809722,1.834568', '938248052', '938248052', 'figols@figols.cat', 'https://www.figols.cat', 'P0807900F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08080.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08080.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08080.jpg', '080804', 'Fígols', 40, 29.31, 1154, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08081', 'Fogars de Montclús', 'Fogars de Montclús', '', 'fogars_de_montclus', 'fogars_de_montclus', '41.7279142,2.4432631', 41, '8', NULL, 'Plaça de les Escoles, 1', '08479', '41.7279142,2.4432631', '938475104', '938475220', 'fogarsm@fogarsdemontclus.cat', 'https://www.fogarsdemontclus.cat', 'P0808000D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08081.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08081.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08081.jpg', '080811', 'Fogars_de_Montclús', 494, 39.72, 550, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08082', 'Fogars de la Selva', 'Fogars de la Selva', '', 'fogars_de_la_selva', 'fogars_de_la_selva', '41.7342133,2.6728108', 34, '8', NULL, 'Plaça de la Vila, s/n', '08495', '41.7342133,2.6728108', '972864973', '972865284', 'fds.ajuntament@fogarsdelaselva.cat', 'https://www.fogarsdelaselva.cat', 'P0808100B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08082.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08082.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08082.jpg', '080826', 'Fogars_de_la_Selva', 1704, 32.12, 45, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08083', 'Folgueroles', 'Folgueroles', '', 'folgueroles', 'folgueroles', '41.9392868,2.3180742', 24, '8', NULL, 'Plaça Verdaguer, 2  \'Can Dachs\'', '08519', '41.9392868,2.3180742', '938122054', '938122192', 'folgueroles@folgueroles.cat', 'https://www.folgueroles.cat', 'P0808200J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08083.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08083.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08083.jpg', '080832', 'Folgueroles', 2262, 10.47, 552, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08084', 'Fonollosa', 'Fonollosa', '', 'fonollosa', 'fonollosa', '41.7626001,1.6678299', 7, '8', NULL, 'Església, s/n', '08259', '41.7421221,1.7140013', '938366005', '938366005', 'fonollosa@fonollosa.cat', 'https://www.fonollosa.cat', 'P0808300H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08084.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08084.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08084.jpg', '080847', 'Fonollosa', 1570, 51.67, 525, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08085', 'Font-rubí', 'Font-rubí', '', 'fontrubi', 'fontrubi', '41.4141702,1.6516189', 3, '8', NULL, 'Pl. Ajuntament, 1 (Guardiola de Font-Rubí)', '08736', '41.4141702,1.6516189', '938979212', '938979283', 'fontrubi@font-rubi.cat', 'https://www.font-rubi.org', 'P0808400F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08085.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08085.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08085.jpg', '080850', 'Font-rubí', 1441, 37.42, 315, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08086', 'Les Franqueses del Vallès', 'Franqueses del Vallès', 'Les', 'les_franqueses_del_valles', 'franqueses_del_valles', '41.6362062,2.2968737', 41, '8', NULL, 'Carretera de Ribes, 2', '08520', '41.6362062,2.2968737', '938467676', '938467767', 'ajuntament@lesfranqueses.cat', 'https://www.lesfranqueses.cat', 'P0808500C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08086.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08086.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08086.jpg', '080863', 'Les_Franqueses_del_Vallès', 20881, 29.14, 232, '2026-03-05 13:12:27', '2026-07-04 23:08:58'),
	('08087', 'Gallifa', 'Gallifa', '', 'gallifa', 'gallifa', '41.6937007,2.1164334', 40, '8', NULL, 'Plaça de l\'Ajuntament, 1', '08146', '41.6937007,2.1164334', '938662121', '938661151', 'gallifa@gallifa.cat', 'https://www.gallifa.cat', 'P0808600A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08087.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08087.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08087.jpg', '080879', 'Gallifa', 171, 16.33, 502, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08088', 'La Garriga', 'Garriga', 'La', 'la_garriga', 'garriga', '41.6852651,2.2857434', 41, '8', NULL, 'Pl. Església, 2', '08530', '41.6852651,2.2857434', '938605050', '938718281', 'oac@ajlagarriga.cat', 'https://www.lagarriga.cat', 'P0808700I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08088.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08088.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08088.jpg', '080885', 'La_Garriga', 17426, 18.80, 252, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08089', 'Gavà', 'Gavà', '', 'gava', 'gava', '41.3040384,1.9994729', 11, '8', NULL, 'Plaça de Jaume Balmes, s/nº', '08850', '41.3040384,1.9994729', '932639100', '932639108', 'ajuntament@gava.cat', 'https://www.GavaCiutat.cat', 'P0808800G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08089.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08089.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08089.jpg', '080898', 'Gavà', 48243, 30.75, 9, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08090', 'Gaià', 'Gaià', '', 'gaia', 'gaia', '41.9162122,1.922936', 7, '8', NULL, 'Pl. Ajuntament, 1', '08672', '41.9162122,1.922936', '938390151', '938204189', 'gaia@gaia.cat', 'https://www.gaia.cat', 'P0808900E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08090.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08090.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08090.jpg', '080902', 'Gaià', 172, 39.48, 481, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08091', 'Gelida', 'Gelida', '', 'gelida', 'gelida', '41.4409882,1.8632169', 3, '8', NULL, 'Plaça de la Vila, 12', '08790', '41.4409882,1.8632169', '937790058', '937790100', 'gelida@gelida.cat', 'https://www.gelida.cat', 'P0809000C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08091.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08091.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08091.jpg', '080919', 'Gelida', 8198, 26.69, 196, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08092', 'Gironella', 'Gironella', '', 'gironella', 'gironella', '42.0337646,1.8825729', 14, '8', NULL, 'Plaça de la Vila, 13', '08680', '42.0337646,1.8825729', '938250033', '938250368', 'gironella@gironella.cat', 'https://www.gironella.cat', 'P0809100A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08092.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08092.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08092.jpg', '080924', 'Gironella', 5083, 6.78, 469, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08093', 'Gisclareny', 'Gisclareny', '', 'gisclareny', 'gisclareny', '42.2499780,1.7866137', 14, '8', NULL, 'Plaça del Roser, s/n', '08695', '42.2499780,1.7866137', '937441020', '938244580', 'gisclareny@gisclareny.cat', 'http://www.gisclareny.cat', 'P0809200I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08093.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08093.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08093.jpg', '080930', 'Gisclareny', 28, 36.47, 1339, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08094', 'La Granada', 'Granada', 'La', 'la_granada', 'granada', '41.3766679,1.7186912', 3, '8', NULL, 'Carrer de l\'Estació, 25', '08792', '41.3766679,1.7186912', '938974025', '938974406', 'granada@lagranada.cat', 'http://www.lagranada.com', 'P0809300G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08094.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08094.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08094.jpg', '080945', 'La_Granada', 2287, 6.52, 272, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08095', 'Granera', 'Granera', '', 'granera', 'granera', '41.7255146,2.0570042', 42, '8', NULL, 'Església, s/n', '08183', '41.7254037,2.0566656', '938668152', '938662262', 'granera@granera.cat', 'https://www.granera.cat', 'P0809400E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08095.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08095.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08095.jpg', '080958', 'Granera', 84, 23.73, 768, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08096', 'Granollers', 'Granollers', '', 'granollers', 'granollers', '41.6079555,2.2876008', 41, '8', NULL, 'Plaça de la Porxada, 6', '08400', '41.6080374,2.2870899', '938426610', '938426601', 'bustiaoberta@ajuntament.granollers.cat', 'https://www.granollers.cat', 'P0809500B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08096.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08096.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08096.jpg', '080961', 'Granollers', 65341, 14.87, 145, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08097', 'Gualba', 'Gualba', '', 'gualba', 'gualba', '41.7321147,2.5018684', 41, '8', NULL, 'Pg. Montseny, 13', '08474', '41.7321147,2.5018684', '938487027', '938487070', 'gualba@gualba.cat', 'https://www.gualba.cat/', 'P0809600J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08097.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08097.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08097.jpg', '080977', 'Gualba', 1766, 23.29, 177, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08098', 'Sant Salvador de Guardiola', 'Sant Salvador de Guardiola', '', 'sant_salvador_de_guardiola', 'sant_salvador_de_guardiola', '41.6797988,1.7671087', 7, '8', NULL, 'Carrer de Dalt, 19', '08253', '41.6779777,1.7663413', '938358025', '938358236', 'ssg.ajuntament@santsalvadordeguardiola.cat', 'https://www.santsalvadordeguardiola.cat', 'P0809700H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08098.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08098.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08098.jpg', '080983', 'Sant_Salvador_de_Guardiola', 3612, 37.15, 374, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08099', 'Guardiola de Berguedà', 'Guardiola de Berguedà', '', 'guardiola_de_bergueda', 'guardiola_de_bergueda', '42.23178045,1.8816944795631643', 14, '8', NULL, 'Pl. Municipal, 3', '08694', '42.23178045,1.8816944795631643', '938227059', '938227024', 'guardiola@guardioladebergueda.cat', 'https://www.guardioladebergueda.cat', 'P0809800F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08099.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08099.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08099.jpg', '080996', 'Guardiola_de_Berguedà', 964, 61.73, 720, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08100', 'Gurb', 'Gurb', '', 'gurb', 'gurb', '41.9409869,2.2433019', 24, '8', NULL, 'Mas l\'Esperança', '08503', '41.9399402,2.2431227', '938860166', '938860047', 'ajgurb@gurb.cat', 'https://www.gurb.cat', 'P0809900D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08100.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08100.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08100.jpg', '081000', 'Gurb', 2741, 51.57, 563, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08101', 'L\'Hospitalet de Llobregat', 'Hospitalet de Llobregat', 'L\'', 'lhospitalet_de_llobregat', 'hospitalet_de_llobregat', '41.3592352,2.0997586', 13, '8', NULL, 'Plaça de l\'Ajuntament, 11', '08901', '41.3592352,2.0997586', '934029400', '933381847', 'secretaria@l-h.cat', 'https://www.l-h.cat', 'P0810000J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08101.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08101.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08101.jpg', '081017', 'L\'Hospitalet_de_Llobregat', 289510, 12.40, 8, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08102', 'Igualada', 'Igualada', '', 'igualada', 'igualada', '41.5790182,1.6173460', 6, '8', NULL, 'Pl. Ajuntament, 1', '08700', '41.5786827,1.6172131', '938031950', '938051964', 'atencio.ciutadana@aj-igualada.net', 'https://www.igualada.cat', 'P0810100H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08102.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08102.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08102.jpg', '081022', 'Igualada', 42085, 8.11, 316, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08103', 'Jorba', 'Jorba', '', 'jorba', 'jorba', '41.6019310,1.5478241', 6, '8', NULL, 'Major, 2', '08719', '41.6019310,1.5478241', '938094000', '938078121', 'jorba@jorba.cat', 'https://www.jorba.cat', 'P0810200F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08103.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08103.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08103.jpg', '081038', 'Jorba', 850, 30.90, 380, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08104', 'La Llacuna', 'Llacuna', 'La', 'la_llacuna', 'llacuna', '41.4729806,1.5346070', 6, '8', NULL, 'Pl. Major, 1', '08779', '41.4729806,1.5346070', '938976063', '938976063', 'llacuna@lallacuna.cat', 'https://www.lallacuna.cat', 'P0810300D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08104.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08104.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08104.jpg', '081043', 'La_Llacuna', 959, 52.23, 615, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08105', 'La Llagosta', 'Llagosta', 'La', 'la_llagosta', 'llagosta', '41.5131188,2.1931226', 41, '8', NULL, 'Pl. Antoni Baque, 1', '08120', '41.5131188,2.1931226', '935603911', '935741142', 'llagosta@llagosta.cat', 'https://www.llagosta.cat', 'P0810400B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08105.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08105.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08105.jpg', '081056', 'La_Llagosta', 13280, 3.03, 45, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08106', 'Llinars del Vallès', 'Llinars del Vallès', '', 'llinars_del_valles', 'llinars_del_valles', '41.6392819,2.4043381', 41, '8', NULL, 'Plaça de la Vila, 1', '08450', '41.6392819,2.4043381', '938412750', '938412814', 'llinars@llinarsdelvalles.cat', 'https://www.llinarsdelvalles.cat/', 'P0810500I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08106.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08106.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08106.jpg', '081069', 'Llinars_del_Vallès', 10956, 27.63, 198, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08107', 'Lliçà d\'Amunt', 'Lliçà d\'Amunt', '', 'llica_damunt', 'llica_damunt', '41.6083433,2.2394683', 41, '8', NULL, 'Carrer d\'Anselm Clavé, 73', '08186', '41.6083433,2.2394683', '938415225', '938414175', 'ajuntament@llicamunt.cat', 'https://www.llicamunt.cat', 'P0810600G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08107.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08107.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08107.jpg', '081075', 'Lliçà_d\'Amunt', 16540, 22.33, 145, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08108', 'Lliçà de Vall', 'Lliçà de Vall', '', 'llica_de_vall', 'llica_de_vall', '41.5862431,2.2398562', 41, '8', NULL, 'Plaça de la Vila, s/n', '08185', '41.5862431,2.2398562', '938439000', '938439375', 'llissadevall@llissadevall.cat', 'https://www.llissadevall.cat', 'P0810700E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08108.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08108.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08108.jpg', '081081', 'Lliçà_de_Vall', 6903, 10.83, 125, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08109', 'Lluçà', 'Lluçà', '', 'lluca', 'lluca', '42.0504661,2.0328951', 43, '8', NULL, 'Carrer dels Rourets, 1', '08514', '42.0504661,2.0328951', '938554062', '938554042', 'llusa@lluca.cat', 'https://www.lluca.cat/', 'P0810800C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08109.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08109.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08109.jpg', '081094', 'Lluçà', 281, 52.98, 751, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08110', 'Malgrat de Mar', 'Malgrat de Mar', '', 'malgrat_de_mar', 'malgrat_de_mar', '41.6475312,2.7450377', 21, '8', NULL, 'Carme, 30', '08380', '41.6475312,2.7450377', '937653300', '937610993', 'correu@ajmalgrat.cat', 'https://www.ajmalgrat.cat', 'P0810900A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08110.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08110.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08110.jpg', '081108', 'Malgrat_de_Mar', 19714, 8.82, 4, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08111', 'Malla', 'Malla', '', 'malla', 'malla', '41.8870160,2.2354564', 24, '8', NULL, 'Casa Consistorial, s/n', '08522', '41.8870160,2.2354564', '938856306', '938814029', 'malla@malla-osona.cat', 'https://www.malla-osona.cat', 'P0811000I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08111.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08111.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08111.jpg', '081115', 'Malla', 285, 11.01, 580, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08112', 'Manlleu', 'Manlleu', '', 'manlleu', 'manlleu', '41.9999811,2.2841159', 24, '8', NULL, 'Pl. Fra Bernadí, 6', '08560', '42.0000201,2.2841079', '938506666', '938507970', 'atenciociutadana@manlleu.cat', 'https://www.manlleu.cat', 'P0811100G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08112.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08112.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08112.jpg', '081120', 'Manlleu', 21425, 17.23, 461, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08113', 'Manresa', 'Manresa', '', 'manresa', 'manresa', '41.7288939,1.8286765', 7, '8', NULL, 'Pl. Major, 1', '08241', '41.7233842,1.8270111', '938782300', '938782303', 'ajt@ajmanresa.cat', 'https://www.manresa.cat', 'P0811200E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08113.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08113.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08113.jpg', '081136', 'Manresa', 80974, 41.65, 238, '2026-03-05 13:12:27', '2026-06-19 22:42:01'),
	('08114', 'Martorell', 'Martorell', '', 'martorell', 'martorell', '41.4743886,1.9307159', 11, '8', NULL, 'Plaça de la Vila, 46', '08760', '41.4743157,1.9308415', '937750050', '937740595', 'ajuntament@martorell.cat', 'https://www.martorell.cat', 'P0811300C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08114.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08114.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08114.jpg', '081141', 'Martorell', 29175, 12.76, 56, '2026-03-05 13:12:27', '2026-08-28 11:11:13'),
	('08115', 'Martorelles', 'Martorelles', '', 'martorelles', 'martorelles', '41.5285100,2.2374755', 41, '8', NULL, 'Pl. Ajuntament, 1', '08107', '41.5285100,2.2374755', '935705732', '935705964', 'web@martorelles.cat', 'https://www.martorelles.cat', 'P0811400A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08115.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08115.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08115.jpg', '081154', 'Martorelles', 4992, 3.61, 96, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08116', 'Les Masies de Roda', 'Masies de Roda', 'Les', 'les_masies_de_roda', 'masies_de_roda', '41.9895263,2.3076733', 24, '8', NULL, 'Ctra. de Roda a Manlleu, s/n', '08510', '41.9885288,2.3086977', '938540027', '938540007', 'masiesr@lesmasiesderoda.cat', 'https://www.lesmasiesderoda.cat', 'P0811500H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08116.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08116.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08116.jpg', '081167', 'Les_Masies_de_Roda', 758, 16.41, 516, '2026-03-05 13:12:27', '2026-06-16 16:43:19'),
	('08117', 'Les Masies de Voltregà', 'Masies de Voltregà', 'Les', 'les_masies_de_voltrega', 'masies_de_voltrega', '42.0125603,2.2501678', 24, '8', NULL, 'Ctra. C-17z, Pk. 70,400 casa Forta el Despujol', '08508', '42.0125603,2.2501678', '938570028', '938570079', 'masiesv@lesmasiesdevoltrega.cat', 'https://www.lesmasiesdevoltrega.cat', 'P0811600F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08117.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08117.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08117.jpg', '081173', 'Les_Masies_de_Voltregà', 3345, 22.35, 533, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08118', 'El Masnou', 'Masnou', 'El', 'el_masnou', 'masnou', '41.4796899,2.3118347', 21, '8', NULL, 'Prat de la Riba, 1', '08320', '41.4810244,2.3267870', '935571700', '935571701', 'ajuntament@elmasnou.cat', 'https://www.elmasnou.cat', 'P0811700D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08118.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08118.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08118.jpg', '081189', 'El_Masnou', 24761, 3.39, 27, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08119', 'Masquefa', 'Masquefa', '', 'masquefa', 'masquefa', '41.5026351,1.8110722', 6, '8', NULL, 'Major, 91-93', '08783', '41.5026351,1.8110722', '937725030', '937725311', 'masquefa@masquefa.cat', 'https://www.masquefa.cat', 'P0811800B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08119.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08119.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08119.jpg', '081192', 'Masquefa', 10120, 17.06, 257, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08120', 'Matadepera', 'Matadepera', '', 'matadepera', 'matadepera', '41.5967462,2.0265889', 40, '8', NULL, 'Pl. Ajuntament, 1', '08230', '41.5967462,2.0265889', '937870200', '937300048', 'oac@matadepera.cat', 'https://www.matadepera.cat', 'P0811900J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08120.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08120.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08120.jpg', '081206', 'Matadepera', 9776, 25.36, 423, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08121', 'Mataró', 'Mataró', '', 'mataro', 'mataro', '41.5403472,2.4176895', 21, '8', NULL, 'Carrer de la Riera, 48', '08301', '41.5398297,2.4424276', '937582100', '937582122', 'ajmataro@ajmataro.cat', 'https://www.mataro.cat', 'P0812000H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08121.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08121.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08121.jpg', '081213', 'Mataró', 131683, 22.53, 28, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08122', 'Mediona', 'Mediona', '', 'mediona', 'mediona', '41.4773943,1.6350185', 3, '8', NULL, 'Carrer del Dr. Trueta, 10', '08773', '41.4768300,1.6112841', '938985002', '938985299', 'mediona@diba.cat', 'https://www.mediona.cat', 'P0812100F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08122.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08122.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08122.jpg', '081228', 'Mediona', 2658, 47.55, 430, '2026-03-05 13:12:27', '2026-07-30 01:30:12'),
	('08123', 'Molins de Rei', 'Molins de Rei', '', 'molins_de_rei', 'molins_de_rei', '41.4138087,2.0159626', 11, '8', NULL, 'Plaça de Catalunya, 1', '08750', '41.4138087,2.0159626', '936803340', '936803362', 'sam@molinsderei.cat', 'https://www.molinsderei.cat', 'P0812200D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08123.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08123.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08123.jpg', '081234', 'Molins_de_Rei', 27300, 15.94, 37, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08124', 'Mollet del Vallès', 'Mollet del Vallès', '', 'mollet_del_valles', 'mollet_del_valles', '41.5355381,2.2104224', 41, '8', NULL, 'Plaça Major, 1', '08100', '41.5355381,2.2104224', '935719500', '935719504', 'ajuntament@molletvalles.cat', 'https://www.molletvalles.cat', 'P0812300B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08124.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08124.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08124.jpg', '081249', 'Mollet_del_Vallès', 52990, 10.77, 65, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08125', 'Montcada i Reixac', 'Montcada i Reixac', '', 'montcada_i_reixac', 'montcada_i_reixac', '41.4800154,2.1872317', 40, '8', NULL, 'Av. de la Unitat, 6', '08110', '41.4800154,2.1872317', '935726474', '935726493', 'oac@montcada.org', 'https://www.montcada.cat', 'P0812400J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08125.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08125.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08125.jpg', '081252', 'Montcada_i_Reixac', 37460, 23.47, 36, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08126', 'Montgat', 'Montgat', '', 'montgat', 'montgat', '41.4668611,2.2789842', 21, '8', NULL, 'Pl. Vila, s/n', '08390', '41.4665657,2.2789595', '934694900', '934692400', 'montgat@montgat.cat', 'https://www.montgat.cat', 'P0812500G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08126.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08126.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08126.jpg', '081265', 'Montgat', 12879, 2.91, 20, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08127', 'Monistrol de Montserrat', 'Monistrol de Montserrat', '', 'monistrol_de_montserrat', 'monistrol_de_montserrat', '41.6105028,1.8452698', 7, '8', NULL, 'Pl. Font Gran, 2', '08691', '41.6097177,1.8424075', '938350011', '938284163', 'monistrolm@monistroldemontserrat.cat', 'https://www.monistroldemontserrat.cat', 'P0812600E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08127.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08127.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08127.jpg', '081271', 'Monistrol_de_Montserrat', 3250, 11.77, 161, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08128', 'Monistrol de Calders', 'Monistrol de Calders', '', 'monistrol_de_calders', 'monistrol_de_calders', '41.7608379,2.0146236', 42, '8', NULL, 'Vinya, 9', '08275', '41.7604822,2.0141702', '938399000', '938398037', 'monistrolc@monistroldecalders.cat', 'https://www.monistroldecalders.cat', 'P0812700C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08128.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08128.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08128.jpg', '081287', 'Monistrol_de_Calders', 764, 21.97, 447, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08129', 'Muntanyola', 'Muntanyola', '', 'muntanyola', 'muntanyola', '41.8769342,2.1790836', 24, '8', NULL, 'Carrer de les Afores, s/n', '08529', '41.8769342,2.1790836', '938830186', '938137087', 'muntanyola@muntanyola.cat', 'https://www.muntanyola.cat', 'P0812800A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08129.png', '', 'https://media.diba.cat/diba/municipis/img/vistes/vista08129.jpg', '081290', 'Muntanyola', 697, 40.30, 807, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08130', 'Montclar', 'Montclar', '', 'montclar', 'montclar', '42.0181730,1.7653302', 14, '8', NULL, 'Casa de la Vila s/n', '08614', '42.0181730,1.7653302', '938231092', '938231092', 'montclar@montclar.cat', 'https://www.montclar.cat', 'P0812900I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08130.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08130.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08130.jpg', '081304', 'Montclar', 132, 21.89, 728, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08131', 'Montesquiu', 'Montesquiu', '', 'montesquiu', 'montesquiu', '42.1087280,2.2086111', 24, '8', NULL, 'Pl. Emili Juncadella, 1-2', '08585', '42.1087280,2.2086111', '938529100', '938551279', 'montesquiu@montesquiu.cat', 'https://www.montesquiu.cat', 'P0813000G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08131.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08131.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08131.jpg', '081311', 'Montesquiu', 1131, 4.94, 728, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08132', 'Montmajor', 'Montmajor', '', 'montmajor', 'montmajor', '42.2848778,3.0839026', 14, '8', NULL, 'Plaça del Mercat, 1', '08612', '42.0177071,1.7353648', '938246000', '938246000', 'montmajor@montmajor.cat', 'https://www.montmajor.cat', 'P0813100E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08132.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08132.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08132.jpg', '081326', 'Montmajor', 473, 76.49, 756, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08133', 'Montmaneu', 'Montmaneu', '', 'montmaneu', 'montmaneu', '41.6259778,1.4147627', 6, '8', NULL, 'Carrer de la Panadella, 8', '08717', '41.6251264,1.4131870', '938092010', '938092010', 'montmaneu@montmaneu.cat', 'https://www.montmaneu.cat', 'P0813200C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08133.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08133.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08133.jpg', '081332', 'Montmaneu', 181, 13.62, 709, '2026-03-05 13:12:27', '2026-08-20 08:16:40'),
	('08134', 'Figaró-Montmany', 'Figaró-Montmany', '', 'figaromontmany', 'figaromontmany', '41.7197363,2.2732016', 41, '8', NULL, 'Carretera de Ribes 42-44', '08590', '41.7197363,2.2732016', '938429111/637281633', '938429136', 'figaro@figaro-montmany.cat', 'https://www.figaro-montmany.cat', 'P0813300A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08134.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08134.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08134.jpg', '081347', 'Figaró-Montmany', 1195, 14.99, 330, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08135', 'Montmeló', 'Montmeló', '', 'montmelo', 'montmelo', '41.551513150000005,2.248190049425774', 41, '8', NULL, 'Plaça de la Vila, 1', '08160', '41.551513150000005,2.248190049425774', '935720000', '935720420', 'info@montmelo.cat', 'https://www.montmelo.cat', 'P0813400I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08135.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08135.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08135.jpg', '081350', 'Montmeló', 8886, 4.00, 72, '2026-03-05 13:12:27', '2026-03-05 13:12:27'),
	('08136', 'Montornès del Vallès', 'Montornès del Vallès', '', 'montornes_del_valles', 'montornes_del_valles', '41.5455066,2.2662770', 41, '8', NULL, 'Av. Llibertat, 2', '08170', '41.5455066,2.2662770', '935721170', '935682762', 'alcaldia@montornes.cat', 'https://www.montornes.cat', 'P0813500F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08136.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08136.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08136.jpg', '081363', 'Montornès_del_Vallès', 17102, 10.23, 116, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08137', 'Montseny', 'Montseny', '', 'montseny', 'montseny', '41.7593697,2.3952061', 41, '8', NULL, 'Plaça de la Vila,11', '08469', '41.7593697,2.3952061', '938473003', '938473003', 'montseny@montseny.cat', 'https://www.montseny.cat', 'P0813600D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08137.png', '', 'https://media.diba.cat/diba/municipis/img/vistes/vista08137.jpg', '081379', 'Montseny', 388, 26.77, 528, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08138', 'Moià', 'Moià', '', 'moia', 'moia', '41.8103145,2.0900485', 42, '8', NULL, 'Plaça de Sant Sebastià, 1', '08180', '41.812918,2.0945381', '938300000', '938301325', 'ajuntament@ajmoia.cat', 'https://www.moia.cat', 'P0813700B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08138.png', '', 'https://media.diba.cat/diba/municipis/img/vistes/vista08138.jpg', '081385', 'Moià', 6828, 75.31, 717, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08139', 'Mura', 'Mura', '', 'mura', 'mura', '41.6998219,1.9764684', 7, '8', NULL, 'Plaça de l\'Ajuntament, s/n', '08278', '41.6998219,1.9764684', '938317226', '938317226', 'mura@mura.cat', 'https://www.mura.cat', 'P0813800J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08139.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08139.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08139.jpg', '081398', 'Mura', 235, 47.79, 454, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08140', 'Navarcles', 'Navarcles', '', 'navarcles', 'navarcles', '41.7531567,1.9033612', 7, '8', NULL, 'Plaça de la Vila, 1', '08270', '41.7516007,1.9037678', '938310011', '938270114', 'navarcles@navarcles.cat', 'https://www.navarcles.cat', 'P0813900H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08140.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08140.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08140.jpg', '081402', 'Navarcles', 6276, 5.52, 269, '2026-03-05 13:12:27', '2026-09-12 10:14:37'),
	('08141', 'Navàs', 'Navàs', '', 'navas', 'navas', '41.8999484,1.8790166', 7, '8', NULL, 'Pl. Ajuntament, 8', '08670', '41.8999484,1.8790166', '938390022', '938390022', 'navas@navas.cat', 'https://www.navas.cat', 'P0814000F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08141.png', '', 'https://media.diba.cat/diba/municipis/img/vistes/vista08141.jpg', '081419', 'Navàs', 6238, 80.62, 681, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08142', 'La Nou de Berguedà', 'Nou de Berguedà', 'La', 'la_nou_de_bergueda', 'nou_de_bergueda', '42.1925298,1.8025852', 14, '8', NULL, 'Casa Consistorial', '08699', '42.1925298,1.8025852', '938259000', '938248027', 'nou@lanoudebergueda.cat', 'https://www.lanoudebergueda.cat', 'P0814100D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08142.png', '', 'https://media.diba.cat/diba/municipis/img/vistes/vista08142.jpg', '081424', 'La_Nou_de_Berguedà', 163, 25.01, 876, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08143', 'Òdena', 'Òdena', '', 'odena', 'odena', '41.5984526,1.5933228', 6, '8', NULL, 'Pl. Major, 2', '08711', '41.5984526,1.5933228', '938017434', '938017548', 'odena@odena.cat', 'https://www.odena.cat', 'P0814200B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08143.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08143.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08143.jpg', '081430', 'Òdena', 3760, 52.66, 421, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08144', 'Olvan', 'Olvan', '', 'olvan', 'olvan', '42.0571091,1.9064046', 14, '8', NULL, 'Pl. Ajuntament, s/n', '08611', '42.0571058,1.9062411', '938250013', '938228685', 'olvan@olvan.cat', 'https://www.olvan.cat', 'P0814300J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08144.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08144.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08144.jpg', '081445', 'Olvan', 924, 35.58, 553, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08145', 'Olèrdola', 'Olèrdola', '', 'olerdola', 'olerdola', '41.3211705,1.7216413', 3, '8', NULL, 'Av. Catalunya, 12', '08734', '41.3211705,1.7216413', '938903502', '938171059', 'olerdola@olerdola.cat', 'https://www.olerdola.cat', 'P0814400H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08145.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08145.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08145.jpg', '081458', 'Olèrdola', 3945, 30.15, 233, '2026-03-05 13:12:27', '2026-06-16 02:52:26'),
	('08146', 'Olesa de Bonesvalls', 'Olesa de Bonesvalls', '', 'olesa_de_bonesvalls', 'olesa_de_bonesvalls', '41.3505032,1.8492996', 3, '8', NULL, 'Plaça de la Vila, 1', '08795', '41.3505032,1.8492996', '938984008', '938984007', 'bonesvalls@olesadebonesvalls.cat', 'https://www.olesadebonesvalls.cat', 'P0814500E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08146.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08146.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08146.jpg', '081461', 'Olesa_de_Bonesvalls', 2135, 30.79, 265, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08147', 'Olesa de Montserrat', 'Olesa de Montserrat', '', 'olesa_de_montserrat', 'olesa_de_montserrat', '41.5439614,1.8913809', 11, '8', NULL, 'Pl. Fèlix Figueras i Aragay, s/n.', '08640', '41.5439614,1.8913809', '937780050', '937780752', 'ajuntament@olesademontserrat.cat', 'https://www.olesademontserrat.cat', 'P0814600C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08147.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08147.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08147.jpg', '081477', 'Olesa_de_Montserrat', 24966, 16.63, 124, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08148', 'Olivella', 'Olivella', '', 'olivella', 'olivella', '41.3103129,1.8114946', 17, '8', NULL, 'Pl. Major, s/n', '08818', '41.3103129,1.8114946', '938968000', '938968042', 'ajuntament@olivella.cat', 'https://www.olivella.cat', 'P0814700A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08148.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08148.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08148.jpg', '081483', 'Olivella', 4435, 38.75, 211, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08149', 'Olost', 'Olost', '', 'olost', 'olost', '41.9851518,2.0941372', 43, '8', NULL, 'Plaça Major, 1', '08516', '41.9859972,2.0959084', '938880211', '938880552', 'olost@olost.cat', 'https://www.olost.cat', 'P0814800I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08149.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08149.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08149.jpg', '081496', 'Olost', 1209, 29.37, 669, '2026-03-05 13:12:27', '2026-07-30 01:30:12'),
	('08150', 'Orís', 'Orís', '', 'oris', 'oris', '41.2180711,1.7231584', 24, '8', NULL, 'Avinguda del Castell , 1', '08573', '42.0581395,2.2380059', '938590247', '938504070', 'oris.info@oris.cat', 'https://www.oris.cat', 'P0814900G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08150.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08150.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08150.jpg', '081509', 'Orís', 355, 27.17, 564, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08151', 'Oristà', 'Oristà', '', 'orista', 'orista', '41.9324493,2.0605578', 43, '8', NULL, 'Plaça Major, 1', '08518', '41.9324493,2.0605578', '938128006', '938128132', 'orista@orista.cat', 'https://www.orista.cat', 'P0815000E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08151.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08151.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08151.jpg', '081516', 'Oristà', 555, 68.49, 468, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08152', 'Orpí', 'Orpí', '', 'orpi', 'orpi', '41.5286732,1.606733', 6, '8', NULL, 'Pl. St. Jordi Barri Can Bou', '08787', '41.5286732,1.606733', '938080139', '938080106', 'orpi@orpi.cat', 'https://www.orpi.cat', 'P0815100C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08152.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08152.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08152.jpg', '081521', 'Orpí', 175, 15.23, 477, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08153', 'Òrrius', 'Òrrius', '', 'orrius', 'orrius', '41.5550967,2.3547486', 21, '8', NULL, 'Plaça de l\'Església, 5', '08317', '41.5550967,2.3547486', '937971455', '937560673', 'orrius@orrius.cat', 'https://www.orrius.cat', 'P0815200A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08153.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08153.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08153.jpg', '081537', 'Òrrius', 812, 5.66, 269, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08154', 'Pacs del Penedès', 'Pacs del Penedès', '', 'pacs_del_penedes', 'pacs_del_penedes', '41.3611151,1.6696604', 3, '8', NULL, 'Avgda. Diputació, 1-3', '08796', '41.3611151,1.6696604', '938171485', '938170737', 'pacs@pacsdelpenedes.cat', 'https://www.pacsdelpenedes.cat', 'P0815300I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08154.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08154.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08154.jpg', '081542', 'Pacs_del_Penedès', 932, 6.27, 201, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08155', 'Palafolls', 'Palafolls', '', 'palafolls', 'palafolls', '41.6673913,2.7499262', 21, '8', NULL, 'Pl. Major, 11', '08389', '41.6669670,2.7491024', '937620043', '937652211', 'palafolls@palafolls.cat', 'https://www.palafolls.cat', 'P0815400G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08155.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08155.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08155.jpg', '081555', 'Palafolls', 10038, 16.56, 16, '2026-03-05 13:12:27', '2026-07-30 01:30:12'),
	('08156', 'Palau-solità i Plegamans', 'Palau-solità i Plegamans', '', 'palausolita_i_plegamans', 'palausolita_i_plegamans', '41.5875279,2.1785112', 40, '8', NULL, 'Pl. Vila, 1', '08184', '41.5875279,2.1785112', '938648056', '938649259', 'info@palauplegamans.cat', 'https://www.palauplegamans.cat', 'P0815500D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08156.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08156.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08156.jpg', '081568', 'Palau-solità_i_Plegamans', 15614, 14.93, 130, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08157', 'Pallejà', 'Pallejà', '', 'palleja', 'palleja', '41.4225304,1.9967638', 11, '8', NULL, 'Carrer del Sol, 1', '08780', '41.4225304,1.9967638', '936630000', '936631640', 'palleja@palleja.cat', 'https://www.palleja.cat', 'P0815600B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08157.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08157.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08157.jpg', '081574', 'Pallejà', 12006, 8.30, 87, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08158', 'El Papiol', 'Papiol', 'El', 'el_papiol', 'papiol', '41.4379916,2.0108896', 11, '8', NULL, 'Avinguda de la Generalitat, 7-9', '08754', '41.4379683,2.0110376', '936730220', '936731539', 'papiol@elpapiol.cat', 'https://www.elpapiol.cat', 'P0815700J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08158.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08158.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08158.jpg', '081580', 'El_Papiol', 4404, 8.95, 135, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08159', 'Parets del Vallès', 'Parets del Vallès', '', 'parets_del_valles', 'parets_del_valles', '41.5731820,2.2337656', 41, '8', NULL, 'Carrer Major, 2-4', '08150', '41.5731820,2.2337656', '935738888', '935738889', 'info@parets.cat', 'https://www.parets.cat', 'P0815800H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08159.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08159.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08159.jpg', '081593', 'Parets_del_Vallès', 18885, 9.12, 94, '2026-03-05 13:12:27', '2026-07-17 07:29:46'),
	('08160', 'Perafita', 'Perafita', '', 'perafita', 'perafita', '42.0423757,2.1071220', 43, '8', NULL, 'Major, 19', '08589', '42.0406385,2.1065483', '938530001', '938530001', 'perafita@perafita.cat', 'https://www.perafita.cat', 'P0815900F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08160.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08160.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08160.jpg', '081607', 'Perafita', 433, 19.59, 754, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08161', 'Piera', 'Piera', '', 'piera', 'piera', '41.5216923,1.7526335', 6, '8', NULL, 'Carrer de la Plaça, 16-18', '08784', '41.5182616,1.7439239', '937788200', '937760036', 'piera@ajpiera.cat', 'https://www.viladepiera.cat', 'P0816000D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08161.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08161.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08161.jpg', '081614', 'Piera', 17880, 57.20, 324, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08162', 'Els Hostalets de Pierola', 'Hostalets de Pierola', 'Els', 'els_hostalets_de_pierola', 'hostalets_de_pierola', '41.5325083,1.7700220', 6, '8', NULL, 'Pl. De Cal Figueres, 1', '08781', '41.5325083,1.7700220', '937712112', '937712398', 'ajuntament@elshostaletsdepierola.cat', 'https://www.elshostaletsdepierola.cat', 'P0816100B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08162.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08162.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08162.jpg', '081629', 'Els_Hostalets_de_Pierola', 3258, 33.49, 474, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08163', 'Pineda de Mar', 'Pineda de Mar', '', 'pineda_de_mar', 'pineda_de_mar', '41.6275894,2.6896277', 21, '8', NULL, 'Pl. Catalunya, 1', '08397', '41.6275894,2.6896277', '937671560', '937671212', 'ajuntament@pinedademar.cat', 'https://www.pinedademar.cat', 'P0816200J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08163.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08163.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08163.jpg', '081635', 'Pineda_de_Mar', 30108, 10.74, 10, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08164', 'El Pla del Penedès', 'Pla del Penedès', 'El', 'el_pla_del_penedes', 'pla_del_penedes', '41.4176715,1.7103658', 3, '8', NULL, 'Plaça Pau Fontanals, 1', '08733', '41.4176715,1.7103658', '938988003', '938989031', 'pla@elpladelpenedes.cat', 'https://www.elpladelpenedes.cat', 'P0816300H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08164.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08164.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08164.jpg', '081640', 'El_Pla_del_Penedès', 1393, 9.57, 216, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08165', 'La Pobla de Claramunt', 'Pobla de Claramunt', 'La', 'la_pobla_de_claramunt', 'pobla_de_claramunt', '41.5462302,1.6728832', 6, '8', NULL, 'Avinguda de Catalunya, 16', '08787', '41.5462302,1.6728832', '938086075', '938086112', 'claramunt@lapobladeclaramunt.cat', 'https://www.lapobladeclaramunt.cat', 'P0816400F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08165.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08165.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08165.jpg', '081653', 'La_Pobla_de_Claramunt', 2350, 18.54, 246, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08166', 'La Pobla de Lillet', 'Pobla de Lillet', 'La', 'la_pobla_de_lillet', 'pobla_de_lillet', '42.2440108,1.9742813', 14, '8', NULL, 'Plaça de l\'Ajuntament, s/n', '08696', '42.2440108,1.9742813', '938236011', '938236403', 'lillet@poblalillet.cat', 'https://www.poblalillet.cat', 'P0816500C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08166.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08166.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08166.jpg', '081666', 'La_Pobla_de_Lillet', 1105, 51.45, 843, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08167', 'Polinyà', 'Polinyà', '', 'polinya', 'polinya', '41.5610313,2.1521582', 40, '8', NULL, 'Pl. Vila, 1', '08213', '41.5610313,2.1521582', '937130264', '937130248', 'polinya@ajpolinya.cat', 'https://www.polinya.cat', 'P0816600A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08167.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08167.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08167.jpg', '081672', 'Polinyà', 8581, 8.79, 158, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08168', 'Pontons', 'Pontons', '', 'pontons', 'pontons', '41.4155898,1.5162960', 3, '8', NULL, 'Pl. Vila, 1', '08738', '41.4156052,1.5168544', '938987056', '938987000', 'pontons@pontons.org', 'https://www.pontons.org', 'P0816700I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08168.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08168.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08168.jpg', '081688', 'Pontons', 516, 25.94, 632, '2026-03-05 13:12:27', '2026-09-12 10:14:37'),
	('08169', 'El Prat de Llobregat', 'Prat de Llobregat', 'El', 'el_prat_de_llobregat', 'prat_de_llobregat', '41.3305918,2.0930815', 11, '8', NULL, 'Pl. Vila, 1', '08820', '41.3307921,2.0930187', '933790050', '933793416', 'oiac@elprat.cat', 'https://www.elprat.cat', 'P0816800G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08169.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08169.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08169.jpg', '081691', 'El_Prat_de_Llobregat', 66338, 31.41, 5, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08170', 'Els Prats de Rei', 'Prats de Rei', 'Els', 'els_prats_de_rei', 'prats_de_rei', '41.6032506,1.6204699', 6, '8', NULL, 'Plaça Major, 1', '08281', '41.7055690,1.5416153', '938698192', '938698192', 'pratsr@pratsderei.cat', 'https://www.pratsderei.cat', 'P0816900E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08170.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08170.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08170.jpg', '081705', 'Els_Prats_de_Rei', 554, 26.06, 608, '2026-03-05 13:12:27', '2026-09-27 17:22:47'),
	('08171', 'Prats de Lluçanès', 'Prats de Lluçanès', '', 'prats_de_llucanes', 'prats_de_llucanes', '42.0088886,2.0306807', 43, '8', NULL, 'Passeig del Lluçanès, s/n Edifici Cal Bach', '08513', '42.0088886,2.0306807', '938560100', '938508070', 'ajuntament@pratsdellucanes.cat', 'https://www.pratsdellucanes.cat', 'P0817000C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08171.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08171.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08171.jpg', '081712', 'Prats_de_Lluçanès', 2761, 13.78, 707, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08172', 'Premià de Mar', 'Premià de Mar', '', 'premia_de_mar', 'premia_de_mar', '41.4898365,2.3568174', 21, '8', NULL, 'Plaça de l\'Ajuntament, 1', '08330', '41.4898365,2.3568174', '937417400', '937417425', 'info@premiademar.cat', 'https://www.premiademar.cat', 'P0817100A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08172.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08172.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08172.jpg', '081727', 'Premià_de_Mar', 29431, 2.11, 8, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08174', 'Puigdàlber', 'Puigdàlber', '', 'puigdalber', 'puigdalber', '41.404276,1.701106', 3, '8', NULL, 'Plaça de la Vila, 1', '08797', '41.404276,1.701106', '938989077', '938989095', 'puigdalber@puigdalber.cat', 'https://www.puigdalber.cat', 'P0817300G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08174.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08174.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08174.jpg', '081748', 'Puigdàlber', 621, 0.63, 239, '2026-03-05 13:12:27', '2026-08-01 04:07:01'),
	('08175', 'Puig-reig', 'Puig-reig', '', 'puigreig', 'puigreig', '41.9735499,1.8775308', 14, '8', NULL, 'Carrer de Pau Casals, 1', '08692', '41.9740189,1.8790426', '938380000', '938381302', 'ajuntament@puig-reig.cat', 'https://www.puig-reig.cat', 'P0817400E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08175.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08175.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08175.jpg', '081751', 'Puig-reig', 4565, 45.75, 455, '2026-03-05 13:12:27', '2026-09-12 10:14:37'),
	('08176', 'Pujalt', 'Pujalt', '', 'pujalt', 'pujalt', '41.7018419,1.3663357', 6, '8', NULL, 'Carrer de Sant Andreu, s/n', '08282', '41.7018419,1.3663357', '938681288', '938680102', 'pujalt@pujalt.cat', 'https://www.pujalt.cat', 'P0817500B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08176.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08176.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08176.jpg', '081764', 'Pujalt', 204, 31.43, 770, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08177', 'La Quar', 'Quar', 'La', 'la_quar', 'quar', '42.1050263,1.9807775', 14, '8', NULL, 'C/ Antoni Raurell, 1', '08619', '42.1050263,1.9807775', '620196808', '938242007', 'laquar@laquar.cat', 'https://www.laquar.cat', 'P0817600J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08177.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08177.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08177.jpg', '081770', 'La_Quar', 41, 38.25, 1061, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08178', 'Rajadell', 'Rajadell', '', 'rajadell', 'rajadell', '41.7291916,1.7056275', 7, '8', NULL, 'C/ Major, 3', '08256', '41.7276452,1.7061352', '938368026', '938368026', 'rajadell@rajadell.cat', 'https://www.rajadell.cat/', 'P0817700H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08178.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08178.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08178.jpg', '081786', 'Rajadell', 597, 45.53, 405, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08179', 'Rellinars', 'Rellinars', '', 'rellinars', 'rellinars', '41.6367945,1.9104470', 40, '8', NULL, 'Pl. Ajuntament, s/n', '08299', '41.6367945,1.9104470', '938345000', '938345101', 'rellinars@rellinars.cat', 'https://www.rellinars.cat', 'P0817800F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08179.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08179.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08179.jpg', '081799', 'Rellinars', 926, 17.79, 322, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08180', 'Ripollet', 'Ripollet', '', 'ripollet', 'ripollet', '41.4993372,2.1573095', 40, '8', NULL, 'Carrer de Balmes, 2', '08291', '41.4972620,2.1531103', '935046000', '935808148', 'ripollet@ripollet.org', 'https://www.ripollet.cat', 'P0817900D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08180.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08180.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08180.jpg', '081803', 'Ripollet', 39897, 4.33, 79, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08181', 'La Roca del Vallès', 'Roca del Vallès', 'La', 'la_roca_del_valles', 'roca_del_valles', '41.5856267,2.3163483', 41, '8', NULL, 'Carrer de Catalunya, 18-24', '08430', '41.5874609,2.320664', '938422016', '938420459', 'ajuntament@laroca.cat', 'http://www.laroca.cat', 'P0818000B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08181.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08181.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08181.jpg', '081810', 'La_Roca_del_Vallès', 11014, 36.90, 123, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08182', 'El Pont de Vilomara i Rocafort', 'Pont de Vilomara i Rocafort', 'El', 'el_pont_de_vilomara_i_rocafort', 'pont_de_vilomara_i_rocafort', '41.7008286,1.8691118', 7, '8', NULL, 'Pl. de l\'Ajuntament, 1', '08254', '41.7008286,1.8691118', '938318811', '938317550', 'pont@elpont.cat', 'https://www.elpont.cat', 'P0818100J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08182.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08182.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08182.jpg', '081825', 'El_Pont_de_Vilomara_i_Rocafort', 4193, 27.41, 202, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08183', 'Roda de Ter', 'Roda de Ter', '', 'roda_de_ter', 'roda_de_ter', '41.9814126,2.3096755', 24, '8', NULL, 'Plaça Major, 4', '08510', '41.9809181,2.3093603', '938500075', '938540931', 'rodadeter@rodadeter.cat', 'https://www.rodadeter.cat', 'P0818200H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08183.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08183.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08183.jpg', '081831', 'Roda_de_Ter', 6937, 2.23, 443, '2026-03-05 13:12:27', '2026-09-12 10:14:37'),
	('08184', 'Rubí', 'Rubí', '', 'rubi', 'rubi', '41.4937252,2.0310861', 40, '8', NULL, 'Pl. Pere Aguilera, 1', '08191', '41.4937252,2.0310861', '935887000', '935884526', 'alcaldia@ajrubi.cat', 'https://www.rubi.cat/', 'P0818300F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08184.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08184.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08184.jpg', '081846', 'Rubí', 82823, 32.30, 123, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08185', 'Rubió', 'Rubió', '', 'rubio', 'rubio', '41.4936194,2.0319476', 6, '8', NULL, 'Pl. Ajuntament, s/n', '08719', '41.6445080,1.5699988', '938094181', '938094181', 'rubio@rubio.cat', 'https://www.rubio.cat', 'P0818400D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08185.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08185.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08185.jpg', '081859', 'Rubió', 225, 48.00, 629, '2026-03-05 13:12:27', '2026-06-19 22:42:01'),
	('08187', 'Sabadell', 'Sabadell', '', 'sabadell', 'sabadell', '41.5460801,2.1083214', 40, '8', NULL, 'Pl. St. Roc, 1', '08201', '41.5463476,2.1085358', '937453100', '937453111', 'alcaldia@ajsabadell.cat', 'https://www.sabadell.cat', 'P0818600I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08187.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08187.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08187.jpg', '081878', 'Sabadell', 225368, 37.79, 190, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08188', 'Sagàs', 'Sagàs', '', 'sagas', 'sagas', '42.0313855,1.9609084', 14, '8', NULL, 'Crta de Vic, s/n', '08517', '42.0313855,1.9609084', '938251150', '938251150', 'sagas@sagas.cat', 'https://www.sagas.cat', 'P0818700G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08188.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08188.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08188.jpg', '081884', 'Sagàs', 154, 44.60, 738, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08189', 'Sant Pere Sallavinera', 'Sant Pere Sallavinera', '', 'sant_pere_sallavinera', 'sant_pere_sallavinera', '41.7365708,1.5745225', 6, '8', NULL, 'Carrer del Raval, s/n', '08281', '41.7365708,1.5745225', '938698830', '938698830', 'st.peres@sallavinera.cat', 'http://www.sallavinera.cat', 'P0818800E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08189.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08189.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08189.jpg', '081897', 'Sant_Pere_Sallavinera', 177, 22.02, 587, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08190', 'Saldes', 'Saldes', '', 'saldes', 'saldes', '42.2287591,1.7349486', 14, '8', NULL, 'Pl. Pedraforca, s/n', '08697', '42.2286673,1.7355622', '938258005', '938258069', 'saldes@saldes.cat', 'https://www.saldes.cat', 'P0818900C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08190.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08190.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08190.jpg', '081901', 'Saldes', 302, 66.40, 1215, '2026-03-05 13:12:27', '2026-08-20 08:16:40'),
	('08191', 'Sallent', 'Sallent', '', 'sallent', 'sallent', '41.8390397,1.9081612', 7, '8', NULL, 'Torres Amat, 26', '08650', '41.8238778,1.8950035', '938370200', '938206160', 'sallent@sallent.cat', 'https://www.sallent.cat', 'P0819000A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08191.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08191.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08191.jpg', '081918', 'Sallent', 7030, 65.22, 275, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08192', 'Santpedor', 'Santpedor', '', 'santpedor', 'santpedor', '41.7847594,1.8386882', 7, '8', NULL, 'Plaça Gran U d\'Octubre, 4', '08251', '41.7847594,1.8386882', '938272828', '938321608', 'santpedor@santpedor.cat', 'https://www.santpedor.cat', 'P0819100I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08192.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08192.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08192.jpg', '081923', 'Santpedor', 7744, 16.59, 320, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08193', 'Sant Iscle de Vallalta', 'Sant Iscle de Vallalta', '', 'sant_iscle_de_vallalta', 'sant_iscle_de_vallalta', '41.6233768,2.5704005', 21, '8', NULL, 'Escoles, 2', '08359', '41.6233768,2.5704005', '937946128', '937946048', 'st.iscle@santiscle.cat', 'https://www.santiscle.cat', 'P0819200G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08193.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08193.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08193.jpg', '081939', 'Sant_Iscle_de_Vallalta', 1477, 17.77, 129, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08194', 'Sant Adrià de Besòs', 'Sant Adrià de Besòs', '', 'sant_adria_de_besos', 'sant_adria_de_besos', '41.4300956,2.2178385', 13, '8', NULL, 'Pl. Vila, 12', '08930', '41.4300956,2.2178385', '933812004', '933817056', 'alcaldia@sant-adria.net', 'https://www.sant-adria.cat/', 'P0819300E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08194.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08194.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08194.jpg', '081944', 'Sant_Adrià_de_Besòs', 39323, 3.82, 14, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08195', 'Sant Agustí de Lluçanès', 'Sant Agustí de Lluçanès', '', 'sant_agusti_de_llucanes', 'sant_agusti_de_llucanes', '42.0938147,2.1309632', 24, '8', NULL, 'C/ Alou, 6', '08586', '42.0938147,2.1309632', '938527001', '938527001', 'st.agusti@santagustidellucanes.cat', 'https://www.santagustidellucanes.cat', 'P0819400C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08195.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08195.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08195.jpg', '081957', 'Sant_Agustí_de_Lluçanès', 106, 13.22, 816, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08196', 'Sant Andreu de la Barca', 'Sant Andreu de la Barca', '', 'sant_andreu_de_la_barca', 'sant_andreu_de_la_barca', '41.4501296,1.9715064', 11, '8', NULL, 'Plaça de l\'Ajuntament, 1', '08740', '41.4501296,1.9715064', '936356400', '936356413', 'ajuntament@sabarca.cat', 'https://www.sabarca.cat', 'P0819500J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08196.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08196.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08196.jpg', '081960', 'Sant_Andreu_de_la_Barca', 27094, 5.50, 42, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08197', 'Sant Andreu de Llavaneres', 'Sant Andreu de Llavaneres', '', 'sant_andreu_de_llavaneres', 'sant_andreu_de_llavaneres', '41.5739524,2.4828158', 21, '8', NULL, 'Pl. Vila, 1', '08392', '41.5744128,2.4833717', '937023600', '937952630', 'ajuntament@ajllavaneres.cat', 'https://ajllavaneres.cat', 'P0819600H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08197.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08197.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08197.jpg', '081976', 'Sant_Andreu_de_Llavaneres', 11938, 11.83, 125, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08198', 'Sant Antoni de Vilamajor', 'Sant Antoni de Vilamajor', '', 'sant_antoni_de_vilamajor', 'sant_antoni_de_vilamajor', '41.6735604,2.4012202', 41, '8', NULL, 'Plaça Montseny, 5', '08459', '41.6735820,2.4007087', '938452400', '938452051', 'ajuntament@savilamajor.cat', 'https://www.santantonidevilamajor.cat', 'P0819700F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08198.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08198.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08198.jpg', '081982', 'Sant_Antoni_de_Vilamajor', 6724, 13.70, 258, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08199', 'Sant Bartomeu del Grau', 'Sant Bartomeu del Grau', '', 'sant_bartomeu_del_grau', 'sant_bartomeu_del_grau', '41.9807659,2.1750391', 24, '8', NULL, 'Passeig del Grau, 10', '08519', '41.9807659,2.1750391', '938889000', '938889800', 'st.bartomeu@sbg.cat', 'https://www.sbg.cat', 'P0819800D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08199.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08199.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08199.jpg', '081995', 'Sant_Bartomeu_del_Grau', 933, 34.40, 867, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08200', 'Sant Boi de Llobregat', 'Sant Boi de Llobregat', '', 'sant_boi_de_llobregat', 'sant_boi_de_llobregat', '41.3241016,2.0511278', 11, '8', NULL, 'Plaça de l\'Ajuntament, 1', '08830', '41.3241016,2.0511278', '936351200', '936301856', 'comunica@santboi.cat', 'https://www.santboi.cat', 'P0819900B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08200.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08200.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08200.jpg', '082009', 'Sant_Boi_de_Llobregat', 85610, 21.47, 30, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08201', 'Sant Boi de Lluçanès', 'Sant Boi de Lluçanès', '', 'sant_boi_de_llucanes', 'sant_boi_de_llucanes', '42.0603988,2.1489990', 24, '8', NULL, 'Passeig Lluçanès, 8', '08589', '42.0603988,2.1489990', '938578241', '938578028', 'st.boillu@santboidellucanes.cat', 'https://www.santboidellucanes.cat', 'P0820000H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08201.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08201.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08201.jpg', '082016', 'Sant_Boi_de_Lluçanès', 602, 19.53, 810, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08202', 'Sant Celoni', 'Sant Celoni', '', 'sant_celoni', 'sant_celoni', '41.6895041,2.4914526', 41, '8', NULL, 'Pl. Vila, 1', '08470', '41.6899533,2.4920522', '938641211', '938673914', 'santceloni@santceloni.cat', 'https://www.santceloni.cat', 'P0820100F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08202.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08202.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08202.jpg', '082021', 'Sant_Celoni', 18977, 65.23, 152, '2026-03-05 13:12:27', '2026-06-24 17:05:23'),
	('08203', 'Sant Cebrià de Vallalta', 'Sant Cebrià de Vallalta', '', 'sant_cebria_de_vallalta', 'sant_cebria_de_vallalta', '41.6197864,2.6002866', 21, '8', NULL, 'C. Centre, 27', '08396', '41.6197864,2.6002866', '937631024', '937630219', 'scvl.ajuntament@stcebria.cat', 'https://www.stcebria.cat', 'P0820200D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08203.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08203.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08203.jpg', '082037', 'Sant_Cebrià_de_Vallalta', 3872, 15.68, 71, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08204', 'Sant Climent de Llobregat', 'Sant Climent de Llobregat', '', 'sant_climent_de_llobregat', 'sant_climent_de_llobregat', '41.3366993,1.9970575', 11, '8', NULL, 'Pl. Vila, 1', '08849', '41.3373217,1.9958166', '936580791', '936370971', 'st.climent@santclimentdellobregat.cat', 'https://www.santclimentdellobregat.cat', 'P0820300B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08204.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08204.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08204.jpg', '082042', 'Sant_Climent_de_Llobregat', 4187, 10.81, 87, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08205', 'Sant Cugat del Vallès', 'Sant Cugat del Vallès', '', 'sant_cugat_del_valles', 'sant_cugat_del_valles', '41.47056933371675,2.085294882677327', 40, '8', NULL, 'Pl. de la Vila, 1', '08172', '41.47056933371675,2.085294882677327', '935657000', '936755406', 'bustiaciutadana@santcugat.cat', 'https://www.santcugat.cat', 'P0820400J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08205.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08205.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08205.jpg', '082055', 'Sant_Cugat_del_Vallès', 97983, 48.23, 124, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08206', 'Sant Cugat Sesgarrigues', 'Sant Cugat Sesgarrigues', '', 'sant_cugat_sesgarrigues', 'sant_cugat_sesgarrigues', '41.3644619,1.7530702', 3, '8', NULL, 'Carrer de Sant Antoni, 31', '08798', '41.3644619,1.7530702', '938970103', '938970690', 'st.cugats@santcugatsesgarrigues.cat', 'https://www.santcugatsesgarrigues.cat', 'P0820500G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08206.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08206.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08206.jpg', '082068', 'Sant_Cugat_Sesgarrigues', 1038, 6.24, 266, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08207', 'Sant Esteve de Palautordera', 'Sant Esteve de Palautordera', '', 'sant_esteve_de_palautordera', 'sant_esteve_de_palautordera', '41.7040768,2.4343426', 41, '8', NULL, 'Major, 14', '08461', '41.7034905,2.4363975', '938480082', '938482750', 'ajuntament@santestevedepalautordera.cat', 'https://www.santestevedepalautordera.cat', 'P0820600E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08207.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08207.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08207.jpg', '082074', 'Sant_Esteve_de_Palautordera', 3099, 10.64, 231, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08208', 'Sant Esteve Sesrovires', 'Sant Esteve Sesrovires', '', 'sant_esteve_sesrovires', 'sant_esteve_sesrovires', '41.4932429,1.8747348', 11, '8', NULL, 'Carrer Major, 8', '08635', '41.4927624,1.8741722', '937713017', '937713120', 'ajuntament@sesrovires.cat', 'https://www.sesrovires.cat', 'P0820700C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08208.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08208.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08208.jpg', '082080', 'Sant_Esteve_Sesrovires', 8121, 18.56, 173, '2026-03-05 13:12:27', '2026-09-12 10:14:37'),
	('08209', 'Sant Fost de Campsentelles', 'Sant Fost de Campsentelles', '', 'sant_fost_de_campsentelles', 'sant_fost_de_campsentelles', '41.5187520,2.2328323', 41, '8', NULL, 'Pl. Vila, 1', '08105', '41.5187160,2.2326116', '935796980', '935796982', 'ajuntament@santfost.cat', 'https://www.santfost.cat', 'P0820800A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08209.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08209.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08209.jpg', '082093', 'Sant_Fost_de_Campsentelles', 9419, 13.15, 112, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08210', 'Sant Feliu de Codines', 'Sant Feliu de Codines', '', 'sant_feliu_de_codines', 'sant_feliu_de_codines', '41.6885036,2.1646769', 41, '8', NULL, 'Plaça Josep Umbert Ventura, 2', '08182', '41.6888063,2.1644362', '938662768', '938662634', 'sfdc.ajuntament@santfeliudecodines.cat', 'https://www.santfeliudecodines.cat', 'P0820900I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08210.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08210.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08210.jpg', '082107', 'Sant_Feliu_de_Codines', 6827, 15.00, 480, '2026-03-05 13:12:27', '2026-07-30 01:30:12'),
	('08211', 'Sant Feliu de Llobregat', 'Sant Feliu de Llobregat', '', 'sant_feliu_de_llobregat', 'sant_feliu_de_llobregat', '41.3812851,2.0446381', 11, '8', NULL, 'Pl. Vila, 1', '08980', '41.3807992,2.0449187', '936858000', '936858019', 'ajuntament@santfeliu.cat', 'https://www.santfeliu.cat', 'P0821000G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08211.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08211.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08211.jpg', '082114', 'Sant_Feliu_de_Llobregat', 46781, 11.82, 25, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08212', 'Sant Feliu Sasserra', 'Sant Feliu Sasserra', '', 'sant_feliu_sasserra', 'sant_feliu_sasserra', '41.9447451,2.0213124', 7, '8', NULL, 'Pl. Major, 1', '08274', '41.9443922,2.0223761', '938819011', '938819017', 'sfs.ajuntament@santfeliusasserra.cat', 'https://www.santfeliusasserra.cat', 'P0821100E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08212.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08212.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08212.jpg', '082129', 'Sant_Feliu_Sasserra', 633, 22.36, 617, '2026-03-05 13:12:27', '2026-07-18 13:26:47'),
	('08213', 'Sant Fruitós de Bages', 'Sant Fruitós de Bages', '', 'sant_fruitos_de_bages', 'sant_fruitos_de_bages', '41.7507358,1.8734962', 7, '8', NULL, 'Ctra. de Vic, 35-37', '08272', '41.7507358,1.8734962', '938789700', '938760486', 'ajuntament@santfruitos.cat', 'https://www.santfruitos.cat', 'P0821200C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08213.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08213.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08213.jpg', '082135', 'Sant_Fruitós_de_Bages', 9333, 22.20, 246, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08214', 'Vilassar de Dalt', 'Vilassar de Dalt', '', 'vilassar_de_dalt', 'vilassar_de_dalt', '41.5163595,2.3592879', 21, '8', NULL, 'Plaça de la Vila, 1', '08339', '41.5170511,2.3585352', '937539800', '937507750', 'oac@vilassardedalt.cat', 'https://www.vilassar.cat', 'P0821300A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08214.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08214.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08214.jpg', '082140', 'Vilassar_de_Dalt', 9404, 8.86, 135, '2026-03-05 13:12:27', '2026-09-17 16:47:21'),
	('08215', 'Sant Hipòlit de Voltregà', 'Sant Hipòlit de Voltregà', '', 'sant_hipolit_de_voltrega', 'sant_hipolit_de_voltrega', '42.0153845,2.2364265', 24, '8', NULL, 'Pl. Vila, 1', '08512', '42.0153845,2.2364265', '938502626', '938502339', 'st.hipolit@santhipolitdevoltrega.cat', 'https://www.santhipolitdevoltrega.cat', 'P0821400I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08215.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08215.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08215.jpg', '082153', 'Sant_Hipòlit_de_Voltregà', 3666, 0.92, 533, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08216', 'Sant Jaume de Frontanyà', 'Sant Jaume de Frontanyà', '', 'sant_jaume_de_frontanya', 'sant_jaume_de_frontanya', '42.1871657,2.023848', 14, '8', NULL, 'Casa Consistorial', '08619', '42.1871657,2.023848', '938239194', '938239120', 'st.jaume@santjaumedefrontanya.cat', 'https://www.santjaumedefrontanya.cat', 'P0821500F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08216.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08216.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08216.jpg', '082166', 'Sant_Jaume_de_Frontanyà', 25, 21.26, 1072, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08217', 'Sant Joan Despí', 'Sant Joan Despí', '', 'sant_joan_despi', 'sant_joan_despi', '41.3463452,2.0891035', 11, '8', NULL, 'Carretera del Mig, 9', '08970', '41.3463452,2.0891035', '934806000', '934806055', 'ajuntament@sjdespi.net', 'https://www.santjoandespi.cat', 'P0821600D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08217.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08217.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08217.jpg', '082172', 'Sant_Joan_Despí', 35926, 6.17, 14, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08218', 'Sant Joan de Vilatorrada', 'Sant Joan de Vilatorrada', '', 'sant_joan_de_vilatorrada', 'sant_joan_de_vilatorrada', '41.7422073,1.8036250', 7, '8', NULL, 'Major, 91-93', '08250', '41.7419996,1.8047862', '938764040', '938764440', 'ajuntament@santjoanvilatorrada.cat', 'https://www.santjoanvilatorrada.cat', 'P0822500E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08218.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08218.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08218.jpg', '082188', 'Sant_Joan_de_Vilatorrada', 11133, 16.42, 277, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08219', 'Vilassar de Mar', 'Vilassar de Mar', '', 'vilassar_de_mar', 'vilassar_de_mar', '41.5064041,2.3913883', 21, '8', NULL, 'Pl. Ajuntament, 6', '08340', '41.5064041,2.3913883', '937542400', '937594950', 'ajuntament@vilassardemar.cat', 'https://www.vilassardemar.cat', 'P0821700B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08219.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08219.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08219.jpg', '082191', 'Vilassar_de_Mar', 21227, 4.00, 10, '2026-03-05 13:12:27', '2026-03-05 13:12:27'),
	('08220', 'Sant Julià de Vilatorta', 'Sant Julià de Vilatorta', '', 'sant_julia_de_vilatorta', 'sant_julia_de_vilatorta', '41.922473100000005,2.325509703160474', 24, '8', NULL, 'Plaça u d\'Octubre', '08504', '41.922473100000005,2.325509703160474', '938122179', '938122063', 'sjv.ajuntament@vilatorta.cat', 'https://www.vilatorta.cat', 'P0821800J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08220.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08220.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08220.jpg', '082205', 'Sant_Julià_de_Vilatorta', 3323, 15.94, 595, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08221', 'Sant Just Desvern', 'Sant Just Desvern', '', 'sant_just_desvern', 'sant_just_desvern', '41.3858295,2.0757651', 11, '8', NULL, 'Pl. Verdaguer, 2', '08960', '41.3858915,2.0756530', '934804800', '934804879', 'ajuntament@santjust.cat', 'https://www.santjust.cat', 'P0821900H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08221.png', '', 'https://media.diba.cat/diba/municipis/img/vistes/vista08221.jpg', '082212', 'Sant_Just_Desvern', 21037, 7.81, 122, '2026-03-05 13:12:27', '2026-09-20 16:46:41'),
	('08222', 'Sant Llorenç d\'Hortons', 'Sant Llorenç d\'Hortons', '', 'sant_llorenc_dhortons', 'sant_llorenc_dhortons', '41.4673709,1.8248628', 3, '8', NULL, 'Carrer Major, 36', '08791', '41.4673709,1.8248628', '937716000', '937716308', 'st.llorensh@ajhortons.cat', 'https://www.ajhortons.cat', 'P0822000F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08222.png', '', 'https://media.diba.cat/diba/municipis/img/vistes/vista08222.jpg', '082227', 'Sant_Llorenç_d\'Hortons', 2627, 19.72, 196, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08223', 'Sant Llorenç Savall', 'Sant Llorenç Savall', '', 'sant_llorenc_savall', 'sant_llorenc_savall', '41.6805083,2.0575422', 40, '8', NULL, 'Carrer Sant Feliu, 2', '08212', '41.6805083,2.0575422', '937140018', '937141007', 'st.llorenss@savall.cat', 'https://www.savall.cat', 'P0822100D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08223.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08223.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08223.jpg', '082233', 'Sant_Llorenç_Savall', 2587, 41.11, 466, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08224', 'Sant Martí de Centelles', 'Sant Martí de Centelles', '', 'sant_marti_de_centelles', 'sant_marti_de_centelles', '41.7638765,2.2510062', 24, '8', NULL, 'Estació, 4', '08592', '41.7638765,2.2510062', '938442406', '938442406', 'st.martic@santmarticentelles.cat', 'https://www.santmarticentelles.cat', 'P0822200B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08224.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08224.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08224.jpg', '082248', 'Sant_Martí_de_Centelles', 1263, 25.55, 462, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08225', 'Sant Martí d\'Albars', 'Sant Martí d\'Albars', '', 'sant_marti_dalbars', 'sant_marti_dalbars', '42.0453001,2.0711117', 43, '8', NULL, 'La Blava, s/n', '08515', '42.0453001,2.0711117', '938530162', '938530101', 'st.martia@santmartidalbars.cat', 'https://www.santmartidalbars.cat', 'P0822300J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08225.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08225.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08225.jpg', '082251', 'Sant_Martí_d\'Albars', 135, 14.73, 651, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08226', 'Sant Martí de Tous', 'Sant Martí de Tous', '', 'sant_marti_de_tous', 'sant_marti_de_tous', '41.560759,1.523426', 6, '8', NULL, 'Plaça de la Independència, 1', '08712', '41.560759,1.523426', '938096002', '938096117', 'st.martitous@tous.cat', 'https://www.tous.cat', 'P0822600C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08226.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08226.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08226.jpg', '082264', 'Sant_Martí_de_Tous', 1269, 39.21, 465, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08227', 'Sant Martí Sarroca', 'Sant Martí Sarroca', '', 'sant_marti_sarroca', 'sant_marti_sarroca', '41.3833860,1.6121983', 3, '8', NULL, 'Carrer de Ferran Muñoz, 4-6', '08731', '41.3833860,1.6121983', '938991111', '938991512', 'st.martisa@santmartisarroca.cat', 'https://www.santmartisarroca.cat', 'P0822700A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08227.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08227.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08227.jpg', '082270', 'Sant_Martí_Sarroca', 3485, 35.27, 340, '2026-03-05 13:12:27', '2026-08-06 02:47:08'),
	('08228', 'Sant Martí Sesgueioles', 'Sant Martí Sesgueioles', '', 'sant_marti_sesgueioles', 'sant_marti_sesgueioles', '41.70067,1.49087', 6, '8', NULL, 'Plaça de l\'Ajuntament, s/n', '08282', '41.70067,1.49087', '938681122', '938681122', 'st.martises@sesgueioles.cat', 'https://www.sesgueioles.cat', 'P0822800I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08228.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08228.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08228.jpg', '082286', 'Sant_Martí_Sesgueioles', 353, 3.87, 646, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08229', 'Sant Mateu de Bages', 'Sant Mateu de Bages', '', 'sant_mateu_de_bages', 'sant_mateu_de_bages', '41.7969781,1.7327386', 7, '8', NULL, 'Casa Consistorial', '08263', '41.7969781,1.7327386', '938360010', '931157043', 'smb.ajuntament@santmateudebages.cat', 'https://www.santmateudebages.cat', 'P0822900G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08229.png', '', 'https://media.diba.cat/diba/municipis/img/vistes/vista08229.jpg', '082299', 'Sant_Mateu_de_Bages', 641, 102.92, 569, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08230', 'Premià de Dalt', 'Premià de Dalt', '', 'premia_de_dalt', 'premia_de_dalt', '41.5054913,2.3462056', 21, '8', NULL, 'Plaça de La Fàbrica, 1', '08338', '41.5054913,2.3462056', '936931515', '936931599', 'ajuntament@premiadedalt.cat', 'https://www.premiadedalt.cat', 'P0823000E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08230.png', '', 'https://media.diba.cat/diba/municipis/img/vistes/vista08230.jpg', '082303', 'Premià_de_Dalt', 10632, 6.57, 142, '2026-03-05 13:12:27', '2026-06-25 19:04:30'),
	('08231', 'Sant Pere de Ribes', 'Sant Pere de Ribes', '', 'sant_pere_de_ribes', 'sant_pere_de_ribes', '41.2622345,1.7721142', 17, '8', NULL, 'Pl. Vila, 1', '08810', '41.2622345,1.7721142', '938967300', '938967301', 'ajuntament@santperederibes.cat', 'https://www.santperederibes.cat', 'P0823100C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08231.png', '', 'https://media.diba.cat/diba/municipis/img/vistes/vista08231.jpg', '082310', 'Sant_Pere_de_Ribes', 32705, 40.80, 46, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08232', 'Sant Pere de Riudebitlles', 'Sant Pere de Riudebitlles', '', 'sant_pere_de_riudebitlles', 'sant_pere_de_riudebitlles', '41.4535893,1.7017388', 3, '8', NULL, 'Plaça de les Eres, 1', '08776', '41.4533951,1.7017485', '938995061', '938996081', 'st.pereriu@santperederiudebitlles.cat', 'https://www.santperederiudebitlles.cat', 'P0823200A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08232.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08232.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08232.jpg', '082325', 'Sant_Pere_de_Riudebitlles', 2516, 5.38, 245, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08233', 'Sant Pere de Torelló', 'Sant Pere de Torelló', '', 'sant_pere_de_torello', 'sant_pere_de_torello', '42.0751312,2.2965618', 24, '8', NULL, 'Verdaguer, 18', '08572', '42.0751312,2.2965618', '938584024', '938509130', 'st.peret@diba.cat', 'http://www.stpere.cat', 'P0823300I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08233.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08233.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08233.jpg', '082331', 'Sant_Pere_de_Torelló', 2596, 55.13, 621, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08234', 'Sant Pere de Vilamajor', 'Sant Pere de Vilamajor', '', 'sant_pere_de_vilamajor', 'sant_pere_de_vilamajor', '41.6843288,2.3900497', 41, '8', NULL, 'Nou, 26', '08458', '41.6850228,2.3907418', '938450008', '938452059', 'ajuntament@vilamajor.cat', 'https://www.vilamajor.cat', 'P0823400G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08234.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08234.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08234.jpg', '082346', 'Sant_Pere_de_Vilamajor', 5074, 34.72, 305, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08235', 'Sant Pol de Mar', 'Sant Pol de Mar', '', 'sant_pol_de_mar', 'sant_pol_de_mar', '41.6030132,2.6238092', 21, '8', NULL, 'Plaça de la Vila, s/n', '08395', '41.6001920,2.6214262', '937600451', '937601352', 'oac@santpol.cat', 'https://www.santpol.cat', 'P0823500D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08235.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08235.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08235.jpg', '082359', 'Sant_Pol_de_Mar', 5793, 7.53, 15, '2026-03-05 13:12:27', '2026-06-19 22:42:01'),
	('08236', 'Sant Quintí de Mediona', 'Sant Quintí de Mediona', '', 'sant_quinti_de_mediona', 'sant_quinti_de_mediona', '41.4619537,1.6630237', 3, '8', NULL, 'Pl. Església, 4', '08777', '41.4619537,1.6630237', '938998028', '938998400', 'sqm.ajuntament@santquintimediona.cat', 'https://www.santquintimediona.cat', 'P0823600B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08236.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08236.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08236.jpg', '082362', 'Sant_Quintí_de_Mediona', 2580, 13.84, 326, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08237', 'Sant Quirze de Besora', 'Sant Quirze de Besora', '', 'sant_quirze_de_besora', 'sant_quirze_de_besora', '42.1014260,2.2170289', 24, '8', NULL, 'Pl. Major, 1', '08580', '42.1001440,2.2227497', '938529017', '938529142', 'info@ajsantquirze.cat', 'https://www.ajsantquirze.cat', 'P0823700J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08237.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08237.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08237.jpg', '082378', 'Sant_Quirze_de_Besora', 2164, 8.10, 587, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08238', 'Sant Quirze del Vallès', 'Sant Quirze del Vallès', '', 'sant_quirze_del_valles', 'sant_quirze_del_valles', '41.5252458,2.0687060', 40, '8', NULL, 'Pl. Vila, 1', '08192', '41.5252458,2.0687060', '937216800', '937211531', 'ajuntament@santquirzevalles.cat', 'https://www.santquirzevalles.cat', 'P0823800H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08238.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08238.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08238.jpg', '082384', 'Sant_Quirze_del_Vallès', 20209, 14.07, 188, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08239', 'Sant Quirze Safaja', 'Sant Quirze Safaja', '', 'sant_quirze_safaja', 'sant_quirze_safaja', '41.7271551,2.1530012', 42, '8', NULL, 'Carretera de Barcelona, 2', '08189', '41.7251207,2.1467654', '938660368', '938662262', 'st.quirzes@sqs.cat', 'https://www.santquirzesafaja.es', 'P0823900F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08239.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08239.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08239.jpg', '082397', 'Sant_Quirze_Safaja', 663, 26.21, 627, '2026-03-05 13:12:27', '2026-09-27 17:22:47'),
	('08240', 'Sant Sadurní d\'Anoia', 'Sant Sadurní d\'Anoia', '', 'sant_sadurni_danoia', 'sant_sadurni_danoia', '41.4241879,1.7862963', 3, '8', NULL, 'Pl. Ajuntament, 1', '08770', '41.4241879,1.7862963', '938910325', '938183470', 'ajuntament@santsadurni.cat', 'https://www.santsadurni.cat', 'P0824000D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08240.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08240.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08240.jpg', '082401', 'Sant_Sadurní_d\'Anoia', 12911, 18.96, 141, '2026-03-05 13:12:27', '2026-07-20 04:02:03'),
	('08241', 'Sant Sadurní d\'Osormort', 'Sant Sadurní d\'Osormort', '', 'sant_sadurni_dosormort', 'sant_sadurni_dosormort', '41.903220,2.381632', 24, '8', NULL, 'Carretera de Vic, s/n', '08504', '41.903220,2.381632', '938887375', '938887375', 'st.sadurnio@santsadurnidosormort.cat', 'https://www.santsadurnidosormort.cat', 'P0824100B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08241.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08241.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08241.jpg', '082418', 'Sant_Sadurní_d\'Osormort', 89, 30.60, 526, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08242', 'Marganell', 'Marganell', '', 'marganell', 'marganell', '41.6406660,1.7900194', 7, '8', NULL, 'C/ Sant Esteve, s/n', '08298', '41.6403500,1.7921169', '938357063', '938357210', 'mrgn.ajuntament@marganell.cat', 'https://www.marganell.net', 'P0824200J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08242.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08242.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08242.jpg', '082423', 'Marganell', 309, 13.53, 679, '2026-03-05 13:12:27', '2026-06-24 17:05:23'),
	('08243', 'Santa Cecília de Voltregà', 'Santa Cecília de Voltregà', '', 'santa_cecilia_de_voltrega', 'santa_cecilia_de_voltrega', '41.9926134,2.2224459', 24, '8', NULL, 'Plaça Església s/n', '08509', '41.9926134,2.2224459', '938502474', '938502474', 'st.cecilia@santacecilia.cat', 'https://www.santacecilia.cat', 'P0824300H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08243.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08243.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08243.jpg', '082439', 'Santa_Cecília_de_Voltregà', 197, 8.63, 519, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08244', 'Santa Coloma de Cervelló', 'Santa Coloma de Cervelló', '', 'santa_coloma_de_cervello', 'santa_coloma_de_cervello', '41.3691728,2.0161118', 11, '8', NULL, 'Pau Casals, 26-34', '08690', '41.3691728,2.0161118', '936450700', '936340195', 'scc.ajuntament@santacolomadecervello.cat', 'https://www.santacolomadecervello.cat', 'P0824400F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08244.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08244.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08244.jpg', '082444', 'Santa_Coloma_de_Cervelló', 8273, 7.49, 173, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08245', 'Santa Coloma de Gramenet', 'Santa Coloma de Gramenet', '', 'santa_coloma_de_gramenet', 'santa_coloma_de_gramenet', '41.4519395,2.2080809', 13, '8', NULL, 'Plaça de la Vila, s/n', '08921', '41.4515597,2.2082338', '934624000', '934660067', 'st.colomag@gramenet.cat', 'https://www.gramenet.cat', 'P0824500C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08245.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08245.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08245.jpg', '082457', 'Santa_Coloma_de_Gramenet', 123981, 7.00, 56, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08246', 'Santa Eugènia de Berga', 'Santa Eugènia de Berga', '', 'santa_eugenia_de_berga', 'santa_eugenia_de_berga', '41.9002737,2.2831230', 24, '8', NULL, 'Pl. Major, 1', '08507', '41.9002737,2.2831230', '938855803', '938895321', 'st.eugenia@santaeugenia.cat', 'https://www.santaeugenia.cat', 'P0824600A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08246.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08246.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08246.jpg', '082460', 'Santa_Eugènia_de_Berga', 2348, 7.01, 538, '2026-03-05 13:12:27', '2026-06-16 16:43:19'),
	('08247', 'Santa Eulàlia de Riuprimer', 'Santa Eulàlia de Riuprimer', '', 'santa_eulalia_de_riuprimer', 'santa_eulalia_de_riuprimer', '41.9111046,2.1897552', 24, '8', NULL, 'C/ Major, 30', '08505', '41.9111046,2.1897552', '938138000', '938137077', 'st.eulaliariu@diba.cat', 'https://www.santaeulaliariuprimer.cat', 'P0824700I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08247.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08247.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08247.jpg', '082476', 'Santa_Eulàlia_de_Riuprimer', 1516, 13.82, 568, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08248', 'Santa Eulàlia de Ronçana', 'Santa Eulàlia de Ronçana', '', 'santa_eulalia_de_roncana', 'santa_eulalia_de_roncana', '41.6529965,2.2278374', 41, '8', NULL, 'Carretera de la Sagrera, 3', '08187', '41.6529965,2.2278374', '938448025', '938449380', 'ser@ser.cat', 'https://www.ser.cat', 'P0824800G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08248.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08248.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08248.jpg', '082482', 'Santa_Eulàlia_de_Ronçana', 7953, 14.23, 162, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08249', 'Santa Fe del Penedès', 'Santa Fe del Penedès', '', 'santa_fe_del_penedes', 'santa_fe_del_penedes', '41.3843306,1.7215953', 3, '8', NULL, 'C/ de l¿Horta, 1', '08792', '41.3843306,1.7215953', '938974211', '938974211', 'st.fe@santafepenedes.cat', 'https://www.santafepenedes.cat', 'P0824900E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08249.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08249.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08249.jpg', '082495', 'Santa_Fe_del_Penedès', 365, 3.40, 240, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08250', 'Santa Margarida de Montbui', 'Santa Margarida de Montbui', '', 'santa_margarida_de_montbui', 'santa_margarida_de_montbui', '41.5577979,1.5811668', 6, '8', NULL, 'Ctra. de Valls, 57', '08710', '41.5776371,1.6073982', '938034735', '938052345', 'ajuntament@montbui.cat', 'https://www.montbui.cat', 'P0825000C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08250.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08250.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08250.jpg', '082508', 'Santa_Margarida_de_Montbui', 10631, 27.59, 391, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08251', 'Santa Margarida i els Monjos', 'Santa Margarida i els Monjos', '', 'santa_margarida_i_els_monjos', 'santa_margarida_i_els_monjos', '41.3213389,1.6623519', 3, '8', NULL, 'Avinguda de Catalunya, 74', '08730', '41.3210822,1.6629293', '938980211', '938980360', 'info@smmonjos.cat', 'https://www.santamargaridaielsmonjos.cat', 'P0825100A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08251.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08251.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08251.jpg', '082515', 'Santa_Margarida_i_els_Monjos', 7806, 17.16, 161, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08252', 'Barberà del Vallès', 'Barberà del Vallès', '', 'barbera_del_valles', 'barbera_del_valles', '41.5168237,2.1154611', 40, '8', NULL, 'Av. Generalitat, 70', '08210', '41.515574,2.1221791', '937297171', '937191815', 'barbera@bdv.cat', 'https://www.bdv.cat', 'P0825200I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08252.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08252.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08252.jpg', '082520', 'Barberà_del_Vallès', 33987, 8.31, 146, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08253', 'Santa Maria de Besora', 'Santa Maria de Besora', '', 'santa_maria_de_besora', 'santa_maria_de_besora', '42.1272818,2.2589189', 24, '8', NULL, 'Pg. Pla de Teia núm.13', '08589', '42.1272818,2.2589189', '938550976', '938550976', 'st.m.besora@santamariabesora.cat', 'https://www.santamariabesora.cat', 'P0825300G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08253.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08253.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08253.jpg', '082536', 'Santa_Maria_de_Besora', 164, 24.73, 866, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08254', 'L\'Esquirol', 'Esquirol', 'L\'', 'lesquirol', 'esquirol', '42.035084,2.369045', 24, '8', NULL, 'Carrer Nou, 1', '08511', '42.035084,2.369045', '938568000', '938568305', 'lesquirol@diba.cat', 'https://www.lesquirol.cat', 'P0825400E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08254.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08254.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08254.jpg', '082541', 'L\'Esquirol', 2301, 61.80, 693, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08255', 'Santa Maria de Merlès', 'Santa Maria de Merlès', '', 'santa_maria_de_merles', 'santa_maria_de_merles', '42.00135655,1.9780246490142361', 14, '8', NULL, 'Pl. Sta. Maria, s/n', '08517', '42.00135655,1.9780246490142361', '938250400', '938250400', 'st.m.merles@santamariademerles.cat', 'https://www.santamariademerles.cat', 'P0825500B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08255.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08255.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08255.jpg', '082554', 'Santa_Maria_de_Merlès', 179, 52.08, 532, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08256', 'Santa Maria de Martorelles', 'Santa Maria de Martorelles', '', 'santa_maria_de_martorelles', 'santa_maria_de_martorelles', '41.5198085,2.2537044', 41, '8', NULL, 'Plaça Mossèn Josep Paituvi, 1', '08106', '41.5197095,2.2538547', '935931828', '935791547', 'smmt.ajuntament@santamariademartorelles.cat', 'https://www.santamariademartorelles.cat', 'P0825600J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08256.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08256.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08256.jpg', '082567', 'Santa_Maria_de_Martorelles', 868, 4.51, 181, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08257', 'Santa Maria de Miralles', 'Santa Maria de Miralles', '', 'santa_maria_de_miralles', 'santa_maria_de_miralles', '41.5004287,1.5282249', 6, '8', NULL, 'Ctra. d\'Igualada a Valls, s/n', '08787', '41.5303125,1.5350396', '938080301', '938080374', 'st.m.miralles@santamariademiralles.cat', 'https://www.santamariamiralles.cat', 'P0825700H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08257.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08257.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08257.jpg', '082573', 'Santa_Maria_de_Miralles', 144, 25.04, 628, '2026-03-05 13:12:27', '2026-09-12 10:14:37'),
	('08258', 'Santa Maria d\'Oló', 'Santa Maria d\'Oló', '', 'santa_maria_dolo', 'santa_maria_dolo', '41.4234967,1.6614574', 42, '8', NULL, 'Av. Manuel López, 1 1r.', '08273', '41.4234967,1.6614574', '938385000', '938385005', 'st.m.olo@olo.cat', 'https://www.olo.cat', 'P0825800F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08258.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08258.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08258.jpg', '082589', 'Santa_Maria_d\'Oló', 1099, 66.21, 542, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08259', 'Santa Maria de Palautordera', 'Santa Maria de Palautordera', '', 'santa_maria_de_palautordera', 'santa_maria_de_palautordera', '41.6937810,2.4442321', 41, '8', NULL, 'Plaça de la Vila, 1', '08460', '41.6932111,2.4458869', '938479620', '938479629', 'ajuntament@smpalautordera.cat', 'https://www.smpalautordera.cat', 'P0825900D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08259.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08259.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08259.jpg', '082592', 'Santa_Maria_de_Palautordera', 10080, 16.94, 208, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08260', 'Santa Perpètua de Mogoda', 'Santa Perpètua de Mogoda', '', 'santa_perpetua_de_mogoda', 'santa_perpetua_de_mogoda', '41.5344820,2.1835355', 40, '8', NULL, 'Pl. Vila, 5', '08130', '41.5344820,2.1835355', '935743234', '935607498', 'alcaldia@staperpetua.cat', 'https://www.staperpetua.cat', 'P0826000B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08260.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08260.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08260.jpg', '082606', 'Santa_Perpètua_de_Mogoda', 26130, 15.83, 74, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08261', 'Santa Susanna', 'Santa Susanna', '', 'santa_susanna', 'santa_susanna', '41.6357256,2.7065933', 21, '8', NULL, 'Plaça de Catalunya, s/n', '08398', '41.6355138,2.7075120', '937678441', '937678750', 'ajuntament@stasusanna.org', 'https://www.stasusanna.org', 'P0826100J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08261.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08261.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08261.jpg', '082613', 'Santa_Susanna', 4099, 12.63, 10, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08262', 'Sant Vicenç de Castellet', 'Sant Vicenç de Castellet', '', 'sant_vicenc_de_castellet', 'sant_vicenc_de_castellet', '41.6654580,1.8639709', 7, '8', NULL, 'Plaça de l\'Ajuntament, 10', '08295', '41.6654580,1.8639709', '936930611', '936930610', 'svcastellet@svc.cat', 'https://www.svc.cat', 'P0826200H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08262.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08262.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08262.jpg', '082628', 'Sant_Vicenç_de_Castellet', 10164, 17.13, 176, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08263', 'Sant Vicenç dels Horts', 'Sant Vicenç dels Horts', '', 'sant_vicenc_dels_horts', 'sant_vicenc_dels_horts', '41.3932328,2.0094925', 11, '8', NULL, 'Plaça de la Vila, 1', '08620', '41.3932328,2.0094925', '936561551', '933969833', 'ajuntament@svh.cat', 'https://www.svh.cat', 'P0826300F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08263.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08263.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08263.jpg', '082634', 'Sant_Vicenç_dels_Horts', 28746, 9.12, 22, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08264', 'Sant Vicenç de Montalt', 'Sant Vicenç de Montalt', '', 'sant_vicenc_de_montalt', 'sant_vicenc_de_montalt', '41.5767562,2.5101685', 21, '8', NULL, 'Sant Antoni, 13', '08394', '41.5767562,2.5101685', '937910511', '937912961', 'oficines@svmontalt.cat', 'https://www.svmontalt.cat', 'P0826400D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08264.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08264.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08264.jpg', '082649', 'Sant_Vicenç_de_Montalt', 6802, 8.05, 143, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08265', 'Sant Vicenç de Torelló', 'Sant Vicenç de Torelló', '', 'sant_vicenc_de_torello', 'sant_vicenc_de_torello', '42.0624286,2.2738672', 24, '8', NULL, 'Pl. Ajuntament, 6', '08571', '42.0624286,2.2738672', '938590003', '938504228', 'info@svdt.cat', 'https://www.santvicencdetorello.cat', 'P0826500A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08265.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08265.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08265.jpg', '082652', 'Sant_Vicenç_de_Torelló', 2117, 6.56, 554, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08266', 'Cerdanyola del Vallès', 'Cerdanyola del Vallès', '', 'cerdanyola_del_valles', 'cerdanyola_del_valles', '41.49123101130664,2.1407550256497214', 40, '8', NULL, 'Pl. Francesc Layret, s/n', '08290', '41.49123101130664,2.1407550256497214', '935808888', '935801620', 'alcaldia@cerdanyola.cat', 'https://www.cerdanyola.cat', 'P0826600I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08266.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08266.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08266.jpg', '082665', 'Cerdanyola_del_Vallès', 58528, 30.56, 32, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08267', 'Sentmenat', 'Sentmenat', '', 'sentmenat', 'sentmenat', '41.6110502,2.1374456', 40, '8', NULL, 'Pl. Casa de la Vila, 1', '08181', '41.6110502,2.1374456', '937153030 ', '937153466', 'sentmenat@sentmenat.cat', 'https://www.sentmenat.cat', 'P0826700G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08267.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08267.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08267.jpg', '082671', 'Sentmenat', 9548, 28.80, 241, '2026-03-05 13:12:27', '2026-09-27 17:22:47'),
	('08268', 'Cercs', 'Cercs', '', 'cercs', 'cercs', '42.1463987,1.8606233', 14, '8', NULL, 'Ctra. Ribes, 20', '08698', '42.1488686,1.8639730', '938247890', '938247991', 'cercs@cercs.cat', 'https://www.cercs.cat', 'P0826800E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08268.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08268.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08268.jpg', '082687', 'Cercs', 1236, 47.35, 650, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08269', 'Seva', 'Seva', '', 'seva', 'seva', '41.8361292,2.2835441', 24, '8', NULL, 'Carrer de Dalt, 5', '08553', '41.8361292,2.2835441', '938840111', '938840214', 'seva@seva.cat', 'https://www.seva.cat', 'P0826900C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08269.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08269.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08269.jpg', '082690', 'Seva', 3808, 30.40, 663, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08270', 'Sitges', 'Sitges', '', 'sitges', 'sitges', '41.2352225,1.8118609', 17, '8', NULL, 'Pl. Ajuntament, s/n', '08870', '41.2352225,1.8118609', '938117600', '938948803', 'ajuntament@sitges.cat', 'https://www.sitges.cat', 'P0827000A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08270.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08270.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08270.jpg', '082704', 'Sitges', 32609, 43.85, 10, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08271', 'Sobremunt', 'Sobremunt', '', 'sobremunt', 'sobremunt', '42.0353017,2.1651560', 43, '8', NULL, 'Cal Tic', '08589', '42.0353017,2.1651560', '938527071', '938527071', 'sobremunt@sobremunt.cat', 'https://www.sobremunt.cat', 'P0827100I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08271.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08271.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08271.jpg', '082711', 'Sobremunt', 90, 13.79, 880, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08272', 'Sora', 'Sora', '', 'sora', 'sora', '42.1119353,2.1610531', 24, '8', NULL, 'Major, s/n', '08588', '42.1119353,2.1610531', '938529193', '938529193', 'sora@sora.cat', 'https://www.sora.cat', 'P0827200G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08272.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08272.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08272.jpg', '082726', 'Sora', 225, 31.73, 716, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08273', 'Subirats', 'Subirats', '', 'subirats', 'subirats', '41.3825361,1.7946450', 3, '8', NULL, 'Ponent, 13', '08739', '41.3825361,1.7946450', '938993011', '938994811', 'ajuntament@subirats.cat', 'https://www.subirats.cat', 'P0827300E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08273.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08273.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08273.jpg', '082732', 'Subirats', 3288, 55.90, 243, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08274', 'Súria', 'Súria', '', 'suria', 'suria', '41.831042701009565,1.754201926617409', 7, '8', NULL, 'Carrer d\'Ernest Solvay, 13', '08260', '41.831042701009565,1.754201926617409', '938682800', '938682931', 'suria@suria.cat', 'https://www.suria.cat', 'P0827400C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08274.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08274.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08274.jpg', '082747', 'Súria', 6200, 23.60, 326, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08275', 'Tavèrnoles', 'Tavèrnoles', '', 'tavernoles', 'tavernoles', '41.9527720,2.3265514', 24, '8', NULL, 'Carrer de l\'Església, 1', '08519', '41.9527720,2.3265514', '938887308', '938122328', 'tavernoles@tavernoles.cat', 'https://www.tavernoles.cat', 'P0827500J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08275.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08275.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08275.jpg', '082750', 'Tavèrnoles', 354, 18.76, 537, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08276', 'Tagamanent', 'Tagamanent', '', 'tagamanent', 'tagamanent', '41.7591738,2.2936424', 41, '8', NULL, 'Pl. Vila, s/n', '08593', '41.7374442,2.2669442', '938429126', '938429124', 'tagamanent@tagamanent.cat', 'https://www.tagamanent.cat', 'P0827600H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08276.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08276.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08276.jpg', '082763', 'Tagamanent', 328, 43.31, 315, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08277', 'Talamanca', 'Talamanca', '', 'talamanca', 'talamanca', '41.7381785,1.9772537', 7, '8', NULL, 'Pl. Església, 1', '08278', '41.7380998,1.9772753', '938270036', '938270042', 'talamanca@talamanca.cat', 'https://www.talamanca.cat', 'P0827700F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08277.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08277.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08277.jpg', '082779', 'Talamanca', 209, 29.43, 552, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08278', 'Taradell', 'Taradell', '', 'taradell', 'taradell', '41.8762624,2.2859511', 24, '8', NULL, 'Vila, 45', '08552', '41.8736512,2.2874290', '938126100', '938800975', 'taradell@taradell.cat', 'https://www.taradell.cat', 'P0827800D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08278.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08278.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08278.jpg', '082785', 'Taradell', 6854, 26.48, 623, '2026-03-05 13:12:27', '2026-09-27 17:22:47'),
	('08279', 'Terrassa', 'Terrassa', '', 'terrassa', 'terrassa', '41.5629623,2.0100492', 40, '8', NULL, 'Raval de Montserrat, 14', '08221', '41.5631581,2.0101093', '937397000', '937397067', 'ajuntament@terrassa.cat', 'https://www.terrassa.cat', 'P0827900B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08279.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08279.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08279.jpg', '082798', 'Terrassa', 233270, 70.16, 277, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08280', 'Tavertet', 'Tavertet', '', 'tavertet', 'tavertet', '41.9955470,2.4182542', 24, '8', NULL, 'Jaume Balmes, s/n', '08511', '41.9955470,2.4182542', '938565079', '938845079', 'tavertet@tavertet.cat', 'https://www.tavertet.cat', 'P0828000J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08280.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08280.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08280.jpg', '082802', 'Tavertet', 128, 32.49, 869, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08281', 'Teià', 'Teià', '', 'teia', 'teia', '41.49983071583396,2.31976425264082', 21, '8', NULL, 'Pere Noguera, 12', '08329', '41.49983071583396,2.31976425264082', '935551234', '935409352', 'ajuntament@teia.cat', 'https://www.teia.cat', 'P0828100H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08281.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08281.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08281.jpg', '082819', 'Teià', 6898, 6.63, 128, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08282', 'Tiana', 'Tiana', '', 'tiana', 'tiana', '41.4819226,2.2689473', 21, '8', NULL, 'Pl. Vila, 1', '08391', '41.4816767,2.2690302', '933955011', '934657518', 'tiana@tiana.cat', 'https://www.tiana.cat', 'P0828200F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08282.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08282.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08282.jpg', '082824', 'Tiana', 9331, 7.95, 136, '2026-03-05 13:12:27', '2026-09-12 10:14:37'),
	('08283', 'Tona', 'Tona', '', 'tona', 'tona', '41.8538039,2.2280115', 24, '8', NULL, 'Carrer de la Font, 8-10', '08551', '41.8501703,2.2299167', '938870201', '938870498', 'tona@tona.cat', 'https://www.tona.cat', 'P0828300D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08283.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08283.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08283.jpg', '082830', 'Tona', 8527, 16.54, 596, '2026-03-05 13:12:27', '2026-09-12 10:14:37'),
	('08284', 'Tordera', 'Tordera', '', 'tordera', 'tordera', '41.7013581,2.7182911', 21, '8', NULL, 'Pl. Església, 2', '08490', '41.7013581,2.7182911', '937643717', '937643853', 'tordera@tordera.cat', 'https://www.tordera.cat', 'P0828400B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08284.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08284.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08284.jpg', '082845', 'Tordera', 19039, 84.09, 34, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08285', 'Torelló', 'Torelló', '', 'torello', 'torello', '42.0493339,2.2500986', 24, '8', NULL, 'C/ Ges d\'Avall, 5', '08570', '42.0470704,2.2579641', '938591050', '938504320', 'torello@ajtorello.cat', 'https://www.ajtorello.cat', 'P0828500I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08285.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08285.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08285.jpg', '082858', 'Torelló', 15334, 13.48, 508, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08286', 'La Torre de Claramunt', 'Torre de Claramunt', 'La', 'la_torre_de_claramunt', 'torre_de_claramunt', '41.5335238,1.6583183', 6, '8', NULL, 'Pl. Ajuntament, 1', '08789', '41.5335238,1.6583183', '938010329', '938010817', 'torre@latorredeclaramunt.cat', 'https://www.latorredeclaramunt.cat', 'P0828600G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08286.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08286.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08286.jpg', '082861', 'La_Torre_de_Claramunt', 4158, 15.02, 363, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08287', 'Torrelavit', 'Torrelavit', '', 'torrelavit', 'torrelavit', '41.4432636,1.7266773', 3, '8', NULL, 'Molí, 29', '08775', '41.4421674,1.7409455', '938995002', '938996155', 'torrelavit@torrelavit.cat', 'https://www.torrelavit.cat', 'P0828700E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08287.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08287.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08287.jpg', '082877', 'Torrelavit', 1572, 23.65, 202, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08288', 'Torrelles de Foix', 'Torrelles de Foix', '', 'torrelles_de_foix', 'torrelles_de_foix', '41.3877350,1.5723453', 3, '8', NULL, 'Pl. Lluís Companys, 1', '08737', '41.3879444,1.5725422', '938971001', '938972125', 'torrellesf@torrellesdefoix.cat', 'https://www.torrellesdefoix.cat', 'P0828800C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08288.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08288.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08288.jpg', '082883', 'Torrelles_de_Foix', 2746, 36.72, 367, '2026-03-05 13:12:27', '2026-08-18 10:43:25'),
	('08289', 'Torrelles de Llobregat', 'Torrelles de Llobregat', '', 'torrelles_de_llobregat', 'torrelles_de_llobregat', '41.3590252,1.9811280', 11, '8', NULL, 'Pl. Ajuntament, 1', '08629', '41.3590252,1.9811280', '936890000', '936890510', 'info@torrelles.cat', 'https://www.torrelles.cat', 'P0828900A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08289.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08289.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08289.jpg', '082896', 'Torrelles_de_Llobregat', 6170, 13.56, 126, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08290', 'Ullastrell', 'Ullastrell', '', 'ullastrell', 'ullastrell', '41.5265763,1.9560471', 40, '8', NULL, 'Carrer de la Serra, 17', '08231', '41.5262128,1.9547560', '937887262', '937887195', 'ullastrell@ullastrell.cat', 'https://www.ullastrell.cat', 'P0829000I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08290.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08290.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08290.jpg', '082900', 'Ullastrell', 2171, 7.31, 342, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08291', 'Vacarisses', 'Vacarisses', '', 'vacarisses', 'vacarisses', '41.6063852,1.9194608', 40, '8', NULL, 'Carrer de Pau Casals, 17', '08233', '41.6059089,1.9188516', '938359002', '938359407', 'vacarisses@vacarisses.cat', 'https://www.vacarisses.cat', 'P0829100G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08291.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08291.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08291.jpg', '082917', 'Vacarisses', 7729, 40.70, 382, '2026-03-05 13:12:27', '2026-06-19 22:42:01'),
	('08292', 'Vallbona d\'Anoia', 'Vallbona d\'Anoia', '', 'vallbona_danoia', 'vallbona_danoia', '41.5183194,1.7062369', 6, '8', NULL, 'Carrer Major, 110', '08785', '41.5199685,1.7094642', '937718002', '937719210', 'vallbona@vallbonadanoia.cat', 'https://www.vallbonadanoia.cat', 'P0829200E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08292.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08292.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08292.jpg', '082922', 'Vallbona_d\'Anoia', 1424, 6.45, 289, '2026-03-05 13:12:27', '2026-09-12 10:14:37'),
	('08293', 'Vallcebre', 'Vallcebre', '', 'vallcebre', 'vallcebre', '42.2036044,1.8188180', 14, '8', NULL, 'Plaça de l\'Ajuntament, s/n', '08699', '42.2036044,1.8188180', '938227031', '938227031', 'vallcebre@vallcebre.cat', 'https://www.vallcebre.cat', 'P0829300C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08293.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08293.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08293.jpg', '082938', 'Vallcebre', 260, 27.99, 1118, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08294', 'Vallgorguina', 'Vallgorguina', '', 'vallgorguina', 'vallgorguina', '41.6475574,2.5106298', 41, '8', NULL, 'Pl. Vila, 4', '08471', '41.6475574,2.5106298', '938679125', '938679259', 'vallgorguina@vallgorguina.cat', 'https://www.vallgorguina.cat', 'P0829500H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08294.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08294.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08294.jpg', '082943', 'Vallgorguina', 3261, 22.13, 222, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08295', 'Vallirana', 'Vallirana', '', 'vallirana', 'vallirana', '41.3877623,1.9320806', 11, '8', NULL, 'Carrer Major, 329', '08759', '41.3879160,1.9321168', '936830810', '936832897', 'vlrn.info@vallirana.cat', 'https://www.vallirana.cat', 'P0829600F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08295.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08295.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08295.jpg', '082956', 'Vallirana', 16245, 23.88, 177, '2026-03-05 13:12:27', '2026-08-27 18:23:06'),
	('08296', 'Vallromanes', 'Vallromanes', '', 'vallromanes', 'vallromanes', '41.5315951,2.2982266', 41, '8', NULL, 'Plaça de l\'Església, 6', '08188', '41.5315951,2.2982266', '935729159', '935729190', 'vallromanes@vallromanes.cat', 'https://www.vallromanes.cat', 'P0829700D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08296.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08296.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08296.jpg', '082969', 'Vallromanes', 2778, 10.65, 153, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08297', 'Veciana', 'Veciana', '', 'veciana', 'veciana', '41.6559029,1.4885194', 6, '8', NULL, 'Plaça de Ramon Servitje, s/n', '08289', '41.6559029,1.4885194', '938090055', '938090023', 'veciana@veciana.cat', 'https://www.veciana.cat', 'P0829800B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08297.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08297.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08297.jpg', '082975', 'Veciana', 168, 38.90, 653, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08298', 'Vic', 'Vic', '', 'vic', 'vic', '41.9302021,2.2545943', 24, '8', NULL, 'Ciutat, 1', '08500', '41.9299151,2.2550461', '938862100', '938862921', 'atenciociutadana@vic.cat', 'https://www.vic.cat', 'P0829900J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08298.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08298.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08298.jpg', '082981', 'Vic', 50796, 30.58, 487, '2026-03-05 13:12:27', '2026-09-19 22:08:39'),
	('08299', 'Vilada', 'Vilada', '', 'vilada', 'vilada', '42.1371181,1.9313919', 14, '8', NULL, 'Pl. Vila, s/n', '08613', '42.1371181,1.9313919', '938238128', '938238803', 'vilada@vilada.cat', 'https://www.vilada.cat', 'P0830000F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08299.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08299.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08299.jpg', '082994', 'Vilada', 432, 22.34, 750, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08300', 'Viladecavalls', 'Viladecavalls', '', 'viladecavalls', 'viladecavalls', '41.5563969,1.9553173', 40, '8', NULL, 'Carrer Antoni Soler i Hospital 7-9', '08232', '41.5563969,1.9553173', '937887141', '937892079', 'viladecavalls@viladecavalls.cat', 'https://www.viladecavalls.cat', 'P0830100D', 'https://media.diba.cat/diba/municipis/img/escuts/ec08300.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08300.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08300.jpg', '083008', 'Viladecavalls', 7817, 20.09, 274, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08301', 'Viladecans', 'Viladecans', '', 'viladecans', 'viladecans', '41.3163083,2.0156034', 11, '8', NULL, 'Jaume Abril, 2', '08840', '41.3159820,2.0200179', '936351800', '936370402', 'viladecansinfo@viladecans.cat', 'https://www.viladecans.cat', 'P0830200B', 'https://media.diba.cat/diba/municipis/img/escuts/ec08301.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08301.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08301.jpg', '083015', 'Viladecans', 67587, 20.40, 18, '2026-03-05 13:12:27', '2026-09-24 22:52:30'),
	('08302', 'Vilanova del Camí', 'Vilanova del Camí', '', 'vilanova_del_cami', 'vilanova_del_cami', '41.5722738,1.6342533', 6, '8', NULL, 'Plaça del Castell, 1', '08788', '41.5722738,1.6342533', '938054422', '938054290', 'vilanovac@vilanovadelcami.cat', 'https://www.vilanovadelcami.cat', 'P0830300J', 'https://media.diba.cat/diba/municipis/img/escuts/ec08302.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08302.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08302.jpg', '083020', 'Vilanova_del_Camí', 12936, 10.28, 302, '2026-03-05 13:12:27', '2026-08-03 16:08:48'),
	('08303', 'Vilanova de Sau', 'Vilanova de Sau', '', 'vilanova_de_sau', 'vilanova_de_sau', '41.9472211,2.3841032', 24, '8', NULL, 'Passeig de Verdaguer, 7', '08519', '41.9472211,2.3841032', '938847006', '938847106', 'vilanovas@vilanovadesau.cat', 'https://www.vilanovadesau.cat', 'P0830400H', 'https://media.diba.cat/diba/municipis/img/escuts/ec08303.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08303.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08303.jpg', '083036', 'Vilanova_de_Sau', 325, 58.84, 558, '2026-03-05 13:12:27', '2026-09-28 06:35:11'),
	('08304', 'Vilobí del Penedès', 'Vilobí del Penedès', '', 'vilobi_del_penedes', 'vilobi_del_penedes', '41.3464352,1.6980117', 3, '8', NULL, 'Plaça de la Vila, 1', '08735', '41.3464352,1.6980117', '938978980', '938978311', 'vilobi@vilobi.cat', 'https://www.vilobi.cat', 'P0830500E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08304.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08304.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08304.jpg', '083041', 'Vilobí_del_Penedès', 1154, 9.34, 333, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08305', 'Vilafranca del Penedès', 'Vilafranca del Penedès', '', 'vilafranca_del_penedes', 'vilafranca_del_penedes', '41.3464821,1.6979845', 3, '8', NULL, 'Cort, 14', '08720', '41.3464821,1.6979845', '938920358', '938921166', 'ajuntament@vilafranca.cat', 'https://www.vilafranca.cat', 'P0830600C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08305.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08305.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08305.jpg', '083054', 'Vilafranca_del_Penedès', 42607, 19.65, 223, '2026-03-05 13:12:27', '2026-06-16 02:52:26'),
	('08306', 'Vilalba Sasserra', 'Vilalba Sasserra', '', 'vilalba_sasserra', 'vilalba_sasserra', '41.6530012,2.4405379', 41, '8', NULL, 'Plaça de la Vila, 1', '08455', '41.6529845,2.4410201', '938410383', '938410383', 'vilalba@vilalbasasserra.cat', 'https://www.vilalbasasserra.cat', 'P0830700A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08306.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08306.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08306.jpg', '083067', 'Vilalba_Sasserra', 794, 6.05, 200, '2026-03-05 13:12:27', '2026-06-09 18:57:33'),
	('08307', 'Vilanova i la Geltrú', 'Vilanova i la Geltrú', '', 'vilanova_i_la_geltru', 'vilanova_i_la_geltru', '41.2240945,1.7260084', 17, '8', NULL, 'Plaça de la Vila, 8', '08800', '41.2240945,1.7260084', '938140000', '938142425', 'ajt_vilanova@vilanova.cat', 'https://www.vilanova.cat', 'P0830800I', 'https://media.diba.cat/diba/municipis/img/escuts/ec08307.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08307.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08307.jpg', '083073', 'Vilanova_i_la_Geltrú', 71641, 33.99, 22, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08308', 'Viver i Serrateix', 'Viver i Serrateix', '', 'viver_i_serrateix', 'viver_i_serrateix', '41.9497667,1.8014850', 14, '8', NULL, 'Casa de la Vila', '08673', '41.9497667,1.8014850', '938204922', '938204922', 'vvsr.ajuntament@viveriserrateix.cat', 'https://www.viveriserrateix.cat', 'P0830900G', 'https://media.diba.cat/diba/municipis/img/escuts/ec08308.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08308.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08308.jpg', '083089', 'Viver_i_Serrateix', 178, 66.80, 729, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08901', 'Rupit i Pruit', 'Rupit i Pruit', '', 'rupit_i_pruit', 'rupit_i_pruit', '42.023587,2.465077', 24, '8', NULL, 'Pl. Major, 6', '08569', '42.023587,2.465077', '938522003', '938522051', 'rupit@rupitpruit.cat', 'https://www.rupitpruit.cat', 'P0818500A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08901.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08901.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08901.jpg', '089019', 'Rupit_i_Pruit', 280, 47.81, 822, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08902', 'Vilanova del Vallès', 'Vilanova del Vallès', '', 'vilanova_del_valles', 'vilanova_del_valles', '41.5536734,2.2862297', 41, '8', NULL, 'Plaça de l\'Ajuntament, 1', '08410', '41.5536734,2.2862297', '938459277', '938456186', 'ajuntament@vilanovadelvalles.cat', 'https://www.vilanovadelvalles.cat', 'P0831000E', 'https://media.diba.cat/diba/municipis/img/escuts/ec08902.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08902.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08902.jpg', '089024', 'Vilanova_del_Vallès', 5693, 15.20, 100, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08903', 'Sant Julià de Cerdanyola', 'Sant Julià de Cerdanyola', '', 'sant_julia_de_cerdanyola', 'sant_julia_de_cerdanyola', '42.2197601,1.8951755', 14, '8', NULL, 'Local Municipal, s/n', '08694', '42.2197601,1.8951755', '938227667', '938227669', 'st.juliac@santjuliadecerdanyola.cat', 'https://www.santjuliadecerdanyola.cat', 'P0831100C', 'https://media.diba.cat/diba/municipis/img/escuts/ec08903.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08903.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08903.jpg', '089030', 'Sant_Julià_de_Cerdanyola', 234, 11.79, 920, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08904', 'Badia del Vallès', 'Badia del Vallès', '', 'badia_del_valles', 'badia_del_valles', '41.5079128,2.1157470', 40, '8', NULL, 'Av. Burgos, s/n', '08214', '41.5079128,2.1157470', '937182216', '937182042', 'badia@badiadelvalles.net', 'https://www.badiadelvalles.cat', 'P0831200A', 'https://media.diba.cat/diba/municipis/img/escuts/ec08904.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08904.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08904.jpg', '089045', 'Badia_del_Vallès', 13060, 0.93, 122, '2026-03-05 13:12:27', '2026-05-28 19:28:54'),
	('08905', 'La Palma de Cervelló', 'Palma de Cervelló', 'La', 'la_palma_de_cervello', 'palma_de_cervello', '41.4109843,1.9712064', 11, '8', NULL, 'c/ Sant Cristòfor, s/n.', '08756', '41.4109843,1.9712064', '936720202', '936720125', 'palma@lapalmadecervello.cat', 'http://www.lapalmadecervello.cat', 'P5831301F', 'https://media.diba.cat/diba/municipis/img/escuts/ec08905.png', 'https://media.diba.cat/diba/municipis/img/banderes/bn08905.gif', 'https://media.diba.cat/diba/municipis/img/vistes/vista08905.jpg', '089058', 'La_Palma_de_Cervelló', 3056, 5.46, NULL, '2026-03-05 13:12:27', '2026-05-28 19:28:54');

INSERT INTO `patr_estats_conservacio` (`id`, `nom`, `descripcio`, `color`, `prioritat_intervencio`, `actiu`, `created_at`, `updated_at`) VALUES
	(1, 'Excel·lent', 'En perfecte estat de conservació', '#28A745', 1, 1, '2026-02-04 15:14:43', '2026-02-04 15:14:43'),
	(2, 'Bo', 'Bon estat general, mínim manteniment necessari', '#17A2B8', 1, 1, '2026-02-04 15:14:43', '2026-02-04 15:14:43'),
	(3, 'Regular', 'Requereix manteniment preventiu', '#FFC107', 2, 1, '2026-02-04 15:14:43', '2026-02-04 15:14:43'),
	(4, 'Dolent', 'Requereix intervenció urgent', '#FD7E14', 4, 1, '2026-02-04 15:14:43', '2026-02-04 15:14:43'),
	(5, 'Molt dolent', 'Estat ruïnós, risc de pèrdua', '#DC3545', 5, 1, '2026-02-04 15:14:43', '2026-02-04 15:14:43'),
	(6, 'Desconegut', 'Estat no avaluat', '#6C757D', 3, 1, '2026-02-04 15:14:43', '2026-02-04 15:14:43');

INSERT INTO `patr_subtipus_patrimoni` (`id`, `tipus_patrimoni_id`, `nom`, `descripcio`, `actiu`) VALUES
	(1, 1, 'Religiós', 'Edificis d’ús religiós', 1),
	(2, 1, 'Civil', 'Edificis d’ús civil', 1),
	(3, 1, 'Industrial', 'Edificis d’ús industrial', 1),
	(4, 4, 'Arqueològic', 'Patrimoni arqueològic', 1),
	(5, 4, 'Etnològic', 'Patrimoni etnològic', 1),
	(6, 4, 'Popular', 'Patrimoni popular', 1),
	(7, 2, 'Popular', 'Arquitectura popular', 1),
	(8, 2, 'Industrial', 'Arquitectura industrial', 1),
	(9, 5, 'Commemoratiu', 'Monument commemoratiu', 1),
	(10, 5, 'Natural', 'Monument natural', 1),
	(11, 3, 'Públic', 'Espai públic', 1),
	(12, 3, 'Natural', 'Espai natural', 1);

INSERT INTO `patr_tipus_arxius` (`id`, `nom`, `extensions_permeses`, `mida_maxima_mb`, `descripcio`, `actiu`, `created_at`) VALUES
	(1, 'Imatge', '["jpg","jpeg","png","gif","webp"]', 10, 'Fotografies i imatges', 1, '2026-02-04 15:14:43'),
	(2, 'Document', '["pdf","doc","docx","odt"]', 20, 'Documents de text', 1, '2026-02-04 15:14:43'),
	(3, 'Plànol', '["pdf","dwg","dxf","jpg"]', 50, 'Plànols tècnics', 1, '2026-02-04 15:14:43'),
	(4, 'Vídeo', '["mp4","avi","mov"]', 100, 'Vídeos', 1, '2026-02-04 15:14:43'),
	(5, 'Audio', '["mp3","wav","ogg"]', 20, 'Àudio i narracions', 1, '2026-02-04 15:14:43'),
	(6, '3D', '["obj","stl","ply"]', 100, 'Models 3D', 1, '2026-02-04 15:14:43'),
	(7, 'Bibliografia', 'txt,pdf,doc', 20, 'Referències bibliogràfiques', 1, '2026-05-30 19:02:28');

INSERT INTO `patr_tipus_notes` (`id`, `nom`, `actiu`) VALUES
	(1, 'Bibliografia', 1),
	(2, 'Observació tècnica', 1),
	(3, 'Nota interna', 1),
	(4, 'Altra', 1);

INSERT INTO `patr_tipus_patrimoni` (`id`, `nom`, `descripcio`, `color`, `icona`, `actiu`, `created_at`, `updated_at`) VALUES
	(1, 'Edifici', 'Construccions de qualsevol ús', '#8B4513', 'fa-building', 1, '2026-02-25 12:35:31', '2026-02-25 12:37:50'),
	(2, 'Arquitectura', 'Obres arquitectòniques singulars', '#4169E1', 'fa-home', 1, '2026-02-25 12:35:31', '2026-02-25 12:37:54'),
	(3, 'Espai', 'Zones públiques o naturals', '#228B22', 'fa-tree', 1, '2026-02-25 12:35:31', '2026-02-25 12:37:56'),
	(4, 'Patrimoni', 'Elements patrimonials diversos', '#D2691E', 'fa-gem', 1, '2026-02-25 12:35:31', '2026-02-25 12:37:58'),
	(5, 'Monument', 'Monuments commemoratius o naturals', '#708090', 'fa-monument', 1, '2026-02-25 12:35:31', '2026-02-25 12:38:00');

INSERT INTO `provincies` (`id`, `Codi`, `Nom`) VALUES
	('1', '8', 'Barcelona');

INSERT INTO `rols` (`Id`, `Nom`) VALUES
	(1, 'Professora');

INSERT INTO `sexes` (`Id`, `Codi`, `Descripcio`) VALUES
	(1, 'M', 'Masculí'),
	(2, 'F', 'Femení'),
	(3, 'A', 'Altres');

INSERT INTO `volu_ambits_voluntaris` (`Id`, `Nom`, `Descripcio`, `PotSerEnllacAjuntament`, `PotSerCoordinador`, `Actiu`, `DataCreacio`) VALUES
	(1, 'Àmbit social', 'Acompanyament a persones grans, suport a persones amb dificultats, accions solidàries', 1, 1, 1, '2026-01-22 10:17:51.000000'),
	(2, 'Àmbit cultural', 'Suport en activitats festives, culturals, educatives o de patrimoni', 1, 1, 1, '2026-01-22 10:17:51.000000'),
	(3, 'Àmbit ambiental', 'Neteja de camins, activitats de sensibilització, protecció del medi natural', 1, 1, 1, '2026-01-22 10:17:51.000000'),
	(4, 'Àmbit esportiu', 'Suport en activitats esportives d\'àmbit local', 1, 1, 1, '2026-01-22 10:17:51.000000');

INSERT INTO `volu_estats_voluntaris` (`Id`, `Nom`, `Descripcio`, `Color`, `Actiu`, `Ordre`) VALUES
	(1, 'Candidat', 'Persona interessada en ser voluntari, pendent de validació', '#ffc107', 1, 1),
	(2, 'En procés', 'Voluntari en procés de documentació i formació inicial', '#17a2b8', 1, 2),
	(3, 'Actiu', 'Voluntari actiu participant en activitats', '#28a745', 1, 3),
	(4, 'Suspès temporalment', 'Voluntari suspès temporalment per motius diversos', '#fd7e14', 1, 4),
	(5, 'Baixa', 'Voluntari donat de baixa', '#6c757d', 1, 5);

INSERT INTO `volu_subambits_voluntaris` (`Id`, `Nom`, `Descripcio`, `AmbitVoluntariId`, `Actiu`, `DataCreacio`) VALUES
	(1, 'Acompanyament a persones grans', 'Acompanyament i suport a persones grans', 1, 1, '2026-01-22 10:17:51.000000'),
	(2, 'Suport a persones amb dificultats', 'Suport a persones amb dificultats o discapacitats', 1, 1, '2026-01-22 10:17:51.000000'),
	(3, 'Accions solidàries', 'Accions de solidaritat i ajuda comunitària', 1, 1, '2026-01-22 10:17:51.000000'),
	(4, 'Suport en activitats festives', 'Suport en activitats festives i celebracions', 2, 1, '2026-01-22 10:17:51.000000'),
	(5, 'Suport en activitats culturals', 'Suport en activitats culturals diverses', 2, 1, '2026-01-22 10:17:51.000000'),
	(6, 'Suport en activitats educatives', 'Suport en activitats educatives i formatives', 2, 1, '2026-01-22 10:17:51.000000'),
	(7, 'Suport en activitats de patrimoni', 'Suport en activitats de preservació del patrimoni', 2, 1, '2026-01-22 10:17:51.000000'),
	(8, 'Neteja de camins', 'Neteja i manteniment de camins i senders', 3, 1, '2026-01-22 10:17:51.000000'),
	(9, 'Activitats de sensibilització', 'Activitats de conscienciació ambiental', 3, 1, '2026-01-22 10:17:51.000000'),
	(10, 'Protecció del medi natural', 'Protecció i conservació del medi natural', 3, 1, '2026-01-22 10:17:51.000000'),
	(11, 'Colònies felines', 'Gestió i cura de colònies de gats', 3, 1, '2026-01-22 10:17:51.000000'),
	(12, 'Suport en activitats esportives d\'àmbit local', 'Suport en activitats esportives locals', 4, 1, '2026-01-22 10:17:51.000000');

INSERT INTO `volu_tipologies_voluntaris` (`Id`, `Nom`, `Descripcio`, `PotSerEnllacAjuntament`, `PotSerCoordinador`, `Actiu`, `DataCreacio`) VALUES
	(1, 'Coordinador General', 'Voluntari que coordina tots els altres voluntaris de l\'ens', 1, 1, 1, '2025-08-02 14:42:00'),
	(2, 'Coordinador d\'Àrea', 'Voluntari que coordina els voluntaris d\'una àrea específica', 0, 1, 1, '2025-08-02 14:42:00'),
	(3, 'Enllaç amb Ajuntament', 'Voluntari que fa d\'intermediari entre els voluntaris i l\'ajuntament', 1, 0, 1, '2025-08-02 14:42:00'),
	(4, 'Voluntari Base', 'Voluntari que participa en les activitats sense funcions de coordinació', 0, 0, 1, '2025-08-02 14:42:00'),
	(5, 'Voluntari Especialitzat', 'Voluntari amb coneixements específics per a tasques especialitzades', 0, 0, 1, '2025-08-02 14:42:00');


SET FOREIGN_KEY_CHECKS = 1;
