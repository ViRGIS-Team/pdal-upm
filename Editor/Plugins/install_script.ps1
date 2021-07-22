Param( 
    [string]$package,
    [string]$install,  
    [string]$destination,
    [string]$shared_assets
    )

$logfile = -join($destination, "\pdal_log.txt")

$outp = conda install -c conda-forge --prefix "$destination" --copy  --mkdir $install -y -vv *>&1 

Set-Location $destination
$temp = [System.IO.Path]::GetFileName((Get-Location).toString()) 

if ( $temp -ne "Conda")
{
    Write-Output "Working Directory Invalid" (Get-Location).tostring() >> "$logfile"
    Exit
}

Write-Output $outp >> "$logfile"

# Move the shared data to the shared assets folder to esnure that it gets built into the client

Write-Output "Processing gdal data"
$file = -join($destination, "\Library\share\gdal")
Write-Output "Copy $file to $shared_assets" >> "$logfile"
Move-Item -Path $file -Destination $shared_assets  -Force

Write-Output "Processing proj data"
$file = -join($destination, "\Library\share\proj")
Write-Output "Copy $file to $shared_assets"  >> "$logfile"
Move-Item -Path $file -Destination $shared_assets  -Force

# Tree shakering

Remove-Item *.dll

Get-ChildItem -exclude .*, conda-meta, *.meta, Library | Remove-Item -Recurse

Set-Location Library

Get-ChildItem -exclude bin | Remove-Item -Recurse
Set-Location bin
Get-ChildItem -exclude *.dll, *.exe, *.txt | Remove-Item -Recurse
Remove-Item api-*
Remove-Item vcr*
Remove-Item msvcp*