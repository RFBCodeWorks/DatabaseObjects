# RFBCodeworks.DatabaseObjects.MsOffice
## About
This library extends the RFBCodeworks.DatabaseObjects library with a database to represent Microsoft Access, and Microsoft Excel. 

These objects use an OLEDBCommand to communicate to the files. 
Since this requires an OLEDB connection, this library is compiled only for x86/x64.

## MSAccessDataBase
- Represents an MS Access Database.

# Working with Excel

## ExcelWorkbook
- Represents an Excel workbook document. 
- Derived from RFBCodeworks.DatabaseObjects.AbstractDatabase
- Has and methods properties specific to interacting with an excel workbook.

## ExcelWorkbook.Worksheet
- Represents a 'Worksheet' within the specified workbook.
- Derived from RFBCodeworks.DatabaseObjects.DatabaseTable
- Various methods have been overridden since they are not compatible with excel workbooks (Primarily the 'INSERT') methods

## ExcelWorkbook.PrimaryKeyWorksheet
- Represents a 'Worksheet' whose table is meant to act as a table with a primary key (such as an Auto-Number column)
- Derived from ExcelWorkbook.Worksheet
- Implements RFBCodeworks.DatabaseObjects.IPrimaryKeyTable

# Troubleshooting
- Driver Missing Error
    - 32-bit computers -- Install the appropriate driver. This can be found from the 32-bit Microsoft Access Runtime.
    - 64-bit computers
        - If the computer has any 32-bit Microsoft office products installed, then the calling application must be compiled and running in 32-bit mode.
          Unfortunately, this is due to the required backwards compatibility that MS implements, so nothing can be done on my end about this (as far as I know).
          Your options are basically uninstall all 32-bit Microsoft Office products/upgrade to 64-bit office, or build your application in 32-bit mode.
          If the computer does not have the driver installed at all (or has the 64-bit driver installed but 32-bit Office is installed), then you will have to download the 32-bit driver.

        - If the computer does not have any 32-bit microsoft office features installed, then you are missing the 64-bit driver.

## Required Libraries
- SqlKata
- System.Data.OleDb
- RFBCodeworks.DatabaseObjects
- RFBCodeworks.SqlKata.MSOfficeCompilers
- RFBCodeworks.SqlKata.Extensions