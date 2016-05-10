@echo off
SETLOCAL
SET DNX_FEED=https://www.nuget.org/api/v2
SET BUILDCMD_DNX_VERSION=1.0.0-rc1-update1

IF EXIST .nuget/nuget.exe goto afternuget
md .nuget
copy build-tools/nuget.exe .nuget/nuget.exe > nul


:afternuget
SET FIND_DNX="where.exe dnx"
FOR /F %%i IN (' %FIND_DNX% ') DO SET DNX_EXE_PATH=%%i
IF NOT "%DNX_EXE_PATH%"=="" goto build

IF "%BUILDCMD_DNX_VERSION%"=="" (
    SET BUILDCMD_DNX_VERSION=latest
)
IF "%SKIP_DNX_INSTALL%"=="" (
    CALL build-tools\KoreBuild\build\dnvm install %BUILDCMD_DNX_VERSION% -runtime CLR -arch x64 -alias default
) ELSE (
    CALL build-tools\KoreBuild\build\dnvm use default -runtime CLR -arch x86
)

:build
cd %~dp0
build-tools\Sake\tools\Sake.exe -I build-tools\KoreBuild\build -I build-tools\ext %*
