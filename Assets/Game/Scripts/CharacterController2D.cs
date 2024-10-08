using Game.Scripts.Managers;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float m_RunSpeed = 40f; // Amount of force added when the player jumps.
    [SerializeField] private float m_JumpForce = 400f; // Amount of force added when the player jumps.
    [SerializeField] private float m_DashForce = 400f; // Amount of force added when the player dashes.
    [SerializeField] private float m_dashCooldown = 0.5f;

    [Range(0, 1)]
    [SerializeField]
    private float m_CrouchSpeed = .36f; // Amount of maxSpeed applied to crouching movement. 1 = 100%

    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f; // How much to smooth out the movement
    [SerializeField] private bool m_AirControl = false; // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround; // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck; // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck; // A position marking where to check for ceilings
    [SerializeField] private Collider2D m_CrouchDisableCollider; // A collider that will be disabled when crouching
    [SerializeField] private Animator m_animator; // Get the correct animator to have correct animations playing

    private float dashTime = 0;
    const float k_GroundedRadius = .5f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded; // Whether or not the player is grounded.
    const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true; // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;
    private float k_BottomlessDeathHeight = -50f;

    [Header("Events")][Space] public UnityEvent OnDashEvent = new UnityEvent();
    public UnityEvent OnLandEvent = new UnityEvent();
    public UnityEvent<bool> OnCrouchEvent = new UnityEvent<bool>();
    public UnityEvent OnDeathEvent = new UnityEvent();
    public UnityEvent OnFinishEvent = new UnityEvent();
    private bool m_wasCrouching = false;

    [Header("Time Speed Up")] public float DashTimeSpeedUp = 300f;
    public float JumpTimeSpeedUp = 200f;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        
            m_animator = GetComponentInChildren<Animator>();
        
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        foreach (var t in colliders)
        {
            if (t.gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }

        if (transform.position.y < k_BottomlessDeathHeight) OnDeathEvent.Invoke();
    }

    public void Move(float move, bool crouch, bool jump, bool dash)
    {
        move *= m_RunSpeed;
        bool isRunning = Mathf.Abs(move) > 0.01f;
        m_animator.SetBool("isRunning", isRunning);

        // If crouching, check to see if the character can stand up
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }

        if (m_Grounded && !jump && !dash && move > Mathf.Epsilon)
        {
            ParticlesManager.Instance.FollowPoint(
                "Walk",
                () => m_GroundCheck.position,
                () => !m_Grounded || Mathf.Abs(m_Rigidbody2D.velocity.y) < Mathf.Epsilon,
                () => m_FacingRight ? Quaternion.Euler(-90, 0, 0) : Quaternion.Euler(-90, 180, 0)
            );
   
        }

        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {
            // If crouching
            if (crouch)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                move *= m_CrouchSpeed;

                // Disable one of the colliders when crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = false;
            }
            else
            {
                // Enable the collider when not crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true;

                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity,
                m_MovementSmoothing);

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
        }

        // If the player should jump...
        if (m_Grounded && jump)
        {
            // Add a vertical force to the player.
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            SoundManager.Instance.Play("Jump");
            AgeManager.Instance.SpeedUp(JumpTimeSpeedUp, 3f);
        }

        if (dash && dashTime <= 0)
        {
            float direction = m_FacingRight ? 1 : -1;
            m_Rigidbody2D.AddForce(new Vector2(direction * m_DashForce, 0f), ForceMode2D.Impulse);
            OnDashEvent.Invoke();
            dashTime = m_dashCooldown;
            SoundManager.Instance.Play("Dash");
            AgeManager.Instance.SpeedUp(DashTimeSpeedUp, 2f);
            ParticlesManager.Instance.FollowPoint(
                "Dash",
                () => transform.position,
                () => dashTime <= 0,
                () => m_FacingRight ? Quaternion.Euler(0, 0, 90) : Quaternion.Euler(0, 180, 90)
            );
        }

        if (dashTime >= 0)
        {
            dashTime -= Time.deltaTime;
        }
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void SetController(CharacterController2D controller)
    {
        m_GroundCheck = controller.m_GroundCheck;
        m_CeilingCheck = controller.m_CeilingCheck;
        m_DashForce = controller.m_DashForce;
        m_JumpForce = controller.m_JumpForce;
        m_RunSpeed = controller.m_RunSpeed;
        m_animator = controller.m_animator;
    }

    void OnTriggerEnter2D(Collider2D trigger)
    {
        if (trigger.gameObject.tag == "Death")
        {
            OnFinishEvent.Invoke();
        }
    }
}