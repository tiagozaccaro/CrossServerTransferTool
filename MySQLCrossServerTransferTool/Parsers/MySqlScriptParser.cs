using System;
using System.IO;
using System.Threading;
using MySql.Data.MySqlClient;
using MySQLCrossServerTransferTool.Connectors;

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

            Thread.Sleep(10000);
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

                if(lineUPPER.StartsWith("EXEC ON"))
                {
                    if (lineUPPER.Contains("FROMDB"))
                    {
                        dbExecution = dbFrom;
                        Console.WriteLine($"Executing database changed to: FROMDB");
                    }
                    else if (lineUPPER.Contains("TODB"))
                    {
                        dbExecution = dbTo;
                        Console.WriteLine($"Executing database changed to: TODB");
                    }                    
                }
                else if (lineUPPER.StartsWith("SELECT"))
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
                else if(lineUPPER.Contains("PRINT"))
                {
                    Console.WriteLine(line.Substring(line.IndexOf("\"") + 1, line.LastIndexOf("\"") - (line.IndexOf("\"") + 1)).Trim());
                }
                else
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();

                    Console.WriteLine(line);
                    var rows = dbExecution.ExecuteNonQuery(line);

                    if (rows > 0)
                    {
                        Console.WriteLine($"Rows affected: {rows}");
                    }

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
            }
        }
    }
}
