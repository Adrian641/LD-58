using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public PlayerHandler handler;

    AudioManager AudioManager;

    private void Awake()
    {
        AudioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private void Update()
    {
       handler = GameObject.FindObjectOfType<PlayerHandler>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AudioManager.PlaySFX(AudioManager.checkpoint);
            animator.SetBool("isTriggered", true);
            handler.lastSavedPos = transform.position;
        }
    }
}

