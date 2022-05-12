using UnityEngine;

[CreateAssetMenu(menuName = "Match-3/BlockType")]
public class BlockType : ScriptableObject
{
    public int Value;
    
    [SerializeField] private Sprite sprite;
    public Sprite BlockSprite => sprite;
}
