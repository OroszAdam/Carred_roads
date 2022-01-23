using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{
    //Transform of the player
    public Transform player;

    public Vector3 offset = new Vector3(0f, 0f, 0f);

    private void Start()
    {
        if (!GameManager.Instance.IsDead)
        {
            offset = this.transform.position;
            transform.position = player.position + offset;
        }
        //transform.rotation = Quaternion.Euler(45, 0, 0);
    }

    private void LateUpdate()
    {
        if (!GameManager.Instance.IsDead)
        {
            Vector3 desiredPosition = player.position + offset;
            desiredPosition.x = 0;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 3);
        }
    }
}
