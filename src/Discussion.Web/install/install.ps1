
function Resolve-FullPath($path){
    return $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($path)
}

$installerFolder = $PSScriptRoot

Write-Host "Installer script is at $installerFolder"



$dotnetLocalInstallFolder = Resolve-FullPath($installerFolder + "\..\dotnet\")
if (-not (Test-Path $dotnetLocalInstallFolder)) 
{
     New-Item -Path $dotnetLocalInstallFolder -ItemType "directory"
}

$cliPath = Resolve-FullPath($dotnetLocalInstallFolder + "\dotnet.exe")
if (-not (Test-Path $cliPath))
{
    $dotnetVersionFile = $installerFolder + "\cli.version.win"
    $dotnetChannel = "preview"
    $dotnetVersion = Get-Content $dotnetVersionFile

	Write-Host "Installing runtime to $dotnetLocalInstallFolder"
     & "$installerFolder\dotnet-install.ps1" -Channel $dotnetChannel -Version $dotnetVersion -Architecture x64 -InstallDir $dotnetLocalInstallFolder
    
    # wokaround for CLI issue: https://github.com/dotnet/cli/issues/2143
    $sharedPath = (Join-Path (Split-Path ($cliPath) -Parent) "shared");
    (Get-ChildItem $sharedPath -Recurse *dotnet.exe) | %{ $_.FullName } | Remove-Item;     
}else{
	Write-Host "Runtime exsits at $cliPath"
}

