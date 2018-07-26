using System.Linq;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.Data;
using MySQLCrossServerTransferTool.Connectors;
using System;

namespace MySQLCrossServerTransferTool.Models
{
    public class MySqlTable : Table
    {        
        public MySqlTable(string sql, IConnector connector, bool IsSql) : base(sql, connector, IsSql)
        {
        }

        public override IDbCommand CreateTableCommand()
        {
            using (var dr = Connector.ExecuteReader($"SHOW CREATE TABLE {TableName};"))
            {
                using (var dt = new DataTable())
                {
                    dt.Load(dr);

                    if (dt.Rows.Count > 0 && dt.Columns.Count == 2)
                    {
                        return Connector.BuildCommand((string)dt.Rows[0][1]);
                    }
                }
            }

            return null;
        }

        public override IDbCommand DropTableCommand()
        {
            return Connector.BuildCommand($"DROP TABLE IF EXISTS {TableName}");
        }

        public override IDbCommand TruncateTableCommand()
        {
            return Connector.BuildCommand($"TRUNCATE TABLE IF EXISTS {TableName}");
        }

        public override IDbCommand SelectCommand()
        {
            return Connector.BuildCommand($"SELECT * FROM {TableName}");
        }

        public override IDbCommand SelectCommand(int limit)
        {
            var selectCommand = SelectCommand();
            selectCommand.CommandText += $" LIMIT {limit}";
            return selectCommand;
        }

        public override IDbCommand SelectCommand(int start, int limit)
        {
            var selectCommand = SelectCommand();
            selectCommand.CommandText += $" LIMIT {start}, {limit}";
            return selectCommand;
        }

        public override IDbCommand InsertCommand(bool onDuplicateUpdate = false)
        {
            var onDuplicateUpdateSql = string.Empty;

            if (onDuplicateUpdate)
            {
                onDuplicateUpdateSql = $"ON DUPLICATE KEY UPDATE {String.Join(",", Columns.Select(c => $"`{c.Name}` = Values({c.Name})").ToArray())}";
            }

            var insert = $"Insert Into {TableName} ({String.Join(",", Columns.Select(c => $"`{c.Name}`").ToArray())}) Values ({String.Join(",", Columns.Select(c => $"@{c.Name}").ToArray())}) {onDuplicateUpdateSql};";
            var cmdInsert = Connector.BuildCommand(insert, GetParameters());
            cmdInsert.Prepare();
            return cmdInsert;
        }

        public override IDbCommand UpdateCommand()
        {
            var update = $"Update {TableName} {String.Join(",", Columns.Select(c => $"`{c.Name}` = Values({c.Name})").ToArray())};";
            var cmdUpdate = Connector.BuildCommand(update, GetParameters());
            cmdUpdate.Prepare();
            return cmdUpdate;
        }

        public override IDbCommand DeleteCommand()
        {
            return Connector.BuildCommand($"DELETE * FROM {TableName};");
        }

        public override IDbCommand CountCommand()
        {
            return Connector.BuildCommand($"SELECT COUNT(*) FROM {TableName};");
        }

        public override IDbCommand CreateTemporaryTableCommand()
        {
            return Connector.BuildCommand($"CREATE TEMPORARY TABLE Temp_{TableName};");
        }

        public override IDbCommand DropTemporaryTableCommand()
        {
            return Connector.BuildCommand($"DROP TEMPORARY TABLE IF EXISTS Temp_{TableName};");
        }

        public override DbParameter[] GetParameters()
        {
            return Columns.Select(c => new MySqlParameter(c.Name, (MySqlDbType)c.ProviderType)).ToArray();
        }

        public override void LoadData(IDbCommand command)
        {
            if (DataTable != null)
            {
                DataTable.Dispose();
            }

            DataTable = new DataTable();

            using (var _dataAdapter = new MySqlDataAdapter((MySqlCommand)command))
            {
                _dataAdapter.Fill(DataTable);
            }
        }
    }
}
