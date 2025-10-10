using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneChange : MonoBehaviour
{
    [SerializeField] bool goNextLevel;
    [SerializeField] string levelName;

    AudioManager AudioManager;

    private void Awake()
    {
        AudioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private void  OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AudioManager.PlaySFX(AudioManager.arrow);

            if (goNextLevel)
            {
                SceneController.instance.NextLevel();
            }
            //else
            //{
            //    SceneController.instance.LoadScene(levelName);
            //}
        }
    }
}
