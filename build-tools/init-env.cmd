@echo off
cd %~dp0


SETLOCAL
SET DNX_FEED=https://www.nuget.org/api/v2
SET BUILDCMD_DNX_VERSION=1.0.0-rc1-update1

IF EXIST ..\.nuget\nuget.exe goto installdnx
md ..\.nuget
copy nuget.exe ..\.nuget\nuget.exe > nul


:installdnx
IF "%BUILDCMD_DNX_VERSION%"=="" (
    SET BUILDCMD_DNX_VERSION=latest
)
IF "%SKIP_DNX_INSTALL%"=="" (
    rem CALL KoreBuild\build\dnvm install %BUILDCMD_DNX_VERSION% -runtime CoreCLR -arch x86 -alias default
    CALL KoreBuild\build\dnvm install %BUILDCMD_DNX_VERSION% -runtime CLR -arch x86 -alias default
) ELSE (
    CALL KoreBuild\build\dnvm use default -runtime CLR -arch x86
)