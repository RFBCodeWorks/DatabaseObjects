using System;
using System.Collections.Generic;
using System.Text;
using Dao = Microsoft.Office.Interop.Access.Dao;
using DataTypeEnum = Microsoft.Office.Interop.Access.Dao.DataTypeEnum;

namespace RFBCodeWorks.MsAccessDao
{
    /// <summary>
    /// Static methods to interact with and defines tables using DAO
    /// </summary>
    public static class TableCreation
    {
        /// <inheritdoc cref="DatabaseProperties.GetDataTypeEnum{T}"/>
        public static Dao.DataTypeEnum GetDataTypeEnum<T>() => DatabaseProperties.GetDataTypeEnum<T>();

        /// <summary>
        /// Provide a Connection String to use as for a Linked-Table that uses another access database as the source.
        /// </summary>
        /// <param name="path">The path to the database file</param>
        /// <param name="password">The database password</param>
        /// <returns>
        /// With Password: <br/> MS Access;DATABASE=<paramref name="path"/><br/> 
        /// <br/> Without Password: <br/> MS Access;PWD=<paramref name="password"/>;DATABASE=<paramref name="path"/>
        /// </returns>
        public static string GetMdbConnectionString(string path, string password = "")
        {
            string Conn = $";DATABASE={path}";
            if (password != "") Conn = string.Concat(";PWD=", password, Conn);
            return string.Concat("MS Access", Conn);
        }

        /// <summary>
        /// Search a given Dao Database for a table def with the given <paramref name="tableName"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tableName">The name of the table to search for</param>
        /// <returns>TableDef / Null (not found)</returns>
        public static Dao.TableDef SelectTableDef(this Dao.Database db, string tableName)
        {
            if (db is null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("TableName can not be empty!", nameof(tableName));
            foreach (Dao.TableDef def in db.TableDefs)
                if (def.Name == tableName) { return def; }
            return null;
        }

        /// <summary>
        /// Create a new <see cref="Dao.TableDef"/> within the specified <paramref name="db"/>. 
        /// <br/> Note: The table def will be added to the database via <see cref="Dao.TableDefs.Append(object)"/> prior to being returned from the method
        /// </summary>
        /// <param name="db">The owner database</param>
        /// <param name="tableName">The name of the table definition.</param>
        /// <param name="attributes">The table attributes to specify when creating the table definition.</param>
        /// <param name="sourceTableName">The name of a table in an external database that is the original source of the data. The source string becomes the SourceTableName property setting of the new TableDef object.</param>
        /// <param name="connect">A string containing information about the source of an open database, a database used in a pass-through query, or a linked table. See the Connect property for more information about valid connection strings.</param>
        /// <returns>The newly created <see cref="Dao.TableDef"/></returns>
        /// <remarks>
        /// See the following links: <br/>
        /// <br/> - Overview: <see href="https://learn.microsoft.com/en-us/office/client-developer/access/desktop-database-reference/database-createtabledef-method-dao"/>
        /// <br/> - Attributes: <see href="https://learn.microsoft.com/en-us/office/troubleshoot/access/tabledef-attributes-usage"/>
        /// <br/> - Connect: <see href="https://learn.microsoft.com/en-us/office/client-developer/access/desktop-database-reference/tabledef-connect-property-dao"/>
        /// </remarks>
        public static Dao.TableDef CreateTable(this Dao.Database db, string tableName, Dao.TableDefAttributeEnum attributes = Dao.TableDefAttributeEnum.dbSystemObject, string sourceTableName ="", string connect = "")
        {
            if (db is null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("TableName can not be empty!", nameof(tableName));


            if (string.IsNullOrWhiteSpace(connect) && string.IsNullOrWhiteSpace(sourceTableName))
            {
                if (attributes.HasFlag(Dao.TableDefAttributeEnum.dbAttachedODBC) | attributes.HasFlag(Dao.TableDefAttributeEnum.dbAttachedTable))
                {
                    throw new ArgumentException("attributes marked table as attached but no connection information was provided", nameof(attributes));
                }
            }
            // All below cases mean either Connect or SourceTableName was specified
            else if (string.IsNullOrWhiteSpace(connect))
            {
                throw new ArgumentException("SourceTableName was provided, but connection string was not", nameof(connect));
            }
            else if (string.IsNullOrWhiteSpace(sourceTableName))
            {
                throw new ArgumentException("Connection string was provided, but sourceTableName string was not", nameof(sourceTableName));
            }
            else if (!(attributes.HasFlag(Dao.TableDefAttributeEnum.dbAttachedODBC) | attributes.HasFlag(Dao.TableDefAttributeEnum.dbAttachedTable)))
            {
                throw new ArgumentException("A connection string and source table name was provided, but the attributes do not mark the table as 'attached'", nameof(attributes));
            }

            var tb = db.CreateTableDef(tableName, attributes, sourceTableName, connect);
            db.TableDefs.Append(tb);
            return tb;
        }

        /// <summary>
        /// Create a new <see cref="Dao.TableDef"/> within the specified <paramref name="db"/>. 
        /// <br/> The 
        /// Note: The table def will be added to the database via <see cref="Dao.TableDefs.Append(object)"/> prior to being returned from the method
        /// </summary>
        /// <inheritdoc cref="CreateTable(Dao.Database, string, Dao.TableDefAttributeEnum, string, string)"/>
        public static Dao.TableDef CreateAttachedTable(this Dao.Database db, string tableName, string sourceTableName, string connect,
            Dao.TableDefAttributeEnum attributes = Dao.TableDefAttributeEnum.dbSystemObject | Dao.TableDefAttributeEnum.dbAttachedTable)
            => CreateTable(db, tableName, attributes,
                sourceTableName ?? throw new ArgumentException("Expected Source Table Name", nameof(sourceTableName)),
                connect ?? throw new ArgumentException("Expected connection string", nameof(connect)));
        
        /// <summary>
        /// Creates a new 'field' column and appends it to the <paramref name="tblDef"/>
        /// </summary>
        /// <param name="tblDef">Table Definition to create a new field in</param>
        /// <param name="ColName">New column name for this table</param>
        /// <param name="fieldType">Access.Dao.DataTypeEnum</param>
        /// <param name="Required">set TRUE if this is a required field</param>
        /// <param name="AllowZeroLength">Set true if allowing null values</param>
        /// <param name="DefaultValue">Default Value to use</param>
        /// <param name="IsPrimaryKey">Set TRUE if the field being created is intended to be the primary key for the table</param>
        /// <param name="IsIndex">set TRUE if this column will be used an an indexer reference for the table.</param>
        /// <returns>The created field</returns>
        public static Dao.Field CreateField(this Dao.TableDef tblDef, string ColName, DataTypeEnum fieldType, bool Required = false, bool AllowZeroLength = false, object DefaultValue = null, bool IsPrimaryKey = false, bool IsIndex = false)
        {
            if (tblDef is null) throw new ArgumentNullException(nameof(tblDef));
            if (string.IsNullOrWhiteSpace(ColName)) throw new ArgumentException("Column Name cannot be null or empty!", nameof(ColName));
            
            Dao.Field f = tblDef.CreateField(ColName, fieldType);
            f.Required = IsPrimaryKey | Required;
            switch (fieldType) //AllowZeroLength sanitization
            {
                case DataTypeEnum.dbBoolean: break;
                case DataTypeEnum.dbNumeric: break;
                case DataTypeEnum.dbBigInt: break;
                case DataTypeEnum.dbInteger: break;
                case DataTypeEnum.dbLong: break;
                case DataTypeEnum.dbComplexLong: break;
                case DataTypeEnum.dbBinary: break;
                case DataTypeEnum.dbLongBinary: break;
                case DataTypeEnum.dbVarBinary: break;
                case DataTypeEnum.dbDate: break;
                default:
                    f.AllowZeroLength = AllowZeroLength;
                    break;
            }
            switch (true) //Sanitize the defaultvalue
            {
                case true when (AllowZeroLength && DefaultValue == null): f.DefaultValue = DefaultValue; break;
                case true when (fieldType == DataTypeEnum.dbBoolean && DefaultValue == null): f.DefaultValue = false; break;
                case true when (fieldType == DataTypeEnum.dbText && DefaultValue == null): f.DefaultValue = ""; break;
                case true when (fieldType == DataTypeEnum.dbNumeric && DefaultValue == null): f.DefaultValue = 0; break;
                case true: f.DefaultValue = DefaultValue; break;
            }
            tblDef.Fields.Append(f); //Add the finalized field to the table definition

            //Indexing & Primary Keys - Marks the creates field as an index/primary key if necessary
            if (IsPrimaryKey | IsIndex)
            {
                string indexName = IsPrimaryKey ? "PrimaryKey" : $"Index_{ColName}";
                Dao.Index index;
                try { index = tblDef.CreateIndex(indexName); } catch { index = tblDef.Indexes[indexName]; }
                index.Name = indexName;
                index.Primary = IsPrimaryKey;
                index.Required = IsPrimaryKey | Required;
                index.IgnoreNulls = !IsPrimaryKey;
                Dao.Field fld = index.CreateField(ColName);
                ((Dao.IndexFields)index.Fields).Append(fld);
                tblDef.Indexes.Append(index);
            }

            return f;

        }
    }
}
