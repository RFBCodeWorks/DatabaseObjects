using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Text;

namespace DataBaseObjects
{
    /// <summary>
    /// Factory to create DataBase Tables specific to the a database
    /// </summary>
    public class DataBaseTableFactory
    {
        /// <summary>
        /// Instantiate the factory object
        /// </summary>
        /// <param name="database"><inheritdoc cref="DataBaseReference" path="*"/></param>
        public DataBaseTableFactory(IDatabase database)
        {
            DataBaseReference = database ?? throw new ArgumentNullException(nameof(database));
        }

        /// <summary>
        /// The <see cref="IDatabase"/> object all tables will have as their parent
        /// </summary>
        public IDatabase DataBaseReference { get; }

        /// <inheritdoc cref="DataBaseTable.DataBaseTable"/>
        public virtual DataBaseTable CreateTable(string tableName)
        {
            if (tableName.IsNullOrEmpty()) throw new ArgumentException(nameof(tableName));
            return new DataBaseTable(DataBaseReference, tableName);
        }

        /// <inheritdoc cref="SimpleKeyDatabaseTable.SimpleKeyDatabaseTable"/>
        public virtual SimpleKeyDatabaseTable CreateTable(string tableName, string primaryKey)
        {
            if (tableName.IsNullOrEmpty()) throw new ArgumentException(nameof(tableName));
            if (primaryKey.IsNullOrEmpty()) throw new ArgumentException(nameof(primaryKey));
            return new SimpleKeyDatabaseTable(DataBaseReference, tableName, primaryKey);
        }

        /// <inheritdoc cref="CompoundKeyDatabaseTable.CompoundKeyDatabaseTable"/>
        public virtual CompoundKeyDatabaseTable CreateTable(string tableName, params string[] compoundKeyColumns)
        {
            if (tableName.IsNullOrEmpty()) throw new ArgumentException(nameof(tableName));
            if (compoundKeyColumns is null) throw new ArgumentNullException(nameof(compoundKeyColumns));
            if (compoundKeyColumns.Length == 0) throw new ArgumentException("No column names have been specified!", nameof(compoundKeyColumns));
            foreach(string s in compoundKeyColumns)
            {
                if (s.IsNullOrEmpty())
                    throw new ArgumentException("atleast one of the column names in the compoundKeyColumns parameter was null or empty!");
            }
            return new CompoundKeyDatabaseTable(DataBaseReference, tableName, compoundKeyColumns);
        }
    }
    
}
