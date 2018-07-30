EXECON('TODB');
USE Pan;

DROP TEMPORARY TABLE IF EXISTS temp_pan;
DROP TEMPORARY TABLE IF EXISTS temp_recebimento;

PRINT("Cleaning database.");
CREATE TEMPORARY TABLE temp_pan AS SELECT contrato FROM CONTRATO ORDER BY `DataImportacao` DESC LIMIT 2000;
DELETE c.* FROM `contrato` c INNER JOIN `temp_pan` USING (contrato);
DELETE c.* FROM `caddadospan` c INNER JOIN `temp_pan` USING (contrato);
DELETE c.* FROM `cadscore` c INNER JOIN `temp_pan` USING (contrato);
DELETE c.* FROM `parcela` c INNER JOIN `temp_pan` USING (contrato);
DELETE c.* FROM `fone` c INNER JOIN `temp_pan` USING (contrato);
DELETE c.* FROM `email` c INNER JOIN `temp_pan` USING (contrato);
DELETE c.* FROM `endereco` c INNER JOIN `temp_pan` USING (contrato);
DELETE c.* FROM `bemfinanciado` c INNER JOIN `temp_pan` USING (contrato);
DELETE c.* FROM `adicionais` c INNER JOIN `temp_pan` USING (contrato);
DELETE c.* FROM `avalista` c INNER JOIN `temp_pan` USING (contrato);
DELETE c.* FROM `cadproposta` c INNER JOIN `temp_pan` USING (contrato);
DELETE c.* FROM `cadacordopan` c INNER JOIN `temp_pan` USING (contrato);

-- Using Temporary to copy tables without main index.;
CREATE TEMPORARY TABLE temp_recebimento AS SELECT c.id FROM `recebimento` c INNER JOIN `temp_pan` USING (contrato);
DELETE c.* FROM `recebimento` c INNER JOIN `temp_recebimento` USING (id);
DELETE c.* FROM `recebimento_complemento` c INNER JOIN `temp_recebimento` t ON t.id = c.`idRecebimento`;
DELETE c.* FROM `recebimento_chqdevolvido` c INNER JOIN `temp_recebimento` t ON t.id = c.`idRecebimento`;
DELETE c.* FROM `recibo` c INNER JOIN `temp_recebimento` t ON t.id = c.`idRecebimento`;
DROP TEMPORARY TABLE IF EXISTS temp_recebimento;

TRUNCATE TABLE `produto`;

EXECON('FROMDB');
USE Pan;

DROP TEMPORARY TABLE IF EXISTS mytempdb.temp_pan;
DROP TEMPORARY TABLE IF EXISTS mytempdb.temp_recebimento;

PRINT("Filling temporary table with data.");
CREATE TEMPORARY TABLE mytempdb.temp_pan AS SELECT contrato FROM CONTRATO ORDER BY `DataImportacao` DESC LIMIT 2000;
SELECT c.* FROM `contrato` c INNER JOIN mytempdb.`temp_pan` USING (contrato);
SELECT c.* FROM `caddadospan` c INNER JOIN mytempdb.`temp_pan` USING (contrato);
SELECT c.* FROM `cadscore` c INNER JOIN mytempdb.`temp_pan` USING (contrato);
SELECT c.* FROM `parcela` c INNER JOIN mytempdb.`temp_pan` USING (contrato);
SELECT c.* FROM `fone` c INNER JOIN mytempdb.`temp_pan` USING (contrato);
SELECT c.* FROM `email` c INNER JOIN mytempdb.`temp_pan` USING (contrato);
SELECT c.* FROM `endereco` c INNER JOIN mytempdb.`temp_pan` USING (contrato);
SELECT c.* FROM `bemfinanciado` c INNER JOIN mytempdb.`temp_pan` USING (contrato);
SELECT c.* FROM `adicionais` c INNER JOIN mytempdb.`temp_pan` USING (contrato);
SELECT c.* FROM `avalista` c INNER JOIN mytempdb.`temp_pan` USING (contrato);
SELECT c.* FROM `cadproposta` c INNER JOIN mytempdb.`temp_pan` USING (contrato);
SELECT c.* FROM `cadacordopan` c INNER JOIN mytempdb.`temp_pan` USING (contrato);

-- Using Temporary to copy tables without main index.;
CREATE TEMPORARY TABLE mytempdb.temp_recebimento AS SELECT c.id FROM `recebimento` c INNER JOIN mytempdb.`temp_pan` USING (contrato);
SELECT c.* FROM `recebimento` c INNER JOIN mytempdb.`temp_recebimento` USING (id);
SELECT c.* FROM `recebimento_complemento` c INNER JOIN mytempdb.`temp_recebimento` t ON t.id = c.`idRecebimento`;
SELECT c.* FROM `recebimento_chqdevolvido` c INNER JOIN mytempdb.`temp_recebimento` t ON t.id = c.`idRecebimento`;
SELECT c.* FROM `recibo` c INNER JOIN mytempdb.`temp_recebimento` t ON t.id = c.`idRecebimento`;
DROP TEMPORARY TABLE IF EXISTS mytempdb.temp_recebimento;

SELECT * FROM `produto`;
