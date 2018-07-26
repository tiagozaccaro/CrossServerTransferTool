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
        public MySqlTable(string tableName, DataRowCollection rows, IConnector connector) : base(tableName, rows, connector)
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

        public override IDbCommand InsertCommand()
        {
            var insert = $"Insert Into {TableName} ({String.Join(",", Columns.Select(c => $"`{c.Name}`").ToArray())}) Values ({String.Join(",", Columns.Select(c => $"@{c.Name}").ToArray())}) ON DUPLICATE KEY UPDATE {String.Join(",", Columns.Select(c => $"`{c.Name}` = Values({c.Name})").ToArray())};";
            var cmdInsert = Connector.BuildCommand(insert, GetParameters());
            cmdInsert.Prepare();
            return cmdInsert;
        }

        public override IDbCommand UpdateCommand()
        {
            throw new System.NotImplementedException();
        }

        public override IDbCommand DeleteCommand()
        {
            throw new System.NotImplementedException();
        }

        public override DbParameter[] GetParameters()
        {
            return Columns.Select(c => new MySqlParameter(c.Name, (MySqlDbType)c.ProviderType)).ToArray();
        }
    }
}
