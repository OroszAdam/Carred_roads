using System.Collections.Generic;
using UnityEngine;

public class ReplaceBodyOnCollision : MonoBehaviour
{
    private bool _isBodyReplaced = false;

    public List<string> TagsToBreakTo = new List<string>() { "Player" };
    public GameObject NormalBody;
    public GameObject ReplacementBody;

    private void OnCollisionEnter(Collision collision)
    {
        if (TagsToBreakTo.Contains(collision.collider.tag))
        {
            if (!_isBodyReplaced)
            {
                NormalBody.SetActive(false);
                ReplacementBody.SetActive(true);
            }
        }
    }
}
