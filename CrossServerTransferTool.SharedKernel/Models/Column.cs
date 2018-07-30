using System;
using System.Collections.Generic;
using System.Data;

namespace CrossServerTransferTool.SharedKernel.Models
{
    public class Column : IEquatable<Column>
    {        
        public Column(DataRow dataRow)
        {
            for(var x = 0; x < dataRow.ItemArray.Length; x++)
            {
                var content = dataRow.ItemArray[x];

                switch(x)
                {
                    case 0:
                        Name = Convert.IsDBNull(content) ? null : (string)content;
                        break;
                    case 1:
                        ColumnOrdinal = Convert.IsDBNull(content) ? null : (int?)content;
                        break;
                    case 2:
                        ColumnSize = Convert.IsDBNull(content) ? null : (int?)content;
                        break;
                    case 3:
                        NumericPrecision = Convert.IsDBNull(content) ? null : (int?)content;
                        break;
                    case 4:
                        NumericScale = Convert.IsDBNull(content) ? null : (int?)content;
                        break;
                    case 5:
                        IsUnique = Convert.IsDBNull(content) ? null : (bool?)content;
                        break;
                    case 6:
                        IsKey = Convert.IsDBNull(content) ? null : (bool?)content;
                        break;
                    case 7:
                        BaseCatalogName = Convert.IsDBNull(content) ? null : (string)content;
                        break;
                    case 8:
                        BaseColumnName = Convert.IsDBNull(content) ? null : (string)content;
                        break;
                    case 9:
                        BaseTableName = Convert.IsDBNull(content) ? null : (string)content;
                        break;
                    case 10:
                        BaseSchemaName = Convert.IsDBNull(content) ? null : (string)content;
                        break;
                    case 11:
                        DataType = Convert.IsDBNull(content) ? null : (System.Type)content;
                        break;
                    case 12:
                        AllowDBNull = Convert.IsDBNull(content) ? null : (bool?)content;
                        break;
                    case 13:
                        ProviderType = Convert.IsDBNull(content) ? null : (int?)content;
                        break;
                    case 14:
                        IsAliased = Convert.IsDBNull(content) ? null : (bool?)content;
                        break;
                    case 15:
                        IsExpression = Convert.IsDBNull(content) ? null : (bool?)content;
                        break;
                    case 16:
                        IsIdentity = Convert.IsDBNull(content) ? null : (bool?)content;
                        break;
                    case 17:
                        IsAutoIncrement = Convert.IsDBNull(content) ? null : (bool?)content;
                        break;
                    case 18:
                        IsRowVersion = Convert.IsDBNull(content) ? null : (bool?)content;
                        break;
                    case 19:
                        IsHidden = Convert.IsDBNull(content) ? null : (bool?)content;
                        break;
                    case 20:
                        IsLong = Convert.IsDBNull(content) ? null : (bool?)content;
                        break;
                    case 21:
                        IsReadOnly = Convert.IsDBNull(content) ? null : (bool?)content;
                        break;
                }
            }
        }

        public string Name { get; set; }
        public int? ColumnOrdinal { get; set; }
        public int? ColumnSize { get; set; }
        public int? NumericPrecision { get; set; }
        public int? NumericScale { get; set; }
        public bool? IsUnique { get; set; }
        public bool? IsKey { get; set; }
        public string BaseCatalogName { get; set; }
        public string BaseColumnName { get; set; }
        public string BaseSchemaName { get; set; }
        public string BaseTableName { get; set; }
        public Type DataType { get; set; }
        public bool? AllowDBNull { get; set; }
        public int? ProviderType { get; set; }
        public bool? IsAliased { get; set; }
        public bool? IsExpression { get; set; }
        public bool? IsIdentity { get; set; }
        public bool? IsAutoIncrement { get; set; }
        public bool? IsRowVersion { get; set; }
        public bool? IsHidden { get; set; }
        public bool? IsLong { get; set; }
        public bool? IsReadOnly { get; set; }
        
        public bool Equals(Column other)
        {
            return Name == other.Name &&
                   BaseTableName == other.BaseTableName &&
                   ProviderType.Equals(other.ProviderType);
        }
    }
}
