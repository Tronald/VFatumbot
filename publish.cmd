nuget restore
msbuild VFatumbot.sln -p:DeployOnBuild=true -p:PublishProfile=v***REMOVED***-Web-Deploy.pubxml -p:Password=***REMOVED***

