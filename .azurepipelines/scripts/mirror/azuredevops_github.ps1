# Mirror Azure DevOps Repo to Github
# Reference
# https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_functions_advanced_parameters?view=powershell-7.2
# https://github.com/xware-gmbh/azure-devops-to-github-mirror/blob/main/git_mirror.sh

param(
    [Parameter(Mandatory)]
    [string] $Organization,
    [Parameter(Mandatory)]
    [string] $Project,
    [Parameter(Mandatory)]
    [string] $Repository,
    [Parameter(Mandatory)]
    [string] $DesRepo,
    [Parameter(Mandatory)]
    [string] $Branch,
    [Parameter()]
    [switch] $Force
)

if ($IsLinux -and [string]::IsNullOrEmpty($env:TMP))
{
    $env:TMP = "/tmp"
}

$SrcUri = "https://$env:SRC_PAT@dev.azure.com/$Organization/$Project/_git/$Repository"
$DesUri = "https://$env:DES_PAT@github.com/$DesRepo"
$TmpDir = "$env:TMP/TMP_$Repository"
$WorkFolder = "$PSScriptRoot/../../.."

$flag = $true
for ($i = 0; $i -lt 100; ++$i)
{
    $Process = Start-Process -FilePath "git" -ArgumentList "clone $SrcUri $TmpDir --single-branch --branch $Branch" -Wait -NoNewWindow -PassThru
    if ($Process.ExitCode -eq 0) {	
        $flag = $false
        break
    }
}

if ($flag)
{
    Write-Error "Unable to clone the repository!"
    Exit 1
}

Set-Location $TmpDir
$pushArgs = "push $DesUri"
if ($Force) {
    $pushArgs = "$pushArgs --force"
}

$flag = $true
for ($i = 0; $i -lt 100; ++$i)
{
    $Process = Start-Process -FilePath "git" -ArgumentList $pushArgs -Wait -NoNewWindow -PassThru
    if ($Process.ExitCode -eq 0) {	
        $flag = $false
        break
    }
}

if ($flag)
{
    Write-Error "Unable to push the repository!"
    Exit 1
}

Set-Location $WorkFolder
Remove-Item -Path $TmpDir -Recurse -Force