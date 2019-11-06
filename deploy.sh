#!/bin/bash

nuget restore

# 64-bit
#cp ../libAttract/libAttract.0.95.5/Windows/Release/x64/libAttract.dll .
#msbuild VFatumbot.sln -p:Configuration=Release -p:Platform=x64 -p:DeployOnBuild=true -p:PublishProfile=v***REMOVED***-Web-Deploy.pubxml -p:Password=***REMOVED***
#msbuild VFatumbot.sln -p:Configuration=Release -p:Runtime=win-x64 -p:DeployOnBuild=true -p:PublishProfile=v***REMOVED***-Web-Deploy.pubxml -p:Password=***REMOVED***

# 32-bit
cp ../libAttract/libAttract.0.95.5/Windows/Release/Win32/libAttract.dll .
msbuild VFatumbot.sln -p:DeployOnBuild=true -p:PublishProfile=v***REMOVED***-Web-Deploy.pubxml -p:Password=***REMOVED***
#cp ../libAttract/libAttract.0.95.5/Windows/Debug/x64/libAttract.dll .
