using CrossServerTransferTool.SharedKernel.Connectors;
using CrossServerTransferTool.SharedKernel.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace CrossServerTransferTool.SharedKernel.Parsers
{    
    public abstract class ScriptParser
    {
        private readonly ProgramOptions _options;
        public ILogger Logger { get; }
        public string GetScriptFile
        {
            get
            {
                return _options.ScriptFile;
            }
        }

        public ScriptParser(ProgramOptions options, ILoggerFactory loggerFactory)
        {
            _options = options;
            Logger = loggerFactory.CreateLogger<ScriptParser>();
        }

        public abstract IConnector GetConnector(string connectionString, ILogger logger);

        public abstract void ParseScript(IConnector dbFrom, IConnector dbTo);

        public void ExecuteScript()
        {
            Logger.LogInformation("Start Copying Data...");
            Logger.LogInformation($"Script Name: {Path.GetFileName(_options.ScriptFile)}");

            using (var dbFrom = GetConnector(_options.FromConnectionString, Logger))
            {
                using (var dbTo = GetConnector(_options.ToConnectionString, Logger))
                {
                    try
                    {
                        var watch = System.Diagnostics.Stopwatch.StartNew();

                        dbFrom.Open();
                        dbTo.Open();

                        ParseScript(dbFrom, dbTo);

                        // the code that you want to measure comes here
                        watch.Stop();
                        TimeSpan t = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);
                        string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                                t.Hours,
                                                t.Minutes,
                                                t.Seconds,
                                                t.Milliseconds);

                        Logger.LogDebug($"Time Elapsed: {answer}");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error on the execution.");
                        Console.ReadKey();
                    }
                }
            }
        }
    }
}
