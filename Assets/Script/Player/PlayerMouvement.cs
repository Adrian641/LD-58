using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    public bool _isGrabing;

    public bool _isWallJumping;
    public float wallJumpingDir;
    public float wallJumpCounter;
    public float wallJumpTime = 0.075f;
    public float wallJumpDuration = 0.2f;
    public bool onlyChangeDirOnce = false;

    private float downwardAcceleration;

    public Rigidbody2D rb;
    private float horizontalInput;
    private float verticalInput;
    public Vector2 Dir;

    private void Update()
    {
        if (!_isFrozen)
        {
            rb.gravityScale = Data.gravityScale;


            coyoteJumpTimer -= Time.deltaTime;
            bufferJumpTimer -= Time.deltaTime;
            slideAcceleration -= Time.deltaTime;
            accelerationTimer += Time.deltaTime / Data.AccelerationSpeed;
            accelerationTimer = Mathf.Clamp(accelerationTimer, -5, Data.MaxAcceleration);

            verticalInput = Input.GetAxisRaw("Vertical");
            horizontalInput = Input.GetAxisRaw("Horizontal");
            if (horizontalInput == 1)
                Dir.x = 1;
            if (horizontalInput == -1)
                Dir.x = -1;

            if (!_isWallJumping)
            {
                if (horizontalInput != 0 && _isGrounded)
                    Walk(Mathf.Clamp(accelerationTimer, Data.ResetAcceleration + 0.10f, Data.MaxAcceleration - 0.25f));
                else if (horizontalInput != 0 && !_isGrounded)
                    if (wallJumpCounter > -0.65f && Dir.x != wallJumpingDir)
                        Walk(accelerationTimer / Data.restirctAirMouvement);
                    else
                        Walk(accelerationTimer);
                else
                    rb.velocity = new Vector2(0f, rb.velocity.y);
            }

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
            if (_isSliding || _isGrabing)
                animator.SetBool("isOnWall", true);
            else
                animator.SetBool("isOnWall", false);

            if (_canDash)
                animator.SetBool("hasDash", true);
            else
                animator.SetBool("hasDash", false);


            if (_isTouchingWall && !_isGrounded && Dir.x != 0 && rb.velocity.y < 0f && !_isGrabing)
                Slide();
            else
            {
                slideAcceleration = 0f;
                _isSliding = false;
            }

            WallJump();

            if (_isGrounded)
            {
                coyoteJumpTimer = Data.CoyoteJumpTime;
                _canDash = true;
                _isSliding = false;
                animator.SetBool("isStable", false);
                animator.SetBool("isJumping", false);
                animator.SetBool("isFalling", false);
                animator.SetBool("isOnGround", true);
            }
            if (!_isGrounded)
                animator.SetBool("isOnGround", false);

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                accelerationTimer = Data.ResetAcceleration;
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                accelerationTimer = Data.ResetAcceleration;

            if (rb.velocity.y < 0f)
            {
                downwardAcceleration = Mathf.Clamp(downwardAcceleration += Time.deltaTime, 1f, 3f);
                rb.gravityScale = Data.gravityScale + Data.FallSpeed * downwardAcceleration;
            }
            else
            {
                downwardAcceleration = 1f;
            }

            if (rb.velocity.y > 5f)
            {
                animator.SetBool("isJumping", true);
                animator.SetBool("isStable", false);
                animator.SetBool("isFalling", false);
            }

            else if (rb.velocity.y < -5f)
            {
                animator.SetBool("isFalling", true);
                animator.SetBool("isJumping", false);
                animator.SetBool("isStable", false);
            }
            else
            {
                animator.SetBool("isStable", true);
                animator.SetBool("isFalling", false);
                animator.SetBool("isJumping", false);
            }
        }
        else
        {
            frozenTimer -= Time.fixedDeltaTime;
            rb.gravityScale = 0f;
            if (frozenTimer < 0f)
            {
                _isGrabing = false;
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

        if (!_isWallJumping)
            Rotate();

        if (horizontalInput != 0)
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
        //rb.velocity = Vector2.zero;
        rb.velocity = new Vector2(horizontalInput, verticalInput).normalized * Data.DashForce;
        if (horizontalInput == 0f && verticalInput == 0f)
            rb.velocity = new Vector2(Dir.x * Data.DashForce, 0f);
        if (_isTouchingWall)
            _isFrozen = false;
    }

    void Slide()
    {
        accelerationTimer = Data.ResetAcceleration;
        _isSliding = true;
        float slideSpeed = -Data.SlideSpeed * slideAcceleration;
        rb.velocity = new Vector2(0f, slideSpeed);

    }

    void WallJump()
    {
        if(_isSliding)
        {
            accelerationTimer = Data.ResetAcceleration;
            _isWallJumping = false;
            if (!onlyChangeDirOnce)
            {
                wallJumpingDir = -transform.localScale.x;
                onlyChangeDirOnce = true;
            }
            wallJumpCounter = wallJumpTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpCounter -= Time.deltaTime;
        }

        if(wallJumpCounter < 0f)
            onlyChangeDirOnce = false;

        if (Input.GetKeyDown(KeyCode.Space) && wallJumpCounter > -0.02f)
        {
            accelerationTimer = Data.ResetAcceleration;
            _isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDir * Data.WallJumpForce.x, Data.WallJumpForce.y);
            wallJumpCounter = 0f;
        }
            Invoke(nameof(StopWallJumping), wallJumpDuration);
    }

    private void StopWallJumping()
    {
        _isWallJumping = false;
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