using System;
using System.Data;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using MySQLCrossServerTransferTool.Extensions;
using MySQLCrossServerTransferTool.Models;

namespace MySQLCrossServerTransferTool.Converters
{
    public class MySqlConverter : IConverter
    {
        private readonly int _queryLimit = 1000;
        private ILogger _logger;

        public MySqlConverter(ILogger logger)
        {
            _logger = logger;
        }

        public void Convert(Table from, Table to)
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                if (!to.Equals(from))
                {
                    to.DropTableCommand().ExecuteNonQuery();
                    var createCommand = from.CreateTableCommand();
                    createCommand.Connection = to.Connector.GetConnection();
                    createCommand.ExecuteNonQuery();
                }

                int pag = 0;

                using (var insertCommand = from.InsertCommand())
                {
                    insertCommand.Connection = to.Connector.GetConnection();

                    do
                    {
                        var watchPag = System.Diagnostics.Stopwatch.StartNew();

                        using (var dr = from.SelectCommand(pag * _queryLimit, _queryLimit).ExecuteReader())
                        {
                            using (var dt = new DataTable())
                            {
                                dt.Load(dr);

                                if (dt.Rows.Count == 0)
                                {
                                    break;
                                }

                                _logger.LogInformation($"{dt.TableName} Page: {pag + 1}");

                                try
                                {
                                    to.Connector.BeginTransaction();

                                    int prg = 1;

                                    try
                                    {
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            if (row.ItemArray.Length == insertCommand.Parameters.Count)
                                            {
                                                for (var x = 0; x < row.ItemArray.Length; x++)
                                                {
                                                    ((MySqlParameter)insertCommand.Parameters[x]).Value = row.ItemArray[x];
                                                }

                                                ConsoleHelper.DrawTextProgressBar(prg++, dt.Rows.Count);

                                                insertCommand.ExecuteNonQuery();
                                            }
                                        }
                                    } catch(Exception ex)
                                    {
                                        throw ex;
                                    }

                                    Console.WriteLine();

                                    to.Connector.CommitTransaction();
                                }
                                catch (Exception ex)
                                {
                                    to.Connector.RollbackTransaction();

                                    throw new Exception($"Exception on insert into TO Database on Table: {from.TableName}", ex);
                                }
                            }
                        }

                        watchPag.Stop();
                        TimeSpan tpag = TimeSpan.FromMilliseconds(watchPag.ElapsedMilliseconds);
                        string answerPag = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                                tpag.Hours,
                                                tpag.Minutes,
                                                tpag.Seconds,
                                                tpag.Milliseconds);

                        _logger.LogInformation($"Page Elapsed: {answerPag}");

                        pag++;
                    } while (true);
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
