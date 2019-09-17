Import-Module AzureRM.Profile

if($OctopusParameters['Octopus.Environment.Name'] -eq "Test")
{
    [String]$ConnectionString = $OctopusParameters['ConnectionStrings:DefaultConnection']

    $ConfigProjectExtractPath = $OctopusParameters['ConfigProjectExtractPath']
    $ReleaseClientExtractPath = $OctopusParameters['ReleaseClientExtractPath']

    $AppExe = Join-Path -Path $ReleaseClientExtractPath -ChildPath "\Zupa.Authentication.ReleaseSetupClient.exe"
    $ServerConfig = Join-Path -Path $ConfigProjectExtractPath -ChildPath "\serverconfig.json"
    $UserConfig = Join-Path -Path $ConfigProjectExtractPath -ChildPath "\userconfig.json"

    Write-Host "PARAMS"
    Write-Host "ConnectionString: [$ConnectionString]"
    Write-Host "ServerConfig: [$ServerConfig]"
    Write-Host "UserConfig: [$UserConfig]"

    $argList = @("ConnectionString=`"$ConnectionString`"")

    if(-Not( [System.String]::IsNullOrWhiteSpace($ServerConfig)))
    {
	    $argList += "ServerConfig=`"$ServerConfig`"";
    }

    if(-Not( [System.String]::IsNullOrWhiteSpace($UserConfig)))
    {
	    $argList += "UserConfig=`"$UserConfig`"";
    }

    Write-Host "EXECUTING"
    Write-Host "App Exe: $AppExe"
    Write-Host "ArgumentsList: $argList"

    Start-Process -NoNewWindow -FilePath $AppExe -ArgumentList $argList
}
