#!/bin/bash

mkdir -p ../arcor2_AREditor/Assets/Submodules
ln -snf `pwd`/../arcor2_AREditor/Submodules/JSONObject ../arcor2_AREditor/Assets/Submodules/JSONObject
ln -snf `pwd`/../arcor2_AREditor/Submodules/UnityDynamicPanels/Plugins ../arcor2_AREditor/Assets/Submodules/UnityDynamicPanels
ln -snf `pwd`/../arcor2_AREditor/Submodules/UnityRuntimeInspector/Plugins ../arcor2_AREditor/Assets/Submodules/UnityRuntimeInspector
ln -snf `pwd`/../arcor2_AREditor/Submodules/Unity3DRuntimeTransformGizmo/Assets ../arcor2_AREditor/Assets/Submodules/Unity3DRuntimeTransformGizmo
ln -snf `pwd`/../arcor2_AREditor/Submodules/RosSharp/Unity3D/Assets/RosSharp ../arcor2_AREditor/Assets/Submodules/RosSharp
ln -snf `pwd`/../arcor2_AREditor/Submodules/Simple-Side-Menu ../arcor2_AREditor/Assets/Submodules/Simple-Side-Menu
ln -snf `pwd`/../arcor2_AREditor/Submodules/NativeWebSocket/NativeWebSocket/Assets/WebSocket ../arcor2_AREditor/Assets/Submodules/NativeWebSocket
ln -snf `pwd`/../arcor2_AREditor/Submodules/trilleon/client/Assets/Automation ../arcor2_AREditor/Assets/Submodules/Automation
ln -snf `pwd`/../arcor2_AREditor/Submodules/off-screen-indicator/Off Screen Indicator/Assets ../arcor2_AREditor/Assets/Submodules/OffScreenIndicator
rm ../arcor2_AREditor/Submodules/RosSharp/Unity3D/Assets/RosSharp/Plugins/External/Newtonsoft.Json.dll*
rm ../arcor2_AREditor/Submodules/RosSharp/Unity3D/Assets/RosSharp/Plugins/External/Newtonsoft.Json.xml*
