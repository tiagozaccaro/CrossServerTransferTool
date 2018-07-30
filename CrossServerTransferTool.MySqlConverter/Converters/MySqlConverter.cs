using CrossServerTransferTool.MySqlConverter.Models;
using CrossServerTransferTool.SharedKernel.Connectors;
using CrossServerTransferTool.SharedKernel.Converters;
using CrossServerTransferTool.SharedKernel.Models;
using Microsoft.Extensions.Logging;

namespace CrossServerTransferTool.MySqlConverter.Converters
{
    public class MySqlConverter : Converter
    {
        public MySqlConverter(IConnector dbFrom, IConnector dbTo, ILogger logger) : base(dbFrom, dbTo, logger)
        {
        }

        protected override Table GetTable(IConnector connector)
        {
            return new MySqlTable(connector);
        }

        protected override Table GetTable(string tableName, IConnector connector)
        {
            return new MySqlTable(tableName, connector);
        }
    }
}
