using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerMouvementData")] //Create a new playerData object by right clicking in the Project Menu then Create/Player/Player Data and drag onto the player
public class PlayerMouvementData : ScriptableObject
{
    [Header("Gravity")]
    public float gravityScale; // The gravity of the player's rigidbody2D

    [Space(20)]

    [Header("Walk")]
    public float MaxWalkSpeed; // Max walk speed
    [Range(0.01f, 5f)] public float WalkAcceleration; // The rate of the acceleration
    [Range(0.01f, 5f)] public float WalkDeceleration;

    [Space(20)]

    [Header("Jump")]
    public float JumpHeight; // The height of the normal jump
    public float JumpTimeToSummit; // The time to reach the top of the jump : the speed of the jump


    [Space(5)]

    public float MaxFallSpeed; // The maximum downards velocity
    public float JumpForce; // The overall strenght of the jump

    [Space(20)]

    [Header("Wall Jump")]
    [Range(0f, -100f)] public float SlideSpeed;
    public float WallJumpForce;
    public float RestrainedMoveTime;

    [Space(20)]

    [Header("Assist")]
    public float BufferJumpTime; // Jump Coyote Time
    public float CoyoteJumpTime; // Fall Jump Coyote Time
    public float CoyoteWallJumpTime;

    private void OnValidate()
    {

        // Calculate gravity strength using the formula (gravityScale = 2 * jumpHeight / JumpTimeToSummit^2) 
        // Calculate the force of the jump
        gravityScale = (2 * JumpHeight) / (JumpTimeToSummit * JumpTimeToSummit);
        JumpForce = gravityScale * JumpTimeToSummit;

        // Limit the WalkAcceleration & WalkDeceleration
        WalkAcceleration = Mathf.Clamp(WalkAcceleration, 0.01f, MaxWalkSpeed);
        WalkDeceleration = Mathf.Clamp(WalkDeceleration, 0.01f, MaxWalkSpeed);
    }
}
