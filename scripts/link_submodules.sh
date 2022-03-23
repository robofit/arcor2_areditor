#!/bin/bash

mkdir -p ../arcor2_AREditor/Assets/Submodules
ln -snf `pwd`/../arcor2_AREditor/Submodules/JSONObject ../arcor2_AREditor/Assets/Submodules/JSONObject
ln -snf `pwd`/../arcor2_AREditor/Submodules/UnityDynamicPanels/Plugins ../arcor2_AREditor/Assets/Submodules/UnityDynamicPanels
ln -snf `pwd`/../arcor2_AREditor/Submodules/UnityRuntimeInspector/Plugins ../arcor2_AREditor/Assets/Submodules/UnityRuntimeInspector
ln -snf `pwd`/../arcor2_AREditor/Submodules/RosSharp/Unity3D/Assets/RosSharp ../arcor2_AREditor/Assets/Submodules/RosSharp
ln -snf `pwd`/../arcor2_AREditor/Submodules/Simple-Side-Menu ../arcor2_AREditor/Assets/Submodules/Simple-Side-Menu
ln -snf `pwd`/../arcor2_AREditor/Submodules/NativeWebSocket/NativeWebSocket/Assets/WebSocket ../arcor2_AREditor/Assets/Submodules/NativeWebSocket
ln -snf `pwd`/../arcor2_AREditor/Submodules/trilleon/client/Assets/Automation ../arcor2_AREditor/Assets/Submodules/Automation
ln -snf `pwd`/../arcor2_AREditor/Submodules/off-screen-indicator/Off\ Screen\ Indicator/Assets ../arcor2_AREditor/Assets/Submodules/OffScreenIndicator

ln -snf `pwd`/../arcor2_AREditor/Submodules/arcor2_areditor_private/3rdparty/Joystick\ Pack ../arcor2_AREditor/Assets/Submodules/Joystick\ Pack
ln -snf `pwd`/../arcor2_AREditor/Submodules/arcor2_areditor_private/3rdparty/loadingBar ../arcor2_AREditor/Assets/Submodules/loadingBar
ln -snf `pwd`/../arcor2_AREditor/Submodules/arcor2_areditor_private/3rdparty/LunarConsole ../arcor2_AREditor/Assets/Submodules/LunarConsole
ln -snf `pwd`/../arcor2_AREditor/Submodules/arcor2_areditor_private/3rdparty/Modern\ UI\ Pack ../arcor2_AREditor/Assets/Submodules/Modern\ UI\ Pack
ln -snf `pwd`/../arcor2_AREditor/Submodules/arcor2_areditor_private/3rdparty/Plugins ../arcor2_AREditor/Assets/Submodules/Plugins
ln -snf `pwd`/../arcor2_AREditor/Submodules/arcor2_areditor_private/3rdparty/SimpleCollada ../arcor2_AREditor/Assets/Submodules/SimpleCollada
ln -snf `pwd`/../arcor2_AREditor/Submodules/arcor2_areditor_private/3rdparty/TriLib ../arcor2_AREditor/Assets/Submodules/TriLib
ln -snf `pwd`/../arcor2_AREditor/Submodules/arcor2_areditor_private/3rdparty/UIGraph ../arcor2_AREditor/Assets/Submodules/UIGraph

rm -f ../arcor2_AREditor/Submodules/RosSharp/Unity3D/Assets/RosSharp/Plugins/External/Newtonsoft.Json.dll*
rm -f ../arcor2_AREditor/Submodules/RosSharp/Unity3D/Assets/RosSharp/Plugins/External/Newtonsoft.Json.xml*
