using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMouvement : MonoBehaviour
{

    public PlayerMouvementData Data; // Make a reference to the PlayerMouvementData script which holds most of the variables and logic

    public Transform GroundCheck;
    public LayerMask GroundLayer;
    public bool _isGrounded;

    public Transform WallCheck;
    public LayerMask WallLayer;
    public bool _isTouchingWall;

    public bool _isWalking = false;
    public bool _isJumpingUp = false;
    public bool _isJumpingDown = false;

    public bool _isJumpCut; // True if the player releases the jump or if he starts falling
    float JumpCutTimer; // Time since the JumpCut was perfomed ; used for accelerating the player downward more and more as he falls
    float TimeTilLastJump = 1f; //
    float CoyoteJumpTimer;

    float DecelerationTimer;
    float AccelerationTimer; // Time since the player starts walking ; used for accelerating the player more and more as he walk forward
    bool ResetAccelerationTimer; // Used to reset the player's AccelerationTimer when he stops walking

    public float MoveRestrainTimer;

    public bool _canWallJump = false;
    public bool _isWallJumping;
    public float WallJumpRestrainTimer; // make the player unalble to move after wall jumping
    float CoyoteWallJumpTimer; // if jump before landing on wall still jump
    float WallJumpDirection; // the direction of the wall
    private bool FindWallDir; // Detects if close to a wall, if so get the direction of the wall

    Ray ray;
    public float maxDistance = 10;
    public LayerMask layerToDetect;

    float horizontalInput; // Is 0 when the player isn't moving and is 1 or -1 when moving

    public Rigidbody2D rb;

    public Vector2 Dir;

    void Update()
    {

        Rotate();
        //CheckForInteractables();

        AccelerationTimer += Time.deltaTime;
        DecelerationTimer -= (Time.deltaTime * 10);

        JumpCutTimer += Time.deltaTime;
        TimeTilLastJump += Time.deltaTime;
        CoyoteJumpTimer -= Time.deltaTime;

        WallJumpRestrainTimer -= Time.deltaTime; // Time the player will be restrained
        CoyoteWallJumpTimer -= Time.deltaTime;

        MoveRestrainTimer -= Time.deltaTime;

        if (MoveRestrainTimer < 0)
        {

            // Always sets the rigidbody2D's gravity to our variable gravityScale
            rb.gravityScale = Data.gravityScale;

            horizontalInput = Input.GetAxisRaw("Horizontal");

            if (horizontalInput == 1)
                Dir = new Vector2(1, 0);

            if (horizontalInput == -1)
                Dir = new Vector2(-1, 0);

            // Can only walk if is not touching the wall and for a little while after wall jumping
            if (horizontalInput != 0 && !_isTouchingWall && WallJumpRestrainTimer < 0)
                Walk();

            if (Input.GetKeyDown(KeyCode.Space) && CoyoteJumpTimer > 0 && WallJumpRestrainTimer < 0 || Input.GetKeyDown(KeyCode.W) && CoyoteJumpTimer > 0 && WallJumpRestrainTimer < 0)
                Jump();

            // Call BufferJump if jumps while in the air
            if (Input.GetKeyDown(KeyCode.Space) && !_isGrounded || Input.GetKeyDown(KeyCode.W) && !_isGrounded)
                BufferJump();

            // Can only cut the jump is he is in the upwards movement of the jump
            if (Input.GetKeyUp(KeyCode.Space) && _isJumpCut && WallJumpRestrainTimer < 0 || Input.GetKeyUp(KeyCode.W) && _isJumpCut && WallJumpRestrainTimer < 0)
                JumpCut();

            if (_canWallJump)
            {

                // Can only wall jump if on the wall and not on the ground or if CoyoteWallJumpTimer is positive
                if (Input.GetKeyDown(KeyCode.Space) && _isTouchingWall && !_isGrounded || Input.GetKeyDown(KeyCode.W) && _isTouchingWall && !_isGrounded
                    || Input.GetKeyDown(KeyCode.Space) && CoyoteWallJumpTimer > 0 && !_isGrounded || Input.GetKeyDown(KeyCode.W) && CoyoteWallJumpTimer > 0 && !_isGrounded)
                    WallJump();
            }

            if (Input.GetKeyDown(KeyCode.P))
                _canWallJump = true;

        }


        // Creates a capsule under the player that returns true if touching the ground
        _isGrounded = Physics2D.OverlapCapsule(GroundCheck.position, new Vector2(0.33f, 0.33f), CapsuleDirection2D.Horizontal, 0, GroundLayer);

        // Creates a capsule on the side of the player that returns true if touching a wall
        _isTouchingWall = Physics2D.OverlapCapsule(WallCheck.position, new Vector2(0.5f, 0.33f), CapsuleDirection2D.Vertical, 0, WallLayer);

        // Detect a wall in front of player to then determine the walls direction ; I don't fucking know why the capsule didn't work out
        FindWallDir = Physics2D.Raycast(transform.position, Dir, 1, WallLayer);


        // Always resets the CoyoteJumpTimer when on the ground
        if (_isGrounded)
        {
            CoyoteJumpTimer = Data.CoyoteJumpTime;
        }

        // Will jump if on the ground and if the TimeTilLastJump is smaller then the BufferJumpTime set
        if (_isGrounded && TimeTilLastJump < Data.BufferJumpTime && MoveRestrainTimer < 0)
        {
            Jump();
        }

        // Can't cut the jump if already falling
        if (rb.velocity.y < 0)
        {
            _isJumpCut = false;

            _isJumpingUp = false;
            _isJumpingDown = true;
        }
        else
        {
            _isJumpCut = true;

            _isJumpingDown = false;
        }

        // Can reset the AccelerationTimer and decelerate only if not moving anymore or if not on wall;
        if (horizontalInput == 0 && !_isWallJumping && !_isTouchingWall && MoveRestrainTimer < 0)
        {
            ResetAccelerationTimer = false;
            Deceleration();

            _isWalking = false;
        }

        if (_canWallJump)
        {

            // Slide if touching a wall
            if (_isTouchingWall && !_isGrounded && !_isJumpCut && MoveRestrainTimer < 0)
                WallSlide();

            if (_isGrounded)
                _isWallJumping = false;

            if (_isTouchingWall)
            {
                CoyoteWallJumpTimer = Data.CoyoteWallJumpTime;

                _isWalking = false;
            }

            if (FindWallDir)
                WallJumpDirection = Dir.x;
        }
    }

    void Walk()
    {
        _isWalking = true;

        DecelerationTimer = 5;

        // Resets the AccelerationTimer if stopped moving
        if (!ResetAccelerationTimer)
        {
            // Put the AccelerationTimer to 1 so that the players doesn't start moving to slow
            AccelerationTimer = 1f;
            ResetAccelerationTimer = true;
        }
        // Calculate the TargetSpeed so that it increases as the AccelerationTimer increases
        // Apply the TargetSpeed but Clamp the Target speed between his max and his min
        float TargetSpeed = (horizontalInput * Data.MaxWalkSpeed);
        rb.velocity = new Vector2(Mathf.Clamp(TargetSpeed, -Data.MaxWalkSpeed, Data.MaxWalkSpeed), rb.velocity.y);
    }

    void Deceleration()
    {
        // Calculate the DecelerateSpeed so that it decreases as the DecelerationTimer decreases
        // Apply the DecelerateSpeed but Clamp it between his max and his min
        float DecelerateSpeed = Dir.x * Data.WalkDeceleration * DecelerationTimer;
        rb.velocity = new Vector2(Mathf.Clamp(DecelerateSpeed, -Data.MaxWalkSpeed, Data.MaxWalkSpeed), Mathf.Clamp(rb.velocity.y, Data.MaxFallSpeed, Data.JumpForce));

        // ensure that the player doesn't deccelerate into the negative
        if (Dir.x == 1 && DecelerateSpeed < 0f && !_isTouchingWall)
            rb.velocity = new Vector2(0f, rb.velocity.y);
        if (Dir.x == -1 && DecelerateSpeed > 0f && !_isTouchingWall)
            rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    void BufferJump()
    {
        TimeTilLastJump = 0f;
    }

    void Jump()
    {

        _isJumpingUp = true;

        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * Data.JumpForce, ForceMode2D.Impulse);
    }

    void JumpCut()
    {

        // Ensures that you cant use the coyote jump after jumping once
        // Reset the JumpCutTimer
        CoyoteJumpTimer = 0f;
        JumpCutTimer = 0.02f;

        // Calculate the downwards velocity of the player to give him a good air time
        // Apply the downwards velocity
        float JumpFall = -(Data.JumpForce / Data.gravityScale) * JumpCutTimer;
        rb.velocity = new Vector2(rb.velocity.x, JumpFall);
    }

    void WallSlide()
    {
        rb.velocity = new Vector2(0f, Data.SlideSpeed);
    }

    void WallJump()
    {
        // Set _isWallJumping as true
        _isWallJumping = true;

        // Reset WallJumpRestrainTimer
        WallJumpRestrainTimer = Data.RestrainedMoveTime;

        // Always put the veloty to zero before applying velocity
        rb.velocity = Vector2.zero;

        Vector2 WallJumpForce = new Vector2(Data.WallJumpForce, Data.WallJumpForce);

        // Jump when moving
        if (horizontalInput != 0)
            rb.velocity = new Vector2(WallJumpForce.x * -WallJumpDirection, 2 * WallJumpForce.y);

        //  Jump when not moving
        if (horizontalInput == 0)
            rb.velocity = new Vector2(WallJumpForce.x * -WallJumpDirection, 2 * WallJumpForce.y);
    }

    void Rotate()
    {

        if (Dir.x == 1)
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        if (Dir.x == -1)
            transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
    }
}
