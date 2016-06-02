[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True, Position=0, ValueFromPipeline=$True)]
    [string]$newVersion
)
foreach ($projfile in (Get-Item src\*\project.json))
{
    $fullname = $projfile.FullName
    Write-Verbose "patching version to '$newVersion' in $fullname"
    $contents = [IO.File]::ReadAllText($fullname)
    $resultText = $contents -Replace '^(\{\s*"version":\s*)"([^(\\")]*)"', "`$1`"$newVersion-*`""
    [IO.File]::WriteAllText($fullname, $resultText)
}
# appveyor patching

$fullname = (Get-Item "appveyor.yml").FullName
Write-Verbose "patching version to '$newVersion' in $fullname"
$contents = [IO.File]::ReadAllText($fullname)
$resultText = $contents -Replace '(version:) ([^\-]*)', "`$1 $newVersion"
[IO.File]::WriteAllText($fullname, $resultText)
