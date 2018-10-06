dotnetClub.net
----------------------

**欢迎关注 dotnetClub.net 项目。 [Click here to see English readme](https://github.com/jijiechen/dotnetclub/blob/dev/readme-en.md).**

本项目是一个论坛网站的源码，完整地展示了如何用 ASP.NET Core 技术开发一个用户参与的 Web 应用。您可以在 [dotnetclub.net](http://dotnetclub.net) 直接访问本项目的在线实例，并且在该网站上参与 .NET Core 相关技术的讨论。

本项目是一个 Web 应用程序，基于开源和跨平台的 [.NET Core 2.1](https://www.microsoft.com/net/download/dotnet-core/2.1) 运行时。如果你打算研究或者参与贡献，您仍需要完整的 .NET Framework 或者 Mono 运行时来运行相关的开发任务。 


| 分支            | 平台            | 当前状态      |                                                                                                                                                                                              |
|----------------|----------------|--------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| dev            | Linux          | Travis       | [![Travis Status-dev](https://travis-ci.org/jijiechen/dotnetclub.svg?branch=dev)](https://travis-ci.org/jijiechen/dotnetclub/branches)                                                       |
| dev            | Windows        | AppVeyor     | [![AppVeyor Status-dev](https://ci.appveyor.com/api/projects/status/pecgpkageltpj13x/branch/dev?svg=true)](https://ci.appveyor.com/project/jijiechen/dotnetclub/branch/dev)                  |
| master         | Linux          | Travis       | [![Travis Status-master](https://travis-ci.org/jijiechen/dotnetclub.svg?branch=master)](https://travis-ci.org/jijiechen/dotnetclub/branches)                                                 |
| master         | Windows        | AppVeyor     | [![AppVeyor Status-master](https://ci.appveyor.com/api/projects/status/pecgpkageltpj13x/branch/master?svg=true)](https://ci.appveyor.com/project/jijiechen/dotnetclub/branch/master)         |



&nbsp;

[![codecov](https://codecov.io/gh/jijiechen/dotnetclub/branch/dev/graph/badge.svg)](https://codecov.io/gh/jijiechen/dotnetclub) [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

&nbsp;

### 使用源代码


首先，克隆当前仓库，并完成编译一次，就可以将所需要的环境准备好：


``` 
git clone https://github.com/jijiechen/dotnetclub.git
cd dotnetclub
```

本项目使用了 [cakebuild](https://cakebuild.net) 作为构建工具。在执行下面描述的构建命令时，请将 `./build` 换成您自己所在平台对应的文件：

* Windows: `build.ps1`
* Linux: `build-linux.sh`
* macOS: `build.sh`

可以使用下面的命令，来还源、安装所有依赖的包，并编译项目：

```sh
./build --target=build   # 可能需要修改入口文件的名字
```

可以使用下面的命令，来运行单元测试：

```sh
./build --target=test     # 可能需要修改入口文件的名字
```

&nbsp;

### 安装


如果只是要运行本项目，您可以直接使用 [Docker](https://www.docker.com/) 来安装。下面的命令可以帮助您在本地运行一个示例：

```sh
docker run -d --name club -p 5000:5000 jijiechen/dotnetclub:201809260349
```

您也可以直接在本地编译并运行，但您需要在本地安装 .NET Core SDK、node.js （如果是在非 Windows 电脑上，还需要 Mono）等一系列依赖才能编译。环境准备就绪后，实际的编译过程很简单：


```sh
git clone https://github.com/jijiechen/dotnetclub.git
cd dotnetclub
./build --target=ci      # 可能需要修改入口文件的名字
cd src/Discussion.Web/publish
dotnet ./Discussion.Web.dll
```

默认情况下，应用程序会自动创建一个 Sqlite 数据库用于存储，这个数据库会在进程退出时失效。因此，如果你需要留存这些数据话，请修改 `appsettings.json` 或者 `appsettings.<环境>.json` 配置文件来配置数据库位置。

&nbsp;

### 贡献代码

当前项目还处于活跃的开发之中，非常欢迎您参与贡献。如果您打算提交代码，请提前在 [Projects](https://github.com/jijiechen/dotnetclub/projects) 页面上了解最新开发计划，并按照 [GitHub Flow](https://help.github.com/articles/github-flow/) 的流程，使用 [Pull Request](https://help.github.com/articles/about-pull-requests/) 的方式提交代码。简单来说，就是在编写功能或者修复问题时，先创建一个对应的分支，然后再从那个分支提交 Pull Request。

本项目采用 [MIT 开源协议](LICENSES) 开源。

&nbsp;

&nbsp;