using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RFBCodeWorks.DataBaseObjects
{
    /// <summary>
    /// Contains static methods to convert DataTables to Dictionaries
    /// </summary>
    public static class DictionaryBuilders
    {

        /// <summary>
        /// Convert a <see cref="DataTable"/> to a <see cref="Dictionary{TKey, TValue}"/> of the specified type
        /// </summary>
        /// <param name="table">The DataTable to convert</param>
        /// <param name="keySanitizer">Validates the keys before adding to the dictionary</param>
        /// <param name="valueSanitizer">sanitizes the values before adding to the dictionary</param>
        /// <param name="keyColumn">The  0-based index of the unique key column. This would typically be the primary key for the table, and as such index of 0</param>
        /// <param name="valueColumn">The 0-based index of the value column </param>
        /// <typeparam name="K">The type of object found within the <paramref name="keyColumn"/></typeparam>
        /// <typeparam name="V">The type of object found within the <paramref name="valueColumn"/></typeparam>
        /// <returns>a new <see cref="Dictionary{K, V}"/> object</returns>
        public static Dictionary<K, V> BuildDictionary<K, V>(this DataTable table, Func<object, K> keySanitizer, Func<object, V> valueSanitizer, int valueColumn = 1, int keyColumn = 0)
        {
            if (table is null) throw new ArgumentNullException("Cannot construct Dictionary from a null DataTable Object");
            if (keySanitizer is null) throw new ArgumentNullException("keySanitizer parameter is null");
            if (valueSanitizer is null) throw new ArgumentNullException("valueSanitizer parameter is null");
            var Dict = new Dictionary<K, V>();
            using (DataTableReader Rdr = table.CreateDataReader())
            {
                //populate the dictionary
                while (Rdr.Read())
                {
                    Dict.Add(keySanitizer(Rdr.GetValue(keyColumn)), valueSanitizer(Rdr.GetValue(valueColumn)));
                }
            }
            return Dict;
        }

        /// <summary>
        /// Convert a DataTable to a Dictionary whose 'Values' are all the same type.
        /// <br/> Example: A table with an integer ID column, and all other values are strings.
        /// </summary>
        /// <inheritdoc cref="BuildDictionary{K, V}(DataTable, Func{object, K}, Func{object, V}, int, int)"/>
        public static Dictionary<K, V[]> BuildArrayDictionary<K, V>(this DataTable table, Func<object, K> keySanitizer, Func<object, V> valueSanitizer)
        {
            if (table is null) throw new ArgumentNullException("Cannot construct Dictionary from a null DataTable Object");
            if (keySanitizer is null) throw new ArgumentNullException("keySanitizer parameter is null");
            if (valueSanitizer is null) throw new ArgumentNullException("valueSanitizer parameter is null");

            var Dict = new Dictionary<K, V[]>();
            using (DataTableReader Rdr = table.CreateDataReader())
            {
                //populate the dictionary
                while (Rdr.Read())
                {
                    List<V> val = new List<V>();
                    K key = keySanitizer(Rdr.GetValue(0));

                    for (int Index = 1; Index < table.Columns.Count; Index++)
                    {
                        val.Add(valueSanitizer(Rdr.GetValue(Index)));
                    }
                    Dict.Add(key, val.ToArray());
                }
            }
            return Dict;
        }

        /// <summary>
        /// Convert a DataTable to a dictionary of objects
        /// </summary>
        /// <inheritdoc cref="BuildDictionary{K, V}(DataTable, Func{object, K}, Func{object, V}, int, int)"/>
        public static Dictionary<int, object> ToObjectDictionary(this DataTable table, int valueColumn = 1, int keyColumn = 0)
            => BuildDictionary<int, object>(table, ObjectSanitizing.SanitizeOrDefaultInt, (o) => o, valueColumn, keyColumn);

        /// <summary>
        /// Convert a DataTable to a dictionary of strings
        /// </summary>
        /// <inheritdoc cref="BuildDictionary{K, V}(DataTable, Func{object, K}, Func{object, V}, int, int)"/>
        public static Dictionary<int, string> ToStringDictionary(this DataTable table, int valueColumn = 1, int keyColumn = 0)
            => BuildDictionary(table, ObjectSanitizing.SanitizeOrDefaultInt, ObjectSanitizing.SanitizeOrDefaultString, valueColumn, keyColumn);

        /// <summary>
        /// Convert a DataTable to a dictionary of integers
        /// </summary>
        /// <inheritdoc cref="BuildDictionary{K, V}(DataTable, Func{object, K}, Func{object, V}, int, int)"/>
        public static Dictionary<int, int> ToIntDictionary(this DataTable table, int valueColumn = 1, int keyColumn = 0)
            => BuildDictionary(table, ObjectSanitizing.SanitizeOrDefaultInt, ObjectSanitizing.SanitizeOrDefaultInt, valueColumn, keyColumn);

        /// <summary>
        /// Convert a DataTable to a dictionary of booleans
        /// </summary>
        /// <inheritdoc cref="BuildDictionary{K, V}(DataTable, Func{object, K}, Func{object, V}, int, int)"/>
        public static Dictionary<int, bool> ToBoolDictionary(this DataTable table, int valueColumn = 1, int keyColumn = 0)
            => BuildDictionary(table, ObjectSanitizing.SanitizeOrDefaultInt, ObjectSanitizing.SanitizeOrDefaultBool, valueColumn, keyColumn);
        
        /// <inheritdoc cref="BuildArrayDictionary{K, V}(DataTable, Func{object, K}, Func{object, V})"/>
        public static Dictionary<int, object[]> ToObjectArrayDictionary(this DataTable table)
            => BuildArrayDictionary<int, object>(table, ObjectSanitizing.SanitizeOrDefaultInt, (o) => o);

        /// <inheritdoc cref="BuildArrayDictionary{K, V}(DataTable, Func{object, K}, Func{object, V})"/>
        public static Dictionary<int, string[]> ToStringArrayDictionary(this DataTable table)
            => BuildArrayDictionary(table, ObjectSanitizing.SanitizeOrDefaultInt, ObjectSanitizing.SanitizeOrDefaultString);


       
    }

}
