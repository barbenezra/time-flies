using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;

    public float runSpeed = 40f;

    float horizontalMove = 0f;
    bool jump = false;
    bool dash = false;
    bool crouch = false;
	
    // Update is called once per frame
    void Update () {

        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump = true;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            dash = true;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            crouch = true;
        } else if (Input.GetKeyUp(KeyCode.C))
        {
            crouch = false;
        }

        if (Input.GetKey(KeyCode.P))
        {
            GetComponent<PlayerState>().ChangeState(new AdultState());
        }
        
        if (Input.GetKey(KeyCode.X))
        {
            GetComponent<PlayerState>().ChangeState(new BabyState());
        }
    }

    void FixedUpdate()
    {
        // Move our character
        controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump, dash);
        jump = false;
        dash = false;
    }
}
