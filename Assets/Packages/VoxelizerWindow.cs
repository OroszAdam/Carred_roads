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

    private int buttonSize = 160;
    private Vector2 scrollPos_window = Vector2.zero;

    private GameObject selectedObject;
    private bool useUv = true;
    private float unit = 1f;
    private int resolution = 15;
    private string folderName = "FolderName";
    private GameObject voxelPrefab;


    void Update()
    {
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
        if (GUILayout.Toggle(useUv, new GUIContent("Use UV Texture", ""), GUILayout.Width(buttonSize * 6)) != useUv)
        {
            Event.current.Use();
            useUv = !useUv;
        }
        GUILayout.Label("To use the UV\nmaterial asset must be located in Resources folder!");

        unit = EditorGUILayout.FloatField("Unit", unit);
        resolution = EditorGUILayout.IntField("Resolution", resolution);
        folderName = EditorGUILayout.TextField("Folder name", folderName);


        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });

        if (GUILayout.Button(new GUIContent("Create voxelized multibody with colliders", "")))
        {
            CreateVoxelizedMultibody();
        }

        if (GUILayout.Button(new GUIContent("Create voxelized single-body object", "")))
        {
            CreateVoxelizedMesh();
        }

        scrollPos_window = EditorGUILayout.BeginScrollView(scrollPos_window);

        EditorGUILayout.EndScrollView();
    }

    private void CreateVoxelizedMesh()
    {
        selectedObject = (Selection.activeObject != null) ? (GameObject)Selection.activeObject : null;

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
        GameObject parent = new GameObject("Voxelized");
        parent.AddComponent<MeshRenderer>();
        parent.GetComponent<MeshFilter>().sharedMesh = VoxelMesh.Build(voxels.ToArray(), unit, useUv);
    }

    private void CreateVoxelizedMultibody()
    {
        selectedObject = (Selection.activeObject != null) ? (GameObject)Selection.activeObject : null;

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
            mesh,           // a target mesh
            resolution,     // # of voxels for largest AABB bounds
            out voxels,
            out float unit
        );

        // build gameobject with individual, colliding voxel cube objects
        Material material = selectedObject.GetComponent<MeshRenderer>().sharedMaterial;
        GameObject go = VoxelMesh.BuildObject(voxels.ToArray(), unit, material, folderName, useUv);
        go.GetComponent<MeshRenderer>().material = material;
    }
}
