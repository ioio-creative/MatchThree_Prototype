using UnityEngine;

[CreateAssetMenu(menuName = "Match-3/BlockType")]
public class BlockType : ScriptableObject
{
    public int Value;
    
    [SerializeField] private Sprite sprite;
    [SerializeField] private Sprite bombSprite;
    public Sprite BlockSprite => sprite;
    public Sprite BombSprite => bombSprite == null ? sprite : bombSprite;
}
