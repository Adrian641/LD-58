using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public PlayerHandler handler;

    private void Update()
    {
       handler = GameObject.FindObjectOfType<PlayerHandler>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            animator.SetBool("isTriggered", true);
            handler.lastSavedPos = transform.position;
        }
    }
}

