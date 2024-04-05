echo off
dotnet build --nologo --configuration Release .
rm -rf obj
