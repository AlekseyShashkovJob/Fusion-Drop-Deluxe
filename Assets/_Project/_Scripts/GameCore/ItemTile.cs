using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemTile : MonoBehaviour, IPointerClickHandler
{
    public ItemData Data { get; private set; }
    public bool HasMergedThisTurn { get; set; }
    public bool CanBeSliced { get; set; } = true;

    private RectTransform rectTransform;
    private Image image;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    public void Init(ItemData fruitData, Vector2 position, Vector2 unusedEnd = default)
    {
        Data = fruitData;
        image.sprite = Data.fullSprite;
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(OrchardGrid.Instance.CellSize, OrchardGrid.Instance.CellSize);
        gameObject.SetActive(true);
    }

    public void SetPosition(Vector2 pos) => rectTransform.anchoredPosition = pos;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CanBeSliced)
            Slice();
    }

    public void Slice()
    {
		Misc.Services.VibroManager.Vibrate();
		
        Vector3 worldPos = rectTransform.position;
        Vector2 local = OrchardGrid.Instance.gridRoot.InverseTransformPoint(worldPos);

        float cellSize = OrchardGrid.Instance.CellSize;
        int width = OrchardGrid.Instance.width;
        float gridWidthPx = OrchardGrid.Instance.gridRoot.rect.width;
        float leftEdgeX = -gridWidthPx * OrchardGrid.Instance.gridRoot.pivot.x;
        float offsetX = local.x - leftEdgeX;
        float preciseColumn = offsetX / cellSize;

        int leftCol = Mathf.FloorToInt(preciseColumn);
        int rightCol = Mathf.Clamp(leftCol + 1, 0, width - 1);
        leftCol = Mathf.Clamp(leftCol, 0, width - 1);

        float spawnY = local.y - 20f;
        ChopMaster.Instance.SpawnHalves(this, leftCol, rightCol, spawnY);
        gameObject.SetActive(false);
    }
}