using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class BeltManager : MonoBehaviour
{
    [SerializeField] private RectTransform spawnPoint;
    [SerializeField] private RectTransform destroyPoint;

    private readonly float spawnInterval = 1.3f;
    private readonly float moveSpeed = 175f;

    private float timer = 0f;
    private bool spawning = false;

    private List<ItemData> weightedList;

    private class MovingFruit
    {
        public ItemTile controller;
        public Vector2 startPos;
        public Vector2 endPos;
        public float startTime;
        public float journeyLength;
    }

    private readonly List<MovingFruit> movingFruits = new();

    private void Start()
    {
        PrepareFruitWeights();
    }

    private void Update()
    {
        if (spawning)
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                timer = 0f;
                SpawnFruit();
            }
        }

        UpdateMovingFruits();
    }

    public void StartSpawning()
    {
        spawning = true;
        timer = 0f;
    }

    public void StopSpawning()
    {
        spawning = false;
        movingFruits.Clear();
    }

    public void ClearAllFruits()
    {
        foreach (var mf in movingFruits)
        {
            if (mf.controller != null && mf.controller.gameObject.activeSelf)
                ItemPool.Instance.ReturnFruit(mf.controller);
        }
        movingFruits.Clear();
    }

    private void SpawnFruit()
    {
        ItemTile fruit = ItemPool.Instance.GetFruit();
        ItemData data = GetRandomWeightedFruit();

        Vector2 start = spawnPoint.anchoredPosition;
        Vector2 end = destroyPoint.anchoredPosition;

        fruit.Init(data, start);
        fruit.CanBeSliced = true; // На ленте можно резать
        fruit.transform.SetParent(spawnPoint.parent, false);

        movingFruits.Add(new MovingFruit
        {
            controller = fruit,
            startPos = start,
            endPos = end,
            startTime = Time.time,
            journeyLength = Vector2.Distance(start, end)
        });
    }

    private void UpdateMovingFruits()
    {
        float now = Time.time;

        for (int i = 0; i < movingFruits.Count; i++)
        {
            var mf = movingFruits[i];
            if (mf.controller == null || !mf.controller.gameObject.activeInHierarchy)
            {
                mf.journeyLength = -1;
                continue;
            }

            float distCovered = (now - mf.startTime) * moveSpeed;
            float frac = distCovered / mf.journeyLength;

            mf.controller.SetPosition(Vector2.Lerp(mf.startPos, mf.endPos, frac));

            if (frac >= 1f)
            {
                ItemPool.Instance.ReturnFruit(mf.controller);
                mf.journeyLength = -1;
            }
        }

        movingFruits.RemoveAll(mf => mf.journeyLength == -1);
    }

    private void PrepareFruitWeights()
    {
        var fruitTypes = ItemPool.Instance.fruitDatas;
        if (fruitTypes == null || fruitTypes.Count == 0) return;

        int maxValue = fruitTypes.Max(f => f.value);
        var filtered = fruitTypes.Where(f => f.value < maxValue).ToList();

        weightedList = new List<ItemData>();

        foreach (var fruit in filtered)
        {
            int logValue = (int)Mathf.Log(fruit.value, 2);
            int weight = Mathf.Max(1, 10 - logValue);

            for (int i = 0; i < weight; i++)
                weightedList.Add(fruit);
        }
    }

    private ItemData GetRandomWeightedFruit()
    {
        if (weightedList == null || weightedList.Count == 0)
            return ItemPool.Instance.fruitDatas[0];

        int index = Random.Range(0, weightedList.Count);
        return weightedList[index];
    }
}