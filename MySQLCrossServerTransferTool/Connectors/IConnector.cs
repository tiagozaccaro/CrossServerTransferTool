using MySQLCrossServerTransferTool.Models;
using System.Data;

namespace MySQLCrossServerTransferTool.Connectors
{
    public interface IConnector
    {
        IDbConnection GetConnection();
        void Open();
        void BeginTransaction();
        void RollbackTransaction();
        void CommitTransaction();
        bool IsOpen();
        bool IsOnTransaction();
        Table GetTable(string sql, params IDbDataParameter[] parameters);
        Table GetTable(IDbCommand command);
        IDbCommand BuildCommand(string sql, params IDbDataParameter[] parameters);
        int ExecuteNonQuery(IDbCommand command);
        IDataReader ExecuteReader(IDbCommand command);
        IDataAdapter ReturnDataAdapter(IDbCommand command);
        int ExecuteNonQuery(string sql, params IDbDataParameter[] parameters);
        IDataReader ExecuteReader(string sql, params IDbDataParameter[] parameters);
        IDataAdapter ReturnDataAdapter(string sql, params IDbDataParameter[] parameters);
    }
}
