using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataBaseObjects
{
    /// <summary>
    /// Contains static methods to convert DataTables to Dictionaries
    /// </summary>
    public static class DictionaryBuilders
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table">Table to convert</param>
        /// <param name="keySanitizer">Validates the keys before adding to the dictionary</param>
        /// <param name="valueSanitizer">sanitizes the values before adding to the dictionary</param>
        /// <returns></returns>
        public static Dictionary<K, V> KeyValueBuilder<K, V>(DataTable table, Func<object, K> keySanitizer, Func<object, V> valueSanitizer)
        {
            if (table is null) throw new Exception("Cannot construct Dictionary from a null table");
            var Dict = new Dictionary<K, V>();
            using (DataTableReader Rdr = table.CreateDataReader())
            {
                //populate the dictionary
                while (Rdr.Read())
                {
                    Dict.Add(keySanitizer(Rdr.GetValue(0)), valueSanitizer(Rdr.GetValue(1)));
                }
            }
            return Dict;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table">Table to convert</param>
        /// <param name="keySanitizer">Validates the keys before adding to the dictionary</param>
        /// <param name="valueSanitizer">sanitizes the values before adding to the value array</param>
        /// /// <returns></returns>
        public static Dictionary<K, V[]> KeyValueArrayBuilder<K, V>(DataTable table, Func<object, K> keySanitizer, Func<object, V> valueSanitizer)
        {
            if (table is null) throw new Exception("Cannot construct Dictionary from a null table");
            var ColumnDict = table.BuildColumnDictionary();

            var Dict = new Dictionary<K, V[]>();
            using (DataTableReader Rdr = table.CreateDataReader())
            {
                //populate the dictionary
                while (Rdr.Read())
                {
                    List<V> val = new();
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


        public static Dictionary<int, object> ObjectDictionary(DataTable table)
            => KeyValueBuilder<int, object>(table, ObjectExtensions.ConvertToInt, (o) => o);

        public static Dictionary<int, object[]> ObjectArrayDictionary(DataTable table)
            => KeyValueArrayBuilder<int, object>(table, ObjectExtensions.ConvertToInt, (o) => o);

        public static Dictionary<int, string[]> StringArrayDictionary(DataTable table)
            => KeyValueArrayBuilder(table, ObjectExtensions.ConvertToInt<object>, (O) => ObjectExtensions.ConvertToString<object>(O));

        public static Dictionary<int, string> StringDictionary(DataTable table)
            => KeyValueBuilder(table, ObjectExtensions.ConvertToInt<object>, (O) => ObjectExtensions.ConvertToString<object>(O));

        public static Dictionary<int, int> IntDictionary(DataTable table)
            => KeyValueBuilder(table, ObjectExtensions.ConvertToInt<object>, ObjectExtensions.ConvertToInt<object>);

        public static Dictionary<int, bool> BoolDictionary(DataTable table)
            => KeyValueBuilder(table, ObjectExtensions.ConvertToInt<object>, ObjectExtensions.ConvertToBool);

    }

}
