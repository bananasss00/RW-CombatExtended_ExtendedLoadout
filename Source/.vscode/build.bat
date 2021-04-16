echo off

echo remove unnecessary assemblies
DEL ..\*\Assemblies\*.* /Q /F
DEL ..\Assemblies\*.* /Q /F

echo build dll: %1
dotnet build .vscode --configuration %1