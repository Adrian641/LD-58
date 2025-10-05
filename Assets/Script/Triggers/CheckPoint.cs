using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public PlayerHandler handler;

    private void Update()
    {
       handler = GameObject.FindObjectOfType<PlayerHandler>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            handler.lastSavedPos = transform.position;
        }
    }
}

