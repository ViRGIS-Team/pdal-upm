# Unity Package for PDAL

The [Pointcloud Data Abstraction Layer](https://www.pdal.io/) (PDAL) is a C++ BSD library for translating and manipulating point cloud data. It is very much like the GDAL library which handles raster and vector data.

This repo is a Unity Package for using PDAL in a project.

This Package is part of the [ViRGiS project](https://www.virgis.org/) - bringing GiS to VR. 

## Installation

The Package can be installed from [Open UPM](https://openupm.com/packages/com.virgis.mdal/). If you use this method, the dependencies will be automatically loaded provided the relevant scoped registry is included in your project's `manifest.json` :

```
scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.openupm",
        "com.virgis.geometry3sharp",
        "com.virgis.conda-extensions"
      ]
    }
  ],
```


The Package can also be installed using the Unity Package Manager directly from the [GitHub Repo](https://github.com/ViRGIS-Team/pdal-upm).

## Build Tests and Platform Support

This package is build tested on multiple platforms using the [ViRGiS Team Test Project](https://github.com/ViRGIS-Team/test-project).

See that project for working examples and current list of working platforms.

## Version numbers

This package is a wrapper around a C++ library. We want to keep the link to the library version. However, we also need to be able to have multiple
builds of the package for the same underlying library version. Unfortunately, UPM does not have the concept of a build number.

Therefore, this package uses the version numbering proposed by [Favo Yang to solve this](https://medium.com/openupm/how-to-maintain-upm-package-part-3-2d08294269ad#88d8). This adds two digits for build number to the SemVer patch value i.e. 3.1.1 => 3.1.100, 3.1.101, 3.1..102 etc.

This has the unfortunate side effect that 3.1.001 will revert to 3.1.1 and this means :

| Package | Library |
| ------- | ------- |
| 3.1.0   | 3.1.0   |
| 3.1.1   | 3.1.0   |
| 3.1.100 | 3.1.1.  |

## A note about Upgrading

Unity is a bit "graby" about DLLs and SOs. Once it is loaded it keeps a hardlink to the DLL and does not like changing. This means that for this package, once you have upgraded to a new version of the UPM package you will, usually, need to restart the Unity Editor for the change to work.

## Developement and Use in the player

> NOTE - For the avoidance of doubt, conda is NOT required on machines running the distributed application. The required libraries are automatically included in the distribution package created by Unity

The scripts for accessing PDAL data are included in the `pdal` namespace and follow the [PDAL C Api](https://pdal.io/CAPI/doxygen/html/index.html).

For C# API is shown in [the API Documentation](https://virgis-team.github.io/pdal-upm/index.html).

The PDAL library is loaded as an unmanaged native plugin. This plugin will load correctly in the player when built. See below for a note about use in the Editor.

This Library currently works on Windows, Linux and Mac platforms.

## Running in the Editor

This package uses [Conda](https://docs.conda.io/en/latest/) to download the latest version of PDAL.

As of version 2.9.1, this package uses Version 2 of the Conda Extension package. This means that the package now includes a complete self contained and standalone installation of the Conda API. You no longer need to install Conda on your development machines.

Note that when upgrading, you MUST delete the Assets/Conda directory and restart Unity.

## Documentation
See [the API Documentation](https://virgis-team.github.io/pdal-upm/index.html).

A typical sample program :

```C#
using System;
using System.Collections.Generic;
using Pdal;
using Newtonsoft.Json;
using g3;
using UnityEngine;

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

                        BakedPointCloud bpc = BakedPointCloud.Initialize(view);

                        BakedMesh bm = BakedMesh.Initialize(view);
                        Mesh mesh = bm.ToMesh();

                    }
                }
            }
        }
    }
}
```

# Minimum Project / Test Project
You can see a minimum working project in the test project used to test build this package (and two others):

https://github.com/ViRGIS-Team/test-project.

# Use with Unity Cloud Build

As of release 2.4.1, this package will work with Unity Cloud Build.

As of release 2.9.1, this package will work with Unity Cloud Build without any need for additional work or scripts.
