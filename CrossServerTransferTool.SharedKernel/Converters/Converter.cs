
using CrossServerTransferTool.SharedKernel.Connectors;
using CrossServerTransferTool.SharedKernel.Extensions;
using CrossServerTransferTool.SharedKernel.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;

namespace CrossServerTransferTool.SharedKernel.Converters
{
    public abstract class Converter
    {
        private readonly IConnector _dbFrom;
        private readonly IConnector _dbTo;
        private ILogger _logger;

        public Converter(IConnector dbFrom, IConnector dbTo, ILogger logger)
        {
            _dbFrom = dbFrom;
            _dbTo = dbTo;
            _logger = logger;
        }

        protected abstract Table GetTable(IConnector connector);
        protected abstract Table GetTable(string tableName, IConnector connector);

        public void Convert(string query)
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                var from = GetTable(_dbFrom);
                from.RefreshTableInfo(query);

                var to = GetTable(from.TableName, _dbTo);

                if (!to.Equals(from))
                {
                    using (var dropTableCommand = to.DropTableCommand())
                    {
                        dropTableCommand.ExecuteNonQuery();
                    }

                    using (var createCommand = from.CreateTableCommand())
                    {
                        createCommand.Connection = to.Connector.GetConnection();
                        createCommand.ExecuteNonQuery();
                    }

                    to.RefreshTableInfo();
                }

                using (var insertCommand = to.InsertCommand(true))
                {
                    _logger.LogInformation($"{from.TableName}");

                    try
                    {
                        to.Connector.BeginTransaction();

                        int rows = 0;

                        using (var dr = _dbFrom.ExecuteReader(query))
                        {
                            while (dr.Read())
                            {
                                if (dr.FieldCount == insertCommand.Parameters.Count)
                                {
                                    for (var column = 0; column < dr.FieldCount; column++)
                                    {
                                        ((DbParameter)insertCommand.Parameters[column]).Value = dr.GetValue(column);
                                    }

                                    ConsoleHelper.DrawTextProgress("Rows transferred", ++rows, ConsoleColor.DarkRed);

                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                        }

                        ConsoleHelper.DrawTextProgress("Rows transferred", rows, ConsoleColor.DarkBlue);

                        Console.WriteLine();

                        to.Connector.CommitTransaction();
                    }
                    catch (Exception ex)
                    {
                        to.Connector.RollbackTransaction();
                        throw new Exception($"Exception on insert into TO Database on Table: {from.TableName}", ex);
                    }
                }

                watch.Stop();
                TimeSpan t = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);
                string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                        t.Hours,
                                        t.Minutes,
                                        t.Seconds,
                                        t.Milliseconds);

                _logger.LogInformation($"Copy Elapsed: {answer}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
