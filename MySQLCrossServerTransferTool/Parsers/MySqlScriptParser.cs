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
            var scripts = File.ReadAllText(_scriptFile).Replace('\n', ' ').Replace('\r', ' ').Split(';');

            foreach (var script in scripts)
            {
                if (script.Trim().Equals(string.Empty)) continue;

                if (script.Trim().ToUpper().StartsWith("SELECT"))
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();

                    var table = dbFrom.GetTable(script);
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
                else if (script.Trim().ToUpper().StartsWith("DELETE") || script.Trim().ToUpper().StartsWith("TRUNCATE"))
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();

                    Console.WriteLine("Deleting content from table...");
                    dbTo.ExecuteNonQuery(script);

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
                else if(script.Trim().ToUpper().Contains("PRINT"))
                {
                    Console.WriteLine(script.Substring(script.IndexOf("\"") + 1, script.LastIndexOf("\"") - (script.IndexOf("\"") + 1)).Trim());
                }
                else
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();

                    Console.WriteLine(script.Trim());
                    var rows = dbFrom.ExecuteNonQuery(script);

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
