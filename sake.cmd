@echo off
cd %~dp0

.\build-tools\Sake\tools\Sake.exe -I build-tools\KoreBuild\build %*
