using UnityEngine;

public class VoxelObject : MonoBehaviour
{

    public VoxelSpace[,,] voxelSpaceArray;

    public X[] voxelSpaceArray2;

    void Start()
    {
        if (voxelSpaceArray2 != null)
        {
            AssignColors();
        }

    }

    public void AssignColors()
    {
        if (voxelSpaceArray2 == null)
        {
            return;
        }

        for (int x = 0; x < voxelSpaceArray2.Length; x++)
        {
            for (int y = 0; y < voxelSpaceArray2[x].y.Length; y++)
            {
                for (int z = 0; z < voxelSpaceArray2[x].y[y].z.Length; z++)
                {
                    VoxelSpace voxelSpace = voxelSpaceArray2[x].y[y].z[z];
                    GameObject voxel = voxelSpace.voxel;
                    if (voxel == null)
                    {
                        continue;
                    }
                    if (Application.isPlaying == true)
                    {
                        voxel.GetComponent<Renderer>().material.color = voxelSpace.color;
                    }
                    else
                    {
                        voxel.GetComponent<Renderer>().sharedMaterial.color = voxelSpace.color;
                    }

                }
            }
        }
    }

    public void SerializeVoxelSpaceArray()
    {
        if (voxelSpaceArray == null)
        {
            return;
        }

        int x = 0;
        int y = 0;
        int z = 0;

        voxelSpaceArray2 = new X[voxelSpaceArray.GetLength(0)];
        for (x = 0; x < voxelSpaceArray.GetLength(0); x++)
        {
            voxelSpaceArray2[x] = new X();
            voxelSpaceArray2[x].y = new Y[voxelSpaceArray.GetLength(1)];
            for (y = 0; y < voxelSpaceArray.GetLength(1); y++)
            {
                voxelSpaceArray2[x].y[y] = new Y();
                voxelSpaceArray2[x].y[y].z = new VoxelSpace[voxelSpaceArray.GetLength(2)];
                for (z = 0; z < voxelSpaceArray.GetLength(2); z++)
                {

                    voxelSpaceArray2[x].y[y].z[z] = new VoxelSpace();
                    VoxelSpace voxelSpace = voxelSpaceArray[x, y, z];

                    voxelSpaceArray2[x].y[y].z[z] = new VoxelSpace();
                    VoxelSpace voxelSpace2 = voxelSpaceArray2[x].y[y].z[z];
                    //voxelSpace2 = voxelSpace;//don't call this; it will erase all references after the loops are finished; Assign references by hand further down below
                    if (voxelSpace.voxel == null)
                    {
                        continue;
                    }

                    voxelSpace2.voxel = voxelSpace.voxel;
                    voxelSpace2.color = voxelSpace.color;
                }
            }
        }
    }

}
[System.Serializable]
public class X
{
    public Y[] y;

    public X()
    {
        y = new Y[1];
    }

}
[System.Serializable]
public class Y
{
    public VoxelSpace[] z;

    public Y()
    {
        z = new VoxelSpace[1];
    }
}

[System.Serializable]
public class VoxelSpace
{

    public Vector3 pos;
    public bool filled = false;
    public bool hit_t;
    public bool hit_n;
    public bool hit_e;
    public bool hit_s;
    public bool hit_w;
    public bool hit_b;
    public GameObject voxel;
    public Color color;
}