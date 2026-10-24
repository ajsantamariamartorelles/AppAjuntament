-- Dumping structure for table acceptacions_termes
-- Acceptació dels termes i condicions i la política de privacitat (Web +
-- app de Cursets). Vegeu Models/Base/AcceptacioTermes/AcceptacioTermes.cs
-- i Services/AcceptacioTermesService.cs.
CREATE TABLE IF NOT EXISTS `acceptacions_termes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Email` varchar(191) NOT NULL,
  `VersioAcceptada` int(11) NOT NULL,
  `DataAcceptacio` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_acceptacions_termes_Email` (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Acceptació dels termes i condicions i la política de privacitat, per correu';
