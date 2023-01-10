using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RFBCodeWorks.DataBaseObjects
{
    /// <summary>
    /// <inheritdoc cref="ICompositeKeyTable"/>
    /// </summary>
    public class CompositeKeyTable : DataBaseTable, ICompositeKeyTable
    {
        /// <summary>
        /// Create a new <see cref="CompositeKeyTable"/>
        /// </summary>
        /// <param name="compoundKeyColumns"><inheritdoc cref="CompositeKeyColumns" path="*"/> </param>
        /// <inheritdoc cref="DataBaseTable.DataBaseTable(IDatabase, string)"/>
        /// <param name="parent"/><param name="tableName"/>
        public CompositeKeyTable(IDatabase parent, string tableName, params string[] compoundKeyColumns) : base(parent, tableName)
        {
            if (compoundKeyColumns is null) throw new ArgumentNullException(nameof(compoundKeyColumns));
            if (compoundKeyColumns.Length == 0) throw new ArgumentException("No column names have been specified!", nameof(compoundKeyColumns));
            foreach (string s in compoundKeyColumns)
            {
                if (string.IsNullOrWhiteSpace(s))
                    throw new ArgumentException("atleast one of the column names in the compoundKeyColumns parameter was null or empty!");
            }
            CompositeKeyColumns = compoundKeyColumns;
        }

        /// <inheritdoc/>
        public string[] CompositeKeyColumns { get; }

        /// <inheritdoc/>
        public int CompositeColumnCount => CompositeKeyColumns.Length;

        /// <inheritdoc/>
        public DataRow GetDataRow(object[] CompoundKeyValues)
        {
            if (CompoundKeyValues.Length != CompositeColumnCount)
                throw new ArgumentOutOfRangeException(nameof(CompoundKeyValues), $"Incorrect number of arguments received. Received {CompoundKeyValues.Length} values when expecting {CompositeColumnCount} values");
            return Parent.GetDataRow(this.Select().Where(CompositeKeyColumns.CreateKeyValuePairs(CompoundKeyValues)));
        }

        /// <inheritdoc/>
        public Task<DataRow> GetDataRowAsync(object[] CompoundKeyValues, CancellationToken cancellationToken =default)
        {
            if (CompoundKeyValues.Length != CompositeColumnCount)
                throw new ArgumentOutOfRangeException(nameof(CompoundKeyValues), $"Incorrect number of arguments received. Received {CompoundKeyValues.Length} values when expecting {CompositeColumnCount} values");
            return Parent.GetDataRowAsync(this.Select().Where(CompositeKeyColumns.CreateKeyValuePairs(CompoundKeyValues)));
        }
    }
}
