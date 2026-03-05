using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemHalf : MonoBehaviour
{
    public int value;
    public ItemData originalData;

    private Image image;

    private float fallSpeed = 1000f;
    private bool isFalling;
    private Action onLand;

    private RectTransform rectTransform;
    private int targetColumn;
    public int targetRow;

    public void Initialize(Sprite halfSprite, Sprite fullSprite, int value, ItemData originalData)
    {
        image = GetComponent<Image>();
        image.sprite = halfSprite;
        this.value = value;
        this.originalData = originalData;
    }

    public void StartFall(Action onLandCallback, int targetColumn, int targetRow)
    {
        onLand = onLandCallback;
        isFalling = true;
        this.targetColumn = targetColumn;
        this.targetRow = targetRow;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    private void Update()
    {
        if (!isFalling) return;

        float step = fallSpeed * Time.deltaTime;

        Vector2 targetPos = OrchardGrid.Instance.GetCellAnchoredPosition(targetColumn, targetRow);

        // Плавно опускаем позицию вниз, не ниже целевой позиции
        if (rectTransform.anchoredPosition.y - step <= targetPos.y)
        {
            rectTransform.anchoredPosition = targetPos;
            isFalling = false;
            onLand?.Invoke();
        }
        else
        {
            rectTransform.anchoredPosition += Vector2.down * step;
        }
    }
}