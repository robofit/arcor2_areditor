# arcor2_editor

### Instalation
 - clone this repo
 - get submodules (there is one private, see below):
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
 - when downloading Unity (current version for this project is 2021.2.14f1) choose modules according to your needs (e.g. Android build support for creating .apk files)
 - Get third party assets. BUT-based developers may ask to get access to a private repository (added as submodule of this repo). External developers will have to get following assets from Unity AssetStore (extract and copy into Assets/Submodules):
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

 - delete folder repository\arcor2_AREditor\Assets\Submodules\TriLib\TriLibSamples
 - in Unity, go to Project Settings -> XR -> ARCore Extensions and change Android Authentication Strategy from "Keyless" to "Api Key"

### Building
 - when platform is set to Android - building .apk or want to use AR Foundation Remote:  
   - Add "AR_ON" to Scripting Define Symbols in Project Settings -> Player -> Other Settings
 -  when platform is set to Android - running play mode in Unity:  
    - Remove "AR_ON" from Scripting Define Symbols in Project Settings -> Player -> Other Settings
 - when platform is set to Standalone:  
    - No need of (un)setting "AR_ON" in Scripting Define Symbols, it is ignored
 - note: IL2CPP scripting backend is meant mainly for releases, while for faster development cycle the Mono backend is suitable (can be changed in Project Settings -> Player -> Other Settings)
 
