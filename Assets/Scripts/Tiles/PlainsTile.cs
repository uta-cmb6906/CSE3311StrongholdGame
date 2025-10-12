using UnityEngine;

public class PlainsTile : Tile
{


    [Header("Visual Options")]
    [SerializeField] private Sprite[] plainsSprites;


    private SpriteRenderer _cachedRenderer;

    // RandomizeSprite 
    private void Awake()
    {
        // Cache the local renderer reference once
        _cachedRenderer = GetComponent<SpriteRenderer>();
        RandomizeSprite();
    }

    public void RandomizeSprite()
    {
        // Use the cached local reference
        if (_cachedRenderer == null)
        {
            _cachedRenderer = GetComponent<SpriteRenderer>();
        }

        if (_cachedRenderer != null && plainsSprites != null && plainsSprites.Length > 0)
        {
            // Pick a random index
            int randomIndex = UnityEngine.Random.Range(0, plainsSprites.Length);

            // Assign the random sprite using the local reference
            _cachedRenderer.sprite = plainsSprites[randomIndex];
        }
        else
        {
            // The error logging  reflects the use of the local component reference
            Debug.LogError($"PlainsTile at ({X()}, {Y()}) failed to randomize sprite. Check if SpriteRenderer is present or the Sprite array is empty!");
        }
    }
}
