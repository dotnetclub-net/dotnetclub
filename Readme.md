OpenASPNET.ORG
----------------------

**Welcome to the OpenASPNET.ORG project.**

This project is source code for a discussion website, which is used to demonstrate how asp.net core can be used to make a user generated web application. The online instance of this project is hosted at [openaspnet.org](http://openaspnet.org) which is exactly a real community for discussing asp.net core technical topics.

This project is a web application based on the [ASP.NET Core RC2](https://github.com/aspnet/Home/tree/v1.0.0-rc1-update1) open source and cross platform framework. This project will run on [.net core](https://dotnet.github.io/) runtime, but unit testing will still rely on full CLR frameworks(.NET Frmework on Windows and Mono on other systems).

| Build server| Branch         | Platform       | Status                                                                                                                                                                                             |
|-------------|----------------|----------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Travis      | dev            | Linux          | [![Travis Status-dev](https://travis-ci.org/jijiechen/openaspnetorg.svg?branch=dev)](https://travis-ci.org/jijiechen/openaspnetorg/branches)                                                       |
| AppVeyor    | dev            | Windows        | [![AppVeyor Status-dev](https://ci.appveyor.com/api/projects/status/pecgpkageltpj13x/branch/dev?svg=true)](https://ci.appveyor.com/project/jijiechen/openaspnetorg/branch/dev)                     |
| Travis      | master         | Linux          | [![Travis Status-master](https://travis-ci.org/jijiechen/openaspnetorg.svg?branch=master)](https://travis-ci.org/jijiechen/openaspnetorg/branches)                                                 |
| AppVeyor    | master         | Windows        | [![AppVeyor Status-master](https://ci.appveyor.com/api/projects/status/pecgpkageltpj13x/branch/master?svg=true)](https://ci.appveyor.com/project/jijiechen/openaspnetorg/branch/master)            |


### Using the source

First, clone this repository and compile it to get everything ready:

``` 
git clone https://github.com/jijiechen/openaspnetorg.git
cd openaspnetorg
```

You can work on the source with any text editor or IDE.

To restore packages and compile:

``` 
./build --target=build-all
```

To execute tests:

``` 
./build --target=cs-test
```

&nbsp;

### Installation

You need the asp.net core basic environment to run this application. It's pretty simple to run locally when following these steps:

``` 
git clone https://github.com/jijiechen/openaspnetorg.git
cd openaspnetorg
./build --target=build-all
cd src/Discussion.Web
dotnet run
```

By default, the application will use in-memory storage, which will be cleared on process exiting. 

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