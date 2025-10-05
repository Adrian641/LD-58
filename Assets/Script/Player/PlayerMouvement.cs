using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMouvement : MonoBehaviour
{
    [SerializeField] private Animator animator;
    //[SerializeField] private Animator particle;

    public PlayerMouvementData Data; // Make a reference to the PlayerMouvementData script which holds most of the variables and logic

    public bool _isGrounded = false;
    public Transform GroundCheck;
    public LayerMask GroundLayer;

    public bool _isTouchingWall = false;
    public Transform WallCheck;
    public LayerMask WallLayer;

    public Transform TopEdgeCheck;
    public Transform BottomEdgeCheck;

    private float accelerationTimer;

    private float coyoteJumpTimer;
    private float bufferJumpTimer;
    private bool _canBufferJump;

    private bool _canDash;
    private bool _isFrozen;
    private float frozenTimer;
    public TrailRenderer tr;

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

    private float GripCooldown;
    float downGripSlipAcc;

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
            GripCooldown -= Time.deltaTime;

            downGripSlipAcc += (Time.deltaTime * 3f);

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

            Slide();

            WallJump();

            if (_isGrounded)
            {
                //if (_canLand)
                //{
                //    particle.SetBool("isLanding", true);

                //}
                coyoteJumpTimer = Data.CoyoteJumpTime;
                _canDash = true;
                _isSliding = false;
                animator.SetBool("isStable", false);
                animator.SetBool("isJumping", false);
                animator.SetBool("isFalling", false);
                animator.SetBool("isOnGround", true);
                //_canLand = false;
            }
            if (!_isGrounded)
            {
                animator.SetBool("isOnGround", false);
                //particle.SetBool("isLanding", false);
                //_canLand = true;

            }

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                accelerationTimer = Data.ResetAcceleration;
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                accelerationTimer = Data.ResetAcceleration;

            if (_canDash)
                animator.SetBool("hasDash", true);
            else
                animator.SetBool("hasDash", false);

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
        WallGrab(downGripSlipAcc);

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
        tr.emitting = true;
        frozenTimer = Data.DashDuration;
        _canDash = false;
        _isFrozen = true;
        rb.velocity = new Vector2(horizontalInput, verticalInput).normalized * Data.DashForce;
        if (horizontalInput == 0f && verticalInput == 0f)
            rb.velocity = new Vector2(Dir.x * Data.DashForce, 0f);
        if (_isTouchingWall)
            _isFrozen = false;
    }

    void Slide()
    {
        if (_isTouchingWall && !_isGrounded && horizontalInput != 0 && rb.velocity.y < 0f && !_isGrabing)
        {
            accelerationTimer = Data.ResetAcceleration;
            _isSliding = true;
            float slideSpeed = -Data.SlideSpeed * slideAcceleration;
            rb.velocity = new Vector2(0f, slideSpeed);
        }
        else
        {
            slideAcceleration = 0f;
            _isSliding = false;
        }
    }

    void WallJump()
    {
        if (_isSliding)
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

        if (wallJumpCounter < 0f)
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

    void WallGrab(float downAcc)
    {
        if (_isTouchingWall && Input.GetKey(KeyCode.Mouse0) && GripCooldown < 0f)
        {
            

            _isGrabing = true;
            _isSliding = false;
            rb.gravityScale = 0f;
            if (Input.GetKey(KeyCode.W) && _isTouchingWall)
                rb.velocity = new Vector2(0f, Data.ClimbSpeed);
            else if (Input.GetKey(KeyCode.S) && _isTouchingWall)
                rb.velocity = new Vector2(0f, -Data.ClimbSpeed * downAcc);
            else
                rb.velocity = Vector2.zero;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.AddForce(Vector2.up * Data.JumpForce, ForceMode2D.Impulse);
                GripCooldown = 0.4f;
                _isGrabing = false;
            }

            bool isOnEdge = Physics2D.OverlapCapsule(TopEdgeCheck.position, new Vector2(1.34f, 0.13f), CapsuleDirection2D.Horizontal, 0, GroundLayer);
            if (!isOnEdge && Input.GetKey(KeyCode.W))
            {
                rb.gravityScale = Data.gravityScale;
                rb.velocity = new Vector2(0f, Data.JumpForce);
                GripCooldown = 0.8f;
                _isGrabing = false;
            }

            if (Input.GetKeyDown(KeyCode.S))
                downGripSlipAcc = 0.85f;

        }
        else if (_isTouchingWall && Input.GetKey(KeyCode.Mouse0) && GripCooldown > 0f)
        {
            _isSliding = true;
        }
        if (_isTouchingWall && Input.GetKeyUp(KeyCode.Mouse0))
            _isGrabing = false;
        if(_isGrounded)
            _isGrabing = false;
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