using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Scriptable Unit")]

public class ScriptableUnit : ScriptableObject
{
    public Team Team;
    public BaseUnit UnitPrefab;

    [Header("Economy")]
    public int GoldCost = 100;

    [Header("UI Display")]
    public Sprite Icon;
}

public enum Team
{
    Player,
    Enemy

}


