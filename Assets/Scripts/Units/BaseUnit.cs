using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    public Tile OccupiedTile;
    public Team team;
    public bool isPlayer;
    [SerializeField] protected int health;
    [SerializeField] protected int attackDamage;


    public void Attack(BaseUnit enemy)
    {
        enemy.TakeDamage(attackDamage);
        TakeDamage(enemy.attackDamage);
    }

    private void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0) Destroy(this);
    }

    public void Move(Tile destination)
    {
        OccupiedTile._unitStationed = null;
        OccupiedTile = destination;
        OccupiedTile._unitStationed = this;
        transform.position = OccupiedTile.transform.position;
    }
}
