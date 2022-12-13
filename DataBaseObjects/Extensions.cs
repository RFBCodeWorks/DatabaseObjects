using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RFBCodeWorks.DataBaseObjects;
using SqlKata;

namespace RFBCodeWorks.DataBaseObjects
{
    /// <summary>
    /// Contains various extension methods 
    /// </summary>
    public static partial class Extensions
    {

        /// <summary>
        /// Generate a new KeyValuePair array that consists of a single pair
        /// </summary>
        public static KeyValuePair<T,O>[] ConvertToKeyValuePairArray<T,O>(T key, O value)
        {
            return new KeyValuePair<T, O>[] { new KeyValuePair<T, O>(key, value) };
        }

        /// <summary>
        /// Generate a new KeyValuePair array that consists of a single pair
        /// </summary>
        public static KeyValuePair<T, O>[] ConvertToKeyValuePairArray<T, O>(IEnumerable<T> keys, IEnumerable<O> values)
        {
            if (keys.Count() != values.Count()) throw new ArgumentException("Cannot convert to KeyValuePair array - Number of keys does not match number of values");

            int count = keys.Count();
            var list = new List<KeyValuePair<T, O>>();
            var keyList = keys.ToArray();
            var valueList = values.ToArray();
            for (int i = 0; i < count; i++)
            {
                list.Add(new KeyValuePair<T, O>(keyList[i], valueList[i]));
            }
            return list.ToArray();
        }

        /// <summary>
        /// Sanitizes an object and does basic checking to return either TRUE or FALSE. Use this for reading from a DataTable.
        /// </summary>
        /// <param name="value">Object to attempt to convert</param>
        /// <returns>
        /// If (<paramref name="value"/> is null | <see cref="DBNull"/>) return FALSE <br/>
        /// If (<paramref name="value"/>.ToString() == "false"| "0" ) return FALSE <br/>
        /// Else return TRUE.
        /// </returns>
        public static bool ConvertToBool(this object value)
        {
            if (value is null | value is DBNull) return false;
            if (value is bool v) return v;
            string val = value?.ToString()?.ToLower();
            switch (true)
            {
                case true when val == "false":
                case true when val == "0":
                case true when string.IsNullOrWhiteSpace(val):
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Attemps to convert the object into an Int32. <br/>
        /// Mainly for sanitizing data out of a datatable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Object to attempt to convert</param>
        /// <returns>
        /// If <paramref name="value"/> is null | <see cref="DBNull"/> | String.IsNullOrEmpty() == true : return 0  <br/>
        /// If the value is already an int type, (this includes stored as long), use <see cref="Convert.ToInt32(object)"/> <br/>
        /// Otherwise, convert the object to a string and attempt to convert it to an int.
        /// </returns>
        /// <inheritdoc cref="Convert.ToInt32(object)"/>
        public static int ConvertToInt<T>(this T value)
            where T : class
        {
            if (value is null || value is DBNull) return 0;
            if (value is string str)
            {
                if (string.IsNullOrWhiteSpace(str))
                    return 0;
                else
                    return (int)Math.Round(Convert.ToDouble(str));
            }
            //Integer/Long
            else if (
                typeof(T) == typeof(Int16) | typeof(T) == typeof(Int32) | typeof(T) == typeof(Int64) |
                typeof(T) == typeof(UInt16) | typeof(T) == typeof(UInt32) | typeof(T) == typeof(UInt64) |
                typeof(T) == typeof(long) | typeof(T) == typeof(ulong) | typeof(T) == typeof(int)
                )
            {
                return Convert.ToInt32(value);
            }
            else
                return (int)Math.Round(Convert.ToDouble(value.ToString()));
        }

        /// <summary>
        /// Convert this object to its string representation. <br/>
        /// Sanitizes null, <see cref="DBNull"/>, <see cref="Enum"/>, and string types. <br/>
        /// </summary>
        /// <typeparam name="T">Enums, Strings, and Numerics are expected</typeparam>
        /// <returns>
        /// The string representation of the <paramref name="value"/>. <br/> 
        /// If the type is an Enum, returns <see cref="Enum.GetName(Type, object)"/> <br/>
        /// If the <paramref name="value"/> is null or <see cref="DBNull"/> : return <see cref="String.Empty"/> <br/>
        /// Otherwise, return the result of the <see cref="Convert.ToString(object, IFormatProvider)"/> method.
        /// </returns>
        /// <inheritdoc cref="Convert.ToString(object?, IFormatProvider?)"/>
        public static string ConvertToString<T>(this T value, IFormatProvider provider = null)
            where T : class
        {
            switch (true)
            {
                case true when value is string val: return val;
                case true when value is null: return string.Empty;
                case true when value is DBNull: return string.Empty;
                case true when (typeof(T).IsEnum): return Enum.GetName(typeof(T), value) ?? string.Empty;
                default: return (provider is null ? Convert.ToString(value) : Convert.ToString(value, provider)) ?? string.Empty;
            }
        }

        ///<inheritdoc cref="ConvertToString{T}(T, IFormatProvider?)"/>
        public static string ConvertToString(this object value, IFormatProvider provider = null) => ConvertToString(value, provider);
        ///<inheritdoc cref="ConvertToInt{T}(T)"/>
        public static int ConvertToInt(this object value) => ConvertToInt(value);

        /// <summary>
        /// Extension Method to easily add the Variable Data from a method to the <see cref="Exception.Data"/> for logging purposes.
        /// </summary>
        /// <param name="E">Exception to add data to </param>
        /// <param name="nameofVariable">Name of the variable - for ease use : <c>nameof(Variable)</c></param>
        /// <param name="VariableValue">Put the variable or string property here</param>
        internal static void AddVariableData(this Exception E, string nameofVariable, string VariableValue) => E.Data.Add(nameofVariable, VariableValue);

        /// <summary>
        /// Extension Method to easily add the Variable Data from a method to the <see cref="Exception.Data"/> for logging purposes.
        /// </summary>
        /// <param name="E">Exception to add data to </param>
        /// <param name="VariableDict">Dictionary of Variables</param>
        internal static void AddVariableData(this Exception E, System.Collections.Generic.IDictionary<string, string> VariableDict)
        {
            foreach (System.Collections.DictionaryEntry entry in (System.Collections.IDictionary)VariableDict)
                E.Data.Add(entry.Key.ToString(), entry.Value.ToString());
        }
    }

}

namespace SqlKata
{
    /// <summary>s
    /// Contains various extension methods for <see cref="SqlKata.Query"/> class
    /// </summary>
    public static class QueryAsExtensions
    {
        /// <param name="updateCol">Column to Update</param>
        /// <param name="newVal">Value for the column</param>
        /// <inheritdoc cref="SqlKata.Query.AsInsert(IEnumerable{KeyValuePair{string, object}}, bool)"/>
        /// <param name="qry"/>
        /// <param name="returnID"/>
        public static SqlKata.Query AsInsert(this SqlKata.Query qry, string updateCol, object newVal, bool returnID = false)
            => qry.AsInsert(RFBCodeWorks.DataBaseObjects.Extensions.ConvertToKeyValuePairArray(updateCol, newVal), returnID);

        /// <param name="updateCol">Column to Update</param>
        /// <param name="newVal">Value for the column</param>
        /// <inheritdoc cref="SqlKata.Query.AsUpdate(IEnumerable{KeyValuePair{string, object}})" />
        /// <param name="qry"/>
        public static SqlKata.Query AsUpdate(this SqlKata.Query qry, string updateCol, object newVal)
            => qry.AsUpdate(RFBCodeWorks.DataBaseObjects.Extensions.ConvertToKeyValuePairArray(updateCol, newVal));

    }
    
}
