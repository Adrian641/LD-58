using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZones : MonoBehaviour
{
    AudioManager AudioManager;
    private void Awake()
    {
        AudioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    public PlayerHandler handler;

    private void Update()
    {
        handler = GameObject.FindObjectOfType<PlayerHandler>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            handler._isDead = true;
            AudioManager.PlaySFX(AudioManager.death);
        }
    }
}
