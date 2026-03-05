using UnityEngine;

public class ChopMaster : MonoBehaviour
{
    public static ChopMaster Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnHalves(ItemTile fruit, int leftColumn, int rightColumn, float spawnY)
    {
        ItemPool.Instance.ReturnFruit(fruit);

        ItemData data = fruit.Data;
        int halfValue = data.value;

        RectTransform gridRoot = OrchardGrid.Instance.gridRoot;

        if (leftColumn == rightColumn)
        {
            leftColumn = Mathf.Max(leftColumn - 1, 0);
            rightColumn = Mathf.Min(rightColumn + 1, OrchardGrid.Instance.width - 1);
        }

        float leftX = OrchardGrid.Instance.GetCellAnchoredPosition(leftColumn, 0).x;
        float rightX = OrchardGrid.Instance.GetCellAnchoredPosition(rightColumn, 0).x;

        int? leftTargetRow = FindFreeRow(leftColumn);
        int? rightTargetRow = FindFreeRow(rightColumn);

        if (!leftTargetRow.HasValue || !rightTargetRow.HasValue)
        {
            GameCore.GameManager.Instance.GameOver();
            return;
        }

        float halfCellWidth = OrchardGrid.Instance.CellSize / 2f;
        float cellHeight = OrchardGrid.Instance.CellSize;

        // Левая половинка
        ItemHalf leftHalf = ItemPool.Instance.GetHalf();
        leftHalf.Initialize(data.leftHalfSprite, data.fullSprite, halfValue, data);

        RectTransform rtLeft = leftHalf.GetComponent<RectTransform>();
        rtLeft.SetParent(gridRoot, false);
        rtLeft.localScale = Vector3.one;
        rtLeft.localRotation = Quaternion.identity;
        rtLeft.sizeDelta = new Vector2(halfCellWidth, cellHeight);
        rtLeft.anchoredPosition = new Vector2(leftX, spawnY);

        leftHalf.gameObject.SetActive(true);

        leftHalf.StartFall(() =>
        {
            bool result = OrchardGrid.Instance.PlaceHalfFruit(leftHalf);
            if (!result)
            {
                GameCore.GameManager.Instance.GameOver();
            }
        }, leftColumn, leftTargetRow.Value);

        // Правая половинка
        ItemHalf rightHalf = ItemPool.Instance.GetHalf();
        rightHalf.Initialize(data.rightHalfSprite, data.fullSprite, halfValue, data);

        RectTransform rtRight = rightHalf.GetComponent<RectTransform>();
        rtRight.SetParent(gridRoot, false);
        rtRight.localScale = Vector3.one;
        rtRight.localRotation = Quaternion.identity;
        rtRight.sizeDelta = new Vector2(halfCellWidth, cellHeight);
        rtRight.anchoredPosition = new Vector2(rightX, spawnY);

        rightHalf.gameObject.SetActive(true);

        rightHalf.StartFall(() =>
        {
            bool result = OrchardGrid.Instance.PlaceHalfFruit(rightHalf);
            if (!result)
            {
                GameCore.GameManager.Instance.GameOver();
            }
        }, rightColumn, rightTargetRow.Value);
    }

    private int? FindFreeRow(int column)
    {
        // Сначала ищем в основной сетке снизу вверх (0...height-1)
        for (int y = 0; y < OrchardGrid.Instance.height; y++)
        {
            if (OrchardGrid.Instance.GetHalfFruitAt(column, y) == null && OrchardGrid.Instance.GetFruitAt(column, y) == null)
            {
                return y;
            }
        }

        // Если основной сетки нет места, смотрим буферный ряд сверху (height)
        if (OrchardGrid.Instance.GetHalfFruitAt(column, OrchardGrid.Instance.height) == null &&
            OrchardGrid.Instance.GetFruitAt(column, OrchardGrid.Instance.height) == null)
        {
            return OrchardGrid.Instance.height;
        }

        return null;
    }
}