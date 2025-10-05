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
    [Range(1f, 5f)] public float AccelerationSpeed; // The rate of the acceleration
    [Range(1f, 5f)] public float MaxAcceleration;
    public float ResetAcceleration;

    [Space(20)]

    [Header("Jump")]
    public float JumpHeight; // The height of the normal jump
    public float JumpTimeToSummit; // The time to reach the top of the jump : the speed of the jump
    public float FallSpeed;
    public float MinJumpHeight;

    [Space(5)]

    [Range(1f, 10f)] public float restirctAirMouvement;

    [Space(5)]

    public float MaxFallSpeed; // The maximum downards velocity
    public float JumpForce; // The overall strenght of the jump

    [Space(20)]

    [Header("Wall Jump")]
    [Range(0f, -100f)] public float SlideSpeed;
    public Vector2 WallJumpForce;
    public float WallJumpDistance;

    [Space(20)]

    [Header("Dash")]
    public float DashForce;
    public float DashDuration;

    [Space(20)]

    [Header("Climb")]
    public float ClimbSpeed;
    [Range(0, 20)] public int Stamina;

    [Header("Assist")]
    public float BufferJumpTime; // Jump Coyote Time
    public float CoyoteJumpTime; // Fall Jump Coyote Time

    private void OnValidate()
    {

        // Calculate gravity strength using the formula (gravityScale = 2 * jumpHeight / JumpTimeToSummit^2) 
        // Calculate the force of the jump
        gravityScale = (2 * JumpHeight) / (JumpTimeToSummit * JumpTimeToSummit);
        JumpForce = gravityScale * JumpTimeToSummit;
    }
}
