# Download and Install

Hazelcast is composed of two parts: the server, and the client. The client requires a working Hazelcast cluster, composed of one or more servers, in order to run. This cluster handles storage and manipulation of the user data. The client is a library which connects to the cluster, and gives access to such data.

## Hazelcast Client

### Requirements

The Hazelcast .NET client is distributed as a NuGet package which targets [.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) versions 2.0 and 2.1. It can therefore be used in any application targetting .NET versions that support these .NET Standard versions:

* .NET Framework 4.6.2 and above, on Windows
* .NET Core 2.1 and 3.1, on Windows, Linux and MacOS

The upcoming [.NET 5](https://devblogs.microsoft.com/dotnet/introducing-net-5/) version supports .NET Standard 2.1, and therefore should execute the Hazelcast .NET client without issues, but that is not supported yet.

### Distribution

The .NET client is distributed via NuGet as a package named [Hazelcast.NET](https://www.nuget.org/packages/Hazelcast.Net/). 
It can be installed like any other NuGet package, either via the Visual Studio GUI, or via the package manager:

```
PM> Install-Package Hazelcast.NET
```

Or via the .NET CLI:

```
> dotnet add package Hazelcast.NET
```

Or manually added to the project as a package reference:

```
<PackageReference Include="Hazelcast.NET" Version="4.0.0" />
```

### Binding Redirects

When including the `Hazelcast.NET` package in a **.NET Framework** project, be aware that some binding redirects may be required. Please check the [Getting Started](getting-started.md) page for details.

## Hazelcast Server

Browse to [Hazelcast In-Memory Computing](https://hazelcast.com/products/in-memory-computing/) to find out all about the Hazelcast server.

Hazelcast IMDG cluster consists of one or more cluster members. These members generally run on multiple virtual or physical machines and are connected to each other via network. Any data put on the cluster is partitioned to multiple members transparent to the user. It is therefore very easy to scale the system by adding new members as the data grows. Hazelcast IMDG cluster also offers resilience. Should any hardware or software problem causes a crash to any member, the data on that member is recovered from backups and the cluster continues to operate without any downtime. Hazelcast clients are an easy way to connect to a Hazelcast IMDG cluster and perform tasks on distributed data structures that live on the cluster.

There are many different ways to run a Hazelcast cluster or member. The [Installing and Upgrading](https://docs.hazelcast.com/imdg/latest/installation/installing-upgrading.html) section of the Reference Manual details options to install and run a cluster, while the [Deploying in Cloud](https://docs.hazelcast.com/imdg/latest/installation/deploying-in-cloud.html) section details options to run a cluster in the cloud.

If you want to start one member to experiment with the Hazelcast .NET client, two simple ways are possible.

> [!NOTE]
> Running the Hazelcast server requires a Java Runtime Environment. The [Supported JVMs](https://docs.hazelcast.com/imdg/latest/installation/supported-jvms.html) page of the reference details which JVMs are supported. For a quick start, OpenJDK provided by [Adoptium](https://adoptopenjdk.net/) (either version 8, 11 or 16) are OK.

### Standalone JARs

You can download the standalone JARs from the [download page](https://hazelcast.com/get-started/download/). After extracting the downloaded archive, you should find a start script (`start.sh` or `start.bat` depending on your platform) in the `bin` directory, which you can use to start a member.

### Powershell Script

The Hazelcast .NET client repository on [GitHub](https://github.com/hazelcast/hazelcast-csharp-client) provides a Powershell script which can be used to download and run a test member. For instance, this downloads and starts a member version 4.2:

```pwsh
PS> ./hz.ps1 run-server -server 4.2
```



