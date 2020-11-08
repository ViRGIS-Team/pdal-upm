Param( 
    [string]$package,
    [string]$install, 
    [string]$destination,
    [string]$test
    ) 

conda create --name upm -y
conda activate upm
conda install $install -y --no-deps


$env = conda info --json | ConvertFrom-Json 
$conda_bin = -join($env.active_prefix, "\Library\bin")
Write-Output "Processing *.dll"
$file = -join($conda_bin, '/*.dll' )
Write-Output "Copy $file to $destination"
Copy-Item $file -Destination $destination


conda deactivate
conda remove --name upm --all -y