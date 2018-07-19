using MySQLCrossServerTransferTool.Models;
using System.Data;

namespace MySQLCrossServerTransferTool.Connectors
{
    public interface IConnector
    {
        void Open();
        void BeginTransaction();
        void RollbackTransaction();
        void CommitTransaction();
        bool IsOpen();
        bool IsOnTransaction();
        int ExecuteNonQuery(string sql, params IDbDataParameter[] parameters);
        Table GetTable(string sqlSelect, params IDbDataParameter[] parameters);
        void Copy(Table copy);
        IDataAdapter ReturnDataAdapter(string sql, params IDbDataParameter[] parameters);           
    }
}
