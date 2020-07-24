# arcor2_editor
ARCOR2 AR Editor


### Instalation
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
   - [Simple Collada](https://assetstore.unity.com/packages/tools/input-management/simple-collada-19579)
     - requires to apply patch (<b>/scripts/ColladaImporter.patch</b> and <b>/scripts/StringExtensions.patch</b>) on script ColladaImporter.cs and OrbCreationExtensions/StringExtensions.cs
     - on Windows, you can download [UnxUtils](http://unxutils.sourceforge.net/) and use batch file in /scripts/patch_simple_collada_asset.bat (if UnxUtils extracted to "C:\Program Files\") or use this command:
     ```bash       
     path_to_UnxUtils\UnxUtils\usr\local\wbin\patch.exe ..\arcor2_AREditor\Assets\Submodules\SimpleCollada\ColladaImporter.cs -i ColladaImporter.patch
     path_to_UnxUtils\UnxUtils\usr\local\wbin\patch.exe ..\arcor2_AREditor\Assets\Submodules\SimpleCollada\OrbCreationExtensions\StringExtensions.cs -i StringExtensions.patch
     ```
     - on Linux, you can use bash script in /scripts/patch_simple_collada_asset.sh

 - <b>(Optional)</b> - Get Google Cloud API Key to enable Cloud Anchors (follow step 7 in [Codelabs ARCore Extensions tutorial](https://codelabs.developers.google.com/codelabs/arcore-extensions-cloud-anchors/#6))
