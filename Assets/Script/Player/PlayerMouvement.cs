using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerMouvement : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public PlayerMouvementData Data; // Make a reference to the PlayerMouvementData script which holds most of the variables and logic

    public bool _isGrounded = false;
    public Transform GroundCheck;
    public LayerMask GroundLayer;

    public bool _isTouchingWall = false;
    public Transform WallCheck;
    public LayerMask WallLayer;

    private float accelerationTimer;

    private float coyoteJumpTimer;
    private float bufferJumpTimer;
    private bool _canBufferJump;

    private bool _canDash;
    private bool _isFrozen;
    private float frozenTimer;

    private float slideAcceleration;
    public bool _isSliding;
    private float coyoteWallJumpTimer;

    public bool _isGrabing;

    public Rigidbody2D rb;
    public Vector2 Dir;

    private void Update()
    {
        if (!_isFrozen)
        {
            rb.gravityScale = Data.gravityScale;

            coyoteWallJumpTimer -= Time.deltaTime;
            coyoteJumpTimer -= Time.deltaTime;
            bufferJumpTimer -= Time.deltaTime;
            slideAcceleration -= Time.deltaTime;
            accelerationTimer += Time.deltaTime;
            accelerationTimer = Mathf.Clamp(accelerationTimer, -5, Data.MaxAcceleration);
            Debug.Log(accelerationTimer);

            Dir.x = Input.GetAxisRaw("Horizontal");

            if (Dir.x != 0 && _isGrounded)
                Walk(Mathf.Clamp(accelerationTimer, Data.ResetAcceleration + 0.10f, Data.MaxAcceleration - 0.25f));
            else if (Dir.x != 0 && !_isGrounded)
                Walk(accelerationTimer);
            else
                rb.velocity = new Vector2(0f, rb.velocity.y);

            if (Input.GetKeyDown(KeyCode.Space) && !_isGrounded)
            {
                bufferJumpTimer = Data.BufferJumpTime;
                _canBufferJump = true;
            }
            if (Input.GetKeyDown(KeyCode.Space) && _isGrounded || _isGrounded && bufferJumpTimer > 0f && _canBufferJump || Input.GetKeyDown(KeyCode.Space) && !_isGrounded && coyoteJumpTimer > 0f)
                Jump();
            if (Input.GetKeyUp(KeyCode.Space) && rb.velocity.y > Data.MinJumpHeight)
                JumpCut();

            if (Input.GetKeyDown(KeyCode.Mouse1) && _canDash || Input.GetKeyDown(KeyCode.L) && _canDash)
                Dash();

            if (_isTouchingWall && !_isGrounded && Dir.x != 0 && rb.velocity.y < 0f && !_isGrabing)
                Slide();
            else
                slideAcceleration = 0f;

            if (_isSliding && Input.GetKeyDown(KeyCode.Space) && Dir.x != 0 && _isTouchingWall || Input.GetKeyDown(KeyCode.Space) && !_isTouchingWall && coyoteWallJumpTimer > 0f)
                WallJump();

            if (_isGrounded)
            {
                coyoteJumpTimer = Data.CoyoteJumpTime;
                _canDash = true;
                _isSliding = false;
            }
            if(_isTouchingWall)
                coyoteWallJumpTimer = Data.CoyoteJumpTime;

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                accelerationTimer = Data.ResetAcceleration;
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                accelerationTimer = Data.ResetAcceleration;

            if (rb.velocity.y < 0f)
                rb.gravityScale = Data.gravityScale + Data.FallSpeed;
        }
        else
        {
            frozenTimer -= Time.fixedDeltaTime;
            rb.gravityScale = 0f;
            if (frozenTimer < 0f)
            {
                _isFrozen = false;
                rb.velocity = Vector2.zero;
                accelerationTimer = Data.ResetAcceleration;
            }
        }

        if (_isTouchingWall && Input.GetKey(KeyCode.M))
            WallGrab();
        if (_isTouchingWall && Input.GetKeyUp(KeyCode.M))
            frozenTimer = 0f;

        CreateChecks();

        Rotate();

        if (Dir.x != 0)
            animator.SetBool("isRunning", true);
        else
            animator.SetBool("isRunning", false);
    }

    void Walk(float acceleration)
    {
        rb.velocity = new Vector2(acceleration * Data.MaxWalkSpeed * Dir.x, rb.velocity.y);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * Data.JumpForce, ForceMode2D.Impulse);
        _canBufferJump = false;
    }

    void JumpCut()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
    }

    void Dash()
    {
        frozenTimer = Data.DashDuration;
        _canDash = false;
        _isFrozen = true;
        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(Dir.x * Data.DashForce, 0f), ForceMode2D.Impulse);
    }

    void Slide()
    {
        _isSliding = true;
        float slideSpeed = -Data.SlideSpeed * slideAcceleration;
        rb.velocity = new Vector2(0f, slideSpeed);
    }

    void WallJump()
    {
        //accelerationTimer = -Data.WallJumpDistance;
        //_isSliding = false;
        //if (coyoteWallJumpTimer < 0f)
        //{
        //    rb.velocity = Vector2.zero;
        //    rb.velocity = new Vector2(-Dir.x * Data.WallJumpForce * Data.WallJumpForce, Data.WallJumpForce / 2);
        //}
        //else
        //{
        //    rb.velocity = Vector2.zero;
        //    rb.velocity = new Vector2(Dir.x * Data.WallJumpForce * Data.WallJumpForce, Data.WallJumpForce / 2);
        //}
    }

    void WallGrab()
    {
        _isGrabing = true;
        _isSliding = false;
        _isFrozen = true;
        frozenTimer += 1f;
        if (Input.GetKeyDown(KeyCode.W))
            rb.velocity = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.S))
            rb.velocity = Vector2.down;
        else
            rb.velocity = Vector2.zero;
    }

    void CreateChecks()
    {
        _isGrounded = Physics2D.OverlapCapsule(GroundCheck.position, new Vector2(0.147f, 0.147f), CapsuleDirection2D.Vertical, 0, GroundLayer);

        _isTouchingWall = Physics2D.OverlapCapsule(WallCheck.position, new Vector2(0.147f, 0.273f), CapsuleDirection2D.Vertical, 0, GroundLayer);
    }
    void Rotate()
    {
        if (Dir.x == 1)
            transform.localScale = new Vector3(1f, 1f, 1f);

        if (Dir.x == -1)
            transform.localScale = new Vector3(-1f, 1f, 1f);
    }
}
