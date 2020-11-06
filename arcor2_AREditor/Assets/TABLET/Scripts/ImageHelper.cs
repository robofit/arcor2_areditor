using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR || UNITY_STANDALONE
using SFB;
#endif
using System.Threading.Tasks;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
using System.Threading;
#endif
public class ImageHelper
{
    private static string pathToReturn;

    private static bool waitingForCallback = false;

    public static Sprite LoadNewSprite(string filePath, float pixelsPerUnit = 100.0f) {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Sprite NewSprite;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        Texture2D SpriteTexture = NativeCamera.LoadImageAtPath(filePath, 500, false);
#elif UNITY_EDITOR || UNITY_STANDALONE
        Texture2D SpriteTexture = LoadTexture(filePath);        
#endif
        NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), pixelsPerUnit);

        return NewSprite;
    }

    public static Texture2D LoadTexture(string filePath) {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(filePath)) {
            FileData = File.ReadAllBytes(filePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }

    public static void SaveTextureToFile(Texture2D texture, string fileName) {
        byte[] bytes = texture.EncodeToPNG();
        Directory.CreateDirectory(Path.GetDirectoryName(fileName));
        FileStream file = File.Open(fileName, FileMode.Create);
        BinaryWriter binary = new BinaryWriter(file);
        binary.Write(bytes);
        file.Close();
    }
    
    public async static Task<string> OpenImageDialog() {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        waitingForCallback = true;
        NativeCamera.Permission permission = NativeCamera.TakePicture((path) => GetImagePath(path));
        await Task.Run(() => {
            while (waitingForCallback) {
                Thread.Sleep(100);
            }
        });
        return pathToReturn;
        
#elif UNITY_EDITOR || UNITY_STANDALONE


        // Open file with filter
        ExtensionFilter[] extensions = new[] {
        new ExtensionFilter("Image Files", "png", "jpg", "jpeg" )
        };
        //string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
        //if (paths.Length == 0)
        //    return null;
        //else
        //    return paths[0];
        IList<ItemWithStream> paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
        if (paths.Count == 0)
            return null;
        else
            return paths[0].Name;
#endif
    }

    public static void GetImagePath(string path) {
        pathToReturn = path;
        waitingForCallback = false;
    }

    public async static Task<Tuple<Sprite, string>> LoadSpriteAndSaveToDb() {
        string file = await OpenImageDialog();
        if (!string.IsNullOrEmpty(file)) {
            Sprite sprite = LoadNewSprite(file);
            string filename = Application.persistentDataPath + "/images/" + string.Format(@"{0}.png", Guid.NewGuid());
            SaveTextureToFile(sprite.texture, filename);
            return new Tuple<Sprite, string>(sprite, filename);
        }
        return null;
    }
}
