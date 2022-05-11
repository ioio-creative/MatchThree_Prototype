using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardController : MonoBehaviour
{
    public delegate void OnCoordSelectCallback(bool isSwap, Vector2Int coord);
    public static OnCoordSelectCallback OnCoordSelectAck;

    [Header("Setup Params")]
    [SerializeField] private int boardSeed;
    [SerializeField] [Min(1)] private int width;
    [SerializeField] [Min(1)] private int height;
    [SerializeField] private Vector2 worldUnitPerTile;

    [SerializeField][Min(0)] private float swapTimeout;
    [SerializeField][Min(0)] private float removeTimeout;
    private bool isTimeout = false;
    
    [Header("member references")]
    [SerializeField] private SpriteRenderer boardSprite;
    [SerializeField] private Transform tileContainerParent;
    [SerializeField] private BlockPool population;
    [SerializeField] private GameObject blockPrefab;

    [Header("UI")]
    [SerializeField] private InfoView infoUI;
    [SerializeField] private InputField seedInput;

    public Tile[,] Tiles { get; private set; }

    private Tile? highlightedTile = null;

    private List<TestResult> boardTestResults = new List<TestResult>();
    private int boardMoves = 0;

    private void Awake()
    {
        boardSprite.size = new Vector2(width, height);

        BoardUtils.WorldToCoordScale = worldUnitPerTile;
        BoardUtils.BlockZDepth = tileContainerParent.position.z;

        InputController.OnTileClicked += OnCursorSelectedHandler;
    }


    private void Start()
    {
        //boardSeed = UnityEngine.Random.Range(0, 999);
        InitializeBoard(width, height, boardSeed);
    }

    private void InitializeBoard(int width, int height, int seed)
    {
        ClearTiles();

        System.Random boardRNG = new System.Random(seed);
        
        Tiles = new Tile[height, width];

        for (int y = 0; y < Tiles.GetLength(0); y++)
        {
            for (int x = 0; x < Tiles.GetLength(1); x++)
            {
                Tiles[y, x] = new Tile(x, y);
                BlockType newRdmType;
                bool redraw;

                do
                {
                    newRdmType = population.Pool[boardRNG.Next(0, population.Pool.Length)];
                    redraw = false;
                    //check vertical match starting from y = 2
                    if (y >= 2)
                    {
                        redraw = newRdmType == Tiles[y - 1, x].OccupantType &&
                            newRdmType == Tiles[y - 2, x].OccupantType;
                    }
                    if (redraw) continue;

                    //check horizontal match starting from x = 2
                    if (x >= 2)
                    {
                        redraw = newRdmType == Tiles[y, x - 1].OccupantType &&
                            newRdmType == Tiles[y, x - 2].OccupantType;
                    }
                }                
                while (redraw);
                PopulateTile(newRdmType, ref Tiles[y, x]);
            }
        }
        boardMoves = 0;
        UpdateSeedUI();
        UpdateMoveUI();
    }

    //Tile is struct, so pass by reference
    private void PopulateTile(BlockType block, ref Tile tile)
    {
        var newblockView = Instantiate(blockPrefab, tileContainerParent).GetComponent<BlockView>();
        newblockView?.InitializeBlockView(block, tile.Coordinates);

        tile.Occupant = newblockView;
    }

    private void ClearTiles()
    {
        if (Tiles == null) return;
        
        for (int y = 0; y < Tiles.GetLength(0); y++)
        {
            for (int x = 0; x < Tiles.GetLength(1); x++)
            {
                var block = Tiles[y, x].Occupant;
                Tiles[y, x].Occupant = null;
                if (block) Destroy(block.gameObject);
            }
        }
    }


    private bool AreCoordsAdjacent(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1 ? true : false;
    }

    private IEnumerator TakeAMove(Vector2Int a, Vector2Int b)
    {
        SwapTiles(a, b);

        var matchCoords = Tiles.FindMatches(3);

        if (matchCoords.Count > 0)
        {
            yield return StartCoroutine(WaitForTimeoutAndExecute(swapTimeout, () => RemoveMatches(matchCoords)));
            yield return StartCoroutine(WaitForTimeoutAndExecute(removeTimeout + 0.1f, () => ShiftDownTiles()));
            yield return StartCoroutine(WaitForTimeoutAndExecute(0.1f, () => ShiftLeftTiles()));
            bool chainReaction = false;
            do
            {
                var chainMatches = Tiles.FindMatches(3);
                chainReaction = chainMatches.Count > 0;

                if (chainReaction)
                {
                    yield return StartCoroutine(WaitForTimeoutAndExecute(swapTimeout, () => RemoveMatches(chainMatches)));
                    yield return StartCoroutine(WaitForTimeoutAndExecute(removeTimeout + 0.1f, () => ShiftDownTiles()));
                    yield return StartCoroutine(WaitForTimeoutAndExecute(0.1f, () => ShiftLeftTiles()));
                }
            }
            while (chainReaction);

        }
        else if (matchCoords.Count == 0)
        {
            //swap again to revers if no match is found
            //StartCoroutine(WaitForTimeoutAndExecute(swapTimeout + 0.1f, () => SwapTiles(a, b)));
        }

        boardMoves++;
        UpdateMoveUI();
    }
    
    private void SwapTiles(Vector2Int a, Vector2Int b)
    {
        var blockTemp = Tiles[a.y, a.x].Occupant;
        
        Tiles[a.y, a.x].Occupant = Tiles[b.y, b.x].Occupant;
        Tiles[b.y, b.x].Occupant = blockTemp;

        Tiles[a.y, a.x].Occupant.UpdateCoordinates
            (Tiles[a.y, a.x].Coordinates, swapTimeout);
        Tiles[b.y, b.x].Occupant.UpdateCoordinates
            (Tiles[b.y, b.x].Coordinates, swapTimeout);     
    }

    private void RemoveMatches(ICollection<Vector2Int> matches)
    {
        foreach (var match in matches)
        {
            var block = Tiles[match.y, match.x].Occupant;
            Tiles[match.y, match.x].Occupant = null;

            block.RemoveBlock(removeTimeout);
        }
    }

    private void ShiftDownTiles()
    {      
        //iterate every column
        for (int x = 0; x < Tiles.GetLength(1); x++)
        {
            //from bottom to top, top most tile can be skipped
            int y = 0;
            int spaceY = 0;
            bool space = false;
            while (y < Tiles.GetLength(0))
            {
                if (Tiles[y, x].OccupantType == null)
                {
                    if (!space)
                    {
                        spaceY = y;
                        space = true;
                    }
                }
                else if (space)
                {
                    //if occupant encountered, shift down the spaceY
                    var block = Tiles[y, x].Occupant;
                    Tiles[y, x].Occupant = null;
                    Tiles[spaceY, x].Occupant = block;

                    block.UpdateCoordinates(Tiles[spaceY, x].Coordinates, 0);

                    //backtrack y iterator to spaceY
                    y = spaceY;
                    space = false;
                    //spaceY = 0;
                }

                y++;
            }
        }
    }

    private void ShiftLeftTiles()
    {
        int maxEmptyX = Tiles.GetLength(1);
        //check left-most empty column from the right until the first block found
        bool blockFound = false;
        for (int x = Tiles.GetUpperBound(1); x >= 0; x--)
        {           
            for (int y = 0; y < Tiles.GetLength(0); y++)
            {
                if (Tiles[y, x].Occupant != null)
                {
                    blockFound = true;
                    maxEmptyX = Mathf.Min(x + 1, maxEmptyX);
                }
                if (blockFound) break;
            }
            if (blockFound) break;
        }

        
        //find empty column from left
        for (int x = 0 ; x < maxEmptyX; x++)
        {
            while (x < maxEmptyX && Tiles[0, x].Occupant == null)
            {
                //check if complete column is empty
                bool spaceColumn = true;
                for (int y = 1; y < Tiles.GetLength(0); y++)
                {
                    if (Tiles[y, x].Occupant != null)
                    {
                        spaceColumn = false;
                        break;
                    }
                }

                //shift all tiles right to the empty column left 1 space
                if (!spaceColumn) break;
                else
                {
                    for (int shiftX = x; shiftX + 1 < maxEmptyX; shiftX++)
                    {
                        for (int y = 0; y < Tiles.GetLength(0); y++)
                        {
                            Tiles[y, shiftX].Occupant = Tiles[y, shiftX + 1].Occupant;
                            Tiles[y, shiftX + 1].Occupant = null;

                            Tiles[y, shiftX].Occupant?.UpdateCoordinates(Tiles[y, shiftX].Coordinates);
                        }
                    }

                    //must decrement maxEmptyX to avoid infinite loop
                    maxEmptyX--;
                }            
            }
        }
    }

    private IEnumerator Timeout(float duration)
    {
        isTimeout = true;
        yield return new WaitForSeconds(duration);
        isTimeout = false;
        yield return null;
    }

    private IEnumerator WaitForTimeoutAndExecute(float duration, Action del)
    {
        yield return new WaitUntil(() => isTimeout == false);
        isTimeout = true;
        yield return new WaitForSeconds(duration);
        del();
        isTimeout = false;
        yield return null;
    }
    
    private void OnCursorSelectedHandler(Vector2Int coord)
    {
        if (isTimeout) return;

        bool isSwap = false;

        if (highlightedTile == null)
        {
            highlightedTile = Tiles[coord.y, coord.x];
        }
        else if (highlightedTile.Value.Coordinates == coord)
        {
            highlightedTile = null;
        }
        else if (AreCoordsAdjacent(highlightedTile.Value.Coordinates, coord))
        {
            StartCoroutine(TakeAMove(highlightedTile.Value.Coordinates, coord));
            highlightedTile = null;
            isSwap = true;
        }
        else
        {
            highlightedTile = Tiles[coord.y, coord.x];
        }
        
        OnCoordSelectAck(isSwap, coord);
    }
    
    #region Saving
    
    private void UpdateSeedUI()
    {
        infoUI.UpdateBoard(boardSeed);
    }
    private void UpdateMoveUI()
    {
        infoUI.UpdateMoves(boardMoves);
    }

    public void SaveButtonHanlder()
    {
        int blocksLeft = 0;
        for (int y = 0; y < Tiles.GetLength(0); y++)
        {
            for (int x = 0; x < Tiles.GetLength(1); x++)
            {
                if (Tiles[y, x].Occupant != null) blocksLeft++;
            }
        }
        
        boardTestResults.Add(new TestResult
        {
            seed = boardSeed,
            types = population.Pool.Length,
            moves = boardMoves,
            tilesLeft = blocksLeft
        });

        infoUI.SavePrompt(boardSeed);

        boardSeed = UnityEngine.Random.Range(0, 999);
        InitializeBoard(width, height, boardSeed);
    }

    public void ResetButtonHandler()
    {
        InitializeBoard(width, height, boardSeed);
    }

    public void NewBoardButtonHandler()
    {
        boardSeed = UnityEngine.Random.Range(0, 999);
        InitializeBoard(width, height, boardSeed);
    }

    public void ExportButtonHanlder()
    {
        //boardSeed.ToString().CopyToClipboard();
        var resultJSON = new BoardTestJSON();

        resultJSON.results = boardTestResults.ToArray();
        resultJSON.SortBySeed();
        infoUI.UpdateClipboard(resultJSON.SerializeJson());
    }

    public void LoadButtonHandler()
    {
        int.TryParse(seedInput.text, out boardSeed);
        InitializeBoard(width, height, boardSeed);
    }
    #endregion

}
