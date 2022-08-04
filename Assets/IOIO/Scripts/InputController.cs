using System;
using UnityEngine;

public class InputController : MonoBehaviour
{  
    public static Action<Vector2Int> OnTileClicked;

    [SerializeField] private LayerMask mouseRaycastMask;

    [SerializeField] private TileCursor cursor;

    private Vector2Int? selectedCoord;
    private bool isCursorActive;

    private void Awake()
    {
        BoardController.OnCoordSelectAck = SelectTileAck;
    }

    private void Start()
    {
        cursor.Deselect();
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            CheckTileClick();
        }
    }

    /// <summary>
    /// Turns on/off whether InputController accepts input.
    /// Will deselect any active cursor if turned off.
    /// </summary>
    /// <param name="value"></param>
    public void SetCursorActive(bool value)
    {
        isCursorActive = value;
        if (!isCursorActive)
        {
            cursor.Deselect();
            selectedCoord = null;
        }
    }

    private bool CheckTileClick()
    {
        if (!isCursorActive) return false;

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100, mouseRaycastMask);

        if (hit.collider != null)
        {
            var block = hit.transform.GetComponentInParent<BlockView>();
            if (block != null)
            {
                OnTileClicked?.Invoke(block.Coordinates);
            }

            return true;
        }

        return false;
    }

    //Callback function of this.OnTileClicked => BoardController.OnCoordSelectAck
    private void SelectTileAck(bool isSwap, Vector2Int coord)
    {
        if (!isSwap && selectedCoord != coord)
        {
            cursor.SelectTile(BoardUtils.GetWorldPosFromTileCoord(coord));
            selectedCoord = coord;
        }
        else
        {
            cursor.Deselect();
            selectedCoord = null;
        }
    }

}
