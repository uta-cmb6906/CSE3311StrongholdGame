using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Scriptable Unit")]

public class ScriptableUnit : ScriptableObject
{
    public Team Team;
    public BaseUnit UnitPrefab;
}

public enum Team
{
    Player,
    Enemy
}