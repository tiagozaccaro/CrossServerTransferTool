EXECON('TODB');
USE Itaucred;
DROP TEMPORARY TABLE IF EXISTS temp_itaucred;

PRINT('Cleaning data.');
CREATE TEMPORARY TABLE temp_itaucred (cod_oper VARCHAR(5), contrato VARCHAR(16));
FOREACH('SELECT cod FROM PRODUTOS ORDER BY cod asc' : 'INSERT INTO temp_itaucred SELECT cod_oper, contrato FROM CONTRATOS WHERE emp = @cod ORDER BY id ASC LIMIT 1000');

TRUNCATE TABLE `_codparecer`;
DELETE c.* FROM `contratos` c INNER JOIN `temp_itaucred` USING (cod_oper, contrato);
DELETE f.* FROM `fonesadicionais` f INNER JOIN `temp_itaucred` USING (cod_oper, contrato);
DELETE e.* FROM `contrato_enderecos` e INNER JOIN `temp_itaucred` USING (cod_oper, contrato);
DELETE e.* FROM `enderecos` e INNER JOIN `temp_itaucred` USING (cod_oper, contrato);
DELETE p.* FROM `parcelas` p INNER JOIN `temp_itaucred` USING (cod_oper, contrato);
DELETE r.* FROM `recibo` r INNER JOIN `temp_itaucred` USING (cod_oper, contrato);
DELETE e.* FROM `contrato_emails` e INNER JOIN `temp_itaucred` USING (cod_oper, contrato);

EXECON('FROMDB');
USE Itaucred;
DROP TEMPORARY TABLE IF EXISTS temp_itaucred;

PRINT('Filling temporary table with data.');
CREATE TEMPORARY TABLE temp_itaucred (cod_oper VARCHAR(5), contrato VARCHAR(16));
FOREACH('SELECT cod FROM PRODUTOS ORDER BY cod asc' : 'INSERT INTO temp_itaucred SELECT cod_oper, contrato FROM CONTRATOS WHERE emp = @cod ORDER BY id DESC LIMIT 1000');

SELECT * FROM `_codparecer`;
SELECT c.* FROM `contratos` c INNER JOIN `temp_itaucred` USING (cod_oper, contrato);
SELECT f.* FROM `fonesadicionais` f INNER JOIN `temp_itaucred` USING (cod_oper, contrato);
SELECT e.* FROM `contrato_enderecos` e INNER JOIN `temp_itaucred` USING (cod_oper, contrato);
SELECT e.* FROM `enderecos` e INNER JOIN `temp_itaucred` USING (cod_oper, contrato);
SELECT p.* FROM `parcelas` p INNER JOIN `temp_itaucred` USING (cod_oper, contrato);
SELECT r.* FROM `recibo` r INNER JOIN `temp_itaucred` USING (cod_oper, contrato);
SELECT e.* FROM `contrato_emails` e INNER JOIN `temp_itaucred` USING (cod_oper, contrato);