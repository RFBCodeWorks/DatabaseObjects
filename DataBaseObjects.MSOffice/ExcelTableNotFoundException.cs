using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace RFBCodeWorks.DatabaseObjects
{
    [Serializable]
    public class ExcelTableNotFoundException : Exception

    {
        public ExcelTableNotFoundException() { }
        public ExcelTableNotFoundException(string message) : base(message) { }
        public ExcelTableNotFoundException(string message, Exception inner) : base(message, inner) { }
        public ExcelTableNotFoundException(string tableName, string workBookPath, Exception inner = null) : base($"Table '{tableName}' not found in workbook '{workBookPath}'", inner) { }

        //protected ExcelTableNotFoundException(
        //  System.Runtime.Serialization.SerializationInfo info,
        //  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
