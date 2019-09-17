#tool "nuget:?package=OctopusTools"
#addin "Cake.Git"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var solutionFolder = "../";
var solutionFile = solutionFolder + "Zupa.Authentication.sln";
var mainProjectFile = GetFiles(solutionFolder + "src/Zupa.Authentication.AuthService/*.csproj").Single();
var setupProjectFile = GetFiles(solutionFolder + "src/Zupa.Authentication.ReleaseSetupClient/*.csproj").Single();
var projectFiles = new [] {
    mainProjectFile,
    setupProjectFile
};
var artifactsFolder = "../artifacts/";
var publishFolder = artifactsFolder + "publish/";

var octopusDeployUrl = EnvironmentVariable("OctopusDeployUrl");
var octopusDeployApiKey = EnvironmentVariable("OctopusDeployApiKey");

var defaultBranchName = "master";
var isMasterBranch = false;

Task("Default")
    .IsDependentOn("GetBranchName")
    .IsDependentOn("clean")
    .IsDependentOn("restore")
    .IsDependentOn("build")
    .IsDependentOn("test")
    .IsDependentOn("publish")
    .IsDependentOn("pack")
    .IsDependentOn("push");

Task("GetBranchName")
    .Does(() => {       
        var repositoryDirectoryPath = DirectoryPath.FromString(solutionFolder);
        isMasterBranch = (GitBranchCurrent(repositoryDirectoryPath).FriendlyName == defaultBranchName);
});

Task("clean")
    .Does(() => {
        DotNetCoreClean(solutionFile);
        CleanDirectory(artifactsFolder);
});

Task("restore")
    .Does(() => {
        DotNetCoreRestore(solutionFile);
});

Task("build")
    .Does(() => {
        DotNetCoreBuild(
            solutionFile, 
            new DotNetCoreBuildSettings
            {
                Configuration = configuration
            });
});

Task("test")
	.Does(() => {
        var unitTestProjects = GetFiles("../**/*.UnitTests.csproj");
        
        foreach(var project in unitTestProjects) {
            DotNetCoreTest(project.FullPath);
        }

        var componentTestProjects = GetFiles("../**/*.ComponentTests.csproj");
        
        foreach(var project in componentTestProjects) {
            DotNetCoreTest(project.FullPath);
        }
});

Task("publish")
    .Does(() => {
        var mainProjectSettings = new DotNetCorePublishSettings
        {
            Framework = XmlPeek(mainProjectFile.FullPath, "Project/PropertyGroup/TargetFramework"),
            Configuration = configuration,
            OutputDirectory = $"{publishFolder}{XmlPeek(mainProjectFile.FullPath, "Project/PropertyGroup/PackageId")}",
        };
        DotNetCorePublish(mainProjectFile.FullPath, mainProjectSettings);

        var setupProjectSettings = new DotNetCorePublishSettings
        {
            Framework = XmlPeek(setupProjectFile.FullPath, "Project/PropertyGroup/TargetFramework"),
            Configuration = configuration,
            OutputDirectory = $"{publishFolder}{XmlPeek(setupProjectFile.FullPath, "Project/PropertyGroup/PackageId")}",
            Runtime = "win-x64",
            Sources = new[] {"https://api.nuget.org/v3/index.json"}
        };
        DotNetCorePublish(setupProjectFile.FullPath, setupProjectSettings);
});

Task("copyDependencies")
    .Does(()=>{
        var mainPackageId =  XmlPeek(mainProjectFile.FullPath, "Project/PropertyGroup/PackageId");
        CopyFiles("./*.json", $"{publishFolder}{mainPackageId}/");
        CopyFiles("./*.ps1", $"{publishFolder}{mainPackageId}/");

        var setupPackageId =  XmlPeek(setupProjectFile.FullPath, "Project/PropertyGroup/PackageId");
        CopyFiles("../scripts/RunConfig.ps1", $"{publishFolder}{setupPackageId}/");
    });

Task("pack")
    .IsDependentOn("copyDependencies")
    .Does(() => {
        foreach(var project in projectFiles)
        {
            var packageId =  XmlPeek(project.FullPath, "Project/PropertyGroup/PackageId");
            var octoSettings = new OctopusPackSettings
            {
                BasePath = $"{publishFolder}{packageId}",
                OutFolder = artifactsFolder,
                Author = XmlPeek(project.FullPath, "Project/PropertyGroup/Company"),
                Title = packageId,
                Version = XmlPeek(project.FullPath, "Project/PropertyGroup/Version")
            };
            OctoPack(packageId, octoSettings);
        }
});

Task("push")
    .WithCriteria(() => isMasterBranch)
    .Does(() => {
        foreach(var project in projectFiles)
        {
            var url = octopusDeployUrl;
            var apiKey = octopusDeployApiKey;
            var settings = new OctopusPushSettings();
            var packageVersion = XmlPeek(project.FullPath, "Project/PropertyGroup/Version");
            var packageId =  XmlPeek(project.FullPath, "Project/PropertyGroup/PackageId");
            var package = $"{artifactsFolder}/{packageId}.{packageVersion}.nupkg";
            OctoPush(url, apiKey, package, settings);
        }
});

RunTarget(target);
