param(
    [Parameter(Mandatory = $true)]
    [string] $RepositoryRoot,
    [string] $OutputFile
)

$ErrorActionPreference = 'Stop'
$repository = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$gitArguments = @('-c', "safe.directory=$repository", '-C', $repository)
$tag = (& git @gitArguments describe --tags --abbrev=0 --match 'v[0-9]*' HEAD 2>$null)

if ($LASTEXITCODE -ne 0 -or $tag -notmatch '^v(\d+)\.(\d+)\.(\d+)$') {
    $major = 0
    $minor = 0
    $patch = 0
    $distance = 0
    $isTaggedHead = $false
}
else {
    $major = [int] $Matches[1]
    $minor = [int] $Matches[2]
    $patch = [int] $Matches[3]
    $distance = [int] (& git @gitArguments rev-list --count "$tag..HEAD")
    $isTaggedHead = $distance -eq 0
}

$dirty = @(& git @gitArguments status --porcelain --untracked-files=normal).Count -gt 0
if ($isTaggedHead -and -not $dirty) {
    $version = "$major.$minor.$patch"
}
else {
    $nextPatch = $patch + 1
    $stamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddHHmmss')
    $version = "$major.$minor.$nextPatch-dev.$distance+build.$stamp"
}

if ($OutputFile) {
    Set-Content -LiteralPath $OutputFile -Value $version -NoNewline
}
else {
    Write-Output $version
}
