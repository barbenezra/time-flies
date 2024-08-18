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
     
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
      
        rb.velocity = direction * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
    
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
         
            direction *= -1;
        }
    }
}
