using UnityEngine;
using DG.Tweening;

public class BlockView : MonoBehaviour
{
    
    [SerializeField] private BlockType blockdata;
    [SerializeField] private SpriteRenderer blockSprRenderer;

    public BlockType Type => blockdata;
    public Vector2Int Coordinates { get; private set; }
    public Vector2 Location { get; private set; }

    public void InitializeBlockView(BlockType type, Vector2Int initCoordinates)
    {
        blockdata = type;

        blockSprRenderer.sprite = blockdata.BlockSprite;

        Coordinates = initCoordinates;
        Location = BoardUtils.GetWorldPosFromTileCoord(Coordinates);

        transform.position = Location;
    }

    public void UpdateCoordinates(Vector2Int targetCoord, float unitDuration = 0)
    {
        int unitChange = GetUnitDistance(targetCoord, Coordinates);
        Coordinates = targetCoord;
        Location = BoardUtils.GetWorldPosFromTileCoord(Coordinates);
        transform.DOMove(Location, unitChange * unitDuration).SetEase(Ease.Linear);
        //transform.position = Location;
    }

    public void RemoveBlock(float duration = 0)
    {
        blockSprRenderer.transform.DOScale(0, duration).SetEase(Ease.Linear).OnComplete(() => Destroy(gameObject));
    }
    
    private static int GetUnitDistance(Vector2Int a, Vector2Int b)
    {
        var xDiff = Mathf.Abs(a.x - b.x);
        var yDiff = Mathf.Abs(a.y - b.y);

        return Mathf.Max(xDiff, yDiff);
    }
}
