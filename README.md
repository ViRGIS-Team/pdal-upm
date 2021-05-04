# Unity Package for PDAL

The [Pointcloud Data Abstraction Layer](https://www.pdal.io/) (PDAL) is a C++ BSD library for translating and manipulating point cloud data. It is very much like the GDAL library which handles raster and vector data.

This repo is a Unity Package for using PDAL in a project.

This Package is part of the [ViRGiS project](https://www.virgis.org/) - bringing GiS to VR. 

## Installation

The Package can be installed from [Open UPM](https://openupm.com/packages/com.virgis.mdal/). If you use this method, the dependencies will be automatically loaded provided the relevent scoped registry is included in your project's `manifest.json` :
```
scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.openupm",
        "com.virgis.geometry3sharp"
      ]
    }
  ],
```


The Package can also be installed using the Unity Package Manager directly from the [GitHub Repo](https://github.com/ViRGIS-Team/pdal-upm).

This package is dependent on the following packages having been loaded (and the UPM / GH combination does not allow package dependencies  - so you have to do that yourself) :

- Geometry3Sharp -[UPM GH Repo](https://github.com/ViRGIS-Team/geometry3Sharp). This providse the Mesh data strcuture and manipulation tools in c#

## Version numbers

This package is a wrapper around a C++ library. We want to keep the link to the library version. However, we also need to be able to have multiple
builds of the package for the same underlying library version. Unfortunately, UPM does not have the concept of a build number.

Therefore, this package uses the version number ing proposed by [Favo Yang to solve this](https://medium.com/openupm/how-to-maintain-upm-package-part-3-2d08294269ad#88d8). This adds two digits for build number to the SemVer patch value i.e. 3.1.1 => 3.1.100, 3.1.101, 3.1..102 etc.

This has the unfortunate side effect that 3.1.001 will revert to 3.1.1 and this means :

| Package | Library |
| ------- | ------- |
| 3.1.0   | 3.1.0   |
| 3.1.1   | 3.1.0   |
| 3.1.100 | 3.1.1.  |

## Developement and Use in the player

The scripts for accessing PDAL data are included in the `pdal`namespace and follow the [PDAL C Api](https://pdal.io/CAPI/doxygen/html/index.html).

For C# API is shown in [the API Documentation](https://virgis-team.github.io/pdal-upm/html/index.html)..

The PDAL library is loaded as an unmanaged native plugin. This plugin will load correctly in the player when built. See below for a note about use in the Editor.

This Library currently works on Windows, Linux and Mac based platforms.

## Running in the Editor

This package uses [Conda](https://docs.conda.io/en/latest/) to download the latest version of PDAL.

For this package to work , the development machine MUST have a working copy of Conda (either full Conda or Miniconda) installed and in the path. The following CLI command should work without change or embellishment:

```
conda info
```

If the development machine is running Windows it must have a reasonably up to date version of POwershell loaded.

The package will keep the installation of Pdal in `Assets\Conda`. You may want to exclude this folder from source control.

This package installs the GDAL package, which copies data for GDAL and for PROJ into the `Assets/StreamingAssets`folder. You may also want to exclude this folder from source control.

## Documentation
See [the API Documentation](https://virgis-team.github.io/pdal-upm/html/index.html).

A typical sample program :

```C#
using System;
using System.Collections.Generic;
using Pdal;
using Newtonsoft.Json;
using g3;

namespace pdal_mesh
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Config pdal = new Config();
            Console.WriteLine(pdal.Version);

            List<object> pipe = new List<object>();
            pipe.Add(".../CAPI/tests/data/las/1.2-with-color.las");
            pipe.Add(new
            {
                type = "filters.splitter",
                length = 1000
            });
            pipe.Add(new
            {
                type = "filters.delaunay"
            });

            string json = JsonConvert.SerializeObject(pipe.ToArray());

            Pipeline pl = new Pipeline(json);

            long count = pl.Execute();

            Console.WriteLine($"Point Count is {count}");

            using (PointViewIterator views = pl.Views) {
                views.Reset();

                while (views.HasNext())
                {
                    PointView view = views.Next;
                    if (view != null)
                    {
                        Console.WriteLine($"Point Count is {view.Size}");
                        Console.WriteLine($"Triangle Count is {view.MeshSize}");

                        BpcData pc = view.GetBakedPointCloud();

                        DMesh3 mesh = view.getMesh();

                    }
                }
            }
        }
    }
}
```
