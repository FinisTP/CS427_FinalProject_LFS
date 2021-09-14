using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footstep : MonoBehaviour
{
    public AudioClip jumpClip;
    public AudioClip landClip;
    public AudioClip[] clips;
    private AudioSource audioSource;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        print("Footstep here!");
    }

    public void Step(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.75f)
        {
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            audioSource.volume = 1f;
            audioSource.PlayOneShot(clip);
        }
        else if (animationEvent.animatorClipInfo.weight > 0.25f)
        {
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            audioSource.volume = 0.5f;
            audioSource.PlayOneShot(clip);
        }
    }

    public void Jump(AnimationEvent animationEvent)
    {
        audioSource.volume = 1f;
        audioSource.PlayOneShot(jumpClip);
    }

    public void Land(AnimationEvent animationEvent)
    {
        audioSource.volume = 0.5f;
        audioSource.PlayOneShot(landClip);
    }
}
