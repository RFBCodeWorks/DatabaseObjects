using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RFBCodeWorks.DataBaseObjects;
using SqlKata;
using System.Runtime.CompilerServices;

namespace RFBCodeWorks.DataBaseObjects
{
    /// <summary>
    /// Contains various extension methods 
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Add a new pair to the <see cref="Exception.Data"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddVariableData(this Exception e, object key, object value) => e.Data.Add(key, value);

        /// <summary>
        /// Sanitizes an object and does basic checking to return either TRUE or FALSE.
        /// </summary>
        /// <param name="value">Object to attempt to convert</param>
        /// <returns>
        /// If (<paramref name="value"/> is <see langword="null"/> | <see cref="DBNull"/>) return null <br/>
        /// If (<paramref name="value"/> is <see cref="bool"/> b) return b <br/>
        /// If (<paramref name="value"/>.ToString() == "false"| "0" ) return FALSE <br/>
        /// Else attempt to case into a bool? object.
        /// </returns>
        /// <exception cref="InvalidCastException"/>
        public static bool? SanitizeToBool(this object value)
        {
            if (value is null | value is DBNull | (value is string str && string.IsNullOrEmpty(str))) return null;
            if (value is bool b) return b;
            if (value is IConvertible ic) return ic.ToBoolean(default);

            string val = value?.ToString()?.ToLower().Trim();
            switch (val)
            {
                case "0":
                case "false":
                case "":
                    return false;
                case "1":
                case "true":
                    return true;
            }
            try
            {
                return (bool?)value;
            }
            catch(Exception e) 
            {
                e.AddVariableData("Info", "Attempt to cast object to boolean failed");
                e.AddVariableData("Object Type", value.GetType().ToString());
                e.AddVariableData("Object.ToString", value.ToString());
                throw;
            }
            
        }

        /// <returns>
        /// If <paramref name="value"/> is null | <see cref="DBNull"/> | String.IsNullOrEmpty() == true : return 0 <br/>
        /// Otherwise, attempt to convert the object directly using <see cref="Convert.ToInt32(object)"/>.
        /// </returns>
        /// <inheritdoc cref="SanitizeToInt(object)"/>
        public static bool SanitizeOrDefaultBool(this object value) => SanitizeToBool(value) ?? false;

        /// <summary>
        /// Attemps to convert the object into an Int32. <br/>
        /// Mainly for sanitizing data out of a datatable.
        /// </summary>
        /// <param name="value">Object to attempt to convert</param>
        /// <returns>
        /// If <paramref name="value"/> is null | <see cref="DBNull"/> | String.IsNullOrEmpty() == true : return null  <br/>
        /// If the value is already an int type, (this includes stored as long), use <see cref="Convert.ToInt32(object)"/> <br/>
        /// Otherwise, attempt to convert the object directly using <see cref="Convert.ToInt32(object)"/>.
        /// </returns>
        /// <inheritdoc cref="Convert.ToInt32(object)"/>
        public static int? SanitizeToInt(this object value)
        {
            try
            {
                if (value is null | value is DBNull | (value is string str && string.IsNullOrEmpty(str)))
                    return null;
                else if (value is IConvertible v)
                    return Convert.ToInt32(value);
                else
                    return (int?)value;
            }
            catch (Exception e)
            {
                e.AddVariableData("Info", "Attempt to convert object to Int32 failed");
                e.AddVariableData("Object Type", value.GetType().ToString());
                e.AddVariableData("Object.ToString", value.ToString());
                throw;
            }
        }

        /// <returns>
        /// If <paramref name="value"/> is null | <see cref="DBNull"/> | String.IsNullOrEmpty() == true : return 0 <br/>
        /// Otherwise, attempt to convert the object directly using <see cref="Convert.ToInt32(object)"/>.
        /// </returns>
        /// <inheritdoc cref="SanitizeToInt(object)"/>
        public static int SanitizeOrDefaultInt(this object value) => SanitizeToInt(value) ?? 0;

        /// <summary>
        /// Convert this object to its string representation. <br/>
        /// Sanitizes <see langword="null"/>, <see cref="DBNull"/>, <see cref="Enum"/>, and string types. <br/>
        /// </summary>
        /// <returns>
        /// The string representation of the <paramref name="value"/>. <br/> 
        /// If the type is an Enum, returns <see cref="Enum.GetName(Type, object)"/> <br/>
        /// If the <paramref name="value"/> is <see langword="null"/> or <see cref="DBNull"/> : return <see langword="null"/> <br/>
        /// Otherwise, return the result of the <see cref="Convert.ToString(object, IFormatProvider)"/> method.
        /// </returns>
        /// <inheritdoc cref="Convert.ToString(object, IFormatProvider)"/>
        public static string SanitizeToString(this object value, IFormatProvider provider = null)
        {
            switch (true)
            {
                case true when value is null:
                case true when value is DBNull:
                    return null;
                case true when value is string val:
                    return val;
                case true when value is Enum en:
                    return Enum.GetName(en.GetType(), en);
                default: 
                    return (provider is null ? Convert.ToString(value) : Convert.ToString(value, provider));
            }
        }

        /// <returns>
        /// If <paramref name="value"/> is null | <see cref="DBNull"/> | String.IsNullOrEmpty() == true : return 0 <br/>
        /// Otherwise, attempt to convert the object directly using <see cref="Convert.ToInt32(object)"/>.
        /// </returns>
        /// <inheritdoc cref="SanitizeToInt(object)"/>
        public static string SanitizeOrDefaultString(this object value) => SanitizeToString(value) ?? string.Empty;
    }
}
