using UnityEngine;
using UnityEngine.UI;

public class BaseUnit : MonoBehaviour
{
    [SerializeField] private Image HealthBar;

    public Tile OccupiedTile;
    public bool isPlayer;
    protected int maxHealth;
    [SerializeField] protected int health;
    [SerializeField] protected int defense;
    [SerializeField] protected int movementRange;
    [SerializeField] protected int attackRange;
    [SerializeField] protected int meleeDamage;
    [SerializeField] protected int rangedDamage;

    public int Health() => health;
    public int Defense() => defense;
    public int MovementRange() => movementRange;
    public int AttackRange() => attackRange;
    public int MeleeDamage() => meleeDamage;
    public int RangedDamage() => rangedDamage;

    void Start()
    {
        maxHealth = health;
        if (HealthBar != null) HealthBar.fillAmount = 1f;
        else Debug.LogError("HealthBar not assigned!");

        //TODO apply special faction modifiers
    }

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
        float variation = Random.Range(.8f, 1.2f);
        health -= (int)(damage * variation * (100 - reduction) / 100);
        UpdateHealthBar();
        if (health <= 0) Destroy(gameObject);
    }

    //restore unit health
    public void HealUnit()
    {
        health = maxHealth;
    }

    //increase all unit stats by 20%
    public void UpgradeUnit()
    {
        maxHealth = (int) (maxHealth * 1.2f);
        health = (int) (health * 1.2f);
        defense = (int) (defense * 1.2f);
        meleeDamage = (int) (meleeDamage * 1.2f);
        rangedDamage = (int) (rangedDamage * 1.2f);
    }

    private void UpdateHealthBar()
    {
        HealthBar.fillAmount = (float)health / maxHealth;
    }

    public string UnitInfo()
    {
        return this.GetType().Name
            + "\n+ " + health + " Health"
            + "\n+ " + defense + " Defense"
            + "\n+ " + movementRange + " Movement Range"
            + "\n+ " + attackRange + " Attack Range"
            + "\n+ " + meleeDamage + " Melee Damage"
            + "\n+ " + rangedDamage + " Ranged Damage";
    }
}
