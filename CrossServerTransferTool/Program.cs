using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using CrossServerTransferTool.SharedKernel.Models;
using CrossServerTransferTool.SharedKernel.Plugins;
using CrossServerTransferTool.SharedKernel.Parsers;

namespace CrossServerTransferTool
{
    class Program
    {
        public static ServiceProvider ServiceProvider { get; set; }
        private static ILogger _logger;
        
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var options = ProcessArguments(args);
                        
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, options);

            var pluginManager = new PluginManager();
            pluginManager.LoadPlugins();
            var scriptParser = pluginManager.GetPluginType(options.Engine);
            if (scriptParser != null)
            {
                serviceCollection.AddSingleton(scriptParser);
            }
            else
            {
                _logger.LogError("Engine not found!");
                Environment.Exit(-1);
            }

            ServiceProvider = serviceCollection.BuildServiceProvider();

            _logger = ServiceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();

            _logger.LogInformation("MySQL Cross Server Transfer Tool 1.0.2");

            serviceCollection.AddSingleton(_logger);

            ((ScriptParser)ServiceProvider.GetService(scriptParser)).ExecuteScript();

            _logger.LogInformation("All done!");
        }

        private static void ConfigureServices(IServiceCollection serviceCollection, ProgramOptions options)
        {
            //setup our DI
            serviceCollection.AddSingleton(new LoggerFactory()
                .AddConsole(LogLevel.Information));

            serviceCollection.AddSingleton(options);            
            serviceCollection.AddLogging();            
        }

        private static ProgramOptions ProcessArguments(string[] args)
        {
            var programOptions = new ProgramOptions();

            for (var x = 0; x < args.Length; x++)
            {
                var argument = string.Empty;

                switch (args[x])
                {
                    case "--from":
                        argument = GetNextArgument(args, x);

                        if (argument == null)
                        {
                            _logger.LogCritical("From parameter does not have a connection string.");
                            Environment.Exit(-1);
                        }

                        try
                        {
                            programOptions.FromConnectionString = argument;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("From parameter does not have a connection string.", ex);
                            Environment.Exit(-1);
                        }
                        break;

                    case "--to":
                        argument = GetNextArgument(args, x);

                        if (argument == null)
                        {
                            _logger.LogCritical("To parameter does not have a connection string.");
                            Environment.Exit(-1);
                        }

                        try
                        {
                            programOptions.ToConnectionString = argument;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("To parameter does not have a connection string.", ex);
                            Environment.Exit(-1);
                        }
                        break;

                    case "--script":
                        argument = GetNextArgument(args, x);

                        if (argument == null)
                        {
                            _logger.LogCritical("Script does not have a valid script file.");
                            Environment.Exit(-1);
                        }

                        if (!File.Exists(argument) || Path.GetExtension(argument).ToLower() == "sql")
                        {
                            _logger.LogCritical("Script does not exist or is not a script.");
                            Environment.Exit(-1);
                        }

                        programOptions.ScriptFile = argument;
                        break;

                    case "--engine":
                        argument = GetNextArgument(args, x);

                        if (argument == null)
                        {
                            _logger.LogCritical("Engine not informed.");
                            Environment.Exit(-1);
                        }

                        switch (argument)
                        {
                            case "MySql":
                                break;
                            default:
                                _logger.LogCritical("Engine not supported.");
                                Environment.Exit(-1);
                                break;
                        }

                        programOptions.Engine = argument;
                        break;

                    default:
                        break;
                }
            }

            return programOptions;
        }

        private static string GetNextArgument(string[] args, int x)
        {
            if (args.Length < x)
            {
                return null;
            }

            return args[x + 1];
        }
    }
}
