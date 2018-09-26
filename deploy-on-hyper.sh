#!/bin/bash

set -e

oldVer=$1
newVer=$2

RED=`tput setaf 1`
GREEN=`tput setaf 2`
NC=`tput sgr0` # no color


if [ "$newVer" == "" ] || [ "$oldVer" == "" ]; then
    echo "${RED}请指定要从哪个版本升级到哪个版本！${NC}"
    echo "${RED}比如，从第 001 升级到 003，请指定 deploy-on-hyper.sh 001 003${NC}"
    exit 1
fi


new_image="jijiechen/dotnetclub:$newVer"

if [ "$oldVer" == "0" ]; then
    # 首次安装时，创建 volume
    hyper volume create --size=10 --name clubdata

    hyper run -d --name volumes -v clubdata:/club-data --size S3 --restart always hyperhq/nfs-server
   
    hyper exec volumes mkdir /club-data/$newVer
    hyper exec volumes chmod -R 777 /club-data/$newVer
    hyper exec -i volumes tee /club-data/$newVer/appsettings.Production.json < ./src/Discussion.Web/appsettings.Production.json
else
    
    hyper exec volumes mkdir /club-data/$newVer
    hyper exec volumes chmod -R 777 /club-data/$newVer
    # 从旧版本升级到新版本
    hyper run --rm --name upgrade --volumes-from volumes --entrypoint "/club-app/upgrade-from-existing.sh" $new_image $oldVer $newVer
fi


# 运行新版本的程序
hyper run -d -p 80:5000 --name "club$newVer" \
    --volumes-from volumes --size S4 --restart always \
    -e "ASPNETCORE_contentRoot=/club-data/$newVer" --workdir /club-data/$newVer $new_image

# 分配新的 fip
fip=`hyper fip allocate --yes 1`
hyper fip attach $fip "club$newVer"


# 输出部署结果
echo ""
echo "${GREEN}A new version ($new_image) is deployed at IP:$fip${NC}"

# todo: your dns???


# hyper stop $oldVer 
# hyper fip detach $oldVer
# hyper fip release xxxx 