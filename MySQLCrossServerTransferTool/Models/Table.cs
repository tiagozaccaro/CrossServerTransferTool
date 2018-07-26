using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using MySQLCrossServerTransferTool.Commands;
using MySQLCrossServerTransferTool.Connectors;

namespace MySQLCrossServerTransferTool.Models
{
    public abstract class Table : ITableCommands, IDisposable, IEquatable<Table>
    {
        public string TableName { get; set; }
        public Column[] Columns { get; set; }
        public IConnector Connector { get; set; }
        public DataTable DataTable { get; set; }

        public Table(string sql, IConnector connector, bool IsSql)
        {
            Connector = connector;
            IDbCommand selectCommand;
            if (IsSql)
            {
                selectCommand = Connector.BuildCommand(sql);
            } else
            {
                TableName = sql;
                selectCommand = SelectCommand();
            }
            GetTableInfo(selectCommand);
        }

        public Table(string tableName)
        {
            TableName = tableName;
            GetTableInfo(SelectCommand());
        }

        private void GetTableInfo(IDbCommand command)
        {
            var dtSchema = new DataTable();

            using (var dr = command.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo))
            {
                dtSchema = dr.GetSchemaTable();
            }

            using (dtSchema)
            {
                if (dtSchema.Rows.Count > 0)
                {
                    TableName = dtSchema.Rows[0]["BaseTableName"].ToString();
                    Columns = ConvertToColumns(dtSchema.Rows);
                    LoadData(command);
                }
            }
        }

        public abstract void LoadData(IDbCommand command);

        private Column[] ConvertToColumns(DataRowCollection rows)
        {
            var columns = new Column[rows.Count];

            for (var x = 0; x < rows.Count; x++)
            {
                columns[x] = new Column(rows[x]);
            }

            return columns;
        }

        public abstract DbParameter[] GetParameters();
        public abstract IDbCommand CreateTableCommand();
        public abstract IDbCommand DropTableCommand();
        public abstract IDbCommand TruncateTableCommand();
        public abstract IDbCommand SelectCommand();
        public abstract IDbCommand SelectCommand(int limit);
        public abstract IDbCommand SelectCommand(int start, int limit);
        public abstract IDbCommand InsertCommand(bool onDuplicateUpdate = false);
        public abstract IDbCommand UpdateCommand();
        public abstract IDbCommand DeleteCommand();        
        public abstract IDbCommand CountCommand();
        public abstract IDbCommand CreateTemporaryTableCommand();
        public abstract IDbCommand DropTemporaryTableCommand();

        public void Dispose()
        {
            DataTable.Dispose();
        }

        public bool Equals(Table other)
        {
            return TableName == other.TableName && Enumerable.SequenceEqual(Columns, other.Columns);                   
        }
    }
}
