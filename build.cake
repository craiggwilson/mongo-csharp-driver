#addin nuget:?package=Cake.Git
#tool nuget:?package=GitVersion.CommandLine

var target = Argument("target", "Default");

// build parameters
var config = Argument("config", "Release");

// versioning parameters
var baseVersion = Argument("baseVersion", "2.4.1");
var preRelease = Argument("preRelease", "local");
var version = GitVersion(new GitVersionSettings
{
	Branch = "master",
	DynamicRepositoryPath = "."
});

// directories
var rootDir = Directory(".\\");
var artifactsDir = rootDir + Directory("artifacts");
var binDir = rootDir + Directory("bin");
var binDirNet45 = binDir + Directory("net45");

// files
var slnFile = rootDir + File("CSharpDriver.sln");

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
});

Task("Build")
	.IsDependentOn("Clean")
	.Does(() =>
{
	NuGetRestore(slnFile);
	MSBuild(slnFile, settings =>
	{
		settings.SetConfiguration(config)
			.WithProperty("TargetFrameworkVersion", "v4.5")
			.WithProperty("OutputPath", binDirNet45);
	});
});


Task("Default")
  .Does(() =>
{
	Information("AssemblySemVer = {0}", version.AssemblySemVer);
	Information("FullSemVer = {0}", version.FullSemVer);
	Information("NuGetVersion = {0}", version.NuGetVersion);
	Information("NuGetVersionV2 = {0}", version.NuGetVersionV2);
	Information("InformationalVersion = {0}", version.InformationalVersion);
	Information("LegacySemVer = {0}", version.LegacySemVer);
	Information("SemVer = {0}", version.SemVer);
});

RunTarget(target);