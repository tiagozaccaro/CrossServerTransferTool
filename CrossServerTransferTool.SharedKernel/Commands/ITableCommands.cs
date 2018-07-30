using CrossServerTransferTool.SharedKernel.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CrossServerTransferTool.SharedKernel.Commands
{
    public interface ITableCommands
    {
        IDbCommand CreateTableCommand();
        IDbCommand DropTableCommand();
        IDbCommand TruncateTableCommand();
        IDbCommand SelectCommand();
        IDbCommand SelectCommand(int limit);
        IDbCommand SelectCommand(int start, int limit);
        IDbCommand InsertCommand(bool onDuplicateUpdate = false);
        IDbCommand UpdateCommand();
        IDbCommand DeleteCommand();
        IDbCommand CountCommand();
        IDbCommand CreateTemporaryTableCommand();
        IDbCommand DropTemporaryTableCommand();
    }
}
