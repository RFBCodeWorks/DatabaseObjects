using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseObjects
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
            if (primaryKey.IsNullOrEmpty()) throw new ArgumentException("primaryKey parameter is null or empty!");
            PrimaryKey = primaryKey;
        }
         
        /// <inheritdoc/>
        public string PrimaryKey { get; }

        #region < DataReturn >

        /// <remarks> All DataReturn routines that exist within <see cref="AbstractDataBaseTable{T}"/> utilize this method, so error logging the database request can have one central location </remarks>
        /// <inheritdoc/>
        public virtual object GetValue(object PrimaryKeyValue, string ReturnColName) => Parent.GetValue(TableName, PrimaryKey, PrimaryKeyValue, ReturnColName);

        /// <inheritdoc/>
        public virtual bool GetValueAsBool(object PrimaryKeyValue, string ReturnColName) => GetValue(PrimaryKeyValue, ReturnColName).ConvertToBool();

        /// <inheritdoc/>
        public virtual string GetValueAsString(object PrimaryKeyValue, string ReturnColName) => GetValue(PrimaryKeyValue, ReturnColName).ConvertToString();

        /// <inheritdoc/>
        public virtual int GetValueAsInt(object PrimaryKeyValue, string ReturnColName) => GetValue(PrimaryKeyValue, ReturnColName).ConvertToInt();

        #region < Async Methods >

        /// <inheritdoc/>
        public virtual Task<object> GetValueAsync(object PrimaryKeyValue, string ReturnColName) => Task.Run(() => GetValue(PrimaryKeyValue, ReturnColName));

        /// <inheritdoc/>
        public virtual Task<bool> GetValueAsBoolAsync(object PrimaryKeyValue, string ReturnColName) => Task.Run(() => GetValueAsBool(PrimaryKeyValue, ReturnColName));

        /// <inheritdoc/>
        public virtual Task<string> GetValueAsStringAsync(object PrimaryKeyValue, string ReturnColName) => Task.Run(() => GetValueAsString(PrimaryKeyValue, ReturnColName));

        /// <inheritdoc/>
        public virtual Task<int> GetValueAsIntAsync(object PrimaryKeyValue, string ReturnColName) => Task.Run(() => GetValueAsInt(PrimaryKeyValue, ReturnColName));

        #endregion
        #endregion

        #region < GetDataRow >
        
        /// <inheritdoc/>
        public virtual DataRow GetDataRow(object PrimaryKeyValue)
        {
            return Parent.GetDataRow(this.Select().Where(PrimaryKey, PrimaryKeyValue));
        }

        /// <inheritdoc/>
        public virtual Task<DataRow> GetDataRowAsync(object PrimaryKeyValue)
        {
            return Task.Run(() => GetDataRow(PrimaryKeyValue));
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
                    int key = ObjectExtensions.ConvertToInt(Rdr.GetValue(0));
                    if (Rdr.IsDBNull(1)) { val = string.Empty; } else { val = Rdr.GetValue(1).ConvertToString(); }
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
        /// <param name="columns"></param>
        /// <returns></returns>
        protected virtual string[] AddPrimaryKeyToArray(string[] columns)
        {
            if (columns is null || columns.Length == 0) return columns ?? new string[] { };
            if (columns.Contains(PrimaryKey)) return columns;
            return new string[] { PrimaryKey }.AddRange(columns);
        }

    }

}
