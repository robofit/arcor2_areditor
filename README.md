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
   - [Flat Minimalist GUI](https://assetstore.unity.com/packages/2d/gui/flat-minimalist-gui-ui-pack-over-600-png-146895)
   - [JSON Object](https://assetstore.unity.com/packages/tools/input-management/json-object-710)
   - [Simple Side-Menu](https://assetstore.unity.com/packages/tools/gui/simple-side-menu-143623)
   - [UI Graph](https://assetstore.unity.com/packages/tools/gui/ui-graph-51304)

 - <b>(Optional)</b> - Get Google Cloud API Key to enable Cloud Anchors (follow step 7 in [Codelabs ARCore Extensions tutorial](https://codelabs.developers.google.com/codelabs/arcore-extensions-cloud-anchors/#6))
