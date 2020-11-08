# Unity Package for PDAL

The [Pointcloud Data Abstraction Layer](https://www.pdal.io/) (PDAL) is a C++ BSD library for translating and manipulating point cloud data. It is very much like the GDAL library which handles raster and vector data.

This repo is a Unity Package for using PDAL in a project.

This Package is part of the [ViRGiS project](https://www.virgis.org/) - bringing GiS to VR. 

## Installation

The Package can be installed using the Unity Package Manager directly from the [GitHub Repo](https://github.com/ViRGIS-Team/pdal-upm).

This package is dependent on the following packages having been loaded (and the UPM / GH combination does not allow package dependencies  - so you have to do that yourself) :

- Geometry3Sharp -[UPM GH Repo](https://github.com/ViRGIS-Team/geometry3Sharp). This providse the Mesh data strcuture and manipulation tools in c#, and

- GDAL - [UPM GH repo](https://github.com/ViRGIS-Team/gdal-upm).

## Developement and Use in the player

The scripts for accessing PDAL data are included in the `pdal`namespace and follow the [PDAL C Api](https://pdal.io/CAPI/doxygen/html/index.html).

For more details - see the documentation.

The PDAL library is loaded as an unmanaged native plugin. This plugin will load correctly in the player when built. See below for a note about use in the Editor.

This Library currently works on Windows and Mac based platforms.

## Running in the Editor

This package uses [Conda](https://docs.conda.io/en/latest/) to download the latest version of MDAL.

For this package to work , the development machine MUST have a working copy of Conda (either full Conda or Miniconda) installed and in the path. The following CLI command should work without change or embellishment:

```
conda info
```

The package will keep the installation of Mdal in `Assets\Conda`. You may want to exclude this folder from source control.

This package installs the GDAL package, which copies data for GDAL and for PROJ into the `Assets/StreamingAssets`folder. You may also want to exclude this folder from source control.

## Documentation

TBD
