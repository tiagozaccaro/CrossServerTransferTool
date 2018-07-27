using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using MySQLCrossServerTransferTool.Commands;
using MySQLCrossServerTransferTool.Connectors;

namespace MySQLCrossServerTransferTool.Models
{
    public abstract class Table : ITableCommands, IEquatable<Table>
    {
        public string TableName { get; set; }
        public Column[] Columns { get; set; }
        public IConnector Connector { get; set; }

        public Table(IConnector connector)
        {
            Connector = connector;            
        }

        public Table(string tableName, IConnector connector)
        {
            Connector = connector;
            TableName = tableName;
            RefreshTableInfo();
        }

        public void RefreshTableInfo()
        {
            using (var selectCommand = SelectCommand())
            {
                RefreshTableInfo(selectCommand);
            }                
        }

        public void RefreshTableInfo(string sql)
        {
            using (var selectCommand = Connector.BuildCommand(sql))
            {
                RefreshTableInfo(selectCommand);
            }
        }

        public void RefreshTableInfo(IDbCommand selectCommand)
        {
            using (var dr = selectCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo))
            {
                using (var dtSchema = dr.GetSchemaTable())
                {
                    if (dtSchema.Rows.Count > 0)
                    {
                        TableName = dtSchema.Rows[0]["BaseTableName"].ToString();
                        Columns = ConvertToColumns(dtSchema.Rows);
                    }
                }
            }
        }

        public Table(string tableName)
        {
            TableName = tableName;
            RefreshTableInfo();
        }

        private Column[] ConvertToColumns(DataRowCollection rows)
        {
            var columns = new Column[rows.Count];

            for (var x = 0; x < rows.Count; x++)
            {
                columns[x] = new Column(rows[x]);
            }

            return columns;
        }

        public bool Equals(Table other)
        {
            return TableName == other.TableName && Enumerable.SequenceEqual(Columns, other.Columns);
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
    }
}
