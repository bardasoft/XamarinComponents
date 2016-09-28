
#load "../../common.cake"

var TARGET = Argument ("t", Argument ("target", "Default"));

var IOS_PODS = new List<string> {
	"platform :ios, '7.0'",
	"install! 'cocoapods', :integrate_targets => false",
	"target 'Xamarin' do",
	"pod 'AMViralSwitch', '1.0.0'",
	"end",
};

var buildSpec = new BuildSpec () {
	Libs = new ISolutionBuilder [] {
		new IOSSolutionBuilder {
			SolutionPath = "./AMViralSwitch.sln",
			Configuration = "Release",
			BuildsOn = BuildPlatforms.Mac,
			OutputFiles = new [] { 
				new OutputFileCopy {
					FromFile = "./source/AMViralSwitch/bin/unified/Release/AMViralSwitch.dll",
					ToDirectory = "./output/unified/"
				},
				new OutputFileCopy {
					FromFile = "./source/AMViralSwitch/bin/classic/Release/AMViralSwitch.dll",
					ToDirectory = "./output/classic/"
				}
			}
		},
	},

	Samples = new ISolutionBuilder [] {
		new IOSSolutionBuilder { SolutionPath = "./samples/AMViralSwitchSample.sln", BuildsOn = BuildPlatforms.Mac},
	},

	Components = new [] {
		new Component {ManifestDirectory = "./component", BuildsOn = BuildPlatforms.Mac},
	},
};

Task ("externals").IsDependentOn ("externals-base").Does (() =>
{
	EnsureDirectoryExists ("./externals");

	if (CocoaPodVersion () < new System.Version (1, 0))
		IOS_PODS.RemoveAt (1);

	FileWriteLines ("./externals/Podfile", IOS_PODS.ToArray ());

	CocoaPodInstall ("./externals", new CocoaPodInstallSettings { NoIntegrate = true });

	BuildXCodeFatLibrary ("./Pods/Pods.xcodeproj", "AMViralSwitch", "AMViralSwitch", null, null, "AMViralSwitch");
});

Task ("clean").IsDependentOn ("clean-base").Does (() =>
{
	DeleteFiles ("./externals/Podfile.lock");
	CleanXCodeBuild ("./Pods/", null);
});

SetupXamarinBuildTasks (buildSpec, Tasks, Task);

RunTarget (TARGET);