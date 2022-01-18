using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    public PieceType type;
    private Piece currentPiece;

    public void Spawn()
    {
        int amountOfObjs = 0;
        switch (type)
        {
            case PieceType.jump:
                amountOfObjs = LevelManager.Instance.jumps.Count;
                break;
            case PieceType.slide:
                amountOfObjs = LevelManager.Instance.slides.Count;
                break;
            case PieceType.longblock:
                amountOfObjs = LevelManager.Instance.longblocks.Count;
                break;
            case PieceType.ramp:
                amountOfObjs = LevelManager.Instance.ramps.Count;
                break;
        }

        //get a new piece from the pool of pieces
        currentPiece = LevelManager.Instance.GetPiece(type, Random.Range(0, amountOfObjs));
        currentPiece.gameObject.SetActive(true);
        currentPiece.transform.SetParent(transform, false); 
    }
    public void Despawn()
    {
        currentPiece.gameObject.SetActive(false);
    }
}
