#!/usr/bin/env bash
export DNX_FEED=https://www.nuget.org/api/v2
build_dnx_version=1.0.0-rc1-update1

if test ! -f ./.nuget/nuget.exe; then
	if test ! -e .nuget; then
		mkdir .nuget
	fi
    cp ./build-tools/nuget.exe ./.nuget/nuget.exe
fi

if ! type dnx > /dev/null 2>&1 || [ -z "$SKIP_DNX_INSTALL" ]; then
	source ./build-tools/KoreBuild/build/dnvm.sh
    dnvm install %build_dnx_version -p -runtime coreclr -alias default
    dnvm install %build_dnx_version -p -runtime mono -alias default
else
	source ./build-tools/KoreBuild/build/dnvm.sh
    dnvm use default -p -runtime mono
fi

mono ./build-tools/Sake/tools/Sake.exe -I KoreBuild/build "$@"
