using UnityEngine;

[CreateAssetMenu(menuName = "Match-3/BlockPool")]
public class BlockPool : ScriptableObject
{
    [SerializeField] private BlockType[] pool;
    public BlockType[] Pool => pool;
}
