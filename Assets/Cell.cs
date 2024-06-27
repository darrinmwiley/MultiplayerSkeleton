using UnityEngine;

public class GrowingAndSplittingSprite : MonoBehaviour
{
    public float startSize = 1f; // Initial size of the sprite
    public float growthRate = 0.5f; // Size growth rate per second
    public float maxSize = 3f; // Maximum size the sprite can grow to
    public float movementForce = 10f; // Force applied for movement
    public float mass = 1f; // Mass of the Rigidbody2D
    public float drag = 1f; // Linear drag for the Rigidbody2D
    public Material material; // Material for the sprite

    private Vector3 originalScale;
    private bool isGrowing = true;
    private static GameObject controlledCell; // Reference to the currently controlled cell
    private Rigidbody2D rb;
    private CircleCollider2D collider;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Set the initial size of the sprite
        Init();
    }

    void Init()
    {
        // Initialize Rigidbody2D
        rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        // Initialize CircleCollider2D
        collider = gameObject.GetComponent<CircleCollider2D>();
        if (collider == null)
            collider = gameObject.AddComponent<CircleCollider2D>();
        
        // Set Rigidbody2D properties
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.drag = drag;
        rb.mass = mass;

        // Initialize and set SpriteRenderer
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        if (material != null)
        {
            spriteRenderer.material = material;
        }

        // Set initial scale
        originalScale = new Vector3(startSize, startSize, 1f);
        transform.localScale = originalScale;

        // Enable growth
        isGrowing = true;
    }

    void Update()
    {
        // Update Rigidbody2D properties dynamically
        rb.mass = mass;
        rb.drag = drag;

        // Handle growth
        if (isGrowing)
        {
            float newSize = Mathf.Min(transform.localScale.x + growthRate * Time.deltaTime, maxSize);
            transform.localScale = new Vector3(newSize, newSize, 1f);

            if (newSize >= maxSize)
            {
                isGrowing = false;
            }
        }

        bool mouseOver = IsMouseOver();

        // Check for mouse input
        if (Input.GetMouseButtonDown(0) && mouseOver) // Left click
        {
            OnLeftClick();
        }

        // Check for right mouse click
        if (Input.GetMouseButtonDown(1) && mouseOver) // Right click
        {
            OnRightClick();
        }

        // Move the controlled cell
        if (controlledCell == gameObject)
        {
            MoveWithKeyboard();
        }
    }

    private void OnLeftClick()
    {
        controlledCell = gameObject;
    }

    private void OnRightClick()
    {
        Split();
    }

    private void MoveWithKeyboard()
    {
        Vector2 force = Vector2.zero;

        // Arrow keys and WASD for movement
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            force += Vector2.up;
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            force += Vector2.down;
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            force += Vector2.left;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            force += Vector2.right;
        }

        rb.AddForce(force * movementForce);
    }

    private bool IsMouseOver()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D collider = Physics2D.OverlapPoint(mousePos);

        return collider != null && collider.gameObject == gameObject;
    }

    private void Split()
    {
        // Calculate the new size for the split sprites
        float newSize = transform.localScale.x / 2f;
        if (newSize < startSize / 2f)
        {
            return; // Prevent splitting if the new size would be too small
        }

        // Create two new game objects for the split sprites
        GameObject sprite1 = Instantiate(gameObject, transform.position, Quaternion.identity);
        GameObject sprite2 = Instantiate(gameObject, transform.position, Quaternion.identity);

        // Adjust sizes
        sprite1.transform.localScale = new Vector3(newSize, newSize, 1f);
        sprite2.transform.localScale = new Vector3(newSize, newSize, 1f);

        // Apply forces to move them apart
        Rigidbody2D rb1 = sprite1.GetComponent<Rigidbody2D>();
        Rigidbody2D rb2 = sprite2.GetComponent<Rigidbody2D>();

        Vector2 splitDirection = Random.insideUnitCircle.normalized;
        rb1.AddForce(splitDirection * 2, ForceMode2D.Impulse);
        rb2.AddForce(-splitDirection * 2, ForceMode2D.Impulse);

        // Reset control if the controlled cell is the one that split
        if (controlledCell == gameObject)
        {
            controlledCell = null;
        }

        // Destroy the original sprite
        Destroy(gameObject);
    }
}
