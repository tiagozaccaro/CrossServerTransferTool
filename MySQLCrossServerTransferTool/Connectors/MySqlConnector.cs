using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using System.Text;
using MySQLCrossServerTransferTool.Extensions;
using MySQLCrossServerTransferTool.Models;

namespace MySQLCrossServerTransferTool.Connectors
{
    public class MySqlConnector : IConnector, IDisposable
    {
        private MySqlTransaction _dbTransaction;
        private MySqlConnection _dbConnection;
        private readonly int _mysqlCommandTimeOut = 99999;
        private readonly int _queryLimit = 1000;
        
        public MySqlConnector()
        {
            _dbConnection = new MySqlConnection();
        }

        public MySqlConnector(string connectionString) : this()
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

                this.ExecuteNonQuery($"set wait_timeout = {_mysqlCommandTimeOut};");
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
            return CreateCommand(sql, parameters).ExecuteNonQuery();
        }

        public void Copy(Table copy)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            CheckTable(copy);

            var dbFromSelectCommand = copy.SelectCommand;
            dbFromSelectCommand.CommandText += " LIMIT @page, @limit;";
            dbFromSelectCommand.Parameters.Add(new MySqlParameter("@page", MySqlDbType.Int32));
            dbFromSelectCommand.Parameters.Add(new MySqlParameter("@limit", MySqlDbType.Int32));
            dbFromSelectCommand.Prepare();

            string insert = string.Empty;
            MySqlCommand cmdInsert = null;

            int pag = 0;

            do
            {
                var watchPag = System.Diagnostics.Stopwatch.StartNew();

                ((MySqlParameter)dbFromSelectCommand.Parameters["@page"]).Value = pag * _queryLimit;
                ((MySqlParameter)dbFromSelectCommand.Parameters["@limit"]).Value = _queryLimit;

                using (var dr = dbFromSelectCommand.ExecuteReader())
                {
                    using (var dt = new DataTable())
                    {
                        dt.Load(dr);

                        if (dt.Rows.Count == 0)
                        {
                            break;
                        }

                        Console.WriteLine($"{dt.TableName} Page: {pag + 1}");

                        if (insert == string.Empty)
                        {
                            insert = $"Insert Into {dt.TableName} ({String.Join(",", dt.Columns.Cast<DataColumn>().Select(c => $"`{c.ColumnName}`").ToArray())}) Values ({String.Join(",", dt.Columns.Cast<DataColumn>().Select(c => $"@{c.ColumnName}").ToArray())}) ON DUPLICATE KEY UPDATE {String.Join(",", dt.Columns.Cast<DataColumn>().Select(c => $"`{c.ColumnName}` = Values({c.ColumnName})").ToArray())};";
                            cmdInsert = CreateCommand(insert, copy.GetParameters());
                            cmdInsert.Prepare();
                        }

                        try
                        {
                            this.BeginTransaction();

                            int prg = 1;

                            foreach (DataRow row in dt.Rows)
                            {
                                if (row.ItemArray.Count() == cmdInsert.Parameters.Count)
                                {
                                    for (var x = 0; x < row.ItemArray.Count(); x++)
                                    {
                                        cmdInsert.Parameters[x].Value = row.ItemArray[x];
                                    }

                                    ConsoleHelper.DrawTextProgressBar(prg++, dt.Rows.Count);

                                    cmdInsert.ExecuteNonQuery();
                                }
                            }

                            this.CommitTransaction();
                        }
                        catch (Exception ex)
                        {
                            this.RollbackTransaction();

                            throw new Exception($"Exception on insert into TO Database on Table: {copy.TableName}", ex);
                        }
                    }
                }

                Console.WriteLine();

                watchPag.Stop();
                TimeSpan tpag = TimeSpan.FromMilliseconds(watchPag.ElapsedMilliseconds);
                string answerPag = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                        tpag.Hours,
                                        tpag.Minutes,
                                        tpag.Seconds,
                                        tpag.Milliseconds);

                Console.WriteLine($"Page Elapsed: {answerPag}");
                Console.WriteLine();
                
                pag++;
            } while (true);

            if (cmdInsert != null)
            {
                cmdInsert.Dispose();
            }

            watch.Stop();
            TimeSpan t = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);
            string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds);

            Console.WriteLine($"Copy Elapsed: {answer}");
            Console.WriteLine();            
        }

        private MySqlCommand CreateCommand(string sql, params IDbDataParameter[] parameters)
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

        private void CheckTable(Table table)
        {
            var tableCompare = GetTable($"SELECT * FROM {table.TableName} LIMIT 1");

            for(var x = 0; x < table.Columns.Count(); x++)
            {
                if (!table.Columns[x].Equals(tableCompare.Columns[x]))
                {
                    using (var dropTableCommand = CreateCommand($"DROP TABLE IF EXISTS {tableCompare.TableName};"))
                    {
                        dropTableCommand.ExecuteNonQuery();
                    }

                    var createCommand = table.CreateCommand;
                    createCommand.Connection = _dbConnection;
                    createCommand.ExecuteNonQuery();
                    break;
                }
            }
        }

        public Table GetTable(string sqlSelect, params IDbDataParameter[] parameters)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var selectCommand = CreateCommand(sqlSelect, parameters);

            var dtSchema = new DataTable();

            using (var dr = selectCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo))
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

                MySqlCommand createCommand = null;

                using (var showCreateCommand = CreateCommand($"SHOW CREATE TABLE {tableName};"))
                {                    
                    using (var dr = showCreateCommand.ExecuteReader())
                    {
                        using (var dt = new DataTable())
                        {
                            dt.Load(dr);

                            if (dt.Rows.Count > 0)
                            {
                                createCommand = CreateCommand((string)dt.Rows[0][1]);
                            }
                        }
                    }
                }

                var table = new Table(tableName, selectCommand, createCommand, dtSchema.Rows);

                watch.Stop();
                TimeSpan t = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);
                string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                        t.Hours,
                                        t.Minutes,
                                        t.Seconds,
                                        t.Milliseconds);

                Console.WriteLine($"Get Table Elapsed: {answer}");
                Console.WriteLine();
                return table;
            }
        }

        public IDataAdapter ReturnDataAdapter(string sql, params IDbDataParameter[] parameters)
        {
            var command = CreateCommand(sql, parameters);

            var da = new MySqlDataAdapter(command);

            return da;
        }
    }
}
