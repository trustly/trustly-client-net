
// OpenApi 3.0 / Swagger
#addin nuget:?package=NSwag.CodeGeneration.CSharp&version=13.0.3&loaddependencies=true
#addin nuget:?package=NSwag.CodeGeneration.TypeScript&version=13.0.3&loaddependencies=true // Not used, but required by Cake.CodeGen.NSwag
#addin nuget:?package=Cake.CodeGen.NSwag&version=1.2.0&loaddependencies=false

// NPM for OpenRPC generator
#addin nuget:?package=Cake.Npm&version=1.0.0

var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

// https://cakebuild.net/api/Cake.Common.Tools.Chocolatey/ChocolateyAliases/B1E32DCB

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    CleanDirectory($".\\src\\*\\bin\\{configuration}");
    CleanDirectory($".\\src\\*\\obj\\{configuration}");
});

Task("Generate-Clean")
    .Does(() =>
{
    //CleanDirectory($".\\src\\*\\generated-sources");
    if (DirectoryExists($".\\src\\Domain\\generated-sources")) {
        DeleteDirectory($".\\src\\Domain\\generated-sources", new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });
    }
});

Task("Generate-OpenAPI")
    .Does(() =>
{
    var settings = new NpmInstallSettings();

    settings.Global = true; // Global = True is better? How to run the script as global then?
    settings.Production = false;
    settings.LogLevel = NpmLogLevel.Verbose;
    settings.WorkingDirectory = "./src/Domain";

    //settings.AddPackage("@open-rpc/generator");

    NpmInstall(settings);

    NpmRunScript("openapi-generate", settings => {
        settings.WorkingDirectory = "./src/Domain";
        settings.LogLevel = NpmLogLevel.Verbose;
    });

    if (!DirectoryExists($".\\src\\Domain\\generated-sources\\openapi")) {
        CreateDirectory($".\\src\\Domain\\generated-sources\\openapi");
    }

    if (!DirectoryExists($".\\src\\Domain\\generated-sources\\openapi\\Trustly.Api.Domain")) {
        CreateDirectory($".\\src\\Domain\\generated-sources\\openapi\\Trustly.Api.Domain");
    }

    MoveDirectory(
        $".\\src\\Domain\\generated-sources\\openapi-temp\\src\\Trustly.Api.Domain\\Model",
        $".\\src\\Domain\\generated-sources\\openapi\\Trustly.Api.Domain\\Model"
    );

    DeleteDirectory(
        $".\\src\\Domain\\generated-sources\\openapi-temp",
        new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        }
    );
});


Task("Generate-OpenRPC").Does(() => {

});

Task("Generate")
    .IsDependentOn("Generate-Clean")
    .IsDependentOn("Generate-OpenRPC")
    .IsDependentOn("Generate-OpenAPI")
    ;


Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Generate")
    .Does(() =>
{
    DotNetCoreBuild("./trustly-client-net.sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest("./trustly-client-net.sln", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);






// dotnet nuget push AppLogger.1.0.0.nupkg --api-key <key> --source https://api.nuget.org/v3/index.json