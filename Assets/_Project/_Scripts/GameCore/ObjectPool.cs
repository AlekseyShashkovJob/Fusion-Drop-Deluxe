using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour {
    private readonly T prefab;
    private readonly Queue<T> pool = new();
    private Transform parent;

    public ObjectPool(T prefab, int initialSize, Transform parent = null) {
        this.prefab = prefab;
        this.parent = parent;
        for (int i = 0; i < initialSize; i++) {
            T obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public T Get() {
        if (pool.Count > 0) {
            var obj = pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }

        return Object.Instantiate(prefab, parent);
    }

    public void Return(T obj) {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}