#!/bin/bash

while getopts "p:i:d:s:" opt
do
   case "$opt" in
      p ) platform="$OPTARG" ;;
      d ) destination="$OPTARG" ;;
      i ) install="$OPTARG" ;;
      s ) shared_assets="$OPTARG" ;;
   esac
done

export PATH=~/local/miniconda3/bin:$PATH

outp=$(conda install -c conda-forge --prefix "$destination" --copy --mkdir python -y -vv  2>&1)

export CONDA_SUBDIR=$platform
echo $CONDA_SUBDIR > "$destination"/pdal_log.txt
echo $outp >> "$destination"/pdal_log.txt

outp=$(conda install -c conda-forge --prefix "$destination" --copy $install -y -vv  2>&1)

echo $CONDA_SUBDIR > "$destination"/pdal_log.txt
echo $outp >> "$destination"/pdal_log.txt

echo "removing Symlinks" >> "$destination"/pdal_log.txt 2>&1
find "$destination/bin" -type l -delete

echo "Processing gdal data" >> "$destination"/pdal_log.txt 2>&1
echo "copy $destination/share/gdal to $shared_assets" >> "$destination"/pdal_log.txt 2>&1
mkdir -p "$shared_assets/gdal" 
cp "$destination"/share/gdal/* "$shared_assets/gdal"

echo "Processing proj data" >> "$destination"/pdal_log.txt 2>&1
echo "copy $destination/share/proj to $shared_assets" >> "$destination"/pdal_log.txt 2>&1
mkdir -p "$shared_assets/proj" 
cp "$destination"/share/proj/* "$shared_assets/proj"

find "$destination" -type d -not \( -name *bin -or -name *lib -or -name *Conda -or -name *conda-meta \) -maxdepth 1 -print0 | xargs -0 -I {} rm -r {}

find "$destination/lib" -type d -not -name *lib -maxdepth 1 -print0 | xargs -0 -I {} rm -r {}
rm "$destination/lib/terminfo"

rm "$destination"/lib/python3.1