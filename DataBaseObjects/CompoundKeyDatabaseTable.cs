using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace RFBCodeWorks.DataBaseObjects
{
    /// <summary>
    /// <inheritdoc cref="ICompoundKeyDataBaseTable"/>
    /// </summary>
    public class CompoundKeyDatabaseTable : DataBaseTable, ICompoundKeyDataBaseTable
    {
        /// <summary>
        /// Create a new <see cref="CompoundKeyDatabaseTable"/>
        /// </summary>
        /// <param name="compoundKeyColumns"><inheritdoc cref="CompoundKeyColumns" path="*"/> </param>
        /// <inheritdoc cref="DataBaseTable.DataBaseTable(IDatabase, string)"/>
        /// <param name="parent"/><param name="tableName"/>
        public CompoundKeyDatabaseTable(IDatabase parent, string tableName, params string[] compoundKeyColumns) : base(parent, tableName)
        {
            if (compoundKeyColumns is null) throw new ArgumentNullException(nameof(compoundKeyColumns));
            if (compoundKeyColumns.Length == 0) throw new ArgumentException("No column names have been specified!", nameof(compoundKeyColumns));
            foreach (string s in compoundKeyColumns)
            {
                if (string.IsNullOrWhiteSpace(s))
                    throw new ArgumentException("atleast one of the column names in the compoundKeyColumns parameter was null or empty!");
            }
            CompoundKeyColumns = compoundKeyColumns;
        }

        /// <inheritdoc/>
        public string[] CompoundKeyColumns { get; }

        /// <inheritdoc/>
        public int CompoundKeyColumnCount => CompoundKeyColumns.Length;

        /// <inheritdoc/>
        public DataRow GetDataRow(object[] CompoundKeyValues)
        {
            if (CompoundKeyValues.Length != CompoundKeyColumnCount)
                throw new ArgumentOutOfRangeException(nameof(CompoundKeyValues), $"Incorrect number of arguments received. Received {CompoundKeyValues.Length} values when expecting {CompoundKeyColumnCount} values");
            return Parent.GetDataRow(this.Select().Where(Extensions.ConvertToKeyValuePairArray(CompoundKeyColumns, CompoundKeyValues)));
        }

        /// <inheritdoc/>
        public Task<DataRow> GetDataRowAsync(object[] CompoundKeyValues)
        {
            return Task.Run(() => GetDataRow(CompoundKeyValues));
        }
    }
}
