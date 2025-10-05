using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private int groundLayer = 6;
    private int invisibleLayer = 7;

    public bool _isStepedOn = false;
    public float timeUntilBreaks = 1.5f;
    public float timeUntilRespawn = 5f;
    private float breakTimer;
    private float respawnTimer;

    public Collider2D collider;
    public SpriteRenderer spriteRenderer;


    void Update()
    {
        breakTimer -= Time.deltaTime;
        respawnTimer -= Time.deltaTime;
        
        if (_isStepedOn)
        {
            animator.SetBool("isBreaking", true);
            if (breakTimer < 0f)
            {
                gameObject.layer = invisibleLayer;
                collider.enabled = false;
                spriteRenderer.enabled = false;
                respawnTimer = timeUntilRespawn;
                _isStepedOn = false;
            }
        }

        if(!_isStepedOn && respawnTimer < 0f)
        {
            animator.SetBool("isBreaking", false);
            gameObject.layer = groundLayer;
            collider.enabled = true;
            spriteRenderer.enabled = true;
        }
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!_isStepedOn)
            {
                breakTimer = timeUntilBreaks;
                _isStepedOn = true;
            }
        }
    }
}
