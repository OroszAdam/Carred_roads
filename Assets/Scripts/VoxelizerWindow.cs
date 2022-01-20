using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using VoxelSystem;

class VoxelizerWindow : EditorWindow
{
    [MenuItem("Window/Voxelizer")]

    public static void ShowWindow()
    {
        GetWindow(typeof(VoxelizerWindow), false, "Voxelizer", true);
    }

    private int buttonSize = 32;
    private Vector2 scrollPos_window = Vector2.zero;

    private GameObject selectedObject;
    private bool useUv = false;
    private float unit = 1f;
    private int resolution = 100;
    private GameObject voxelPrefab;


    void Update()
    {
        if (Selection.activeObject != null)
        {
            selectedObject = (GameObject)Selection.activeObject;
            if (selectedObject.GetComponent<VoxelObject>())
            {
            }
        }
        else
        {
            selectedObject = null;
        }
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        string selectedGameObjectName = "No GameObject selected.";
        if (selectedObject != null)
        {
            selectedGameObjectName = Selection.activeGameObject.name;
        }
        
        GUILayout.Label(selectedGameObjectName, EditorStyles.boldLabel);
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });

        voxelPrefab = EditorGUILayout.ObjectField("Voxel Prefab", voxelPrefab, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;

        if (GUILayout.Toggle(useUv, new GUIContent("Use UV Texture", ""), GUILayout.Width(buttonSize * 6)) != useUv)
        {
            Event.current.Use();
            useUv = !useUv;
        }

        unit = EditorGUILayout.FloatField("Unit", unit);
        resolution = EditorGUILayout.IntField("Resolution", resolution);

        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });

        if (GUILayout.Button(new GUIContent("Voxelize!", ""), GUILayout.Width(buttonSize * 4)))
        {
            if (Selection.activeObject != null)
            {
                selectedObject = (GameObject)Selection.activeObject;
                if (selectedObject.GetComponent<VoxelObject>())
                {
                }
            }
            else
            {
                selectedObject = null;
            }

            Mesh mesh;
            MeshFilter meshFilter = selectedObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                mesh = meshFilter.sharedMesh;
            }
            else
            {
                mesh = selectedObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            }

            List<Voxel_t> voxels;
            CPUVoxelizer.Voxelize(
                mesh,   // a target mesh
                resolution,     // # of voxels for largest AABB bounds
                out voxels,
                out float unit
            );

            // build voxel cubes integrated mesh
            //selectedObject.GetComponent<MeshFilter>().sharedMesh = VoxelMesh.Build(voxels.ToArray(), unit, useUv);

            Material material = selectedObject.GetComponent<MeshRenderer>().material;
            GameObject go = VoxelMesh.BuildGameObject(voxels.ToArray(), unit, material, useUv);
            go.GetComponent<MeshRenderer>().material = material;

        }

        scrollPos_window = EditorGUILayout.BeginScrollView(scrollPos_window);

        EditorGUILayout.EndScrollView();
    }
}
