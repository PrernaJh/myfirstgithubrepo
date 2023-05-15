SET SONAR_TOKEN=fc305a3b7122a574ecc4bd51f8ffc7bf4f3ebb24
SonarScanner.MSBuild.exe begin /o:"tecmailingllc" /k:"TECMailingLLC_ParcelPrepGov" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login=%SONAR_TOKEN%
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" PackageTracker.sln -t:Rebuild -p:Configuration=Release
SonarScanner.MSBuild.exe end /d:sonar.login=%SONAR_TOKEN%