using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RFBCodeWorks.DatabaseObjects.DatabaseTypes
{
    internal static class StringFormatter
    {
        public static string Format(this string s, string arg) => string.Format(s, arg);
    }

    /// <summary>
    /// Class used to generate the Connection Strings for MSAccess Database
    /// </summary>
    /// <remarks><see href="https://www.connectionstrings.com/access/"/></remarks>
    public class MSAccessConnectionStringBuilder
    {
        
        /// <summary>
        /// 
        /// </summary>
        public bool PersistSecurityInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MSOfficeConnectionProvider Provider { get; set; } = MSAccessDataBase.DefaultProvider;
        
        /// <summary>
        /// 
        /// </summary>
        public string DataSource { get; set; } = string.Empty;
        
        /// <summary>
        /// 
        /// </summary>
        public string DBPassword { get; set; } = null;
        
        /// <summary>
        /// 
        /// </summary>
        public string UserID { get; set; } = null;
        
        /// <summary>
        /// 
        /// </summary>
        public string UserPassword { get; set; } = null;        


        /// <summary>
        /// Generates the connection string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(DataSource)) throw new ArgumentException("Path has no value");

            StringBuilder connection = new StringBuilder();

#pragma warning disable CS0618 // Type or member is obsolete
            connection.Append(Provider switch
            {
                MSOfficeConnectionProvider.Jet4 => IntPtr.Size == 4 ? "Provider=Microsoft.Jet.OLEDB.4.0;" : throw new InvalidOperationException("Jet4.0 is only compatible with 32-Bit assemblies."),
                MSOfficeConnectionProvider.Default or
                MSOfficeConnectionProvider.Ace12 => "Provider=Microsoft.ACE.OLEDB.12.0;",
                MSOfficeConnectionProvider.Ace16 => "Provider=Microsoft.ACE.OLEDB.16.0;",
                _ => throw new NotImplementedException($"Enum Value {Provider} has not been implemented yet")
            });
#pragma warning restore CS0618 // Type or member is obsolete

            connection.Append("Data Source={0};".Format(DataSource));

            if (!string.IsNullOrWhiteSpace(DBPassword))
                connection.Append("Jet OLEDB:Database Password={0};".Format(DBPassword));

            connection.Append("Persist Security Info={0};".Format(PersistSecurityInfo ? "True" : "False"));

            if (!string.IsNullOrWhiteSpace(UserID))
            {
                connection.Append("User Id={0};".Format(UserID));
                connection.Append("Password={0};".Format(UserPassword));
            }

            return connection.ToString();
        }

    }
}
