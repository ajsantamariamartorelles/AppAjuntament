-- Imatge de portada pròpia de la pàgina d'Inici (fitxer src/Web/wwwroot/img/municipi/portada.jpg).
-- Per a BD ja existents (no hi ha migracions). No canvia l'estructura: només omple imatges.VistaURL.
-- L'app dóna prioritat a les imatges de la taula imatges sobre les de l'API de la Diputació, de manera
-- que la pàgina es veu igual tant si la Diputació respon com si no.
--
-- 1) Mira primer si el municipi ja té fila a imatges:
--      SELECT e.Id, e.Nom, e.ImatgeId FROM ens e WHERE e.INE6 = '082567' OR e.CodiEns IN ('825670005', '08256');
--    - Si ImatgeId té valor  -> executa només l'OPCIÓ A.
--    - Si ImatgeId és NULL   -> executa només l'OPCIÓ B (les dues sentències, seguides).

-- (A ens, CodiEns pot guardar el codi de la Seu-e o el de la Diputació segons com s'hagi importat; es cobreixen tots dos.)
--
-- OPCIÓ A: la fila d'imatges ja existeix (només s'omple la vista; escut i bandera no es toquen).
UPDATE imatges i
  JOIN ens e ON e.ImatgeId = i.Id
   SET i.VistaURL = '/img/municipi/portada.jpg'
 WHERE e.INE6 = '082567' OR e.CodiEns IN ('825670005', '08256');

-- OPCIÓ B: el municipi no té fila a imatges: se'n crea una i s'enllaça.
-- INSERT INTO imatges (VistaURL) VALUES ('/img/municipi/portada.jpg');
-- UPDATE ens SET ImatgeId = LAST_INSERT_ID()
--  WHERE (INE6 = '082567' OR CodiEns IN ('825670005', '08256')) AND ImatgeId IS NULL;
