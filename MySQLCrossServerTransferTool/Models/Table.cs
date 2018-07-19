using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MySQLCrossServerTransferTool.Models
{
    public class Table
    {
        public IDbCommand SelectCommand { get; private set; }
        public IDbDataParameter[] Parameters { get; private set; }
        public string TableName { get; set; }
        
        public Table(string tableName, IDbCommand selectCommand, IDbDataParameter[] parameters)
        {
            SelectCommand = selectCommand;
            Parameters = parameters;
            TableName = tableName;
        }
    }
}
