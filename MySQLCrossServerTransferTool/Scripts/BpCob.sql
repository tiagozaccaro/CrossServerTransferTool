EXEC ON TODB;
USE BPCob;
DROP TEMPORARY TABLE IF EXISTS temp_BpCob;

PRINT("Cleaning data.");
CREATE TEMPORARY TABLE temp_BpCob (cdContrato INT);

FOREACH('SELECT cdBanco FROM cadBanco ORDER BY cdBanco asc' : 'INSERT INTO temp_BpCob SELECT cdContrato FROM cadContrato WHERE cdBanco = @cdBanco ORDER BY cdContrato Asc LIMIT 2000');

TRUNCATE TABLE tipofone;
TRUNCATE TABLE tipoendereco;
TRUNCATE TABLE tipoemail;
TRUNCATE TABLE cadferiado;

DELETE c.* FROM `cadcontrato` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `cadcontratoacordorecovery` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `cadcontratoativos` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `cadcontratobradescocomercial` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `cadcontratobradescoconsorcio` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `cadcontratobradescovarejo` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `cadcontratocetelem` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `cadcontratofidelizado` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `cadcontratomagazineluiza` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `caddadosamc` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `caddadosativos` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `caddadosbradesco` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `caddadosbradescocomercial` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `caddadosbradescoconsorcio` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `caddadosbv` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `caddadosnextel` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `caddadospanimobiliario` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `caddadosparcelavolvo` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `caddadosprocessobradescovarejo` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `caddadosrecovery` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `caddadossancor` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `caddadossantander` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `caddadosvolks` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `caddadosvolvo` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `cadfone` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `cadEndereco` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `cadEmail` c INNER JOIN `temp_BpCob` USING (cdContrato);
DELETE c.* FROM `cadParcela` c INNER JOIN `temp_BpCob` USING (cdContrato);

EXEC ON FROMDB;
USE BpCob;
DROP TEMPORARY TABLE IF EXISTS temp_BpCob;

PRINT("Filling temporary table with data.");
FOREACH('SELECT cdBanco FROM cadBanco ORDER BY cdBanco asc' : 'INSERT INTO temp_BpCob SELECT cdContrato FROM cadContrato WHERE cdBanco = @cdBanco ORDER BY cdContrato Desc LIMIT 2000');

SELECT * FROM tipofone;
SELECT * FROM tipoendereco;
SELECT * FROM tipoemail;
SELECT * FROM cadferiado;

SELECT c.* FROM `cadcontrato` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `cadcontratoacordorecovery` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `cadcontratoativos` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `cadcontratobradescocomercial` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `cadcontratobradescoconsorcio` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `cadcontratobradescovarejo` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `cadcontratocetelem` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `cadcontratofidelizado` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `cadcontratomagazineluiza` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `caddadosamc` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `caddadosativos` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `caddadosbradesco` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `caddadosbradescocomercial` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `caddadosbradescoconsorcio` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `caddadosbv` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `caddadosnextel` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `caddadospanimobiliario` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `caddadosparcelavolvo` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `caddadosprocessobradescovarejo` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `caddadosrecovery` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `caddadossancor` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `caddadossantander` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `caddadosvolks` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `caddadosvolvo` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `cadfone` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `cadEndereco` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `cadEmail` c INNER JOIN `temp_BpCob` USING (cdContrato);
SELECT c.* FROM `cadParcela` c INNER JOIN `temp_BpCob` USING (cdContrato);