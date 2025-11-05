using UnityEngine;
using UnityEngine.UI;

public class BaseUnit : MonoBehaviour
{
    [SerializeField] private Image HealthBar;

    public Tile OccupiedTile;
    public bool isPlayer;
    public int maxHealth;
    [SerializeField] protected int cost;
    [SerializeField] protected int health;
    [SerializeField] protected int defense;
    [SerializeField] protected int movementRange;
    [SerializeField] protected int attackRange;
    [SerializeField] protected int meleeDamage;
    [SerializeField] protected int rangedDamage;
    [SerializeField] protected bool actionPointRemaining = true;

    public Team OwnerTeam => isPlayer ? Team.Player : Team.Enemy; // converts isPlayer -> Team

    public int Cost() => cost;
    public int Health() => health;
    public int Defense() => defense;
    public int MovementRange() => movementRange;
    public int AttackRange() => attackRange;
    public int MeleeDamage() => meleeDamage;
    public int RangedDamage() => rangedDamage;
    public bool IsPlayer() => isPlayer;
    public bool ActionpointRemaining() => actionPointRemaining;

    public bool ResetAction() => actionPointRemaining = true;

    public bool DeleteAction() => actionPointRemaining = false;

    void Start()
    {
        maxHealth = health;
        if (HealthBar != null) HealthBar.fillAmount = 1f;
        else Debug.LogError("HealthBar not assigned!");

        if (isPlayer && actionPointRemaining) OccupiedTile.HighlightTile(Color.yellow, true);

        //TODO apply special faction modifiers
    }

    /*//Remove Unit from list held in game manager
    private void OnDestroy()
    {
        if (isPlayer)
            GameManager.Instance.playerUnits.Remove(this);
        else
            GameManager.Instance.enemyUnits.Remove(this);
    }*/

    //highlight all of a units valid tiles blue for within movement range and red for attackable
    public void HighlightValidTiles()
    {
        //highlight empty tiles within movement range in blue
        for (int x = OccupiedTile.X() - movementRange; x <= OccupiedTile.X() + movementRange; x++)
        {
            for (int y = OccupiedTile.Y() - movementRange; y <= OccupiedTile.Y() + movementRange; y++)
            {
                Tile tile = GridManager.Instance.GetTileAtPosition(new Vector3(x, y));
                if (tile && !tile.IsOccupied()) tile.HighlightTile(Color.blue, true);
            }
        }

        //highlight enemies within attack range in red
        for (int x = OccupiedTile.X() - attackRange; x <= OccupiedTile.X() + attackRange; x++)
        {
            for (int y = OccupiedTile.Y() - attackRange; y <= OccupiedTile.Y() + attackRange; y++)
            {
                Tile tile = GridManager.Instance.GetTileAtPosition(new Vector3(x, y));
                if (tile && tile.IsOccupied() && !tile.GetStationedUnit().isPlayer) tile.HighlightTile(Color.red, true);
            }
        }
    }

    //unhighlight all of a units valid tiles
    public void UnhighlightValidTiles()
    {
        //range is largest between attack and movement range
        int range = (movementRange >= attackRange) ? movementRange : attackRange;

        //unhighlight all tiles within range
        for (int x = OccupiedTile.X() - range; x <= OccupiedTile.X() + range; x++)
        {
            for (int y = OccupiedTile.Y() - range; y <= OccupiedTile.Y() + range; y++)
            {
                Tile tile = GridManager.Instance.GetTileAtPosition(new Vector3(x, y));
                if (tile) tile.UnhighlightTile();
            }
        }
    }

    //try to move to tile returning true if successful or false if tile out of range
    public bool AttemptMovement(Tile destination)
    {
        if (!InRange(destination, movementRange)) return false;
        Move(destination);
        GameManager.Instance.HighlightAvailableUnits();
        return true;
    }

    //checks if destination tile is within given range of unit
    protected bool InRange(Tile destination, int range)
    {
        return (Mathf.Abs(OccupiedTile.X() - destination.X()) <= range && Mathf.Abs(OccupiedTile.Y() - destination.Y()) <= range);
    }

    protected void Move(Tile destination)
    {
        //remove available unit highlight
        OccupiedTile.UnhighlightTile();

        UnhighlightValidTiles();
        OccupiedTile.ChangeStationed(null);
        OccupiedTile = destination;
        OccupiedTile.ChangeStationed(this);
        transform.position = OccupiedTile.transform.position;
        actionPointRemaining = false;
    }

    //try to attack enemy returning true if successful or false if enemy out of range
    public bool AttemptAttack(BaseUnit enemy)
    {
        if (!InRange(enemy.OccupiedTile, attackRange)) return false;
        Attack(enemy);
        return true;
    }

    protected void Attack(BaseUnit enemy)
    {
        //remove available unit highlight
        OccupiedTile.UnhighlightTile();

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

        actionPointRemaining = false;
        GameManager.Instance.HighlightAvailableUnits();
    }

    //unit recieves specified damage modified by randomness and reduction. If its health is reduced to 0 it is destroyed
    protected void TakeDamage(float damage, float reduction)
    {
        float variation = Random.Range(.8f, 1.2f);
        health -= (int)(damage * variation * (100 - reduction) / 100);
        UpdateHealthBar();
        if (health <= 0)
        {
            //remove from GameManager's unit lists
            if (isPlayer) GameManager.Instance.playerUnits.Remove(this);
            else GameManager.Instance.enemyUnits.Remove(this);
            
            Destroy(gameObject);
        } 
    }

    //restore unit health
    public void HealUnit()
    {
        //remove available unit highlight
        OccupiedTile.UnhighlightTile();

        health = maxHealth;
        UpdateHealthBar();

        actionPointRemaining = false;
    }

    //increase all unit stats by 20%
    public void UpgradeUnit()
    {
        //remove available unit highlight
        OccupiedTile.UnhighlightTile();

        maxHealth = (int)(maxHealth * 1.2f);
        health = (int)(health * 1.2f);
        defense = (int)(defense * 1.2f);
        meleeDamage = (int)(meleeDamage * 1.2f);
        rangedDamage = (int)(rangedDamage * 1.2f);

        actionPointRemaining = false;
    }

    protected void UpdateHealthBar()
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

