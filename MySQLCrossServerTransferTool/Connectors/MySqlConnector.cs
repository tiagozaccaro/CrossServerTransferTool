using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using MySQLCrossServerTransferTool.Models;
using System;
using System.Data;

namespace MySQLCrossServerTransferTool.Connectors
{
    public class MySqlConnector : IConnector, IDisposable
    {
        private MySqlTransaction _dbTransaction;
        private MySqlConnection _dbConnection;
        private readonly int _mysqlCommandTimeOut = 99999;
        private ILogger _logger;
        
        public MySqlConnector(ILogger logger)
        {
            _dbConnection = new MySqlConnection();
            _logger = logger;
        }

        public MySqlConnector(string connectionString, ILogger logger) : this(logger)
        {
            _dbConnection.ConnectionString = connectionString;
        }

        public void BeginTransaction()
        {
            if (!IsOnTransaction() && IsOpen())
            {
                _dbTransaction = _dbConnection.BeginTransaction();
            }
        }

        public void CommitTransaction()
        {
            if (IsOnTransaction())
            {
                _dbTransaction.Commit();
                _dbTransaction.Dispose();
                _dbTransaction = null;
            }
        }

        public bool IsOnTransaction()
        {
            return IsOpen() && !(_dbTransaction is null);
        }

        public bool IsOpen()
        {
            return _dbConnection.State == System.Data.ConnectionState.Open;
        }

        public void Open()
        {
            if (!IsOpen() && !(_dbConnection.ConnectionString == string.Empty))
            {
                _dbConnection.Open();

                this.ExecuteNonQuery($"SET wait_timeout = {_mysqlCommandTimeOut};");
                this.ExecuteNonQuery("SET foreign_key_checks = 0;");
                this.ExecuteNonQuery("SET unique_checks = 0;");
            }
        }

        public void RollbackTransaction()
        {
            if(IsOnTransaction())
            {
                _dbTransaction.Rollback();
                _dbTransaction.Dispose();
                _dbTransaction = null;
            }
        }

        public void Dispose()
        {
            if (IsOnTransaction())
            {
                _dbTransaction.Dispose();
            }

            if(IsOpen())
            {
                _dbConnection.Close();
            }

            _dbConnection.Dispose();
        }

        public int ExecuteNonQuery(string sql, params IDbDataParameter[] parameters)
        {
            return ExecuteNonQuery(BuildCommand(sql, parameters));
        }
        
        public IDataAdapter ReturnDataAdapter(string sql, params IDbDataParameter[] parameters)
        {
            return ReturnDataAdapter(BuildCommand(sql, parameters));            
        }

        public IDataReader ExecuteReader(string sql, params IDbDataParameter[] parameters)
        {
            return ExecuteReader(BuildCommand(sql, parameters));
        }

        public IDbCommand BuildCommand(string sql, params IDbDataParameter[] parameters)
        {
            if (IsOpen())
            {
                MySqlCommand command;

                if (IsOnTransaction())
                {
                    command = new MySqlCommand(sql, _dbConnection, _dbTransaction);
                }
                else
                {
                    command = new MySqlCommand(sql, _dbConnection);
                }

                if (parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                command.CommandTimeout = _mysqlCommandTimeOut;

                return command;
            }
            else
            {
                throw new Exception("Connection not Open.");
            }
        }

        public int ExecuteNonQuery(IDbCommand command)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            _logger.LogInformation(command.CommandText);

            var rows = command.ExecuteNonQuery();

            if (rows > 0)
            {
                _logger.LogInformation($"Rows affected: {rows}");
            }

            watch.Stop();
            TimeSpan t = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);
            string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds);

            _logger.LogInformation($"Execution Time Elapsed: {answer}");
            
            return rows;
        }

        public IDataReader ExecuteReader(IDbCommand command)
        {
            return command.ExecuteReader();
        }

        public IDataAdapter ReturnDataAdapter(IDbCommand command)
        {
            var da = new MySqlDataAdapter((MySqlCommand)command);
            return da;
        }

        public IDbConnection GetConnection()
        {
            return _dbConnection;
        }

        public Table GetTable(string sql, params IDbDataParameter[] parameters)
        {
            return GetTable(BuildCommand(sql, parameters));
        }

        public Table GetTable(IDbCommand command)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var dtSchema = new DataTable();

            using (var dr = command.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo))
            {
                dtSchema = dr.GetSchemaTable();
            }

            using (dtSchema)
            {
                string tableName = string.Empty;

                if (dtSchema.Rows.Count > 0)
                {
                    tableName = dtSchema.Rows[0]["BaseTableName"].ToString();
                }

                var table = new MySqlTable(tableName, dtSchema.Rows, this);

                watch.Stop();
                TimeSpan t = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);
                string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                        t.Hours,
                                        t.Minutes,
                                        t.Seconds,
                                        t.Milliseconds);

                _logger.LogInformation($"Get Table Elapsed: {answer}");
                return table;
            }
        }
    }
}
