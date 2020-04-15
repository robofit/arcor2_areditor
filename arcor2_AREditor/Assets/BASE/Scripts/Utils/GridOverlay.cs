using UnityEngine;
using System.Collections;

public class GridOverlay : MonoBehaviour {

    public bool showMain = true;
    public bool showSub = false;
    public bool showAxis = true;

    public int gridSizeX = 6;
    public int gridSizeY = 0;
    public int gridSizeZ = 6;

    public float smallStep = 0.1f;
    public float largeStep = 1f;

    public float startX = -3;
    public float startY = 0;
    public float startZ = -3;

    private Material lineMaterial;

    public Color mainColor = new Color(0f, 1f, 0f, 1f);
    public Color subColor = new Color(0f, 0.5f, 0f, 1f);

    private int middleZ;
    private int middleY;
    private int middleX;

    private void Start() {
        Camera.onPostRender += OnCameraPostRender;

        middleZ = gridSizeZ / 2;
        middleY = gridSizeY / 2;
        middleX = gridSizeX / 2;
    }

    private void CreateLineMaterial() {
        if (!lineMaterial) {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int) UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    private void OnCameraPostRender(Camera cam) {
        CreateLineMaterial();
        // set the current material
        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);

        if (showSub) {
            GL.Color(subColor);

            //Layers
            for (float j = 0; j <= gridSizeY; j += smallStep) {
                //X axis lines
                for (float i = 0; i <= gridSizeZ; i += smallStep) {
                    GL.Vertex3(startX, startY + j, startZ + i);
                    GL.Vertex3(startX + gridSizeX, startY + j, startZ + i);
                }

                //Z axis lines
                for (float i = 0; i <= gridSizeX; i += smallStep) {
                    GL.Vertex3(startX + i, startY + j, startZ);
                    GL.Vertex3(startX + i, startY + j, startZ + gridSizeZ);
                }
            }

            //Y axis lines
            for (float i = 0; i <= gridSizeZ; i += smallStep) {
                for (float k = 0; k <= gridSizeX; k += smallStep) {
                    GL.Vertex3(startX + k, startY, startZ + i);
                    GL.Vertex3(startX + k, startY + gridSizeY, startZ + i);
                }
            }
        }

        if (showMain) {
            GL.Color(mainColor);
            
            //Layers
            for (float j = 0; j <= gridSizeY; j += largeStep) {
                //X axis lines
                for (float i = 0; i <= gridSizeZ; i += largeStep) {
                    if (showAxis) {
                        if (i == middleZ) {
                            GL.Color(Color.green);
                            GL.Vertex3(startX, startY + j, startZ + i);
                            GL.Vertex3(startX + gridSizeX, startY + j, startZ + i);
                            GL.Color(mainColor);
                            continue;
                        }
                    }
                    GL.Vertex3(startX, startY + j, startZ + i);
                    GL.Vertex3(startX + gridSizeX, startY + j, startZ + i);
                }

                //Z axis lines
                for (float i = 0; i <= gridSizeX; i += largeStep) {
                    if (showAxis) {
                        if (i == middleX) {
                            GL.Color(Color.red);
                            GL.Vertex3(startX + i, startY + j, startZ);
                            GL.Vertex3(startX + i, startY + j, startZ + gridSizeZ);
                            GL.Color(mainColor);
                            continue;
                        }
                    }
                    GL.Vertex3(startX + i, startY + j, startZ);
                    GL.Vertex3(startX + i, startY + j, startZ + gridSizeZ);
                }
            }

            //Y axis lines
            for (float i = 0; i <= gridSizeZ; i += largeStep) {
                for (float k = 0; k <= gridSizeX; k += largeStep) {
                    GL.Vertex3(startX + k, startY, startZ + i);
                    GL.Vertex3(startX + k, startY + gridSizeY, startZ + i);
                }
            }
        }


        GL.End();
    }
}
