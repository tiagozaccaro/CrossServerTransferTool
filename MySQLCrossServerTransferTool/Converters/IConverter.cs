
using MySQLCrossServerTransferTool.Models;

namespace MySQLCrossServerTransferTool.Converters
{
    public interface IConverter
    {
        void Convert(string Sql);         
    }
}
