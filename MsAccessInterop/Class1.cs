using System;
using Dao = Microsoft.Office.Interop.Access.Dao;
using FieldType = Microsoft.Office.Interop.Access.Dao.DataTypeEnum;

namespace MsAccessInterop
{
    /// <summary>
    /// Static methods for reading/writing properties to the database file
    /// </summary>
    public static class DatabaseProperties
    {
        /// <summary>
        /// Update or Create the property in the Assigned Database 
        /// </summary>
        /// <typeparam name="T">The type of value to set</typeparam>
        /// <param name="PropertyName">The name of the property</param>
        /// <param name="value">the value of the property</param>
        /// <param name="dataType">The datatype of the property - should match the type of property</param>
        /// <param name="DB"></param>
        /// <returns></returns>
        public static T SetDBProperty<T>(string PropertyName, T value, Dao.DataTypeEnum dataType, Dao.Database DB)
        {
            T ret;
            try
            {
                DB.Properties[PropertyName].Value = value;
            }
            catch
            {
                try
                {
                    DB.Properties.Append(DB.CreateProperty(PropertyName, dataType, value));
                }
                catch (Exception E)
                {
                    E.AddVariableData(nameof(PropertyName), PropertyName);
                    E.AddVariableData(nameof(value), value.ToString());
                    E.AddVariableData(nameof(dataType), dataType.ToString());
                    Logging.LogErr(E, Logging.ErrorLevel.Unexpected);
                }
            }
            finally
            {
                ret = DB.Properties[PropertyName].Value;
                try { DB.Close(); } catch { }
            }
            return ret;
        }
    }
}
