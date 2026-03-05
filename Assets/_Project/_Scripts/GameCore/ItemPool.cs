using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPool : MonoBehaviour
{
    public static ItemPool Instance;

    public ItemTile fruitPrefab;
    public ItemHalf halfFruitPrefab;
    public int initialSize = 6;

    private ObjectPool<ItemTile> fruitPool;
    private ObjectPool<ItemHalf> halfFruitPool;
    public List<ItemData> fruitDatas;

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

        fruitPool = new ObjectPool<ItemTile>(fruitPrefab, initialSize, this.transform);
        halfFruitPool = new ObjectPool<ItemHalf>(halfFruitPrefab, initialSize * 2, this.transform);
    }

    public ItemTile GetFruit() => fruitPool.Get();
    public void ReturnFruit(ItemTile fruit) => fruitPool.Return(fruit);

    public ItemHalf GetHalf() => halfFruitPool.Get();
    public void ReturnHalf(ItemHalf half) => halfFruitPool.Return(half);
}