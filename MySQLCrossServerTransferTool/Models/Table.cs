using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using MySQLCrossServerTransferTool.Commands;
using MySQLCrossServerTransferTool.Connectors;

namespace MySQLCrossServerTransferTool.Models
{
    public abstract class Table : ITableCommands
    {
        public string TableName { get; set; }
        public Column[] Columns { get; set; }
        public IConnector Connector { get; set; }
        
        public Table(string tableName, DataRowCollection rows, IConnector connector)
        {            
            TableName = tableName;

            var columns = new Column[rows.Count];

            for (var x = 0; x < rows.Count; x++)
            {
                columns[x] = new Column(rows[x]);
            }

            Columns = columns;

            Connector = connector;
        }

        public abstract DbParameter[] GetParameters();
        public abstract IDbCommand CreateTableCommand();
        public abstract IDbCommand DropTableCommand();
        public abstract IDbCommand TruncateTableCommand();
        public abstract IDbCommand SelectCommand();
        public abstract IDbCommand SelectCommand(int limit);
        public abstract IDbCommand SelectCommand(int start, int limit);
        public abstract IDbCommand InsertCommand();
        public abstract IDbCommand UpdateCommand();
        public abstract IDbCommand DeleteCommand();

        public override bool Equals(object obj)
        {
            return obj is Table table &&
                   TableName == table.TableName &&
                   EqualityComparer<Column[]>.Default.Equals(Columns, table.Columns);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TableName, Columns);
        }
    }
}
