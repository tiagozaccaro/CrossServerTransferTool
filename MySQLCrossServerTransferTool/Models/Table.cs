using MySql.Data.MySqlClient;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MySQLCrossServerTransferTool.Models
{
    public class Table
    {
        public IDbCommand SelectCommand { get; private set; }
        public IDbCommand CreateCommand { get; private set; }
        public string TableName { get; set; }
        public Column[] Columns { get; set; }
        
        public Table(string tableName, IDbCommand selectCommand, IDbCommand createCommand, DataRowCollection rows)
        {
            SelectCommand = selectCommand;
            CreateCommand = createCommand;
            TableName = tableName;

            var columns = new Column[rows.Count];

            for (var x = 0; x < rows.Count; x++)
            {
                columns[x] = new Column(rows[x]);
            }

            Columns = columns;
        }

        public MySqlParameter[] GetParameters()
        {
            return Columns.Select(c => new MySqlParameter(c.Name, (MySqlDbType)c.ProviderType)).ToArray();
        }
    }
}
