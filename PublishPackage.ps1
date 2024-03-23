param(
    [Parameter(Mandatory=$true)][string] $version,
    [Parameter(Mandatory=$true)][string] $nugetApiKey
)

$artifacts = ".\artifacts"
if(Test-Path $artifacts) { Remove-Item $artifacts -Force -Recurse }

dotnet clean -c Release
dotnet build -c Release
dotnet test -c Release --no-build -l trx --verbosity=normal
dotnet pack .\src\QuickSearch\QuickSearch.csproj /p:Version=$version -c Release -o $artifacts --no-build

$packagePath = Join-Path $artifacts "QuickSearch.$version.nupkg"
dotnet nuget push $packagePath --source https://api.nuget.org/v3/index.json --api-key $nugetApiKey