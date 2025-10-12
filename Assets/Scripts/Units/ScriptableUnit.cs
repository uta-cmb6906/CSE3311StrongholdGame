using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Scriptable Unit")]

public class ScriptableUnit : ScriptableObject
{
    public Team Team;
    public BaseUnit UnitPrefab;

    [Header("Economy")]
    public int GoldCost = 100;
}

public enum Team
{
    Player,
    Enemy

}

