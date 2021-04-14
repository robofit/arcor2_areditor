# ARCOR2 AREditor

**ARCOR** stands for **A**ugmented **R**eality **C**ollaborative **R**obot. It is a system for simplified programming of collaborative robots based on augmented reality developed by [Robo@FIT](https://www.fit.vut.cz/research/group/robo/.en).

This repository contains the main user interface, Unity-based client application for ARCore-supported tablets: AREditor. The backend part of the solution is available [here](https://github.com/robofit/arcor2).

Development is supported by [Test-it-off: Robotic offline product testing](https://www.fit.vut.cz/research/project/1308/) project (Ministry of Industry and Trade of the Czech Republic).

<p align="center">
<img src="https://user-images.githubusercontent.com/1199396/109945741-d77c6a00-7cd7-11eb-9eb0-0448e346b60a.jpg" width="250" alt="Main menu with overview of scene, projects and execution packages."/>
<img src="https://user-images.githubusercontent.com/1199396/109945708-d0555c00-7cd7-11eb-9e7a-eeee34b3acab.jpg" width="250" alt="AREditor being used to program Dobot M1."/>
<img src="https://user-images.githubusercontent.com/1199396/109945756-dba88780-7cd7-11eb-8b95-49175dcbcc72.jpg" width="250" alt="VR mode."/>
</p>

### Installation

With each [release](https://github.com/robofit/arcor2_areditor/releases), we are building Android apk which can be simply installed on a supported device, which is any [ARCore compatible tablet](https://developers.google.com/ar/discover/supported-devices#google_play_devices). We use primarily Samsung devices (S6, S7). There are also Linux/Windows binaries, but mainly for testing and debugging purposes (no AR).

### Development

These are instructions for developers.

 - clone this repo
 - get submodules:
   ```bash
   git submodule update --init
   ```
 - create symlink of the submodules to the Assets folder:
 
   Windows:
   ```bash
   cd scripts
   .\link_submodules.bat
   ```
   
   Linux:
   ```bash
   cd scripts
   ./link_submodules.sh
   ```
 - download multimedia files from LFS:
   ```bash
   git lfs install
   git lfs pull
   ```
 - get third party assets from Unity AssetStore (extract and copy into Assets/Submodules):
   - [Lunar Mobile Console](https://assetstore.unity.com/packages/tools/gui/lunar-mobile-console-free-82881) (optional)
   - [Loading screen animation](https://assetstore.unity.com/packages/tools/loading-screen-animation-98505) 
   - [Modern UI Pack](https://assetstore.unity.com/packages/tools/gui/modern-ui-pack-150824)
   - [UI Graph](https://assetstore.unity.com/packages/tools/gui/ui-graph-51304)
   - [Joystick Pack](https://assetstore.unity.com/packages/tools/input-management/joystick-pack-107631)
   - [Native Camera for Android & iOS](https://assetstore.unity.com/packages/tools/integration/native-camera-for-android-ios-117802) (only if you want to build Android version)
   - [Trilib 2](https://assetstore.unity.com/packages/tools/modeling/trilib-2-model-loading-package-157548) - manually delete folder Trilib/Plugins/NewtonSoft.Json/ otherwise, there will be most likely conflicts
   - [Simple Collada](https://assetstore.unity.com/packages/tools/input-management/simple-collada-19579)
     - requires to apply patch (<b>/scripts/ColladaImporter.patch</b> and <b>/scripts/StringExtensions.patch</b>) on script ColladaImporter.cs and OrbCreationExtensions/StringExtensions.cs
     - SimpleCollada asset must be moved to Assets/Submodule folder in order to patch script works
     - on Windows, you can download [UnxUtils](http://unxutils.sourceforge.net/) and use batch file in /scripts/patch_simple_collada_asset.bat (if UnxUtils extracted to "C:\Program Files\") or use this command:
     ```bash       
     path_to_UnxUtils\UnxUtils\usr\local\wbin\patch.exe ..\arcor2_AREditor\Assets\Submodules\SimpleCollada\ColladaImporter.cs -i ColladaImporter.patch
     path_to_UnxUtils\UnxUtils\usr\local\wbin\patch.exe ..\arcor2_AREditor\Assets\Submodules\SimpleCollada\OrbCreationExtensions\StringExtensions.cs -i StringExtensions.patch
     ```
     - on Linux, you can use bash script in /scripts/patch_simple_collada_asset.sh

 - <b>(Optional)</b> - Get Google Cloud API Key to enable Cloud Anchors (follow step 7 in [Codelabs ARCore Extensions tutorial](https://codelabs.developers.google.com/codelabs/arcore-extensions-cloud-anchors/#6))
