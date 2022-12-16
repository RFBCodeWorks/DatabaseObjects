using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dao = Microsoft.Office.Interop.Access.Dao;

namespace RFBCodeWorks.DatabaseObjects.MsAccessDao
{
    public static class Management
    {
        /// <summary>
        /// Create a new database at the specified path.
        /// </summary>
        /// <param name="path">The path to the databse that is going to be created.</param>
        /// <param name="locale">
        /// Specfiy a <see cref="Dao.LanguageConstants"/>.
        /// <br/> - If a password for the database is desired, specify one using: ';PWD=MyPassword'
        /// </param>
        /// <param name="options">Specify a version number</param>
        /// <returns>The newly created database</returns>
        public static Dao.Database CreateDatabase(string path, string locale, Dao.DatabaseTypeEnum options)
        {
            if (File.Exists(path)) throw new ArgumentException("File already exists at the specified path!", nameof(path));
            if (string.IsNullOrWhiteSpace(locale)) throw new ArgumentException("No Locale Specified!", nameof(path));
            if (string.IsNullOrWhiteSpace(locale)) throw new ArgumentException("No Locale Specified!", nameof(path));
            return new Dao.DBEngine().CreateDatabase(path, locale, options);
        }

        /// <inheritdoc cref="OpenDatabase(string, bool, bool, string)"/>
        public static Dao.Database OpenDatabase(string path)
        {
            return new Dao.DBEngine().OpenDatabase(path);
        }

        /// <summary>
        /// Opens up an Access Database at the specified path
        /// </summary>
        /// <param name="path">
        /// - The name (path to) of an existing Microsoft Access database file, or the data source name(DSN) of an ODBC data source.
        /// <br/> Name: <see href="https://learn.microsoft.com/en-us/office/client-developer/access/desktop-database-reference/connection-name-property-dao"/>
        /// </param>
        /// <param name="exclusiveMode">
        /// - <see langword="true"/> - Opens the database in exclusive mode.
        /// <br/>- <see langword="false"/> - (Default) Opens the database in shared mode.
        /// </param>
        /// <param name="readOnly">True if you want to open the database with read-only access, or False (default) if you want to open the database with read/write access.</param>
        /// <param name="connection">A connection string. <br/> Specifies various connection information, including passwords.</param>
        /// <returns></returns>
        /// <remarks>
        /// <see href="https://learn.microsoft.com/en-us/office/client-developer/access/desktop-database-reference/dbengine-opendatabase-method-dao"/>
        /// </remarks>
        public static Dao.Database OpenDatabase(string path, bool exclusiveMode = false, bool readOnly = false, string connection = "")
        {
            return new Dao.DBEngine().OpenDatabase(path, exclusiveMode, readOnly, connection);
        }

        /// <summary>
        /// Copies and compacts a closed database, and gives you the option of changing its version, collating order, and encryption.
        /// </summary>
        /// <param name="srcName">The source file path for the database to comnpact.</param>
        /// <param name="dstName">The destination file path that the new database will reside at. Cannot be the same as <paramref name="srcName"/></param>
        /// <param name="dstLocale">
        /// A string expression that specifies a collating order for creating DstName. Preferable to use <see cref="Dao.LanguageConstants"/>.
        /// <br/> - If you omit this argument, the locale of <paramref name="dstName"/> is the same as the locale of <paramref name="srcName"/>
        /// <br/> - You can also create a password for DstName by concatenating the password string (starting with ";pwd=") with a constant in the DstLocale argument, like this: dbLangSpanish + ";pwd=NewPassword".
        /// <br/> - If you want to use the same DstLocale as SrcName (the default value), but specify a new password, simply enter a password string for DstLocale: ";pwd=NewPassword"
        /// </param>
        /// <param name="options">
        /// Specify the options for the compacted database
        /// <br/>This constant affects only the version of the data format of DstName and doesn't affect the version of any Microsoft Access-defined objects, such as forms and reports
        /// <br/> The constants dbEncrypt and dbDecrypt are deprecated and not supported in .ACCDB file formats.
        /// </param>
        /// <param name="password">
        /// A string expression containing an encryption key for the source database, if the database is encrypted. 
        /// <br/> - The string ";pwd=" must precede the actual password. 
        /// <br/> - If you include a password setting in DstLocale, this setting is ignored
        /// <para/>
        /// NOTE: This is deprecated parameter and is not supported in .ACCDB format. To encrypt an .ACCDB file, use the "pwd=" option string.
        /// </param>
        /// <remarks>
        /// For information on this process, see this link:
        /// <br/><see href="https://learn.microsoft.com/en-us/office/client-developer/access/desktop-database-reference/dbengine-compactdatabase-method-dao"/>
        /// </remarks>
        public static void CompactDB(string srcName, string dstName, Dao.DatabaseTypeEnum options, string dstLocale = "", string password = "")
        {
            if (!File.Exists(srcName)) throw new System.IO.FileNotFoundException("Source Database Path not found!", srcName);
            if (File.Exists(dstName)) throw new System.ArgumentException("File already exists at destination path!", dstName);
            new Dao.DBEngine().CompactDatabase(srcName, dstName, dstLocale, options, password);
        }

        /// <summary>
        /// Compacts the database located at <paramref name="srcName"/>, then deletes the original file and replaces it with the newly compacted version.
        /// <br/> - This is done by utilizing a temporary filename to temporarily store the copy. Successful or not, the copy of the database will be deleted, leaving only the original file path.
        /// </summary>
        /// <inheritdoc cref="CompactAndReplace(string, Dao.DatabaseTypeEnum, string, string)"/>
        public static void CompactAndReplace(string srcName, Dao.DatabaseTypeEnum options, string dstLocale = "", string password = "")
        {
            GetNewFileName:
            string dest = System.IO.Path.GetTempFileName();
            var fn = Path.GetFileName(dest).Replace(Path.GetExtension(dest), Path.GetExtension(srcName));
            dest = dest.Replace(Path.GetFileName(dest), fn);
            if (File.Exists(dest)) goto GetNewFileName; // Retry until an unused file path is established

            bool compactSuccess = false;
            bool deletionSuccess = false;
            bool moveSuccess = false;
            string backupAppend = "_Backup" + DateTime.Now.TimeOfDay.TotalMilliseconds;
            string backupName = srcName + backupAppend;
            try
            {
                CompactDB(srcName, dest, options, dstLocale, password); // This is the point where the error is expected to occur
                compactSuccess = true;
                File.Move(srcName, backupName); deletionSuccess = true; // Move the original first
                File.Move(dest, srcName); moveSuccess = true;           // Moves the compacted to the original path
            }
            catch (Exception e)
            {
                if (!compactSuccess)    //Failed during compaction - database likely still open
                {
                    e.Data.Add("AdditionalInfo", "Database was likely still open, or did not have permissions to compact the database");
                }
                else if (!deletionSuccess)  //Failed to move the old file, potentially no write permissions, or file was in use
                {
                    e.Data.Add("AdditionalInfo", "Database Compacted Successfully, but creating a backup of the source database failed.\n - File may have still been in use, or no permissions on drive to write to the file.");
                }
                else if (!moveSuccess)  //Failed to move the new file to old location - same as above
                {
                    e.Data.Add("AdditionalInfo", "Database Compacted Successfully, but creating a moving the compacted version to the original path failed.\n - File may have still been in use, or no permissions on drive to write to the file.");
                }
                throw;
            }
            finally
            {
                if (moveSuccess)
                {
                    File.Delete(backupName);
                }else if (deletionSuccess)
                {
                    //unexpected! this shouldn't really ever happen....
                    File.Move(backupName, srcName);
                }
                if (File.Exists(dest))
                {
                    File.Delete(dest);
                }
            }
        }

        /// <remarks>Note: <br/> The database is required to be closed prior to starting this task, and should not be opened until this task completes!</remarks>
        /// <inheritdoc cref="CompactDB(string, string, Dao.DatabaseTypeEnum, string, string)"/>
        public static Task CompactDBAsync(string srcName, string dstName, Dao.DatabaseTypeEnum options, string dstLocale = "", string password = "")
        {
            return Task.Run(() => CompactDB(srcName, dstName, options, dstLocale, password));
        }

        /// <remarks>Note: <br/> The database is required to be closed prior to starting this task, and should not be opened until this task completes!</remarks>
        /// <inheritdoc cref="CompactAndReplace"/>
        public static Task CompactAndReplaceAsync(string srcName, Dao.DatabaseTypeEnum options, string dstLocale = "", string password = "")
        {
            return Task.Run(() => CompactAndReplace(srcName, options, dstLocale, password));
        }

        /// <summary>
        /// Gets the string that represents the 'General Language ID' and CountryCode USA
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string dbLangGeneral(string password = "")
        {
            if (string.IsNullOrWhiteSpace(password))
                return Dao.LanguageConstants.dbLangGeneral;
            else
                return string.Concat(Dao.LanguageConstants.dbLangGeneral, ";PWD=", password);
        }

        /// <summary>
        /// Extract the password from an OLEDB Connection String
        /// </summary>
        /// <param name="oledbConnectionString"></param>
        /// <returns></returns>
        public static string ExtractPasswordFromOledbConnectionString(string oledbConnectionString)
        {
            string password = "";
            if (oledbConnectionString.Contains("Database Password"))
            {
                int L1 = oledbConnectionString.IndexOf("Database Password");
                int L2 = oledbConnectionString.IndexOf("=", L1) + 1;
                int L3 = -1;
                if (oledbConnectionString.Substring(L2).Contains(";"))
                    L3 = oledbConnectionString.IndexOf(";", L2);
                password = (L3 > 1) ? oledbConnectionString.Substring(L2, L3 - L2) : oledbConnectionString.Substring(L2);
            }
            return password;
        }

        
    }
}
