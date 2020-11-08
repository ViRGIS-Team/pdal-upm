Param( 
    [string]$package,
    [string]$install, 
    [string]$destination,
    [string]$test
    ) 

conda create --name upm -y
conda install $install --name upm -y --no-deps


$env = conda info --envs
$temp = conda info --envs | Select-String  -Pattern 'upm'
$temp -match "C\:.*"
$conda_bin = -join($matches[0], "\Library/bin")

Write-Output "Processing *.dll"
$file = -join($conda_bin, '/*.dll' )
Write-Output "Copy $file to $destination"
Copy-Item $file -Destination $destination

Write-Output "Processing $test"
$file = -join($conda_bin, '/', $test )
Write-Output "Copy $file to $destination"
Copy-Item $file -Destination $destination


conda remove --name upm --all -y