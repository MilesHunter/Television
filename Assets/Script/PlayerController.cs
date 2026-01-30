using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayerMask = 1;

    [Header("Item Pickup")]
    public float pickupRange = 2f;
    public string itemTag = "Mask";
    public Vector3 heldItemOffset = new Vector3(0, 0.5f, 0); // Offset from player center

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // Movement variables
    private float horizontalInput;
    private bool isGrounded;
    private bool facingRight = true;

    // Item pickup system
    private GameObject heldItem = null;

    void Start()
    {
        // Get required components
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Create ground check point if not assigned
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }

    void Update()
    {
        // Get input
        GetInput();

        // Check if grounded
        CheckGrounded();

        // Handle movement
        HandleMovement();

        // Handle jumping
        HandleJump();

        // Handle item pickup
        HandleItemPickup();
    }

    void GetInput()
    {
        // Get horizontal input (A/D keys)
        horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A))
            horizontalInput = -1f;
        else if (Input.GetKey(KeyCode.D))
            horizontalInput = 1f;
    }

    void HandleMovement()
    {
        // Apply horizontal movement
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

        // Handle sprite flipping
        if (horizontalInput > 0 && !facingRight)
            Flip();
        else if (horizontalInput < 0 && facingRight)
            Flip();
    }

    void HandleJump()
    {
        // Jump when space is pressed and player is grounded
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    void CheckGrounded()
    {
        // Check if player is touching ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
    }

    void HandleItemPickup()
    {
        // Check for pickup input
        if (Input.GetKeyDown(KeyCode.J))
        {
            // Find nearby filter (excluding currently held item)
            FilterController nearbyFilter = FindNearbyFilter();

            if (heldItem == null)
            {
                // Case 1: No item held, pick up nearby filter if available
                if (nearbyFilter != null)
                {
                    PickupFilter(nearbyFilter);
                }
            }
            else
            {
                // Case 2 & 3: Item is held
                if (nearbyFilter != null)
                {
                    // Case 3: Swap filters - drop current and pick up new one
                    DropFilter(heldItem.GetComponent<FilterController>());
                    PickupFilter(nearbyFilter);
                }
                else
                {
                    // Case 2: Drop current filter
                    DropFilter(heldItem.GetComponent<FilterController>());
                }
            }
        }

        // Update held item position
        if (heldItem != null)
        {
            UpdateHeldItemPosition();
        }
    }

    FilterController FindNearbyFilter()
    {
        // Find all filter controllers in the scene
        FilterController[] allFilters = FindObjectsOfType<FilterController>();

        foreach (FilterController filter in allFilters)
        {
            // Skip if this is the currently held filter
            if (filter.gameObject == heldItem) continue;

            // Skip if filter cannot be picked up
            if (!filter.CanBePickedUp()) continue;

            float distance = Vector2.Distance(transform.position, filter.transform.position);

            if (distance <= pickupRange)
            {
                return filter;
            }
        }

        return null;
    }

    void PickupItem(GameObject item)
    {
        // Legacy method - kept for backward compatibility
        Debug.Log("Picked up: " + item.name);
        Destroy(item);
    }

    void PickupFilter(FilterController filter)
    {
        Debug.Log("Picked up filter: " + filter.name);

        // Set as held item
        heldItem = filter.gameObject;

        // Use FilterController's pickup method
        filter.OnPickedUp(transform);

        // Position filter at player center with offset
        UpdateHeldItemPosition();

        // Optional: Add pickup effects
        // PlayPickupSound();
        // ShowPickupEffect();
    }

    void DropFilter(FilterController filter)
    {
        if (filter == null) return;

        Debug.Log("Dropped filter: " + filter.name);

        // Use FilterController's drop method
        filter.OnDropped(transform.position);

        // Clear held item reference
        heldItem = null;

        // Optional: Add drop effects
        // PlayDropSound();
        // ShowDropEffect();
    }

    void UpdateHeldItemPosition()
    {
        if (heldItem != null)
        {
            // Position item at player center with offset
            heldItem.transform.position = transform.position + heldItemOffset;
        }
    }

    void Flip()
    {
        // Switch the direction flag and flip the sprite
        facingRight = !facingRight;
        spriteRenderer.flipX = !facingRight;
    }

    // Draw gizmos for debugging
    void OnDrawGizmosSelected()
    {
        // Draw ground check circle
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Draw pickup range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}