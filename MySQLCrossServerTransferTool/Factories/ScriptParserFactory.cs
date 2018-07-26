using Microsoft.Extensions.Logging;
using MySQLCrossServerTransferTool.Models;
using MySQLCrossServerTransferTool.Parsers;

namespace MySQLCrossServerTransferTool.Factories
{
    public class ScriptParserFactory
    {
        private ProgramOptions _options;

        public ScriptParserFactory(ProgramOptions options)
        {
            _options = options;
        }

        public IScriptParser GetInstance(ILogger logger)
        {
            IScriptParser scriptParser = null;

            switch (_options.Engine)
            {
                case "MySql":
                default:
                    scriptParser = new MySqlScriptParser(_options, logger);
                    break;
            }

            return scriptParser;
        }
    }
}
