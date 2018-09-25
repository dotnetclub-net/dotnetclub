dotnetClub.net
----------------------

**Welcome to the dotnetClub.net project. [点此查看中文说明](https://github.com/jijiechen/dotnetclub/blob/dev/readme.md)。**

This project is source code for a discussion website, which is used to demonstrate how ASP.NET Core can be used to make a user generated web application. The online instance of this project is hosted at [dotnetclub.net](http://dotnetclub.net) which is exactly a real community for discussing .NET Core technical topics.

This project is a web application based on the [.NET Core 2.1](https://www.microsoft.com/net/download/dotnet-core/2.1) open source and cross platform framework. This project will run on [.net core](https://dotnet.github.io/) runtime, but the devlopment tasks still rely on full CLR frameworks(.NET Frmework on Windows and Mono on other systems).

| Build server| Branch         | Platform       | Status                                                                                                                                                                                             |
|-------------|----------------|----------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Travis      | dev            | Linux          | [![Travis Status-dev](https://travis-ci.org/jijiechen/dotnetclub.svg?branch=dev)](https://travis-ci.org/jijiechen/dotnetclub/branches)                                                       |
| AppVeyor    | dev            | Windows        | [![AppVeyor Status-dev](https://ci.appveyor.com/api/projects/status/pecgpkageltpj13x/branch/dev?svg=true)](https://ci.appveyor.com/project/jijiechen/dotnetclub/branch/dev)                     |
| Travis      | master         | Linux          | [![Travis Status-master](https://travis-ci.org/jijiechen/dotnetclub.svg?branch=master)](https://travis-ci.org/jijiechen/dotnetclub/branches)                                                 |
| AppVeyor    | master         | Windows        | [![AppVeyor Status-master](https://ci.appveyor.com/api/projects/status/pecgpkageltpj13x/branch/master?svg=true)](https://ci.appveyor.com/project/jijiechen/dotnetclub/branch/master)            |


### Using the source

First, clone this repository and compile it to get everything ready:

```sh
git clone https://github.com/jijiechen/dotnetclub.git
cd dotnetclub
```

You can work on the source with any text editor or IDE. This project uses the [cakebuild](https://cakebuild.net) as a build tool. Please change the `./build` in sample commands on this page to corresponding cakebuild entrypoint file: 

* Windows: `build.ps1`
* Linux: `build-linux.sh`
* macOS: `build.sh`

To restore packages and compile:

```sh
./build --target=build-all   # you may need to change the entrypoint file name
```

To execute tests:

```sh
./build --target=cs-test   # you may need to change the entrypoint file name
```

&nbsp;

### Installation

It's recommended to run this application using [Docker](https://www.docker.com/). You can run the application using this command after docker is installed:

```sh
docker run -d --name club -p 5000:5000 jijiechen/dotnetclub:201809231041
```

You can also compile and run it locally, in that case you'll need .NET Core SDK and node.js tools, and also Mono if you are not working on a Windows PC. After you get a good environment, it's pretty simple to run locally when following these steps:

```sh
git clone https://github.com/jijiechen/dotnetclub.git
cd dotnetclub
./build --target=ci  # you may need to change the entrypoint file name 
cd src/Discussion.Web/publish
dotnet ./Discussion.Web.dll
```

By default, the application will generate a temporary Sqlite database, which will be deleted on process exiting.  If you want to persiste your data, please configure your connection string in configuration files (`appsettings.json`, or `appsettings.<env>.js`)

&nbsp;

### Contributing

Since this project is still under development, so It will be great to have your contribution. 
If you are planning to push back some code, please use the [Git Flow](http://nvie.com/posts/a-successful-git-branching-model/), when you are going to fix an issue or submit a feature, please create a branch for your work and then issue a pull request through that branch.
This project is open source under [the MIT license](https://opensource.org/licenses/MIT).

&nbsp;

&nbsp;