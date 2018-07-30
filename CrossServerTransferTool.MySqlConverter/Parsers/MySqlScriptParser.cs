using System;
using System.Data;
using System.IO;
using Microsoft.Extensions.Logging;
using CrossServerTransferTool.MySqlConverter.Connectors;
using CrossServerTransferTool.SharedKernel.Parsers;
using CrossServerTransferTool.SharedKernel.Models;
using CrossServerTransferTool.SharedKernel.Plugins;
using CrossServerTransferTool.SharedKernel.Connectors;

namespace CrossServerTransferTool.MySqlConverter.Parsers
{
    [PluginName("MySql")]
    public class MySqlScriptParser : ScriptParser
    {
        public MySqlScriptParser(ProgramOptions options, ILoggerFactory loggerFactory) : base(options, loggerFactory)
        {
        }

        public override IConnector GetConnector(string connectionString, ILogger logger)
        {
            return new MySqlConnector(connectionString, Logger);
        }

        public override void ParseScript(IConnector dbFrom, IConnector dbTo)
        { 
            var dbExecution = dbFrom;

            var scripts = File.ReadAllText(GetScriptFile).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

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
                            Logger.LogInformation(method.Parameters[0].Replace("\'", "").Replace("\"", ""));

                            continue;
                        case "EXECON":
                            switch(method.Parameters[0])
                            {
                                case "'FROMDB'":
                                    dbExecution = dbFrom;
                                    Logger.LogInformation($"Executing database changed to: FROMDB");
                                    break;
                                case "'TODB'":
                                    dbExecution = dbTo;
                                    Logger.LogInformation($"Executing database changed to: TODB");
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

                    var converter = new Converters.MySqlConverter(dbFrom, dbTo, Logger);
                    converter.Convert(line);
                    
                    watch.Stop();
                    TimeSpan t = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);
                    string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                            t.Hours,
                                            t.Minutes,
                                            t.Seconds,
                                            t.Milliseconds);

                    Logger.LogInformation($"Execution Time Elapsed: {answer}");
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
                Parameters = lineUPPER.Substring(lineUPPER.IndexOf("(") + 1, lineUPPER.IndexOf(")") - (lineUPPER.IndexOf("(") + 1)).Split(':')
            };
        }
    }
}
