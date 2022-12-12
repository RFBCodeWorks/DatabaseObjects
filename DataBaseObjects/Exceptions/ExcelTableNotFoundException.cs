using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseObjects.Exceptions
{
    [Serializable]
    public class ExcelTableNotFoundException : Exception
    {
        public ExcelTableNotFoundException() { }
        public ExcelTableNotFoundException(string message) : base(message) { }
        public ExcelTableNotFoundException(string message, Exception inner) : base(message, inner) { }
        public ExcelTableNotFoundException(string TableName, string workBookPath, Exception inner = null) : base($"Table '{TableName}' not found in workbook '{workBookPath}'", inner) { }

        protected ExcelTableNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
