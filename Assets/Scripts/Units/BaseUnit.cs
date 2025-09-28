using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    public Tile OccupiedTile;
    public bool isPlayer;
    [SerializeField] protected int health;
    [SerializeField] protected int attackDamage;

    //try to move to tile returning true if successful or false if tile out of range
    public bool AttemptMovement(Tile destination)
    {
        if (!InMovementRange(destination)) return false;
        Move(destination);
        return true;
    }

    protected bool InMovementRange(Tile destination)
    {
        //TODO check if outside of movement range
        return true;
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
        if (!InAttackRange(enemy)) return false;
        Attack(enemy);
        return true;
    }

    protected bool InAttackRange(BaseUnit enemy)
    {
        //TODO check if outside of movement range
        return true;
    }
    
    public void Attack(BaseUnit enemy)
    {
        enemy.TakeDamage(attackDamage);
        TakeDamage(enemy.attackDamage);
    }

    private void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0) Destroy(gameObject);
    }
}
