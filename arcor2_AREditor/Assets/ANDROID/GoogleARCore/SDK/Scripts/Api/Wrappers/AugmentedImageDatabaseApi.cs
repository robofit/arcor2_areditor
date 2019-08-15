//-----------------------------------------------------------------------
// <copyright file="AugmentedImageDatabaseApi.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCoreInternal {
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
    using AndroidImport = GoogleARCoreInternal.DllImportNoop;
    using IOSImport = System.Runtime.InteropServices.DllImportAttribute;
#else
    using AndroidImport = System.Runtime.InteropServices.DllImportAttribute;
#endif

    internal class AugmentedImageDatabaseApi {
        public AugmentedImageDatabaseApi(NativeSession nativeSession) {
        }

        public IntPtr CreateArPrestoAugmentedImageDatabase(byte[] rawData) {
            if (Application.isEditor) {
                // ArPrestoAugmentedImageDatabase_create() not supported in editor.
                return IntPtr.Zero;
            }

            IntPtr outDatabaseHandle = IntPtr.Zero;
            GCHandle handle = new GCHandle();
            IntPtr rawDataHandle = IntPtr.Zero;
            int length = 0;

            if (rawData != null) {
                handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
                rawDataHandle = handle.AddrOfPinnedObject();
                length = rawData.Length;
            }

            ExternApi.ArPrestoAugmentedImageDatabase_create(
                rawDataHandle, length, ref outDatabaseHandle);

            if (handle.IsAllocated) {
                handle.Free();
            }

            return outDatabaseHandle;
        }

        public int AddImageAtRuntime(
            IntPtr databaseHandle, string name, Texture2D image, float width) {
            int outIndex = -1;

            if (InstantPreviewManager.IsProvidingPlatform) {
                InstantPreviewManager.LogLimitedSupportMessage("add images to Augmented Image " +
                    "database");
                return outIndex;
            }

            GCHandle grayscaleBytesHandle = _ConvertTextureToGrayscaleBytes(image);
            if (grayscaleBytesHandle.AddrOfPinnedObject() == IntPtr.Zero) {
                return -1;
            }

            ApiArStatus status = ExternApi.ArPrestoAugmentedImageDatabase_addImageAtRuntime(
                databaseHandle, name, grayscaleBytesHandle.AddrOfPinnedObject(), image.width,
                image.height, image.width, width, ref outIndex);

            if (grayscaleBytesHandle.IsAllocated) {
                grayscaleBytesHandle.Free();
            }

            if (status != ApiArStatus.Success) {
                Debug.LogWarningFormat(
                    "Failed to add aumented image at runtime with status {0}", status);
                return -1;
            }

            return outIndex;
        }

        private GCHandle _ConvertTextureToGrayscaleBytes(Texture2D image) {
            byte[] grayscaleBytes = null;

            if (image.format == TextureFormat.RGB24 || image.format == TextureFormat.RGBA32) {
                Color[] pixels = image.GetPixels();
                grayscaleBytes = new byte[pixels.Length];
                for (int i = 0; i < image.height; i++) {
                    for (int j = 0; j < image.width; j++) {
                        grayscaleBytes[(i * image.width) + j] =
                            (byte) ((
                            (0.213 * pixels[((image.height - 1 - i) * image.width) + j].r) +
                            (0.715 * pixels[((image.height - 1 - i) * image.width) + j].g) +
                            (0.072 * pixels[((image.height - 1 - i) * image.width) + j].b)) * 255);
                    }
                }
            } else {
                Debug.LogError("Unsupported texture format " + image.format);
            }

            return GCHandle.Alloc(grayscaleBytes, GCHandleType.Pinned);
        }

        private struct ExternApi {
#pragma warning disable 626
            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPrestoAugmentedImageDatabase_create(IntPtr rawBytes,
                long rawBytesSize, ref IntPtr outAugmentedImageDatabaseHandle);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern void ArPrestoAugmentedImageDatabase_destroy(
                IntPtr augmentedImageDatabaseHandle);

            [AndroidImport(ApiConstants.ARPrestoApi)]
            public static extern ApiArStatus ArPrestoAugmentedImageDatabase_addImageAtRuntime(
                IntPtr augmentedImageDatabaseHandle,
                string imageName,
                IntPtr imageBytes,
                int imageWidth,
                int imageHeight,
                int imageStride,
                float imageWidthInMeters,
                ref int outIndex);
#pragma warning restore 626
        }
    }
}
