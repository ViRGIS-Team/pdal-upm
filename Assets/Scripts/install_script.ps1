Param( 
    [string]$package,
    [string]$install, 
    [string]$destination,
    [string]$so_list
    ) 

conda create --name upm -y
conda activate upm
conda install $install -y --no-deps

conda list $package | 
        Select-String -Pattern "^($package\D*)(\d+\.+\d+\.+\d+)" | 
            Foreach-Object{
                $package, $current_version = $_.Matches[0].Groups[1..2]
                Write-Output "Current Conda Version : $current_version"
                $env = conda info --json | ConvertFrom-Json 
                $conda_bin = -join($env.active_prefix, "\Library\bin")
                $so_list.Split(",") | Foreach-Object {
                    Write-Output "Processing $_"
                    $file = -join($conda_bin, '/', $_, '.dll' )
                    Write-Output "Copy $file to $destination"
                    Copy-Item $file -Destination $destination
                }
}

conda deactivate
conda remove --name upm --all -y