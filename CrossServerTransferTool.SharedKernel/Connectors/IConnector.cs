using System;
using System.Data;

namespace CrossServerTransferTool.SharedKernel.Connectors
{
    public interface IConnector : IDisposable
    {
        IDbConnection GetConnection();
        void Open();
        void BeginTransaction();
        void RollbackTransaction();
        void CommitTransaction();
        bool IsOpen();
        bool IsOnTransaction();
        IDbCommand BuildCommand(string sql, params IDbDataParameter[] parameters);
        int ExecuteNonQuery(IDbCommand command);
        IDataReader ExecuteReader(IDbCommand command);
        IDataAdapter ReturnDataAdapter(IDbCommand command);
        int ExecuteNonQuery(string sql, params IDbDataParameter[] parameters);
        IDataReader ExecuteReader(string sql, params IDbDataParameter[] parameters);
        IDataAdapter ReturnDataAdapter(string sql, params IDbDataParameter[] parameters);
    }
}
