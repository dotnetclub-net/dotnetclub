#!/usr/bin/env bash

targets=""
repoFolder=""
while [[ $# > 0 ]]; do
    case $1 in
        -r)
            shift
            repoFolder=$1
            ;;
        *)
            targets+=" $1"
            ;;
    esac
    shift
done
if [ ! -e "$repoFolder" ]; then
    printf "Usage: $filename -r [repoFolder] [ [targets] ]\n\n"
    echo "       -r [repo]     The repository to build"
    echo "       [targets]     A space separated list of targets to run"
    exit 1
fi

echo "Building $repoFolder"
cd $repoFolder

scriptRoot="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Make the path relative to the repo root because Sake/Spark doesn't support full paths
koreBuildFolder="${scriptRoot/$repoFolder/}"
koreBuildFolder="${koreBuildFolder#/}"

if test `uname` = Darwin; then
    versionFileName="cli.version.darwin"
else
    versionFileName="cli.version.unix"
fi
versionFile="$koreBuildFolder/$versionFileName"
version=$(<$versionFile)

[ -z "$KOREBUILD_DOTNET_CHANNEL" ] && KOREBUILD_DOTNET_CHANNEL=preview
[ -z "$KOREBUILD_DOTNET_VERSION" ] && KOREBUILD_DOTNET_VERSION=$version

if [ ! -z "$KOREBUILD_SKIP_RUNTIME_INSTALL" ]; then
    echo "Skipping runtime installation because KOREBUILD_SKIP_RUNTIME_INSTALL is set"

    # Add .NET installation directory to the path if it isn't yet included.
    # Add to the _end_ in case preferred .NET CLI is not in the default location.
    [[ ":$PATH:" != *":$DOTNET_INSTALL_DIR:"* ]] && export PATH="$PATH:$DOTNET_INSTALL_DIR"
else
    # Need to set this variable because by default the install script
    # requires sudo
    export DOTNET_INSTALL_DIR=~/.dotnet
    export KOREBUILD_FOLDER="$(dirname $koreBuildFolder)"
    chmod +x $koreBuildFolder/dotnet/dotnet-install.sh

    $koreBuildFolder/dotnet/dotnet-install.sh --channel $KOREBUILD_DOTNET_CHANNEL --version $KOREBUILD_DOTNET_VERSION

    # Add .NET installation directory to the path if it isn't yet included.
    [[ ":$PATH:" != *":$DOTNET_INSTALL_DIR:"* ]] && export PATH="$DOTNET_INSTALL_DIR:$PATH"
fi


# workaround for CLI issue: https://github.com/dotnet/cli/issues/2143
DOTNET_PATH=`which dotnet | head -n 1`
ROOT_PATH=`dirname $DOTNET_PATH`
FOUND=`find $ROOT_PATH/shared -name dotnet`
if [ ! -z "$FOUND" ]; then
    echo $FOUND | xargs rm
fi

if [ "$(uname)" == "Darwin" ]; then
    ulimit -n 2048
fi

sakeFolder=$koreBuildFolder/Sake
if [ ! -d $sakeFolder ]; then
    toolsProject="$koreBuildFolder/project.json"
    dotnet restore "$toolsProject" --packages $scriptRoot -v Minimal
    # Rename the project after restore because we don't want it to be restore afterwards
    mv "$toolsProject" "$toolsProject.norestore"
fi

nugetPath="$koreBuildFolder/nuget.exe"
if [ ! -f $nugetPath ]; then
    nugetUrl="https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
    wget -O $nugetPath $nugetUrl 2>/dev/null || curl -o $nugetPath --location $nugetUrl 2>/dev/null
fi

makeFile="makefile.shade"
if [ ! -e $makeFile ]; then
    makeFile="$koreBuildFolder/shade/makefile.shade"
fi

export KOREBUILD_FOLDER="$koreBuildFolder"
mono $sakeFolder/0.2.2/tools/Sake.exe -I $koreBuildFolder/shade -f $makeFile $targets
