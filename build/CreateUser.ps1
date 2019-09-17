Param(
    [string]$SqlInstance = $(Throw "SqlInstance parameter must have a value"),
    [string]$DatabaseName = $(Throw "DatabaseName parameter must have a value."),
    [string]$WebsiteName = $(Throw "WebsiteName parameter must have a value"),
    [string]$SqlAdminName = $(Throw "SqlAdminName parameter must have a value"),
    [string]$SqlAdminPassword = $(Throw "SqlAdminPassword parameter must have a value"),
    [string]$LoginUsername,     #optional
    [string]$LoginPassword      #optional
)

#variables
$server = "tcp:" + $sqlInstance + ".database.windows.net,1433"

#Load assembly needed to generate password
[Reflection.Assembly]::LoadWithPartialName("System.Web")

function New-UserCredentials
{
    if(-Not( [System.String]::IsNullOrWhiteSpace($Loginusername) -or [System.String]::IsNullOrWhiteSpace($LoginPassword)))
    {
        return @{username = $LoginUsername; Password = $LoginPassword}
    }
    elseIf([System.String]::IsNullOrWhiteSpace($Loginusername) -and [System.String]::IsNullOrWhiteSpace($LoginPassword))
    {
        $baseName = $WebsiteName
        $username = $baseName + "User"
        $password = [System.Web.Security.Membership]::GeneratePassword(10, 1)
        $randomNumber = Get-Random -Maximum 10
        $validatedPassword = $password -replace '[\.{}]', $randomNumber

        return @{Username = $username; Password = $validatedPassword}
    }
    else
    {
        throw [System.InvalidOperationException]"If creating a new login and user call this script with no parameters, otherwise both username and password must be specified."
    }
}

$newLoginCommandText = @"
IF NOT EXISTS (SELECT Name FROM sys.sql_logins WHERE name = @Username)
EXECUTE ('CREATE LOGIN [' + @Username + '] WITH PASSWORD = ''' + @Password + '''')  
"@

function New-SqlLogin($userCredentials)
{
    $adminConnectionString = "Server=$server;Database=master;User ID=$SqlAdminName;Password=$SqlAdminPassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    $adminConnection = New-Object -TypeName System.Data.SqlClient.SqlConnection($adminConnectionString)
    $query = $newLoginCommandText

    $command = New-Object -TypeName System.Data.SqlClient.SqlCommand($query, $adminConnection)
    $userNameParameter = New-Object -TypeName System.Data.SqlClient.SqlParameter("@Username", $userCredentials.Username)
    $passwordParameter = New-Object -TypeName System.Data.SqlClient.SqlParameter("@Password", $userCredentials.Password)
    $command.Parameters.Add($userNameParameter)
    $command.Parameters.Add($passwordParameter)

    $adminConnection.Open()
    $command.ExecuteNonQuery()
    $adminConnection.Close()

    $adminConnectionString
}

$newUserCommandText = @"
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = @Username)
BEGIN
    EXECUTE ('CREATE USER [' + @Username + '] FOR LOGIN [' + @Username + '] WITH DEFAULT_SCHEMA=[dbo]')
    EXECUTE ('GRANT SELECT,INSERT,UPDATE,DELETE ON SCHEMA::dbo TO [' + @Username + ']')
    EXECUTE ('ALTER ROLE db_ddladmin ADD MEMBER [' + @Username + ']')
    
END 
"@

<# Admin Login #>
function New-DatabaseUser($userCredentials)
{
    $adminConnectionString = "Server=$server;Database=$DatabaseName;User ID=$SqlAdminName;Password=$SqlAdminPassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    $adminConnection = New-Object -TypeName System.Data.SqlClient.SqlConnection($adminConnectionString)
    $query = $newUserCommandText

    $command = New-Object -TypeName System.Data.SqlClient.SqlCommand($query, $adminConnection)
    $userNameParameter = New-Object -TypeName System.Data.SqlClient.SqlParameter("@Username", $userCredentials.Username)
    $command.Parameters.Add($userNameParameter)

    $adminConnection.Open()
    $command.ExecuteNonQuery()
    $adminConnection.Close()
}

Write-Host "Getting octopus variable:" $WebsiteName

$credentials = New-UserCredentials
New-SqlLogin $credentials
New-DatabaseUser $credentials

$username = $credentials.Username
$password =  $credentials.Password
$dbConnectionString = "Server=$server;Database=$DatabaseName;User ID=$Username;Password=$Password;TrustServerCertificate=False;Connection Timeout=30;"

Set-OctopusVariable -name "SqlServerUserConnectionString" -value $dbConnectionString
