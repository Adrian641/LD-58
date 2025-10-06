using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("---- Audio Source ----")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("---- Audio Clip ----")]
    public AudioClip background;
    public AudioClip step;
    public AudioClip jump;
    public AudioClip dash;
    public AudioClip wallClimb;
    public AudioClip death;
    public AudioClip checkpoint;
    

}
