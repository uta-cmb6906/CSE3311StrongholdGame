using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    public Tile OccupiedTile;
    public bool isPlayer;
    [SerializeField] protected float health;
    [SerializeField] protected int defense;
    [SerializeField] protected int movementRange;
    [SerializeField] protected int attackRange;
    [SerializeField] protected int meleeDamage;
    [SerializeField] protected int rangedDamage;

    public float Health() => health;
    public int Defense() => defense;
    public int MovementRange() => movementRange;
    public int AttackRange() => attackRange;
    public int MeleeDamage() => meleeDamage;
    public int RangedDamage() => rangedDamage;

    //try to move to tile returning true if successful or false if tile out of range
    public bool AttemptMovement(Tile destination)
    {
        if (!InRange(destination, movementRange)) return false;
        Move(destination);
        return true;
    }

    //checks if destination tile is within given range of unit
    protected bool InRange(Tile destination, int range)
    {
        return (Mathf.Abs(OccupiedTile.X() - destination.X()) <= range && Mathf.Abs(OccupiedTile.Y() - destination.Y()) <= range);
    }

    public void Move(Tile destination)
    {
        OccupiedTile._unitStationed = null;
        OccupiedTile = destination;
        OccupiedTile._unitStationed = this;
        transform.position = OccupiedTile.transform.position;
    }

    //try to attack enemy returning true if successful or false if enemy out of range
    public bool AttemptAttack(BaseUnit enemy)
    {
        if (!InRange(enemy.OccupiedTile, attackRange)) return false;
        Attack(enemy);
        return true;
    }

    public void Attack(BaseUnit enemy)
    {
        //enemy defense modified by its terrain bonus
        float enemyDefense = enemy.Defense() * enemy.OccupiedTile.TerrainModifier();
        
        //if enemy is adjacent do melee attack
        if (InRange(enemy.OccupiedTile, 1))
        {
            TakeDamage(enemy.MeleeDamage(), defense);
            enemy.TakeDamage(meleeDamage, enemyDefense);
        }

        //otherwise do ranged attack
        else enemy.TakeDamage(rangedDamage, enemyDefense);
    }

    //unit recieves specified damage modified by randomness and reduction. If its health is reduced to 0 it is destroyed
    private void TakeDamage(float damage, float reduction)
    {
        float variation = Random.Range(.9f, 1.1f);
        health -= damage * variation * (100 - reduction) / 100;
        if (health <= 0) Destroy(gameObject);
    }
}
