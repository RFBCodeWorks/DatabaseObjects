# RFBCodeworks.SqlKata.MSOfficeCompilers

This library adds two new SqlKata.Compiler.Compilers:
- MSAccessCompiler
    - This compiler has a DAO mode, and a Ansi-92 mode (Ansi-92) is default
    - The Ansi-92 mode is used by all OLEDB connections to MSAccess, so this is the default mode. 
        - This mode uses 'ALIKE' instead of 'LIKE', and performs various sanitization of wildcards to match what Access expects to see.
        - For example, when using ALIKE, you must use a '%' as a wildcard instead of '*'
    - Turning on DAO mode sends the query as-is, without any sanitization as noted above. 

- Excel
    - This compiler is designed to generate the SQL for interacting with Excel documents.


## Required Libraries
- SqlKata
