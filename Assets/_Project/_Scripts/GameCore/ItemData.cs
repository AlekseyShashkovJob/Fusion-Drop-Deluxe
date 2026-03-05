using UnityEngine;

[CreateAssetMenu(menuName = "Game/FruitData")]
public class ItemData : ScriptableObject
{
    public int value;
    public Sprite fullSprite;
    public Sprite leftHalfSprite;
    public Sprite rightHalfSprite;
}