using UnityEngine;

public class ReplaceBodyOnCollision : MonoBehaviour
{
    private bool _isBodyReplaced = false;

    public GameObject NormalBody;
    public GameObject ReplacementBody;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Car" || collision.collider.tag == "Player")
        {
            if (!_isBodyReplaced)
            {
                NormalBody.SetActive(false);
                ReplacementBody.SetActive(true);
            }
        }
    }
}
