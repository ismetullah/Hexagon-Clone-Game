using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Constants;

public struct Position
{
    public Position(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int x, y;

    public override string ToString()
    {
        return string.Format("[Pair: x -> {0}, y -> {1}]", x, y);
    }
}

public class GridManager : MonoBehaviour
{
    #region Public Variables
    public Hex hexPrefab;
    public BombHex bombPrefab;
    public GameObject outlinesObject;
    public GameObject gridObject;

    [Header("Corners")]
    public GameObject topRightCorner;
    public GameObject topLeftCorner;
    public GameObject bottomRightCorner;
    public GameObject bottomLeftCorner;
    #endregion

    #region Private Variables
    private List<List<Hex>> grid;
    private List<BombHex> bombs;

    private List<Position> selectedGroup;

    private List<Color> colors;
    private int gridHeight = DEFAULT_HEIGHT;
    private int gridWidth = DEFAULT_WIDTH;

    private bool isFillingEmptyCells;
    private bool isRotatingHexagons;
    private bool isExplodingHexagons;
    private bool shouldFillBomb;

    private Task rotateTask;

    private GameManager gameManager;
    #endregion

    #region Singleton
    private static GridManager _instance;
    public static GridManager Instance { get { return _instance; } }
    #endregion

    #region Event Functions
    void Awake()
    {

        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        gameManager = GameManager.Instance;
        gameManager.onGameStart += StartGame;
        gameManager.onNewBomb += () => { shouldFillBomb = true; };
    }
    #endregion

    #region Private Methods  
    private void StartGame(int gridHeight, int gridWidth, int colorCount)
    {
        InitVariables(gridHeight, gridWidth, colorCount);
        InitGrid();
    }

    private void InitVariables(int gridHeight = DEFAULT_HEIGHT, int gridWidth = DEFAULT_WIDTH, int colorCount = DEFAULT_COLOR_COUNT)
    {
        if (grid != null)
            ClearOutline();
        this.gridHeight = gridHeight;
        this.gridWidth = gridWidth;

        bombs = new List<BombHex>();
        selectedGroup = new List<Position>();
        colors = Constants.GetColors(colorCount);

        isFillingEmptyCells = false;
        isRotatingHexagons = false;
        isExplodingHexagons = false;
    }

    /**
     * Initializes the grid
     */
    private void InitGrid()
    {
        DestroyGrid();

        grid = new List<List<Hex>>();
        List<int> emptyColumns = new List<int>();

        for (int i = 0; i < gridWidth; ++i)
        {
            emptyColumns.Add(i);

            grid.Add(new List<Hex>());
        }

        PositionCamera();
        StartCoroutine(FillEmpty(emptyColumns, GenerateValidColorGrid()));
    }

    /**
     * Destroys grid if any elements are available.
     */
    private void DestroyGrid()
    {
        if (grid == null) return;

        for (int x = 0; x < grid.Count; x++)
        {
            for (int y = 0; y < grid[x].Count; y++)
            {
                Destroy(grid[x][y].gameObject);
            }
        }
    }

    /**
     * Generates a valid 2 dimensional List of Color.
     */
    private List<List<Color>> GenerateValidColorGrid()
    {
        List<List<Color>> colorGrid = new List<List<Color>>();
        bool shouldPickAnother = false;

        // Generate color grid without neighbors which might cause explosion
        for (int x = 0; x < gridWidth; x++)
        {
            colorGrid.Add(new List<Color>());
            for (int y = 0; y < gridHeight; y++)
            {
                colorGrid[x].Add(this.colors.PickRandom());
                do
                {
                    shouldPickAnother = false;
                    colorGrid[x][y] = this.colors.PickRandom();
                    if (x % 2 == 0)
                    {
                        if ((x - 1 >= 0 && y + 1 < gridHeight) && (colorGrid[x - 1][y] == colorGrid[x][y] && colorGrid[x - 1][y + 1] == colorGrid[x][y]))
                            shouldPickAnother = true;
                        if ((x - 1 >= 0 && y - 1 >= 0) && (colorGrid[x - 1][y] == colorGrid[x][y] && colorGrid[x][y - 1] == colorGrid[x][y]))
                            shouldPickAnother = true;
                    }
                    else
                    {
                        if (x - 1 >= 0 && y - 1 >= 0)
                        {
                            if (colorGrid[x - 1][y] == colorGrid[x][y] && colorGrid[x - 1][y - 1] == colorGrid[x][y])
                                shouldPickAnother = true;
                            if (colorGrid[x][y - 1] == colorGrid[x][y] && colorGrid[x - 1][y - 1] == colorGrid[x][y])
                                shouldPickAnother = true;
                        }
                    }
                } while (shouldPickAnother);
            }
        }

        return colorGrid;
    }

    /**
     * Get all four corners of the grid then
     * position the camera according to those corner coordinates
     */
    private void PositionCamera()
    {
        topRightCorner.transform.position = GetWorldPositionForCell(gridWidth - 1, gridHeight - 1);
        topLeftCorner.transform.position = GetWorldPositionForCell(0, gridHeight - 1);
        bottomRightCorner.transform.position = GetWorldPositionForCell(gridWidth - 1, 0);
        bottomLeftCorner.transform.position = GetWorldPositionForCell(0, 0);
        CameraController.Instance.PositionCamera(this.transform.Find("Corners").gameObject);
    }

    /**
     * Generate a world position for a cell
     */
    private Vector3 GetWorldPositionForCell(int x, int y)
    {
        float xPos = x * X_OFFSET;
        float yPos = y * Y_OFFSET;

        if (x % 2 == 0)
        {
            yPos += Y_OFFSET / 2f;
        }

        return new Vector3(xPos, yPos);
    }

    /**
     * Generate a start position for cells of a column
     */
    private Vector3 GetStartPositionForColumn(int x)
    {
        float xPos = x * X_OFFSET;
        float yPos = gridHeight * Y_OFFSET + DISTANCE_FROM_COLUMN_TOP;
        return new Vector3(xPos, yPos);
    }

    /**
     * This is to fill the empty cells from initialization or after explosions
     */
    private IEnumerator FillEmpty(List<int> emptyColumns, List<List<Color>> colorGrid = null)
    {
        isFillingEmptyCells = true;
        Vector2 startPos;
        foreach (int x in emptyColumns)
        {
            int currentColumnHeight = grid[x].Count;
            for (int y = currentColumnHeight; y < gridHeight; y++)
            {
                startPos = GetWorldPositionForCell(x, y);
                Hex hex;
                if (shouldFillBomb)
                {
                    // If bomb, instantiate a bomb prefab.
                    hex = Instantiate(bombPrefab, GetStartPositionForColumn(x), Quaternion.identity, gridObject.transform);
                    bombs.Add((BombHex)hex);
                    shouldFillBomb = false;
                }
                else
                {
                    hex = Instantiate(hexPrefab, GetStartPositionForColumn(x), Quaternion.identity, gridObject.transform);
                }

                yield return new WaitForSeconds(HEX_FILL_DELAY);

                // Name the gameobject something sensible.
                hex.name = "Hex_" + x + "_" + y;

                // Set properties to Hex
                hex.SetX(x);
                hex.SetY(y);
                if (colorGrid != null)
                {
                    hex.SetColor(colorGrid[x][y]);
                }
                else
                {
                    hex.SetColor(colors.PickRandom());
                }

                // Move Hex to start position
                hex.MoveTo(startPos);

                grid[x].Add(hex);
            }
        }
        isFillingEmptyCells = false;
    }

    /**
     * Clears outlines
     */
    private void ClearOutline()
    {
        foreach (List<Hex> column in grid)
        {
            foreach (Hex hex in column)
            {
                hex.HideOutline();
            }
        }
    }

    /**
     * Draws outline around the selected hexagons
     */
    private void DrawOutline()
    {
        /**
         * Get group of hexagons which should have outlines
         * Then for each hexagon, draw the outline.
         */
        foreach (Position p in selectedGroup)
        {
            Hex hex = grid[p.x][p.y];
            hex.ShowOutline();
            hex.transform.parent = outlinesObject.transform;
        }
    }

    /**
     * Generates a list of hex which should be outlined.
     */
    private List<Position> GenerateSelectedGroup(Collider2D collider, Vector3 tapPosition)
    {
        selectedGroup.Clear();
        Hex selectedHex = collider.gameObject.GetComponent<Hex>();
        Neighbor n1, n2;
        GetNextNeighborsForSelectedGroup(selectedHex, tapPosition, out n1, out n2);
        selectedGroup.Add(new Position(selectedHex.GetX(), selectedHex.GetY()));
        selectedGroup.Add(new Position(n1.x, n1.y));
        selectedGroup.Add(new Position(n2.x, n2.y));

        return selectedGroup;
    }

    /**
     * Gets the next neighbors of the selected hexagon to be outlined.
     */
    private void GetNextNeighborsForSelectedGroup(Hex selectedHex, Vector2 tapPosition, out Neighbor n1, out Neighbor n2)
    {
        Neighbors neighbors = GenerateNeighborsForHex(selectedHex.GetX(), selectedHex.GetY());
        n1 = new Neighbor();
        n2 = new Neighbor();

        float minDistanceSum = Int32.MaxValue;
        float curDistanceSum;
        FieldInfo[] fields = typeof(Neighbors).GetFields();
        for (int i = 0; i < fields.Length; i++)
        {
            curDistanceSum = 0;
            Neighbor? temp1 = (Neighbor?)fields[i].GetValue(neighbors);
            // if i is the last index, temp2 is the first neighbor.
            Neighbor? temp2 = (Neighbor?)fields[(i + 1 < fields.Length) ? i + 1 : 0].GetValue(neighbors);
            // If any of these neighbors doesnt have value, we continue for the next neighbors.
            // If both of them have value, it means they are also neighbors.
            if (!temp1.HasValue || !temp2.HasValue) continue;

            curDistanceSum += Vector2.Distance(tapPosition, grid[temp1.Value.x][temp1.Value.y].GetPosition());
            curDistanceSum += Vector2.Distance(tapPosition, grid[temp2.Value.x][temp2.Value.y].GetPosition());
            if (curDistanceSum < minDistanceSum)
            {
                minDistanceSum = curDistanceSum;
                n1 = temp1.Value;
                n2 = temp2.Value;
            }
        }
    }

    /**
     * Generates possible neightbors for a hex.
     * If the index is out of bounds, then neighbor's value is null.
     */
    private Neighbors GenerateNeighborsForHex(int x, int y)
    {
        Neighbors neighbors = new Neighbors();

        if (y + 1 < gridHeight)
            neighbors.up = new Neighbor(x, y + 1);
        if (y - 1 >= 0)
            neighbors.down = new Neighbor(x, y - 1);

        if (x % 2 == 0)
        {
            if (x - 1 >= 0 && y + 1 < gridHeight)
                neighbors.upLeft = new Neighbor(x - 1, y + 1);
            if (x + 1 < gridWidth && y + 1 < gridHeight)
                neighbors.upRight = new Neighbor(x + 1, y + 1);
            if (x - 1 >= 0)
                neighbors.downLeft = new Neighbor(x - 1, y);
            if (x + 1 < gridWidth)
                neighbors.downRight = new Neighbor(x + 1, y);
        }
        else
        {
            if (x - 1 >= 0)
                neighbors.upLeft = new Neighbor(x - 1, y);
            if (x + 1 < gridWidth)
                neighbors.upRight = new Neighbor(x + 1, y);
            if (x - 1 >= 0 && y - 1 >= 0)
                neighbors.downLeft = new Neighbor(x - 1, y - 1);
            if (x + 1 < gridWidth && y - 1 >= 0)
                neighbors.downRight = new Neighbor(x + 1, y - 1);
        }

        return neighbors;
    }

    /**
     * Coroutine to rotate selected group.
     * Keeps rotating until an explosion or fully cicled.
     */
    private IEnumerator IRotateSelected(bool clockWise)
    {
        CountDownBombs();

        HashSet<Position> explosiveHexagons = new HashSet<Position>();

        // Start rotating
        isRotatingHexagons = true;
        for (int i = 0; i < selectedGroup.Count; ++i)
        {
            CycleSwapSelected(clockWise);
            yield return new WaitForSeconds(DEFAULT_HEX_MOVE_TIME + 0.01f);

            // Break loop if there are to be expoded hexes.
            explosiveHexagons = GetToBeExpodedHexes(gridToColorGrid(grid));
            if (explosiveHexagons.Count > 0)
            {
                break;
            }
        }
        isRotatingHexagons = false;

        // Clear outline before explosion
        ClearOutline();

        /* Explode the hexagons until no explosive hexagons are available */
        bool flag = true;
        isExplodingHexagons = true;
        while (explosiveHexagons.Any())
        {
            if (flag)
            {
                StartCoroutine(FillEmpty(ExplodeHexagons(ToHexSet(explosiveHexagons, grid))));
                flag = false;
            }
            else if (!isFillingEmptyCells)
            {
                explosiveHexagons = GetToBeExpodedHexes(gridToColorGrid(grid));
                flag = true;
            }

            yield return new WaitForSeconds(0.3f);
        }
        isExplodingHexagons = false;

        DrawOutline();
        CheckGameEnd();
    }

    private HashSet<Hex> ToHexSet(HashSet<Position> pairSet, List<List<Hex>> grid)
    {
        HashSet<Hex> result = new HashSet<Hex>();
        foreach (Position pair in pairSet)
        {
            result.Add(grid[pair.x][pair.y]);
        }
        return result;
    }

    /**
     * Swap selected elements in cycle according to the given direction.
     */
    private void CycleSwapSelected(bool clockwise)
    {
        // We dont have to swap elements if there are none.
        if (selectedGroup == null || selectedGroup.Count == 0) return;

        Hex selected1 = grid[selectedGroup[0].x][selectedGroup[0].y];
        Hex selected2 = grid[selectedGroup[1].x][selectedGroup[1].y];
        Hex selected3 = grid[selectedGroup[2].x][selectedGroup[2].y];

        HexObject temp = new HexObject(selected1);
        if (clockwise)
        {
            SwapHexes(new HexObject(selected1), new HexObject(selected2));
            SwapHexes(new HexObject(selected2), new HexObject(selected3));
            SwapHexes(new HexObject(selected3), temp);
        }
        else
        {
            SwapHexes(new HexObject(selected1), new HexObject(selected3));
            SwapHexes(new HexObject(selected3), new HexObject(selected2));
            SwapHexes(new HexObject(selected2), temp);
        }
    }

    /**
     * Swap two hexes.
     */
    private void SwapHexes(HexObject h1, HexObject h2)
    {
        grid[h2.x][h2.y] = h1.hex;
        h1.hex.CopyWithAnimation(h2);
    }

    /**
     * Finds a set of hexes which are to be exploded.
     */
    private HashSet<Position> GetToBeExpodedHexes(List<List<Color>> colorGrid)
    {
        HashSet<Position> toBeExplodedSet = new HashSet<Position>();
        Neighbors neighbors;
        Position curPos;
        for (int x = 0; x < colorGrid.Count; ++x)
        {
            for (int y = 0; y < colorGrid[x].Count; ++y)
            {
                neighbors = GenerateNeighborsForHex(x, y);
                curPos = new Position(x, y);
                // Check up and upRight neighbors
                AddNeighborToExploreIfMatch(colorGrid, toBeExplodedSet, colorGrid[x][y], curPos, neighbors.up, neighbors.upRight);
                // Check upRight and downRight neighbors
                AddNeighborToExploreIfMatch(colorGrid, toBeExplodedSet, colorGrid[x][y], curPos, neighbors.upRight, neighbors.downRight);
                // Check downRight and down neighbors
                AddNeighborToExploreIfMatch(colorGrid, toBeExplodedSet, colorGrid[x][y], curPos, neighbors.downRight, neighbors.down);
            }
        }
        return toBeExplodedSet;
    }

    /**
     * Checks colors of current hex and its given neighbors.
     * If the colors match, they are added to set of hex to be exploded.
     */
    private void AddNeighborToExploreIfMatch(
        List<List<Color>> colorGrid,
        HashSet<Position> toBeExplodedSet,
        Color curColor,
        Position curPos,
        Neighbor? n1,
        Neighbor? n2
        )
    {
        if (n1.HasValue && n2.HasValue)
        {
            Color color1 = colorGrid[n1.Value.x][n1.Value.y];
            Color color2 = colorGrid[n2.Value.x][n2.Value.y];
            if (color1 == curColor && color2 == curColor)
            {
                toBeExplodedSet.Add(curPos);
                toBeExplodedSet.Add(new Position(n1.Value.x, n1.Value.y));
                toBeExplodedSet.Add(new Position(n2.Value.x, n2.Value.y));
            }
        }
    }

    /**
     * Counts down all the bombs in the grid.
     * If the timer of any of them hits 0, the game ends.
     */
    private void CountDownBombs()
    {
        foreach (BombHex bomb in bombs)
        {
            if (bomb.CountDown() == 0)
            {
                GameManager.Instance.EndGame();
            }
        }
    }

    /**
     * Explodes Hex from the grid
     */
    private List<int> ExplodeHexagons(HashSet<Hex> toBeExplodedSet)
    {
        isExplodingHexagons = true;
        HashSet<int> missingColumns = new HashSet<int>();

        // Remove hexes

        foreach (Hex hex in toBeExplodedSet)
        {
            if (hex is BombHex && bombs.Contains((BombHex)hex))
                bombs.Remove((BombHex)hex);
            grid[hex.GetX()].Remove(hex);
            missingColumns.Add(hex.GetX());
            Destroy(hex.gameObject);
        }
        GameManager.Instance.AddScore(toBeExplodedSet.Count);

        // Move down hexes to empty cells
        foreach (int x in missingColumns)
        {
            for (int y = 0; y < grid[x].Count; ++y)
            {
                if (grid[x][y] != null)
                {
                    grid[x][y].SetY(y);
                    grid[x][y].SetX(x);
                    grid[x][y].MoveTo(GetWorldPositionForCell(x, y));
                }
            }
        }

        isExplodingHexagons = false;
        return missingColumns.ToList();
    }

    private List<List<Color>> gridToColorGrid(List<List<Hex>> grid)
    {
        List<List<Color>> newGrid = new List<List<Color>>();
        for (int x = 0; x < grid.Count; x++)
        {
            newGrid.Add(new List<Color>());
            foreach (Hex hex in grid[x])
            {
                newGrid[x].Add(hex.GetColor());
            }
        }
        return newGrid;
    }

    /**
     * Check if there is any group of hexes which can explode when spinned.
     * If there is one, then the game continues.
     * If there is none, the game ends.
     */
    private void CheckGameEnd()
    {
        if (grid == null || grid.Count == 0) return;

        List<List<Color>> newGrid = gridToColorGrid(grid);

        for (int x = 0; x < newGrid.Count - 1; ++x)
        {
            for (int y = 0; y < newGrid[x].Count; ++y)
            {
                Hex currentHex = grid[x][y];
                Neighbors currentNeighbors = GenerateNeighborsForHex(x, y);

                Neighbor? n1 = null, n2 = null;
                if (currentNeighbors.upRight.HasValue && currentNeighbors.downRight.HasValue)
                {
                    n1 = currentNeighbors.upRight.Value;
                    n2 = currentNeighbors.downRight.Value;
                }
                else if (x % 2 == 0)
                {
                    if (currentNeighbors.downRight.HasValue && currentNeighbors.down.HasValue)
                    {
                        n1 = currentNeighbors.downRight.Value;
                        n2 = currentNeighbors.down.Value;
                    }
                }
                else
                {
                    if (currentNeighbors.up.HasValue && currentNeighbors.upRight.HasValue)
                    {
                        n1 = currentNeighbors.up.Value;
                        n2 = currentNeighbors.upRight.Value;
                    }
                }

                if (!n1.HasValue || !n2.HasValue) continue;

                // A for loop for swapping and checking for explosions
                for (int i = 0; i < 3; i++)
                {
                    Color temp = newGrid[currentHex.GetX()][currentHex.GetY()];
                    newGrid[currentHex.GetX()][currentHex.GetY()] = newGrid[n1.Value.x][n1.Value.y];
                    newGrid[n1.Value.x][n1.Value.y] = newGrid[n2.Value.x][n2.Value.y];
                    newGrid[n2.Value.x][n2.Value.y] = temp;

                    // If it is the third swap, dont check for explosion
                    if (i != 2)
                    {
                        HashSet<Position> toBeExplodedHexes = GetToBeExpodedHexes(newGrid);
                        // If explosion is possible, then return from the method.
                        if (toBeExplodedHexes.Count > 0)
                        {
                            return;
                        }
                    }
                }
            }
        }

        // No explosions happened, end game.
        GameManager.Instance.EndGame();
    }

    private Hex GetHex(Neighbor n) => grid[n.x][n.y];
    #endregion

    #region Public Methods
    /**
     * Rotates the selected group of hexes on the specified direction.
     */
    public void RotateSelected(bool clockwise)
    {
        if (rotateTask == null || !rotateTask.Running)
        {
            rotateTask = new Task(IRotateSelected(clockwise));
        }
    }

    /**
     * Selects group of hexagons
     */
    public void SelectGroup(Collider2D collider, Vector3 position)
    {
        ClearOutline();
        GenerateSelectedGroup(collider, position);
        DrawOutline();
    }

    /**
     * Determines if user can interact with grid.
     */
    public bool CanInteract() => !isFillingEmptyCells && !isExplodingHexagons && !isRotatingHexagons;

    public void SetColors(List<Color> colors) => this.colors = colors;

    public Hex GetSelectedHex()
    {
        if (selectedGroup.Count == 0) return null;
        return grid[selectedGroup[0].x][selectedGroup[0].y];
    }
    #endregion
}