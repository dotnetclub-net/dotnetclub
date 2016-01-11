#!/usr/bin/env bash
export DNX_FEED=https://www.nuget.org/api/v2
build_dnx_version=1.0.0-rc1-update1

if test ! -f ../.nuget/nuget.exe; then
	if test ! -e .nuget; then
		mkdir ../.nuget
	fi
    cp ./nuget.exe ../.nuget/nuget.exe
fi

if ! type dnvm > /dev/null 2>&1; then
    source ./KoreBuild/build/dnvm.sh
fi

if ! type dnx > /dev/null 2>&1 || [ -z "$SKIP_DNX_INSTALL" ]; then
    dnvm install %build_dnx_version -runtime coreclr -alias default
    dnvm install %build_dnx_version -runtime mono -alias default
else
    dnvm use default -runtime mono
fi
