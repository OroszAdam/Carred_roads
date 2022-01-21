using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breaking : MonoBehaviour
{
    public GameObject NormalBody;
    public GameObject VoxelizedBody;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag=="Enemy")
        {
            NormalBody.SetActive(false);
            VoxelizedBody.SetActive(true);
        }
    }
}
