using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    public AudioClip background;
    public AudioClip step;
    public AudioClip wallClimb;
    public AudioClip dash;
    public AudioClip jump;
    public AudioClip death;
    public AudioClip checkpoint;
}
