using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor; //for PrefabUtility

public class Voxelizer : MonoBehaviour
{
    public GameObject objectToVoxelize;
    public GameObject voxelizedObject;
    public float delay = 0.5f;
    Vector3 p0;
    Vector3 p1;
    private Vector3 voxelPos;
    public GameObject voxelPrefab;
    public bool waitForTime = false;
    public bool waitOneFrame = false;
    public bool fill = true;
    public float fillPercentage_s = 100f;
    public float fillPercentage_i = 100f;
    public Color[] fillColors;
    public bool createPB = true;
    public bool keepVO = true;

    // Use this for initialization
    void Start()
    {
        LoadVoxelPrefab();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            StartCoroutine(Voxelize(objectToVoxelize, 16, fill, fillPercentage_s, fillPercentage_i, fillColors, createPB, "Models", keepVO, true));
        }
    }

    public void LoadVoxelPrefab()
    {
        voxelPrefab = Resources.Load("Models/Misc/Voxel") as GameObject;
    }

    public void StartVoxelize(GameObject go, int resolution, bool fillInside, float fillPercentage_shell, float fillPercentage_inside, Color[] insideColorArray, bool createPrefab, string prefabPath, bool keepVoxelizedObject, bool useMeshName)
    {
        StartCoroutine(Voxelize(go, resolution, fillInside, fillPercentage_shell, fillPercentage_inside, insideColorArray, createPrefab, prefabPath, keepVoxelizedObject, useMeshName));
    }

    public IEnumerator Voxelize(GameObject go, int resolution, bool fillInside, float fillPercentage_shell, float fillPercentage_inside, Color[] insideColorArray, bool createPrefab, string prefabPath, bool keepVoxelizedObject, bool useMeshName)
    {

        float voxelSize = 1f / resolution;
        float voxelSizeHalf = voxelSize / 2;


        Mesh mesh;
        MeshFilter meshFilter = go.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            mesh = meshFilter.sharedMesh;
        }
        else
        {
            mesh = go.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        }
        go.SetActive(false);

        GameObject goClone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        goClone.name = go.name + "_temp";
        goClone.transform.position = go.transform.position;
        goClone.transform.rotation = go.transform.rotation;

        Collider[] colliders = goClone.GetComponents<Collider>();
        foreach (Collider collider in colliders)
        {
            if (Application.isPlaying)
            {
                Destroy(collider);
            }
            else
            {
                DestroyImmediate(collider);
            }
        }

        goClone.AddComponent<MeshCollider>();
        goClone.GetComponent<MeshCollider>().sharedMesh = mesh;
        goClone.GetComponent<MeshFilter>().sharedMesh = mesh;

        if (Application.isPlaying)
        {
            goClone.GetComponent<MeshRenderer>().material = go.GetComponent<Renderer>().material;
        }
        else
        {
            goClone.GetComponent<MeshRenderer>().material = go.GetComponent<Renderer>().sharedMaterial;
        }

        goClone.layer = go.layer;

        Texture2D texture = null;
        if (Application.isPlaying)
        {
            texture = goClone.GetComponent<Renderer>().material.mainTexture as Texture2D;
        }
        else
        {
            texture = goClone.GetComponent<Renderer>().sharedMaterial.mainTexture as Texture2D;
        }

        string layerName = LayerMask.LayerToName(goClone.layer);
        LayerMask layerMask = new LayerMask();
        layerMask |= (1 << LayerMask.NameToLayer(layerName));

        Vector3 p0_pre = goClone.GetComponent<Renderer>().bounds.min;
        Vector3 p1_pre = goClone.GetComponent<Renderer>().bounds.max;

        Vector3 p0_pre_aligned = WorldToVoxelSpace(p0_pre, resolution, true);
        Vector3 p1_pre_aligned = WorldToVoxelSpace(p1_pre, resolution, true);

        float offset_x = 0f;
        float offset_y = 0f;
        float offset_z = 0f;

        if (p0_pre.x >= p0_pre_aligned.x)
        {
            offset_x = (p0_pre.x - p0_pre_aligned.x) * -1;
        }
        else
        {
            offset_x = p0_pre_aligned.x - p0_pre.x;
        }

        if (p0_pre.y >= p0_pre_aligned.y)
        {
            offset_y = (p0_pre.y - p0_pre_aligned.y) * -1;
        }
        else
        {
            offset_y = p0_pre_aligned.y - p0_pre.y;
        }

        if (p0_pre.z >= p0_pre_aligned.z)
        {
            offset_z = (p0_pre.z - p0_pre_aligned.z) * -1;
        }
        else
        {
            offset_z = p0_pre_aligned.z - p0_pre.z;
        }

        Vector3 offset = new Vector3(offset_x - voxelSizeHalf, offset_y - voxelSizeHalf, offset_z - voxelSizeHalf);

        goClone.transform.position = new Vector3(goClone.transform.position.x + offset.x, goClone.transform.position.y + offset.y, goClone.transform.position.z + offset.z);

        Vector3 p0_post = goClone.GetComponent<Renderer>().bounds.min;
        Vector3 p1_post = goClone.GetComponent<Renderer>().bounds.max;

        p0 = WorldToVoxelSpace(goClone.GetComponent<Renderer>().bounds.min + new Vector3(voxelSizeHalf, voxelSizeHalf, voxelSizeHalf), resolution, true);
        p1 = WorldToVoxelSpace(goClone.GetComponent<Renderer>().bounds.max - new Vector3(voxelSizeHalf, voxelSizeHalf, voxelSizeHalf), resolution, true);

        GameObject go_vox_root_prefab = voxelPrefab;
        GameObject go_vox_root = Instantiate(go_vox_root_prefab, goClone.transform.position, goClone.transform.rotation) as GameObject;
        if (useMeshName == true)
        {
            go_vox_root.name = mesh.name + "_voxelized";
        }
        else
        {
            go_vox_root.name = go.name + "_voxelized";
        }

        Vector3 p0_voxelSpace = VoxelToLocalVoxelSpace(p0, resolution, Vector3.zero);
        Vector3 p1_voxelSpace = VoxelToLocalVoxelSpace(p1, resolution, Vector3.zero);

        int length_0 = (int)(p1_voxelSpace.x - p0_voxelSpace.x) + 1;
        int length_1 = (int)(p1_voxelSpace.y - p0_voxelSpace.y) + 1;
        int length_2 = (int)(p1_voxelSpace.z - p0_voxelSpace.z) + 1;

        go_vox_root.AddComponent<VoxelObject>();
        go_vox_root.GetComponent<VoxelObject>().voxelSpaceArray = new VoxelSpace[length_0, length_1, length_2];
        VoxelSpace[,,] voxelSpaceArray = go_vox_root.GetComponent<VoxelObject>().voxelSpaceArray;

        int x = 0;
        int y = 0;
        int z = 0;

        for (x = 0; x < voxelSpaceArray.GetLength(0); x++)
        {
            for (y = 0; y < voxelSpaceArray.GetLength(1); y++)
            {
                for (z = 0; z < voxelSpaceArray.GetLength(2); z++)
                {
                    voxelSpaceArray[x, y, z] = new VoxelSpace();
                }
            }
        }

        //Register outer voxels
        for (x = 0; x < voxelSpaceArray.GetLength(0); x++)
        {
            for (y = 0; y < voxelSpaceArray.GetLength(1); y++)
            {
                for (z = 0; z < voxelSpaceArray.GetLength(2); z++)
                {
                    Vector3 voxelWorldPos = new Vector3(p0.x + x * voxelSize, p0.y + y * voxelSize, p0.z + z * voxelSize);

                    Vector3[] ray_origin_array = new Vector3[] {
                        voxelWorldPos - Vector3.forward * voxelSize,
                        voxelWorldPos + Vector3.forward * voxelSize,
                        voxelWorldPos - Vector3.right * voxelSize,
                        voxelWorldPos + Vector3.right * voxelSize,
                        voxelWorldPos - Vector3.up * voxelSize,
                        voxelWorldPos + Vector3.up * voxelSize
                    };

                    Vector3[] ray_dir_array = new Vector3[] {
                        Vector3.forward,
                        -Vector3.forward,
                        Vector3.right,
                        -Vector3.right,
                        Vector3.up,
                        -Vector3.up
                    };

                    int i = 0;
                    for (i = 0; i < 6; i++)
                    {
                        Ray ray = new Ray(ray_origin_array[i], ray_dir_array[i]);
                        RaycastHit hit = new RaycastHit();


                        RaycastHit[] hitArray = Physics.RaycastAll(ray, voxelSize, layerMask).OrderBy(h => h.distance).ToArray();
                        bool hasHit = false;
                        foreach (RaycastHit hitCur in hitArray)
                        {
                            if (hitCur.collider.gameObject == goClone)
                            {
                                hasHit = true;
                                hit = hitCur;
                                break;
                            }
                        }

                        if (hasHit == true)
                        {
                            Debug.DrawLine(ray.origin, hit.point, Color.red, delay);

                            //Center of the voxelspace that was hit
                            Vector3 insideHit_aligned_center = WorldToVoxelSpace(hit.point + ray.direction * voxelSizeHalf, resolution, true);

                            //the xyz position of the voxel that was hit, in GLOBAL voxelspace
                            Vector3 insideHit_voxelSpace = VoxelToLocalVoxelSpace(insideHit_aligned_center, resolution, Vector3.zero);

                            //the voxelspace xyz position in LOCAL voxelspace
                            Coordinate voxelPos = new Coordinate((int)(insideHit_voxelSpace.x - p0_voxelSpace.x), (int)(insideHit_voxelSpace.y - p0_voxelSpace.y), (int)(insideHit_voxelSpace.z - p0_voxelSpace.z));

                            VoxelSpace voxelSpace = voxelSpaceArray[voxelPos.x, voxelPos.y, voxelPos.z];
                            if (i == 0) { voxelSpace.hit_s = true; }
                            else if (i == 1) { voxelSpace.hit_n = true; }
                            else if (i == 2) { voxelSpace.hit_w = true; }
                            else if (i == 3) { voxelSpace.hit_e = true; }
                            else if (i == 4) { voxelSpace.hit_b = true; }
                            else if (i == 5) { voxelSpace.hit_t = true; }

                            if (voxelSpace.filled == false)
                            {
                                voxelSpace.filled = true;

                                if (fillPercentage_shell > 0)
                                {
                                    if (UnityEngine.Random.Range(0f, 100f) <= fillPercentage_shell)
                                    {
                                        Vector2 pixelUV = hit.textureCoord;
                                        pixelUV.x *= texture.width;
                                        pixelUV.y *= texture.height;
                                        Color color = texture.GetPixel((int)pixelUV.x, (int)pixelUV.y);
                                        CreateVoxelInArray(voxelSpace, insideHit_aligned_center, color, resolution, go_vox_root.transform);
                                    }
                                }
                            }
                        }
                    }
                    if (waitForTime == true)
                    {
                        if (delay > 0)
                        {
                            yield return new WaitForSeconds(delay);
                        }
                    }

                    else if (waitOneFrame == true)
                    {
                        yield return null;
                    }
                }
            }
        }

        //Fill in inner voxels
        if (fillInside == true)
        {
            for (x = 0; x < voxelSpaceArray.GetLength(0); x++)
            {
                for (y = 0; y < voxelSpaceArray.GetLength(1); y++)
                {
                    for (z = 0; z < voxelSpaceArray.GetLength(2); z++)
                    {

                        if (waitForTime == true)
                        {
                            if (delay > 0)
                            {
                                yield return new WaitForSeconds(delay);
                            }
                        }

                        else if (waitOneFrame == true)
                        {
                            yield return null;
                        }

                        VoxelSpace voxelSpace = voxelSpaceArray[x, y, z];
                        Vector3 voxelWorldPos = new Vector3(p0.x + x * voxelSize, p0.y + y * voxelSize, p0.z + z * voxelSize);

                        if (voxelSpace.filled == false)
                        {
                            continue;
                        }
                        if (z + 1 >= voxelSpaceArray.GetLength(2))
                        {
                            continue;
                        }
                        int i = 0;
                        int toFill = 0;
                        for (i = z + 1; i < voxelSpaceArray.GetLength(2); i++)
                        {
                            if (i == z + 1 && voxelSpaceArray[x, y, i].filled == true)
                            {
                                break;
                            }
                            toFill++;
                            if (i == voxelSpaceArray.GetLength(2) - 1 && voxelSpaceArray[x, y, i].filled == false)
                            {
                                toFill = 0;
                                break;
                            }
                            if (voxelSpaceArray[x, y, i].filled == true)
                            {
                                break;
                            }
                        }

                        if (toFill > 0)
                        {
                            for (i = z + 1; i < z + 1 + toFill; i++)
                            {

                                Vector3 voxelWorldPos_temp = new Vector3(p0.x + x * voxelSize, p0.y + y * voxelSize, p0.z + i * voxelSize);

                                Vector3 pos = new Vector3(p0.x + x * voxelSize, p0.y + y * voxelSize, p0.z + i * voxelSize);
                                if (voxelSpaceArray[x, y, i].filled == false)
                                {
                                    if (fillPercentage_inside > 0)
                                    {
                                        if (UnityEngine.Random.Range(0f, 100f) > fillPercentage_inside)
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    Color insideColor = Color.white;
                                    if (insideColorArray.Length > 0)
                                    {
                                        if (insideColorArray.Length == 1)
                                        {
                                            insideColor = insideColorArray[0];
                                        }
                                        else
                                        {
                                            insideColor = insideColorArray[UnityEngine.Random.Range(0, insideColorArray.Length)];
                                        }
                                    }
                                    CreateVoxelInArray(voxelSpaceArray[x, y, i], pos, insideColor, resolution, go_vox_root.transform);
                                }

                                if (waitForTime == true)
                                {
                                    if (delay > 0)
                                    {
                                        yield return new WaitForSeconds(delay);
                                    }
                                }
                            }
                            break;
                        }

                    }
                }
            }
        }

        go_vox_root.transform.parent = goClone.transform;
        goClone.transform.position = go.transform.position;
        go_vox_root.transform.parent = null;

        if (Application.isPlaying)
        {
            Destroy(goClone);
        }
        else
        {
            DestroyImmediate(goClone);
        }

        go.SetActive(true);


        if (go_vox_root.GetComponent<MeshFilter>() == null)
            go_vox_root.AddComponent<MeshFilter>();
                        
        go_vox_root.GetComponent<MeshFilter>().mesh = mesh;

        voxelizedObject = go_vox_root;
        voxelizedObject.GetComponent<VoxelObject>().SerializeVoxelSpaceArray();

        if (createPrefab == true)
        {
            PrefabUtility.CreatePrefab("Assets/Resources/" + prefabPath + "/" + go_vox_root.name + ".prefab", voxelizedObject, ReplacePrefabOptions.ReplaceNameBased);
        }

        if (keepVoxelizedObject == false)
        {
            if (Application.isPlaying)
            {
                Destroy(go_vox_root);
            }
            else
            {
                DestroyImmediate(go_vox_root);
            }
        }
        else
        {
            voxelizedObject.GetComponent<VoxelObject>().AssignColors();
        }

        yield return null;
    }



    public Vector3 WorldToVoxelSpace(Vector3 worldPos, int resolution, bool center)
    {
        return WorldToVoxelSpace(worldPos.x, worldPos.y, worldPos.z, resolution, center);
    }
    public Vector3 WorldToVoxelSpace(float worldPos_x, float worldPos_y, float worldPos_z, int resolution, bool center)
    {
        return new Vector3(WorldToVoxelSpace(worldPos_x, resolution, center), WorldToVoxelSpace(worldPos_y, resolution, center), WorldToVoxelSpace(worldPos_z, resolution, center));
    }
    public float WorldToVoxelSpace(float worldPos, int resolution, bool center)
    {
        float voxelSize = 1f / resolution;
        float offset = 0;
        if (center == true)
        {
            offset = voxelSize / 2;
        }
        float voxelSizeHalf = voxelSize / 2;
        return (float)(Math.Round((worldPos + voxelSizeHalf) * resolution) / (float)resolution) - offset;
    }

    public Vector3 VoxelToLocalVoxelSpace(Vector3 voxelSpacePos, int resolution, Vector3 pos_min)
    {
        return VoxelToLocalVoxelSpace(voxelSpacePos.x, voxelSpacePos.y, voxelSpacePos.z, resolution, pos_min);
    }
    public Vector3 VoxelToLocalVoxelSpace(float voxelSpacePos_x, float voxelSpacePos_y, float voxelSpacePos_z, int resolution, Vector3 pos_min)
    {
        return new Vector3(VoxelToLocalVoxelSpace(voxelSpacePos_x, resolution, pos_min.x), VoxelToLocalVoxelSpace(voxelSpacePos_y, resolution, pos_min.y), VoxelToLocalVoxelSpace(voxelSpacePos_z, resolution, pos_min.z));
    }
    public float VoxelToLocalVoxelSpace(float pos, int resolution, float pos_min)
    {
        float voxelSize = 1f / resolution;
        return Mathf.Floor(pos / voxelSize);
    }


    public void CreateVoxelInArray(VoxelSpace voxelSpace, Vector3 pos, Color color, int resolution, Transform parent)
    {
        float voxelSize = 1f / resolution;
        voxelSpace.pos = pos;
        voxelSpace.filled = true;
        voxelSpace.color = color;
        voxelSpace.voxel = Instantiate(voxelPrefab, pos, Quaternion.identity) as GameObject;
        voxelSpace.voxel.layer = LayerMask.NameToLayer("Voxel");
        voxelSpace.voxel.transform.localScale = new Vector3(voxelSize, voxelSize, voxelSize);
        voxelSpace.voxel.transform.parent = parent;
    }

    public void CreateVoxelInArray(VoxelSpace voxelSpace, Vector3 pos, Vector2 textureCoords, int resolution, Transform parent)
    {
        float voxelSize = 1f / resolution;
        voxelSpace.pos = pos;
        voxelSpace.filled = true;
        voxelSpace.color = Color.red;
        voxelSpace.voxel = Instantiate(voxelPrefab, pos, Quaternion.identity) as GameObject;
        voxelSpace.voxel.transform.localScale = new Vector3(voxelSize, voxelSize, voxelSize);
        voxelSpace.voxel.transform.parent = parent;

        Mesh mesh = voxelSpace.voxel.GetComponent<MeshFilter>().sharedMesh;
        Vector2[] uvs = new Vector2[mesh.uv.Length];

        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = textureCoords;
        }

        mesh.uv = uvs;
        voxelSpace.voxel.GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}

public class Coordinate
{
    public int x;
    public int y;
    public int z;
    public Coordinate() { }
    public Coordinate(int x1, int y1, int z1)
    {
        x = x1;
        y = y1;
        z = z1;
    }
}