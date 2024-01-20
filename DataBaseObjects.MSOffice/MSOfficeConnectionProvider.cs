using System;
using System.Collections.Generic;
using System.Text;

namespace RFBCodeWorks.DatabaseObjects.DatabaseTypes
{
    /// <summary>
    /// Available Microsoft provided OLEDB providers
    /// </summary>
    public enum MSOfficeConnectionProvider
    {
        /// <summary>
        /// This will provide <see cref="Ace12"/> under most circumstances, and is present for backwards compatibility with prior nuget releases.
        /// </summary>
        /// <remarks>Provides <see cref="Jet4"/> only when using <see cref="ExcelWorkBook.GetConnectionString"/> on 32-bit applications.</remarks>
        Default,

        /// <summary>
        /// Provider=Microsoft.Jet.OLEDB.4.0;
        /// </summary>
        /// <remarks>
        /// - This is 32-bit support only, as its deprecated by Microsoft. 
        /// <br/> - Not compatible with .xlsx or .xlsm files.
        /// </remarks>
#if _WIN32
        Jet4,
#else
        [Obsolete("Jet4.0 is not compatible with 64-Bit assemblies.", false)]
        Jet4,
#endif
        /// <summary>
        /// Provider=Microsoft.ACE.OLEDB.12.0;
        /// </summary>
        /// <remarks>
        /// Microsoft Access 2010 Runtime
        /// </remarks>
        Ace12,

        /// <summary>
        /// Provider=Microsoft.ACE.OLEDB.16.0;
        /// </summary>
        /// <remarks>
        /// - Microsoft Access 2010 Runtime
        /// <br/> - Office 365
        /// </remarks>
        Ace16
    }
}
