using MySQLCrossServerTransferTool.Parsers;
using System;
using System.IO;
using System.Text;

namespace MySQLCrossServerTransferTool
{
    class Program
    {
        private static string _dbFromConnectionString;
        private static string _dbToConnectionString;
        private static string _scriptFile;
        private static string _engine;

        static void Main(string[] args)
        {
            for (var x = 0; x < args.Length; x++)
            {
                var argument = string.Empty;

                switch (args[x])
                {
                    case "--from":
                        argument = GetNextArgument(args, x);

                        if (argument == null)
                        {
                            Console.WriteLine("From parameter does not have a connection string.");
                            Environment.Exit(-1);
                        }

                        try
                        {
                            _dbFromConnectionString = argument;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("From parameter does not have a connection string.");
                            Console.WriteLine(ex.ToString());
                            Environment.Exit(-1);
                        }
                        break;

                    case "--to":
                        argument = GetNextArgument(args, x);

                        if (argument == null)
                        {
                            Console.WriteLine("To parameter does not have a connection string.");
                            Environment.Exit(-1);
                        }

                        try
                        {
                            _dbToConnectionString = argument;
                        } catch(Exception ex)
                        {
                            Console.WriteLine("To parameter does not have a connection string.");
                            Console.WriteLine(ex.ToString());
                            Environment.Exit(-1);
                        }
                        break;

                    case "--script":
                        argument = GetNextArgument(args, x);

                        if (argument == null)
                        {
                            Console.WriteLine("Script does not have a valid script file.");
                            Environment.Exit(-1);
                        }

                        _scriptFile = argument;

                        if (!File.Exists(_scriptFile) || Path.GetExtension(_scriptFile).ToLower() == "sql")
                        {
                            Console.WriteLine("Script does not exist or is not a script.");
                            Environment.Exit(-1);
                        }

                        break;

                    case "--engine":
                        argument = GetNextArgument(args, x);

                        if (argument == null)
                        {
                            Console.WriteLine("Engine not informed.");
                            Environment.Exit(-1);
                        }

                        _engine = argument;

                        switch(_engine)
                        {
                            case "MySql":
                                break;
                            default:
                                Console.WriteLine("Engine not supported.");
                                Environment.Exit(-1);
                                break;
                        }

                        break;

                    default:
                        break;
                }
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var scriptParser = new MySqlScriptParser(_dbFromConnectionString, _dbToConnectionString, _scriptFile);
            scriptParser.ExecuteScript();
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
