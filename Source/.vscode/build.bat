echo off

REM remove unnecessary assemblies
DEL ..\*\Assemblies\*.* /Q /F
DEL ..\Assemblies\*.* /Q /F

REM build dll
dotnet build .vscode