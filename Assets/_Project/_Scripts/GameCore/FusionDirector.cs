using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FusionDirector : MonoBehaviour
{
    public static FusionDirector Instance;

    public Sprite[] mergeAnimationSprites;
    public float animationFrameDuration = 0.1f;
    private const int MaxConcurrentAnimations = 2;

    private Coroutine mergeCoroutine;

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

    private void OnDisable()
    {
        if (mergeCoroutine != null)
        {
            StopCoroutine(mergeCoroutine);
            mergeCoroutine = null;
        }
    }

    public IEnumerator TryMergeAllColumnsCoroutine()
    {
        bool mergedAnyOverall;

        do
        {
            mergedAnyOverall = false;
            ResetMergedFlags();

            List<IEnumerator> animationEnumerators = new List<IEnumerator>();

            for (int x = 0; x < OrchardGrid.Instance.width; x++)
            {
                for (int y = OrchardGrid.Instance.height; y > 0; y--)
                {
                    var upper = OrchardGrid.Instance.GetFruitAt(x, y);
                    var lower = OrchardGrid.Instance.GetFruitAt(x, y - 1);

                    if (upper == null || lower == null || upper.HasMergedThisTurn || lower.HasMergedThisTurn)
                        continue;

                    if (upper.Data.value != lower.Data.value)
                        continue;

                    int newValue = upper.Data.value * 2;
                    var mergedData = ItemPool.Instance.fruitDatas.Find(f => f.value == newValue);
                    if (mergedData == null)
                    {
                        Debug.LogWarning($"FruitData для значения {newValue} не найден.");
                        continue;
                    }

                    OrchardGrid.Instance.SetFruitAt(x, y, null);
                    OrchardGrid.Instance.SetFruitAt(x, y - 1, null);

                    ItemPool.Instance.ReturnFruit(upper);
                    ItemPool.Instance.ReturnFruit(lower);

                    Vector2 pos = OrchardGrid.Instance.GetCellAnchoredPosition(x, y - 1);
                    ItemTile newFruit = ItemPool.Instance.GetFruit();
                    newFruit.Init(mergedData, pos);
                    newFruit.HasMergedThisTurn = true;
                    newFruit.CanBeSliced = false; // <--- FIX: always set non-clickable after merging!

                    RectTransform newFruitRT = newFruit.GetComponent<RectTransform>();
                    newFruitRT.SetParent(OrchardGrid.Instance.gridRoot, false);
                    newFruitRT.sizeDelta = new Vector2(OrchardGrid.Instance.CellSize, OrchardGrid.Instance.CellSize);
                    newFruit.gameObject.SetActive(true);

                    OrchardGrid.Instance.SetFruitAt(x, y - 1, newFruit);

                    GameCore.GameManager.Instance.AddScoreNoWinCheck(mergedData.value);

                    animationEnumerators.Add(PlayMergeAnimationCoroutine(newFruit));
                    mergedAnyOverall = true;
                }
            }

            yield return RunAnimationsWithLimit(animationEnumerators, MaxConcurrentAnimations);

            bool anyMoved = false;
            for (int x = 0; x < OrchardGrid.Instance.width; x++)
            {
                bool moved = OrchardGrid.Instance.CollapseColumn(x);
                if (moved)
                    anyMoved = true;
            }

            if (anyMoved)
                mergedAnyOverall = true;

        } while (mergedAnyOverall);

        GameCore.GameManager.Instance.CheckWinCondition();
    }

    public void StartMergeCoroutine()
    {
        if (mergeCoroutine != null)
        {
            StopCoroutine(mergeCoroutine);
        }
        mergeCoroutine = StartCoroutine(TryMergeAllColumnsCoroutine());
    }

    private void ResetMergedFlags()
    {
        for (int x = 0; x < OrchardGrid.Instance.width; x++)
            for (int y = 0; y <= OrchardGrid.Instance.height; y++)
            {
                var f = OrchardGrid.Instance.GetFruitAt(x, y);
                if (f != null)
                    f.HasMergedThisTurn = false;
            }
    }

    public IEnumerator PlayMergeAnimationCoroutine(ItemTile fruit)
    {
        var image = fruit.GetComponent<UnityEngine.UI.Image>();

        for (int i = 0; i < mergeAnimationSprites.Length; i++)
        {
            image.sprite = mergeAnimationSprites[i];
            yield return new WaitForSeconds(animationFrameDuration);
        }

        image.sprite = fruit.Data.fullSprite;
    }

    private IEnumerator RunAnimationsWithLimit(List<IEnumerator> animations, int maxConcurrent)
    {
        Queue<IEnumerator> queue = new Queue<IEnumerator>(animations);
        int activeCount = 0;

        while (queue.Count > 0 || activeCount > 0)
        {
            while (queue.Count > 0 && activeCount < maxConcurrent)
            {
                IEnumerator anim = queue.Dequeue();
                activeCount++;
                StartCoroutine(RunAnimationWithCallback(anim, () => activeCount--));
            }

            yield return null;
        }
    }

    private IEnumerator RunAnimationWithCallback(IEnumerator anim, System.Action onComplete)
    {
        yield return StartCoroutine(anim);
        onComplete?.Invoke();
    }
}