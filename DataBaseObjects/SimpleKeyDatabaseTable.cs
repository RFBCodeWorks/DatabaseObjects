using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace RFBCodeWorks.DataBaseObjects
{
    /// <summary>
    /// <inheritdoc cref="ISimpleKeyDataBaseTable"/>
    /// </summary>
    public class SimpleKeyDatabaseTable : DataBaseTable, ISimpleKeyDataBaseTable
    {
        /// <summary>
        /// Create a new <see cref="SimpleKeyDatabaseTable"/>
        /// </summary>
        /// <param name="primaryKey"><inheritdoc cref="PrimaryKey" path="*"/> </param>
        /// <inheritdoc cref="DataBaseTable.DataBaseTable(IDatabase, string)"/>
        /// <param name="parent"/><param name="tableName"/>
        public SimpleKeyDatabaseTable(IDatabase parent, string tableName, string primaryKey) : base(parent, tableName)
        {
            if (string.IsNullOrWhiteSpace(primaryKey)) throw new ArgumentException("primaryKey parameter is null or empty!", nameof(primaryKey));
            PrimaryKey = primaryKey;
        }
         
        /// <inheritdoc/>
        public string PrimaryKey { get; }

        #region < DataReturn >

        /// <remarks> All DataReturn routines that exist within <see cref="DataBaseTable"/> utilize this method, so error logging the database request can have one central location </remarks>
        /// <inheritdoc/>
        public virtual object GetValue(object primaryKeyValue, string returnColName) => Parent.GetValue(TableName, PrimaryKey, primaryKeyValue, returnColName);

        /// <inheritdoc/>
        public virtual bool? GetValueAsBool(object primaryKeyValue, string returnColName) => GetValue(primaryKeyValue, returnColName).SanitizeToBool();

        /// <inheritdoc/>
        public virtual string GetValueAsString(object primaryKeyValue, string returnColName) => GetValue(primaryKeyValue, returnColName).SanitizeToString();

        /// <inheritdoc/>
        public virtual int? GetValueAsInt(object primaryKeyValue, string returnColName) => GetValue(primaryKeyValue, returnColName).SanitizeToInt();

        #region < Async Methods >

        /// <inheritdoc/>
        public virtual Task<object> GetValueAsync(object primaryKeyValue, string returnColName) => Task.Run(() => GetValue(primaryKeyValue, returnColName));

        /// <inheritdoc/>
        public virtual Task<bool?> GetValueAsBoolAsync(object primaryKeyValue, string returnColName) => Task.Run(() => GetValueAsBool(primaryKeyValue, returnColName));

        /// <inheritdoc/>
        public virtual Task<string> GetValueAsStringAsync(object primaryKeyValue, string returnColName) => Task.Run(() => GetValueAsString(primaryKeyValue, returnColName));

        /// <inheritdoc/>
        public virtual Task<int?> GetValueAsIntAsync(object primaryKeyValue, string returnColName) => Task.Run(() => GetValueAsInt(primaryKeyValue, returnColName));

        #endregion
        #endregion

        #region < GetDataRow >
        
        /// <inheritdoc/>
        public virtual DataRow GetDataRow(object primaryKeyValue)
        {
            return Parent.GetDataRow(this.Select().Where(PrimaryKey, primaryKeyValue));
        }

        /// <inheritdoc/>
        public virtual Task<DataRow> GetDataRowAsync(object primaryKeyValue)
        {
            return Task.Run(() => GetDataRow(primaryKeyValue));
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
            if (columns is null || columns.Length == 0) return new string[] { };
            var list = columns.ToList();
            list.Remove(PrimaryKey);
            list.Insert(0, PrimaryKey);
            return list.ToArray();
        }

    }

}
