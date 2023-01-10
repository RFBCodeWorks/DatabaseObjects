using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Common;
using SqlKata;

namespace RFBCodeWorks.DataBaseObjects
{
    /// <summary>
    /// <inheritdoc cref="IPrimaryKeyTable"/>
    /// </summary>
    public class PrimaryKeyTable : DataBaseTable, IPrimaryKeyTable
    {
        /// <summary>
        /// Create a new <see cref="PrimaryKeyTable"/>
        /// </summary>
        /// <param name="primaryKey"><inheritdoc cref="PrimaryKey" path="*"/> </param>
        /// <inheritdoc cref="DataBaseTable.DataBaseTable(IDatabase, string)"/>
        /// <param name="parent"/><param name="tableName"/>
        public PrimaryKeyTable(IDatabase parent, string tableName, string primaryKey) : base(parent, tableName)
        {
            if (string.IsNullOrWhiteSpace(primaryKey)) throw new ArgumentException("primaryKey parameter is null or empty!", nameof(primaryKey));
            PrimaryKey = primaryKey;
        }
         
        /// <inheritdoc/>
        public string PrimaryKey { get; }

        #region < GetValue >

        /// <remarks> All DataReturn routines that exist within <see cref="DataBaseTable"/> utilize this method, so error logging the database request can have one central location </remarks>
        /// <inheritdoc/>
        public virtual object GetValue(object primaryKeyValue, string returnColName) => GetValue(PrimaryKey, primaryKeyValue, returnColName);

        /// <inheritdoc/>
        public virtual Task<object> GetValueAsync(object primaryKeyValue, string returnColName, CancellationToken cancellationToken = default) => GetValueAsync(PrimaryKey, primaryKeyValue, returnColName, cancellationToken);

        #endregion

        #region < GetDataRow >
        
        /// <inheritdoc/>
        public virtual DataRow GetDataRow(object primaryKeyValue)
        {
            return Parent.GetDataRow(this.Select().Where(PrimaryKey, primaryKeyValue));
        }

        /// <inheritdoc/>
        public virtual Task<DataRow> GetDataRowAsync(object primaryKeyValue, CancellationToken cancellationToken = default)
        {
            return Parent.GetDataRowAsync(this.Select().Where(PrimaryKey, primaryKeyValue), cancellationToken);
        }


        #endregion

        /// <inheritdoc/>
        public virtual Dictionary<int, string> GetDictionary(string valueColumn)
        {
            var tbl = GetDataTable(PrimaryKey, valueColumn);
            if (tbl is null) throw new Exception("Failed to retrieve DataTable from Database - Cannot construct Dictionary");
            var Dict = new Dictionary<int, string>();
            using (DataTableReader Rdr = tbl.CreateDataReader())
            {
                //Populate the first collection in dictionary with table information
                //Dict.Add("ColumnNames", Tbl.Columns[KeyColumn].ColumnName + "|" + Tbl.Columns[ValueColumn].ColumnName);
                //populate the dictionary
                while (Rdr.Read())
                {
                    string val;
                    int key = (int)Extensions.SanitizeToInt(Rdr.GetValue(0));
                    if (Rdr.IsDBNull(1)) { val = string.Empty; } else { val = Rdr.GetValue(1).ToString(); }
                    Dict.Add(key, val);
                }
            }
            return Dict;
        }

       /// <inheritdoc/>
        public virtual Dictionary<X, Y> GetDictionary<X, Y>(Func<DataTable, Dictionary<X, Y>> dictionaryBuilder, params string[] valueColumns)
        {
            return dictionaryBuilder(GetDataTable(AddPrimaryKeyToArray(valueColumns)));
        }


        #region < Update >

        /// <param name="primaryKey">The value to search for within the PrimaryKey column</param>
        /// <inheritdoc cref="DataBaseTable.Update(IEnumerable{KeyValuePair{string, object}}, SqlKata.Extensions.IWhereCondition[])"/>
        /// <inheritdoc cref="Update(object, KeyValuePair{string, object}[])"/>
        /// <param name="values"/>
        public virtual int Update(object primaryKey, params KeyValuePair<string, object>[] values)
        {
            return Update(values, new RFBCodeWorks.SqlKata.Extensions.WhereColumnValue(PrimaryKey, primaryKey));
        }

        /// <inheritdoc cref="DataBaseTable.UpdateAsync(IEnumerable{KeyValuePair{string, object}}, CancellationToken, SqlKata.Extensions.IWhereCondition[])"/>
        /// <inheritdoc cref="Update(object, KeyValuePair{string, object}[])"/>
        public virtual Task<int> UpdateAsync(object primaryKey, CancellationToken cancellationToken = default, params KeyValuePair<string, object>[] values)
        {
            return UpdateAsync(values, cancellationToken, new RFBCodeWorks.SqlKata.Extensions.WhereColumnValue(PrimaryKey, primaryKey));
        }

        #endregion


        /// <summary>
        /// Sanitizes input columns to ensure that the primary key is the first item in the list
        /// </summary>
        /// <param name="columns">
        /// The list of columns to include. 
        /// </param>
        /// <returns>
        /// If the <paramref name="columns"/> array is null or empty, SqlKata will default to selecting all columns, so an empty array will be returned in that scenario.
        /// <br/>Otherwise insert the <see cref="PrimaryKey"/> as the first item in the list, then return it as a new array.
        /// </returns>
        protected virtual string[] AddPrimaryKeyToArray(string[] columns)
        {
            if (columns is null || columns.Length == 0) return Array.Empty<string>();
            var list = columns.ToList();
            list.Remove(PrimaryKey);
            list.Insert(0, PrimaryKey);
            return list.ToArray();
        }

    }

}
