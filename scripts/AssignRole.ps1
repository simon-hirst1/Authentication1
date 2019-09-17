Write-Host "-- Database --"
$serverName = Read-Host -Prompt "Enter the server name"
$database = Read-Host -Prompt "Enter the database name"
$credentials = Get-Credential -Message "Please enter the database credentials" 

Write-Host "-- Assign Role --"
$username = Read-Host -Prompt "Enter the user name"
Write-Host "Enter the relevant role number"
$role = Read-Host -Prompt "1 - Admin"
Switch ($role){
    1 {$role = "Admin"}
}

$query = "
INSERT INTO dbo.AspNetUserRoles(UserId, RoleId)
SELECT dbo.AspNetUsers.Id, dbo.AspNetRoles.Id
FROM dbo.AspNetUsers CROSS JOIN dbo.AspNetRoles
WHERE dbo.AspNetUsers.UserName = '$username' AND dbo.AspNetRoles.Name = '$role'"

Invoke-Sqlcmd -ServerInstance $serverName -Query $query -Database $database -Username $credentials.GetNetworkCredential().UserName -Password $credentials.GetNetworkCredential().Password
