using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    public Vector2 lastSavedPos;

    public bool _isDead;

    private void Update()
    {
        if (_isDead)
        {
            Respawn(lastSavedPos);
            _isDead = false;
        }
    }

    private void Respawn(Vector2 pos)
    {
        transform.position = lastSavedPos;
    }
}
