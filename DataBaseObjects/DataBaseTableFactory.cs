using System;
using System.Collections.Generic;
using System.Text;

namespace RFBCodeWorks.DatabaseObjects
{
    /// <summary>
    /// Factory to create Database Tables specific to the a database
    /// </summary>
    public class DatabaseTableFactory
    {
        /// <summary>
        /// Instantiate the factory object
        /// </summary>
        /// <param name="database"><inheritdoc cref="DataBaseReference" path="*"/></param>
        public DatabaseTableFactory(IDatabase database)
        {
            DataBaseReference = database ?? throw new ArgumentNullException(nameof(database));
        }

        /// <summary>
        /// The <see cref="IDatabase"/> object all tables will have as their parent
        /// </summary>
        public IDatabase DataBaseReference { get; }

        /// <inheritdoc cref="DatabaseTable.DatabaseTable"/>
        public virtual DatabaseTable CreateTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException(nameof(tableName));
            return new DatabaseTable(DataBaseReference, tableName);
        }

        /// <inheritdoc cref="PrimaryKeyTable.PrimaryKeyTable"/>
        public virtual PrimaryKeyTable CreateTable(string tableName, string primaryKey)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException(nameof(tableName));
            if (string.IsNullOrWhiteSpace(primaryKey)) throw new ArgumentException(nameof(primaryKey));
            return new PrimaryKeyTable(DataBaseReference, tableName, primaryKey);
        }

        /// <inheritdoc cref="CompositeKeyTable.CompositeKeyTable"/>
        public virtual CompositeKeyTable CreateTable(string tableName, params string[] compoundKeyColumns)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException(nameof(tableName));
            if (compoundKeyColumns is null) throw new ArgumentNullException(nameof(compoundKeyColumns));
            if (compoundKeyColumns.Length == 0) throw new ArgumentException("No column names have been specified!", nameof(compoundKeyColumns));
            foreach(string s in compoundKeyColumns)
            {
                if (string.IsNullOrWhiteSpace(s))
                    throw new ArgumentException("atleast one of the column names in the compoundKeyColumns parameter was null or empty!");
            }
            return new CompositeKeyTable(DataBaseReference, tableName, compoundKeyColumns);
        }
    }
    
}
