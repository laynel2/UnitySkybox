using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

[ExecuteInEditMode]
public class CubeMapMaker : MonoBehaviour
{
    Camera thisCamera;
    public string filename = "SkyGradient";
    string filetowrite = "";
    int counter = 0;
    Texture2D Image;

    public enum Sizes { _128 = 128, _256 = 256, _512 = 512, _1024 = 1024, _2048 = 2048, _4096 = 4096 };
    public Sizes RenderSize = Sizes._512;
    readonly string[] faces = { "Front", "Back", "Left", "Right", "Up", "Down" };
    //public enum AntiAliasing { _0 = 0, _2 = 2, _4 = 4, _8 = 8};
    //public AntiAliasing AntiAliasingAmount;
    //unity limits your ability to set the anti aliasing amount via code. I think I need to make my own consturctor 

    float fov = 60f;
    int currentdisplay = 0;

    string subpath;


    public void MakeCubeMap()
    {
        if (thisCamera.targetTexture != null)
        {
            thisCamera.targetTexture.Release();
        }

        thisCamera.targetTexture = new RenderTexture((int)RenderSize, (int)RenderSize, 24);
        //thisCamera.targetTexture.antiAliasing = (int)AntiAliasingAmount;

        thisCamera.fieldOfView = 90f;
        filetowrite = filename + counter;

        ResetCamera();
        int xRotation = -90;
        for (int i = 0; i < 3; i++)
        {
            thisCamera.transform.eulerAngles = new Vector3(xRotation, 0f, 0f);
            switch (xRotation)
            {
                case -90:
                    Capture("Up");
                    break;
                case 0:
                    Capture("Front");
                    break;
                case 90:
                    Capture("Down");
                    break;
            }
            xRotation += 90;
        }

        ResetCamera();
        int yRotation = 90;
        for (int i = 0; i < 3; i++)
        {
            thisCamera.transform.eulerAngles = new Vector3(0f, yRotation, 0f);
            switch (yRotation)
            {
                case 90:
                    Capture("Left");
                    break;
                case 180:
                    Capture("Back");
                    break;
                case 270:
                    Capture("Right");
                    break;
            }
            yRotation += 90;
        }

        if (thisCamera.targetTexture != null)
        {
            thisCamera.targetTexture.Release();
            thisCamera.targetTexture = null;
            thisCamera.targetDisplay = currentdisplay;
            thisCamera.fieldOfView = fov;
            ResetCamera();
        }

        Material newSkyboxMaterial = new Material(Shader.Find("Skybox/6 Sided"));

        AssetDatabase.CreateAsset(newSkyboxMaterial, "Assets/GradientSkyboxes/" + filename + "/" + filetowrite + ".mat");
        ForceImportImages(newSkyboxMaterial);
        counter++;
    }

    void Capture(string location)
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = thisCamera.targetTexture;

        thisCamera.Render();

        Image = new Texture2D(thisCamera.targetTexture.width, thisCamera.targetTexture.height);

        Image.ReadPixels(new Rect(0, 0, thisCamera.targetTexture.width, thisCamera.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;

        var Bytes = Image.EncodeToPNG();
        DestroyImmediate(Image);
        string path = Application.dataPath + "/GradientSkyboxes/";
        string fullfileName = location + "_" + filetowrite + "_" + ".png";
        subpath = path + filename + "/";

        Debug.Log("writing: " + fullfileName);
        if (Directory.Exists(path))
        {
            if (!Directory.Exists(subpath))
            {
                Directory.CreateDirectory(subpath);
                if (!File.Exists(subpath + fullfileName))
                {
                    File.WriteAllBytes(subpath + fullfileName, Bytes);
                }
                else
                {
                    Debug.LogError("Name Already in Use");
                }
            }
            else
            {
                if (!File.Exists(subpath + fullfileName))
                {
                    File.WriteAllBytes(subpath + fullfileName, Bytes);
                }
                else
                {
                    Debug.LogError("Name Already in Use");
                }
            }
        }
        else
        {
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(subpath);
            if (!File.Exists(subpath + fullfileName))
            {
                File.WriteAllBytes(subpath + fullfileName, Bytes);
            }
            else
            {
                Debug.LogError("Name Already in Use");
            }
        }
        Debug.Log(fullfileName + " is done!");
    }

    void Start()
    {
        thisCamera = GetComponent<Camera>();

        fov = thisCamera.fieldOfView;
        currentdisplay = thisCamera.targetDisplay;
    }

    void ResetCamera()
    {
        thisCamera.transform.rotation = Quaternion.identity;
        thisCamera.transform.position = Vector3.zero;
    }

    void ForceImportImages(Material mat)
    {
        string relativePath = "Assets/GradientSkyboxes/" + filename + "/";
        AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ImportRecursive);

        foreach (string file in System.IO.Directory.GetFiles(relativePath))
        {
            if (Path.GetExtension(relativePath + file) == ".png" && file.Contains(counter.ToString()))
            {
                Texture2D t = LoadPNG(file);
                foreach (string face in faces)
                {
                    if (file.Contains(face))
                    {
                        mat.SetTexture("_" + face + "Tex", t); //_FrontTex
                    }
                }
            }
        }

        ChangeSkyboxMaterial(mat);
    }

    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }

    public static void ChangeSkyboxMaterial (Material skymaterial)
    {
        RenderSettings.skybox = skymaterial;
    }

    public void ReturnSkybox()
    {
        ChangeSkyboxMaterial(AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat"));
    }
}
