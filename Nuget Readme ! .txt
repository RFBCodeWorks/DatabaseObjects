Note about Uploading to nuget : 

Verify Nuget Packages are built properly prior to uploading. 
- Must perform a 'Batch Build' to build all platforms (x86|x64|AnyCpu), then explicitly perform a PACK.

Packages that need this verification:
 - RFBCodeWorks.DatabaseObjects
 - RFBCodeWorks.DatabaseObjects.MsOffice  ( This is the primary one, but others were included just in case )
 - RFBCodeWorks.MsAccessDao
 - RFBCodeWorks.SqlKata.MsOfficeCompilers


 --- Note :  the 'lib' folder should contain all of the AnyCPU compiled dlls

Nuget Package Structure should be: 

lib
________Target
________Target
________Target

runtimes

____win-x86
________native
____________Target
____________Target
____________Target

____win-x64
________native
____________Target
____________Target
____________Target