cd src
"C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\VSIXInstaller.exe" /q /u:"56bbe1ba-aaee-4883-848f-e3c8656f8db2"
del "..\dist\Sawczyn.EFDesigner.EFModel.DslPackage.vsix"
REM msbuild efdesigner.sln /t:Rebuild /p:Configuration=Debug
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" EFDesigner2022.sln /t:Rebuild /p:Configuration=Release
copy /Y "DslPackage\bin\Release\Sawczyn.EFDesigner.EFModel.DslPackage.vsix" ..\dist
"C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\VSIXInstaller.exe" /q "..\dist\Sawczyn.EFDesigner.EFModel.DslPackage.vsix"
cd ..
