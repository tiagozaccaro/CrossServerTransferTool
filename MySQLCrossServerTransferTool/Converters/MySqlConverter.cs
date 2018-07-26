using System;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using MySQLCrossServerTransferTool.Connectors;
using MySQLCrossServerTransferTool.Extensions;
using MySQLCrossServerTransferTool.Models;

namespace MySQLCrossServerTransferTool.Converters
{
    public class MySqlConverter : IConverter
    {
        private readonly int _queryLimit = 1000;
        private readonly IConnector _dbFrom;
        private readonly IConnector _dbTo;
        private ILogger _logger;

        public MySqlConverter(IConnector dbFrom, IConnector dbTo, ILogger logger)
        {
            _dbFrom = dbFrom;
            _dbTo = dbTo;
            _logger = logger;
        }

        public void Convert(string Sql)
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                using (var from = new MySqlTable(Sql, _dbFrom, true))
                {
                    var to = new MySqlTable(from.TableName, _dbTo, false);
                    if (!to.Equals(from))
                    {
                        to.DropTableCommand().ExecuteNonQuery();
                        var createCommand = from.CreateTableCommand();
                        createCommand.Connection = to.Connector.GetConnection();
                        createCommand.ExecuteNonQuery();
                        to.Dispose();
                        to = new MySqlTable(from.TableName, _dbTo, false);
                    }

                    using (var insertCommand = to.InsertCommand(true))
                    {   
                        _logger.LogInformation($"{from.TableName}");

                        try
                        {
                            to.Connector.BeginTransaction();

                            for (int x = 0; x < from.DataTable.Rows.Count; x++)
                            {
                                if (from.DataTable.Rows[x].ItemArray.Length == insertCommand.Parameters.Count)
                                {
                                    for (var y = 0; y < from.DataTable.Rows[x].ItemArray.Length; y++)
                                    {
                                        ((MySqlParameter)insertCommand.Parameters[y]).Value = from.DataTable.Rows[x].ItemArray[y];
                                    }

                                    ConsoleHelper.DrawTextProgressBar(x + 1, from.DataTable.Rows.Count);

                                    insertCommand.ExecuteNonQuery();
                                }
                            }

                            to.Connector.CommitTransaction();
                        }
                        catch (Exception ex)
                        {
                            to.Connector.RollbackTransaction();
                            throw new Exception($"Exception on insert into TO Database on Table: {from.TableName}", ex);
                        }                         
                    }

                    to.Dispose();
                }

                watch.Stop();
                TimeSpan t = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);
                string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                        t.Hours,
                                        t.Minutes,
                                        t.Seconds,
                                        t.Milliseconds);

                _logger.LogInformation($"Copy Elapsed: {answer}");
            } catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
