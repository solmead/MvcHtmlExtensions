FOR /F "delims=|" %%I IN ('DIR "bin\Debug\*.*" /B /O:D') DO SET NewestFile=%%I
"..\.nuget\nuget" push bin\Debug\%NewestFile% EAIAucit1~ -Source http://uc-nuget.azurewebsites.net/api/v2/package