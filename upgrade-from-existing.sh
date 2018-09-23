#!/bin/bash

# this script is executed in an transitional upgrading image

set -e

oldVer=$1
newVer=$2


# todo: Make existing site readonly



if [ ! -d /club-data/$newVer/ ]; then
    mkdir /club-data/$newVer/
    chmod -R 777 /club-data/$newVer/
fi

cp /club-data/$oldVer/appsettings.Production.json /club-data/$newVer/
cp /club-data/$oldVer/dotnetclub.db /club-data/$newVer/


dotnet /club-app/Discussion.Migrations.dll "Data Source=/club-data/$newVer/dotnetclub.db"


