OpenASPNET.ORG
----------------------

**Welcome to the OpenASPNET.ORG project.**

This project is source code for a discussion website, which is used to demonstrate how asp.net core can be used to make a user generated web application. The online instance of this project is hosted at [openaspnet.org](http://openaspnet.org) which is exactly a real community for discussing asp.net core technical topics.

This project is a web application based on the [ASP.NET Core RC2](https://github.com/aspnet/Home/tree/v1.0.0-rc1-update1) open source and cross platform framework. Notice that, currently this project runs on the full CLR runtime (depends on full .NET framework or Mono). Maintainer is now working hard to migrate it to pure [.net core](https://dotnet.github.io/) runtime. Due to some compatibility issues, currently this project fails to run on the Mono runtime, after being migrated to .net core runtime, problems will be solved.

AppVeyor: dev [![AppVeyor Status-dev](https://ci.appveyor.com/api/projects/status/pecgpkageltpj13x/branch/dev?svg=true)](https://ci.appveyor.com/project/jijiechen/openaspnetorg/branch/dev)  master [![AppVeyor Status-master](https://ci.appveyor.com/api/projects/status/pecgpkageltpj13x/branch/master?svg=true)](https://ci.appveyor.com/project/jijiechen/openaspnetorg/branch/master)

Travis: dev  ![Travis Status-dev](https://travis-ci.org/jijiechen/openaspnetorg.svg?branch=dev)   master ![Travis Status-master](https://travis-ci.org/jijiechen/openaspnetorg.svg?branch=master)

### Using the source

First, clone this repository and compile it to get everything ready:

``` 
git clone https://github.com/jijiechen/openaspnetorg.git
cd openaspnetorg
```

You can work on the source with any text editor or IDE.

To restore packages and compile:

``` 
./build build-all
```

To execute tests:

``` 
./build test
```

&nbsp;

### Installation

You need the asp.net core basic environment to run this application. It's pretty simple to run locally when following these steps:

``` 
git clone https://github.com/jijiechen/openaspnetorg.git
cd openaspnetorg
./build build-all
cd src/Discussion.Web
dnx web
```

By default, the application will use in-memory storage, which will be cleared on process exiting. If you want a persistent storage, please setup a mongo instance and configure it into appsettings.json file.

Besides, you can also package artifacts and publish this project to a server. &nbsp;

``` 
./build publish
```

By executing this command, you can get output generated for deploying. Go to `src/Discussion.Web/bin/output` and push contents to your server.

&nbsp;

### Contributing

Since this project is still under development, so It will be great to have your contribution. 
If you are planning to push back some code, please use the [Git Flow](http://nvie.com/posts/a-successful-git-branching-model/), when you are going to fix an issue or submit a feature, please create a branch for your work and then issue a pull request through that branch.
This project is open source under [the MIT license](https://opensource.org/licenses/MIT).

&nbsp;

&nbsp;