var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

////////////////////////////////////////////////////////////////
// Tasks

Task("Build")
    .Does(context =>
{
    DotNetBuild("./src/RadLine.sln", new DotNetBuildSettings {
        Configuration = configuration,
        NoIncremental = context.HasArgument("rebuild"),
        MSBuildSettings = new DotNetMSBuildSettings()
            .TreatAllWarningsAs(MSBuildTreatAllWarningsAs.Error)
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(context =>
{
    DotNetTest("./src/RadLine.Tests/RadLine.Tests.csproj", new DotNetTestSettings {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true,
    });
});

Task("Package")
    .IsDependentOn("Test")
    .Does(context =>
{
    context.CleanDirectory("./.artifacts");

    context.DotNetPack($"./src/RadLine.sln", new DotNetPackSettings {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true,
        OutputDirectory = "./.artifacts",
        MSBuildSettings = new DotNetMSBuildSettings()
            .TreatAllWarningsAs(MSBuildTreatAllWarningsAs.Error)
    });
});

Task("Publish-NuGet")
    .IsDependentOn("Package")
    .Does(context =>
{
    // Publish to GitHub Packages
    foreach(var file in context.GetFiles("./.artifacts/*.nupkg"))
    {
        EnsureDirectoryDoesNotExist("C:/Nuget/packages/radline/" + file.GetFilename().GetFilenameWithoutExtension().ToString()[8..], new DeleteDirectorySettings {
            Recursive = true,
            Force = true,
        });
        EnsureDirectoryDoesNotExist(EnvironmentVariable("USERPROFILE") + "/.nuget/packages/radline/" + file.GetFilename().GetFilenameWithoutExtension().ToString()[8..], new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });
        context.Information("Publishing {0}...", file.GetFilename().FullPath);
        DotNetNuGetPush(file.FullPath, new DotNetNuGetPushSettings
        {
            Source = "C:/Nuget/Packages",
        });
    }
});

////////////////////////////////////////////////////////////////
// Targets

Task("Publish")
    .IsDependentOn("Publish-NuGet");

Task("Default")
    .IsDependentOn("Package");

////////////////////////////////////////////////////////////////
// Execution

RunTarget(target)