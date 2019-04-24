FROM openshift/jenkins-slave-base-centos7

USER root

RUN rpm --import "https://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF"
RUN curl -s https://download.mono-project.com/repo/centos7-stable.repo | tee /etc/yum.repos.d/mono-centos7-stable.repo
RUN rpm -Uvh "https://packages.microsoft.com/config/rhel/7/packages-microsoft-prod.rpm"

RUN yum update -y && yum install -y mono-devel "dotnet-sdk-2.1"

# NodeJS v10.15.3 (LTS), EoL 2021-04-01
RUN curl -s -o nodejs.rpm https://rpm.nodesource.com/pub_10.x/el/7/x86_64/nodejs-10.15.3-1nodesource.x86_64.rpm && rpm -Uvh nodejs.rpm
RUN curl -s -L https://dl.yarnpkg.com/rpm/yarn.repo | tee /etc/yum.repos.d/yarn.repo
RUN rpm --import https://dl.yarnpkg.com/rpm/pubkey.gpg
RUN yum install -y yarn


USER 1001