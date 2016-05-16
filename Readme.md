OpenASPNET.ORG
----------------------

**Welcome to the OpenASPNET.ORG project.**

This project is source code for a discussion website, which is used to demonstrate how asp.net core can be used to make a user generated web application. The online instance of this project is hosted at [openaspnet.org](http://openaspnet.org) which is exactly a real community for discussing asp.net core technical topics.

&nbsp;

### Using the source

First, clone this repository and compile it to get everything ready:

``` 
git clone https://github.com/jijiechen/openaspnetorg.git
cd openaspnetorg
```

You can work on the source with any text editor or IDE.

To restore packages and compile:

``` 
./sake build-all
```

To execute tests:&nbsp;

``` 
./sake test
```

&nbsp;

### Installation

You need the asp.net core basic environment to run this application. It's pretty simple to run locally when following these steps:

``` 
git clone https://github.com/jijiechen/openaspnetorg.git
cd openaspnetorg
./sake build-all
cd src/Discussion.Web
dnx web
```

By default, the application will use in-memory storage, which will be cleared on process exiting. If you want a persistent storage, please setup a mongo instance and configure it into appsettings.json file.

Besides, you can also package artifacts and publish this project to a server. &nbsp;

``` 
./sake publish
```

By executing this command, you can get output generated for deploying. Go to `src/Discussion.Web/bin/output` and push contents to your server.

&nbsp;

### Contributing

Since this project is still under development, so It will be great to have your contribution. If you are planning to push back some code, please use the **dev** branch as your working branch. Pull reqeusts are only accepted through **dev** branch.
This project is under [the MIT license](https://opensource.org/licenses/MIT).

&nbsp;

&nbsp;

&nbsp;