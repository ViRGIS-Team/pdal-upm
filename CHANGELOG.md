This plugin will be updated as and when new versions of PDAL are released and will use the version numbers from PDAL.

# Version 2.5.0

Updates PDAL to version 2.5.0 using pdalc version 2.2.0 to get over the Pipeline Executor deprecation.

This version also moves the conda build to an Asset Post Processor and standardizes assembly names - so there may/will be minor changes in how it operates - including if other assemblies reference this assembly directly.

This version also moves the processing in `BakedPointCloud` to the Unity Job System for faster processing.

# 2.2.0+3 :

- Fixes Bug whne there is a space in the project name
