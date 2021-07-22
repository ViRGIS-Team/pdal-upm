#!/bin/bash

while getopts "p:i:d:s:" opt
do
   case "$opt" in
      p ) package="$OPTARG" ;;
      d ) destination="$OPTARG" ;;
      i ) install="$OPTARG" ;;
      s ) shared_assets="$OPTARG" ;;
   esac
done

outp=$(conda install -c conda-forge --prefix "$destination" --copy --mkdir $install -y -vv  2>&1)

echo $outp > "$destination"/pdal_log.txt

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
