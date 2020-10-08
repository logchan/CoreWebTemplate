$project_name = "CoreWebTemplate"
$project_dir = $project_name
$build_dir = "template-build"

if (Test-Path $build_dir) {
    Remove-Item $build_dir -Recurse
}

mkdir $build_dir | Out-Null

$project_root = (Get-Item -Path $project_dir).FullName
$build_root = (Get-Item -Path $build_dir).FullName
Write-Host $build_root
foreach ($f in Get-ChildItem $project_dir\* -Recurse -Exclude bin, obj, *.user) {
    if ($f.GetType() -eq [System.IO.DirectoryInfo]) {
        continue
    }
    if ($f.FullName.Contains('\bin') -or $f.FullName.Contains('\obj') -or $f.Extension -eq '.user') {
        continue
    }

    $subpath = $f.Directory.FullName
    if ($subpath.Length -gt $project_root.Length) {
        $subpath = $f.Directory.FullName.Substring($project_root.Length + 1)
    }
    else {
        $subpath = ""
    }

    $dir = [System.IO.Path]::Combine($build_root, $subpath)
    [System.IO.Directory]::CreateDirectory($dir) | Out-Null

    $dest = [System.IO.Path]::Combine($dir, $f.Name)
    Write-Host $dest
    $f.CopyTo($dest) | Out-Null
}

$xml = New-Object "System.Text.StringBuilder"
foreach ($f in Get-ChildItem $build_dir -Recurse -File) {
    $s = Get-Content $f.FullName
    $s = $s.Replace($project_name, '$safeprojectname$')
    Set-Content -Value $s -Path $f.FullName

    $name = $f.FullName.Substring($build_root.Length + 1)
    $xml.AppendLine("<ProjectItem ReplaceParameters=""true"">$name</ProjectItem>") | Out-Null
}

$xml = $xml.ToString()
Write-Host $xml

$template = "$project_name.vstemplate"
$xml = (Get-Content $template).Replace("#items#", $xml)
$template = [System.IO.Path]::Combine($build_root, $template)
Set-Content -Value $xml -Path $template

$zip_file = [System.IO.Path]::Combine($build_root, "..", "$project_name-packed.zip")
if (Test-Path $zip_file) {
    Remove-Item $zip_file
}

Add-Type -Assembly System.IO.Compression.FileSystem
$compression_level = [System.IO.Compression.CompressionLevel]::Optimal
[System.IO.Compression.ZipFile]::CreateFromDirectory($build_root, $zip_file, $compression_level, $false)