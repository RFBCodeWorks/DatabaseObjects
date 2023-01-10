using System;
using Dao = Microsoft.Office.Interop.Access.Dao;
using DataTypeEnum = Microsoft.Office.Interop.Access.Dao.DataTypeEnum;

namespace RFBCodeWorks.MsAccessDao
{
    /// <summary>
    /// Static methods for reading/writing properties to the database file
    /// </summary>
    public static class DatabaseProperties
    {

        /// <summary>
        /// Gets the value of a named property from the database
        /// </summary>
        /// <param name="db">The database to retrive a property from</param>
        /// <param name="propertyName">The name of the property to retrieve</param>
        /// <returns>The property object</returns>
        public static Dao.Property GetProperty(this Dao.Database db, string propertyName)
        {
            if (db is null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentException("PropertyName cannot be empty!", nameof(propertyName));
            return db.Properties[propertyName];
        }

        /// <summary>
        /// Gets the value of a named property from the database
        /// </summary>
        /// <param name="db">The database to retrive a property from</param>
        /// <param name="propertyName">The name of the property to retrieve</param>
        /// <returns>The property object</returns>
        public static object GetPropertyValue(this Dao.Database db, string propertyName)
        {
            return GetProperty(db, propertyName)?.Value;
        }

        /// <summary>
        /// Gets the value of a property by its index from the database
        /// </summary>
        /// <param name="db">The database to retrive a property from</param>
        /// <param name="propertyIndex">The index of the property to retrieve</param>
        /// <returns>The property object</returns>
        public static Dao.Property GetProperty(this Dao.Database db, int propertyIndex)
        {
            if (db is null) throw new ArgumentNullException(nameof(db));
            if (propertyIndex < 0) throw new ArgumentOutOfRangeException(nameof(propertyIndex), "propertyIndex must be greater or equal to than 0");
            return db.Properties[propertyIndex];
        }

        /// <summary>
        /// Gets the value of a property by its index from the database
        /// </summary>
        /// <param name="db">The database to retrive a property from</param>
        /// <param name="propertyIndex">The index of the property to retrieve</param>
        /// <returns>The property object</returns>
        public static object GetPropertyValue(this Dao.Database db, int propertyIndex)
        {
            return GetProperty(db, propertyIndex)?.Value;
        }

        /// <summary>
        /// Reads the value of a property by its name. 
        /// <br/>If the property does not exist, attempt to set it to the <paramref name="defaultValue"/>
        /// </summary>
        /// <param name="defaultValue">If the property does not exist, set it to this value. If the property does exist, this value is ignored.</param>
        /// <returns>The value of the property read from the <paramref name="db"/></returns>
        /// <inheritdoc cref="GetDataTypeEnum{T}"/>
        /// <inheritdoc cref="GetPropertyValue(Dao.Database, string)"/>
        /// <param name="db"/><param name="propertyName"/>
        public static T InitializeProperty<T>(this Dao.Database db, string propertyName, T defaultValue)
        {
            if (db is null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentException("PropertyName cannot be empty!", nameof(propertyName));
            try
            {
                return (T)GetPropertyValue(db, propertyName);
            }
            catch (NullReferenceException)
            {
                return db.SetProperty(propertyName, defaultValue);
            }
        }

        /// <typeparam name="T"><inheritdoc cref="GetDataTypeEnum{T}"/></typeparam>
        /// <inheritdoc cref="SetProperty{T}(Dao.Database, string, DataTypeEnum, T, bool)"/>
        public static T SetProperty<T>(this Dao.Database db, string propertyName, T value)
            => SetProperty(db, propertyName, GetDataTypeEnum<T>(), value, true);

        /// <summary>
        /// Opens the <paramref name="db"/>, and attempts to update the property. Update or Create the property in the Assigned Database 
        /// </summary>
        /// <typeparam name="T">The type of value to set</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="value">the value of the property</param>
        /// <param name="dataType">The datatype of the property - should match the type of property</param>
        /// <param name="db"></param>
        /// <param name="createIfMissing">
        /// If set <see langword="false"/>, will only apply the property if it already exists. <br/>
        /// If <see langword="true"/>, tries to create the property</param>
        /// <returns>
        /// The current value of the assigned property, if it exists. 
        /// <br/> If it does not exist (or cannot be created), then an exception will be thrown.
        /// </returns>
        /// <exception cref="AggregateException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public static T SetProperty<T>(this Dao.Database db, string propertyName, Dao.DataTypeEnum dataType, T value, bool createIfMissing = true)
        {
            if (db is null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentException("PropertyName cannot be empty!", nameof(propertyName));

            T ret;
            try
            {
                db.Properties[propertyName].Value = value;
            }
            catch (Exception E2) when (!createIfMissing)
            {
                E2.Data.Add(nameof(propertyName), propertyName);
                E2.Data.Add(nameof(value), value.ToString());
                E2.Data.Add(nameof(dataType), dataType.ToString());
                throw;
            }
            catch (NullReferenceException E1) when (createIfMissing)
            {
                try
                {
                    db.Properties.Append(db.CreateProperty(propertyName, dataType, value));
                }
                catch (Exception E)
                {
                    E.Data.Add(nameof(propertyName), propertyName);
                    E.Data.Add(nameof(value), value.ToString());
                    E.Data.Add(nameof(dataType), dataType.ToString());
                    throw new AggregateException($"Was unable to update the '{propertyName}' property of the database. See exceptions.", E1, E);
                }
            }
            finally
            {
                ret = (T)db.Properties[propertyName].Value;
                try { db.Close(); } catch { }
            }
            return ret;
        }

        /// <summary>
        /// Retreives the enum translation of the provided type
        /// </summary>
        /// <typeparam name="T">One of the following types: 
        /// <br/> - <see cref="string"/>
        /// <br/> - <see cref="char"/>
        /// <br/> - <see cref="sbyte"/>
        /// <br/> - <see cref="byte"/>
        /// <br/> - <see cref="int"/>
        /// <br/> - <see cref="short"/>
        /// <br/> - <see cref="long"/>
        /// <br/> - <see cref="float"/>
        /// <br/> - <see cref="Single"/>
        /// <br/> - <see cref="double"/>
        /// <br/> - <see cref="Guid"/>
        /// <br/> - <see cref="decimal"/>
        /// <br/> - <see cref="bool"/>
        /// <br/> - <see cref="DateTime"/>
        /// </typeparam>
        /// <returns>The approrpiate <see cref="Dao.DataTypeEnum"/> for the supplied type </returns>
        public static Dao.DataTypeEnum GetDataTypeEnum<T>()
        {
            Type type = typeof(T);
            switch (true)
            {
                case true when type == typeof(char):
                case true when type == typeof(string):
                    return DataTypeEnum.dbText;    // 10    -- could also be Memo, depending on length required. Memo is longer string

                case true when type == typeof(sbyte):
                case true when type == typeof(byte):
                    return DataTypeEnum.dbByte;    // 2

                case true when type == typeof(int):
                case true when type == typeof(short):
                    return DataTypeEnum.dbInteger; // 3

                case true when type == typeof(long):
                    return DataTypeEnum.dbLong;    // 4

                case true when type == typeof(float):
                case true when type == typeof(Single):
                    return DataTypeEnum.dbSingle;  // 6

                case true when type == typeof(double):
                    return DataTypeEnum.dbComplexDouble; // 20

                case true when type == typeof(Guid):
                    return DataTypeEnum.dbGUID; // 1

                case true when type == typeof(decimal):
                    return DataTypeEnum.dbDecimal; // 20

                case true when type == typeof(DateTime):
                    return DataTypeEnum.dbDate;    // 23

                case true when type == typeof(bool): 
                    return DataTypeEnum.dbBoolean; // 1

                default:
                    var e = new NotImplementedException("This translation method does not have the provided type defined. \n" +
                        "Expected types include: char, string, sbyte, byte, int, short, long, float, single, double, Guid, decimal, DateTime, and bool");
                    e.Data.Add("Type", typeof(T));
                    throw e;
            }
        }
    }
}
