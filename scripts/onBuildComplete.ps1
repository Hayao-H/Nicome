if (-not ($args[0])) { return 100 }
elseif ($args[0] -eq "Debug") {
    return 01
}

$baseDir=Convert-Path ..

$targetFile=[System.IO.Path]::Combine($baseDir,"nicome","source.zip")
$pdbFilePath=[System.IO.Path]::Combine($baseDir,"nicome","Nicome.pdb")

if (Test-Path $targetFile){
    Remove-Item $targetFile
}

if (Test-Path $pdbFilePath){
    Remove-Item $pdbFilePath
}

Compress-Archive -Path "$baseDir/program.cs","$baseDir/enums.cs","$baseDir/CLI","$baseDir/Store","$baseDir/Utils","$baseDir/WWW" -DestinationPath $targetFile

return 0