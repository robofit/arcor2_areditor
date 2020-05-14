using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using USFB;

public class ImageHelper
{

    public static Sprite LoadNewSprite(string filePath, float pixelsPerUnit = 100.0f) {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Sprite NewSprite;
        Texture2D SpriteTexture = LoadTexture(filePath);
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

    public static string OpenImageDialog() {
        // Open file with filter
        ExtensionFilter[] extensions = new[] {
        new ExtensionFilter("Image Files", "png", "jpg", "jpeg" )
        };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
        if (paths.Length == 0)
            return null;
        else
            return paths[0];
    }

    public static Sprite LoadSpriteAndSaveToDb(out string filename) {
        string file = OpenImageDialog();
        filename = null;
        if (!string.IsNullOrEmpty(file)) {
            Sprite sprite = LoadNewSprite(file);
            filename = Application.persistentDataPath + "/images/" + string.Format(@"{0}.png", Guid.NewGuid());
            SaveTextureToFile(sprite.texture, filename);
            return sprite;
        }
        return null;
    }
}
