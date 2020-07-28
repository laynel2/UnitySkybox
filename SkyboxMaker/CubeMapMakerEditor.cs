using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

[CustomEditor(typeof(CubeMapMaker))]
public class CubeMapMakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CubeMapMaker myScript = (CubeMapMaker)target;
        if (GUILayout.Button("RenderCubeMap"))
        {
            myScript.MakeCubeMap();
        }
    }
}