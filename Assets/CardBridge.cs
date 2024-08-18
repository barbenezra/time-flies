using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBridge : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] float speed = 2f;

    private Vector2 direction = Vector2.right; // Initial direction of movement
    private Rigidbody2D rb;

    void Start()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Move the platform in the current direction
        rb.velocity = direction * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the platform collided with an object on the floor layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Reverse the direction when hitting a floor object
            direction *= -1;
        }
    }
}
