#!/usr/bin/env bash


installerFolder="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
echo "Installer script is at $installerFolder"



dotnetLocalInstallFolder=$installerFolder/../dotnet/


if [ ! -d $dotnetLocalInstallFolder ]; then
    mkdir $dotnetLocalInstallFolder
fi

if [ ! -f $dotnetLocalInstallFolder/dotnet ]; then
    echo "Installer runtime to $dotnetLocalInstallFolder"

    if test `uname` = Darwin; then
        versionFileName="cli.version.darwin"
    else
        versionFileName="cli.version.unix"
    fi
    versionFile="$installerFolder/$versionFileName"
    version=$(<$versionFile)

    DOTNET_CHANNEL=preview
    DOTNET_VERSION=$version

    # Need to set this variable because by default the install script
    # requires sudo
    # export DOTNET_INSTALL_DIR=~/.dotnet
    chmod +x $installerFolder/dotnet-install.sh

    $installerFolder/dotnet-install.sh --channel $DOTNET_CHANNEL --version $DOTNET_VERSION --install-dir $dotnetLocalInstallFolder

    # workaround for CLI issue: https://github.com/dotnet/cli/issues/2143
    FOUND=`find $dotnetLocalInstallFolder/shared -name dotnet`
    if [ ! -z "$FOUND" ]; then
        echo $FOUND | xargs rm
    fi

    if [ "$(uname)" == "Darwin" ]; then
        ulimit -n 2048
    fi
else
    echo "Runtime exists at $dotnetLocalInstallFolder"
fi




