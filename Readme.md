# OpenASPNET.org Discussion site

* * *



This site is the first site of openaspnet.org, it is a site for members to share thoughts, ask questions, and share knowledge.



We are using Jusfr.Persistent to persist data, and Jusfr.Persistent is a NuGet package that can be install from an internal package source.
Use following steps to setup your source:

1. nuget sources add -Name "czrepo" -Source "http://nuget.openaspnet.org/nuget/czrepo/"
2. nuget sources update -Name "czrepo" -Source "http://nuget.openaspnet.org/nuget/czrepo/" -UserName "dev" -Password "xxxx"
3. nuget install Jusfr.Persistent.Mongo -Source http://nuget.openaspnet.org/nuget/czrepo -Source http://nuget.openaspnet.org/nuget/nuget.org



Want to push a package to that repo?

**nuget push** xxxx.nupkg -Source http://nuget.openaspnet.org/nuget/czrepo/ -ApiKey yyyy