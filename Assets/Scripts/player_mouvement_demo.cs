using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_mouvement_demo : MonoBehaviour
{

    //public MousieMouvementData Data; // Make a reference to the MousieMouvementData script which holds most of the variables and logic
    //public MousieDamageController mousieDamage;

    #region Dont remember how to do data script :(   should all be modifiable trough the inspector

    public float Data_gravityScale = 3.75f;
    public float Data_MaxWalkSpeed = 8f;
    public float Data_WalkAcceleration = 4f;
    public float Data_WalkDeceleration = 0.04f;
    public float Data_JumpHeight = 30f;
    public float Data_JumpTimeToSummit = 4f;
    public float Data_MaxFallSpeed = -30f;
    public float Data_JumpForce = 15f;
    public float Data_SlideSpeed = -1.5f;
    public float Data_WallJumpForce = 5f;
    public float Data_RestrainedMoveTime = 0.125f;
    public float Data_BufferJumpTime = 0.1f;
    public float Data_CoyoteJumpTime = 0.125f;
    public float Data_CoyoteWallJumpTime = 0.1f;

    #endregion

    #region Collision Check Variables

    public Transform GroundCheck;
    public LayerMask GroundLayer;
    public bool _isGrounded;

    public Transform WallCheck;
    public LayerMask WallLayer;
    public bool _isTouchingWall;

    #endregion

    #region Animation Booleans

    public bool _isWalking = false;
    public bool _isJumpingUp = false;
    public bool _isJumpingDown = false;

    #endregion

    #region Jump Variables

    public bool _isJumpCut; // True if the player releases the jump or if he starts falling
    float JumpCutTimer; // Time since the JumpCut was perfomed ; used for accelerating the player downward more and more as he falls
    float TimeTilLastJump = 1f; //
    float CoyoteJumpTimer;

    #endregion

    #region Walk Variables

    float DecelerationTimer;
    float AccelerationTimer; // Time since the player starts walking ; used for accelerating the player more and more as he walk forward
    bool ResetAccelerationTimer; // Used to reset the player's AccelerationTimer when he stops walking

    public float MoveRestrainTimer;

    #endregion

    #region Wall Jump Variables

    public bool _canWallJump = true;
    public bool _isWallJumping;
    public float WallJumpRestrainTimer; // make the player unalble to move after wall jumping
    float CoyoteWallJumpTimer; // if jump before landing on wall still jump
    float WallJumpDirection; // the direction of the wall
    private bool FindWallDir; // Detects if close to a wall, if so get the direction of the wall

    #endregion

    #region Interacting Variables

    Ray ray;
    public float maxDistance = 10;
    public LayerMask layerToDetect;

    #endregion

    float horizontalInput; // Is 0 when the player isn't moving and is 1 or -1 when moving

    public Rigidbody2D rb;

    public Vector2 Dir;

    void Update()
    {

        Rotate();
        //CheckForInteractables();

        #region Timers

        AccelerationTimer += Time.deltaTime;
        DecelerationTimer -= (Time.deltaTime * 10);

        JumpCutTimer += Time.deltaTime;
        TimeTilLastJump += Time.deltaTime;
        CoyoteJumpTimer -= Time.deltaTime;

        WallJumpRestrainTimer = -1; // Time the player will be restrained
        CoyoteWallJumpTimer -= Time.deltaTime;

        MoveRestrainTimer = -1;

        #endregion

        #region Input Handler

        if (MoveRestrainTimer < 0)
        {

            // Always sets the rigidbody2D's gravity to our variable gravityScale
            rb.gravityScale = Data_gravityScale;

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
                // Can only wall jump if on the wall and not on the ground or if CoyoteWallJumpTimer is positive
                if (Input.GetKeyDown(KeyCode.Space) && _isTouchingWall && !_isGrounded || Input.GetKeyDown(KeyCode.W) && _isTouchingWall && !_isGrounded
                    || Input.GetKeyDown(KeyCode.Space) && CoyoteWallJumpTimer > 0 && !_isGrounded || Input.GetKeyDown(KeyCode.W) && CoyoteWallJumpTimer > 0 && !_isGrounded)
                    WallJump();

            //if (Input.GetKeyDown(KeyCode.P))
            //{
            //    _canWallJump = true;
            //}

        }

        #endregion

        #region Collision Checks

        // Creates a capsule under the player that returns true if touching the ground
        _isGrounded = Physics2D.OverlapCapsule(GroundCheck.position, new Vector2(0.33f, 0.33f), CapsuleDirection2D.Horizontal, 0, GroundLayer);

        // Creates a capsule on the side of the player that returns true if touching a wall
        _isTouchingWall = Physics2D.OverlapCapsule(WallCheck.position, new Vector2(0.5f, 1f), CapsuleDirection2D.Vertical, 0, WallLayer);

        // Detect a wall in front of player to then determine the walls direction ; I don't fucking know why the capsule didn't work out
        FindWallDir = Physics2D.Raycast(transform.position, Dir, 1, WallLayer);



        #endregion

        #region Jump Functions

        // Always resets the CoyoteJumpTimer when on the ground
        if (_isGrounded)
        {
            CoyoteJumpTimer = Data_CoyoteJumpTime;
        }

        // Will jump if on the ground and if the TimeTilLastJump is smaller then the BufferJumpTime set
        if (_isGrounded && TimeTilLastJump < Data_BufferJumpTime && MoveRestrainTimer < 0)
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

        #endregion

        #region Walk Functions

        // Can reset the AccelerationTimer and decelerate only if not moving anymore or if not on wall;
        if (horizontalInput == 0 && !_isWallJumping && !_isTouchingWall && MoveRestrainTimer < 0)
        {
            ResetAccelerationTimer = false;
            Deceleration();

            _isWalking = false;
        }

        #endregion

        #region Wall Jump Functions

        if (_canWallJump)
        {

            // Slide if touching a wall
            if (_isTouchingWall && !_isGrounded && !_isJumpCut && MoveRestrainTimer < 0)
            {
                WallSlide();
            }

            if (_isGrounded)
            {
                _isWallJumping = false;
            }

            if (_isTouchingWall)
            {
                CoyoteWallJumpTimer = Data_CoyoteWallJumpTime;

                _isWalking = false;
            }

            if (FindWallDir)
            {
                WallJumpDirection = Dir.x;
            }
        }


        #endregion
    }

    #region Walk

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
        float TargetSpeed = (horizontalInput * Data_MaxWalkSpeed);
        rb.velocity = new Vector2(Mathf.Clamp(TargetSpeed, -Data_MaxWalkSpeed, Data_MaxWalkSpeed), rb.velocity.y);
    }

    void Deceleration()
    {
        // Calculate the DecelerateSpeed so that it decreases as the DecelerationTimer decreases
        // Apply the DecelerateSpeed but Clamp it between his max and his min
        float DecelerateSpeed = Dir.x * Data_WalkDeceleration * DecelerationTimer;
        rb.velocity = new Vector2(Mathf.Clamp(DecelerateSpeed, -Data_MaxWalkSpeed, Data_MaxWalkSpeed), Mathf.Clamp(rb.velocity.y, Data_MaxFallSpeed, Data_JumpForce));

        // ensure that the player doesn't deccelerate into the negative
        if (Dir.x == 1 && DecelerateSpeed < 0f && !_isTouchingWall)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        if (Dir.x == -1 && DecelerateSpeed > 0f && !_isTouchingWall)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    #endregion

    #region Jump

    void BufferJump()
    {
        TimeTilLastJump = 0f;
    }

    void Jump()
    {

        _isJumpingUp = true;

        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * Data_JumpForce, ForceMode2D.Impulse);
    }

    void JumpCut()
    {

        // Ensures that you cant use the coyote jump after jumping once
        // Reset the JumpCutTimer
        CoyoteJumpTimer = 0f;
        JumpCutTimer = 0.02f;

        // Calculate the downwards velocity of the player to give him a good air time
        // Apply the downwards velocity
        float JumpFall = -(Data_JumpForce / Data_gravityScale) * JumpCutTimer;
        rb.velocity = new Vector2(rb.velocity.x, JumpFall);
    }

    #endregion

    #region Wall Jump

    void WallSlide()
    {
        rb.velocity = new Vector2(0f, Data_SlideSpeed);
    }

    void WallJump()
    {
        // Set _isWallJumping as true
        _isWallJumping = true;

        // Reset WallJumpRestrainTimer
        WallJumpRestrainTimer = Data_RestrainedMoveTime;

        // Always put the veloty to zero before applying velocity
        rb.velocity = Vector2.zero;

        Vector2 WallJumpForce = new Vector2(Data_WallJumpForce, Data_WallJumpForce);

        // Jump when moving
        if (horizontalInput != 0)
            rb.velocity = new Vector2(WallJumpForce.x * -WallJumpDirection, 2 * WallJumpForce.y);

        //  Jump when not moving
        if (horizontalInput == 0)
            rb.velocity = new Vector2(WallJumpForce.x * -WallJumpDirection, 2 * WallJumpForce.y);
    }

    #endregion

    #region Rotate

    void Rotate()
    {

        if (Dir.x == 1)
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        if (Dir.x == -1)
            transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
    }

    #endregion

    #region Interact

    /*void CheckForInteractables() {
        ray = new Ray2D(transform.position, Dir);

        if (Physics2D.Raycast(transform.position, Dir, layerToDetect) {
            Debug.Log(hit.collider.gameObject.name);
        }
    }*/

    #endregion

}
