
using UnityEngine;
using UnityEngine.UI;

public class InfoView : MonoBehaviour
{
    [SerializeField] private Text seed;
    [SerializeField] private Text moves;
    [SerializeField] private Text savePrompt;
    [SerializeField] private InputField clipboard;

    public void UpdateBoard(int _seed)
    {
        seed.text = "Seed#: " + _seed;
    }

    public void UpdateMoves(int _moves)
    {
        moves.text = "Moves: " + _moves;
    }

    public void SavePrompt(int _seed)
    {
        savePrompt.text = "Board#[" + _seed + "] has been saved"; 
    }

    public void MessagePrompt(string msg)
    {
        savePrompt.text = msg;
    }

    public void UpdateClipboard(string str)
    {
        clipboard.text = str;
    }
}
