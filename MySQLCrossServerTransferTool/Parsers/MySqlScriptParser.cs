using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using MySql.Data.MySqlClient;
using MySQLCrossServerTransferTool.Connectors;
using MySQLCrossServerTransferTool.Models;

namespace MySQLCrossServerTransferTool.Parsers
{
    public class MySqlScriptParser
    {
        private readonly string _dbFromConnectionString;
        private readonly string _dbToConnectionString;
        private readonly string _scriptFile;

        public MySqlScriptParser(string dbFromConnectionString, string dbToConnectionString, string scriptFile)
        {
            _dbFromConnectionString = dbFromConnectionString;
            _dbToConnectionString = dbToConnectionString;
            _scriptFile = scriptFile;
        }

        public void ExecuteScript()
        {
            Console.WriteLine("MySQL Cross Server Transfer Tool 1.0.1");
            Console.WriteLine("Start Copying Data...");
            Console.WriteLine($"Script Name: {Path.GetFileName(_scriptFile)}");
            Console.WriteLine();

            using (var dbFrom = new MySqlConnector(_dbFromConnectionString))
            {
                using (var dbTo = new MySqlConnector(_dbToConnectionString))
                {
                    try
                    {
                        var watch = System.Diagnostics.Stopwatch.StartNew();

                        dbFrom.Open();
                        dbTo.Open();

                        this.ExecuteScriptFile(dbFrom, dbTo);
                        
                        // the code that you want to measure comes here
                        watch.Stop();
                        TimeSpan t = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);
                        string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                                t.Hours,
                                                t.Minutes,
                                                t.Seconds,
                                                t.Milliseconds);

                        Console.WriteLine($"Time Elapsed: {answer}");
                    }
                    catch (Exception ex)
                    {                        
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine(String.Empty);
                    }
                }
            }

            Console.WriteLine("Finished.");

            Console.ReadKey();
        }

        private void ExecuteScriptFile(MySqlConnector dbFrom, MySqlConnector dbTo)
        {
            var dbExecution = dbFrom;

            var scripts = File.ReadAllText(_scriptFile).Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var script in scripts)
            {
                var line = script.Trim();
                var lineUPPER = line.ToUpper();

                if (line.Equals(string.Empty)) continue;

                var method = GetMethodAndParameters(lineUPPER);

                if (method != null)
                {
                    switch(method.Name)
                    {
                        case "PRINT":
                            Console.WriteLine(method.Parameters[0].Replace("\'", "").Replace("\"", ""));

                            continue;
                        case "EXECON":
                            switch(method.Parameters[0])
                            {
                                case "'FROMDB'":
                                    dbExecution = dbFrom;
                                    Console.WriteLine($"Executing database changed to: FROMDB");
                                    break;
                                case "'TODB'":
                                    dbExecution = dbTo;
                                    Console.WriteLine($"Executing database changed to: TODB");
                                    break;
                            }

                            continue;
                        case "FOREACH":

                            using (var dr = dbExecution.ExecuteReader(method.Parameters[0].Replace("\'", "").Replace("\"", "")))
                            {
                                using (var dt = new DataTable())
                                {
                                    dt.Load(dr);

                                    for (var x = 0; x < dt.Rows.Count; x++)
                                    {
                                        dbExecution.ExecuteNonQuery(method.Parameters[1].Trim().Substring(1, method.Parameters[1].Trim().Length - 2).Replace($"@{dt.Columns[0].ColumnName}", dt.Rows[x][0].ToString()));
                                    }
                                }
                            }

                            continue;
                    }
                }

                if (lineUPPER.StartsWith("SELECT"))
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();

                    var table = dbExecution.GetTable(line);
                    dbTo.Copy(table);

                    watch.Stop();
                    TimeSpan t = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);
                    string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                            t.Hours,
                                            t.Minutes,
                                            t.Seconds,
                                            t.Milliseconds);

                    Console.WriteLine($"Execution Time Elapsed: {answer}");
                    Console.WriteLine();
                }  
                else
                {
                    
                    var rows = dbExecution.ExecuteNonQuery(line);                    
                }
            }
        }

        private Method GetMethodAndParameters(string lineUPPER)
        {
            if (lineUPPER.IndexOf("(") == -1 || lineUPPER.IndexOf(")") == -1)
            {
                return null;
            }

            return new Method
            {
                Name = lineUPPER.Substring(0, lineUPPER.IndexOf("(")),
                Parameters = lineUPPER.Substring(lineUPPER.IndexOf("(") + 1, lineUPPER.IndexOf(")") - (lineUPPER.IndexOf("(") + 1)).Split(":")
            };
        }
    }
}
