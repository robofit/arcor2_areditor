#!/bin/bash

patch ../arcor2_AREditor/Assets/Submodules/SimpleCollada/ColladaImporter.cs -i ColladaImporter.patch
patch ../arcor2_AREditor/Assets/Submodules/SimpleCollada/OrbCreationExtensions/StringExtensions.cs -i StringExtensions.patch
