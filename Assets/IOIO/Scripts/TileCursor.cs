using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCursor : MonoBehaviour
{
    [SerializeField] private SpriteRenderer cursorSpr;

    public void SelectTile(Vector3 position)
    {
        transform.position = position;
        cursorSpr.enabled = true;
    }

    public void Deselect()
    {
        transform.position = Vector3.zero;
        cursorSpr.enabled = false;
    }
}
