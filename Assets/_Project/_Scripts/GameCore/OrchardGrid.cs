using UnityEngine;

public class OrchardGrid : MonoBehaviour
{
    public static OrchardGrid Instance;

    public int width = 5;
    public int height = 6;

    public float spacingX = 10f;
    public float spacingY = 10f;

    public float CellSize { get; private set; }

    public RectTransform gridRoot;

    private ItemHalf[,] grid;
    private ItemTile[,] fruitGrid;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        CalculateCellSize();

        grid = new ItemHalf[width, height + 1];
        fruitGrid = new ItemTile[width, height + 1];
    }

    void OnRectTransformDimensionsChange()
    {
        CalculateCellSize();
    }

    void CalculateCellSize()
    {
        float gridWidth = gridRoot.rect.width;
        float gridHeight = gridRoot.rect.height;

        float totalSpacingX = spacingX * (width - 1);
        float totalSpacingY = spacingY * (height - 1);

        float cellWidth = (gridWidth - totalSpacingX) / width;
        float cellHeight = (gridHeight - totalSpacingY) / height;

        CellSize = Mathf.Min(cellWidth, cellHeight);
    }

    public void ClearGrid()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height + 1; y++)
            {
                if (grid[x, y] != null)
                {
                    ItemPool.Instance.ReturnHalf(grid[x, y]);
                    grid[x, y] = null;
                }

                if (fruitGrid[x, y] != null)
                {
                    ItemPool.Instance.ReturnFruit(fruitGrid[x, y]);
                    fruitGrid[x, y] = null;
                }
            }
    }

    public bool PlaceHalfFruit(ItemHalf hf)
    {
        RectTransform hfRT = hf.GetComponent<RectTransform>();
        Vector2 localPos = hfRT.anchoredPosition;

        float stepX = CellSize + spacingX;

        float contentWidth = width * CellSize + (width - 1) * spacingX;
        float offsetCenter = (gridRoot.rect.width - contentWidth) * 0.5f;

        float gridWidthPx = gridRoot.rect.width;
        float leftEdgeX = -gridWidthPx * gridRoot.pivot.x + offsetCenter;

        float offsetX = localPos.x - leftEdgeX;

        int x = Mathf.Clamp(Mathf.FloorToInt(offsetX / stepX), 0, width - 1);

        int bufferRow = height;
        ItemTile topFruit = fruitGrid[x, bufferRow];

        if (topFruit != null)
        {
            if (topFruit.Data.value == hf.value)
            {
                TryMergeWithFruit(hf, x, bufferRow);
                return true;
            }
            return false;
        }
        else
        {
            int currentY = height - 1;

            while (currentY > 0 &&
                   grid[x, currentY - 1] == null &&
                   fruitGrid[x, currentY - 1] == null)
            {
                currentY--;
            }

            if (fruitGrid[x, currentY] != null)
            {
                if (fruitGrid[x, currentY].Data.value == hf.value)
                {
                    TryMergeWithFruit(hf, x, currentY);
                    return true;
                }
                return false;
            }

            grid[x, currentY] = hf;

            hfRT.SetParent(gridRoot, false);
            hfRT.sizeDelta = new Vector2(CellSize / 2f, CellSize);

            hf.gameObject.SetActive(true);

            CreateFruitFromHalf(hf, x, currentY);

            return true;
        }
    }

    public bool CollapseColumn(int x)
    {
        bool moved = false;
        int targetY = 0;

        for (int y = 0; y < height; y++)
        {
            var fruit = fruitGrid[x, y];

            if (fruit != null)
            {
                if (y != targetY)
                {
                    fruitGrid[x, targetY] = fruit;
                    fruitGrid[x, y] = null;

                    Vector2 pos = GetCellAnchoredPosition(x, targetY);
                    fruit.GetComponent<RectTransform>().anchoredPosition = pos;

                    moved = true;
                }

                targetY++;
            }
        }

        return moved;
    }

    public ItemHalf GetHalfFruitAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height + 1) return null;
        return grid[x, y];
    }

    public ItemTile GetFruitAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height + 1) return null;
        return fruitGrid[x, y];
    }

    public void SetFruitAt(int x, int y, ItemTile fruit)
    {
        if (x < 0 || y < 0 || x >= width || y >= height + 1) return;
        fruitGrid[x, y] = fruit;
    }

    public Vector2 GetCellAnchoredPosition(int column, int row)
    {
        float gridWidth = gridRoot.rect.width;
        float gridHeight = gridRoot.rect.height;

        float stepX = CellSize + spacingX;
        float stepY = CellSize + spacingY;

        float contentWidth = width * CellSize + (width - 1) * spacingX;
        float contentHeight = height * CellSize + (height - 1) * spacingY;

        float offsetX = (gridWidth - contentWidth) * 0.5f;
        float offsetY = (gridHeight - contentHeight) * 0.5f;

        float leftEdge = -gridWidth * gridRoot.pivot.x + offsetX;
        float bottomEdge = -gridHeight * gridRoot.pivot.y + offsetY;

        float x = leftEdge + CellSize * 0.5f + column * stepX;

        float y;

        if (row == height)
            y = bottomEdge + CellSize * 0.5f + height * stepY;
        else
            y = bottomEdge + CellSize * 0.5f + row * stepY;

        return new Vector2(x, y);
    }

    private void CreateFruitFromHalf(ItemHalf hf, int x, int y)
    {
        Vector2 cellPos = GetCellAnchoredPosition(x, y);

        grid[x, y] = null;

        hf.gameObject.SetActive(false);
        ItemPool.Instance.ReturnHalf(hf);

        ItemData fruitData = hf.originalData;

        if (fruitData == null)
        {
            Debug.LogWarning($"HalfFruit.originalData не установлен. Значение: {hf.value}");
            return;
        }

        ItemTile newFruit = ItemPool.Instance.GetFruit();
        newFruit.Init(fruitData, cellPos);

        newFruit.CanBeSliced = false;

        RectTransform newFruitRT = newFruit.GetComponent<RectTransform>();
        newFruitRT.SetParent(gridRoot, false);
        newFruitRT.sizeDelta = new Vector2(CellSize, CellSize);

        newFruit.gameObject.SetActive(true);

        fruitGrid[x, y] = newFruit;

        FusionDirector.Instance.StartMergeCoroutine();
    }

    private void TryMergeWithFruit(ItemHalf hf, int x, int y)
    {
        int newValue = hf.value * 2;

        ItemData mergedData = ItemPool.Instance.fruitDatas.Find(f => f.value == newValue);

        if (mergedData == null)
        {
            Debug.LogWarning($"FruitData для значения {newValue} не найден.");
            return;
        }

        ItemPool.Instance.ReturnHalf(hf);
        ItemPool.Instance.ReturnFruit(fruitGrid[x, y]);

        fruitGrid[x, y] = null;

        Vector2 pos = GetCellAnchoredPosition(x, y);

        ItemTile newFruit = ItemPool.Instance.GetFruit();
        newFruit.Init(mergedData, pos);

        newFruit.CanBeSliced = false;

        RectTransform newFruitRT = newFruit.GetComponent<RectTransform>();
        newFruitRT.SetParent(gridRoot, false);
        newFruitRT.sizeDelta = new Vector2(CellSize, CellSize);

        newFruit.gameObject.SetActive(true);

        fruitGrid[x, y] = newFruit;

        GameCore.GameManager.Instance.AddScoreNoWinCheck(mergedData.value);

        StartCoroutine(FusionDirector.Instance.PlayMergeAnimationCoroutine(newFruit));

        CollapseColumn(x);

        FusionDirector.Instance.StartMergeCoroutine();
    }
}