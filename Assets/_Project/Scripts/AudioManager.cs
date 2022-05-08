using UnityEngine;
//using Lofelt.NiceVibrations;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; protected set; }
    public AudioSource source;
    public AudioClip click;
    public AudioClip gemCollect;
    [HideInInspector] public bool isHeavyVibration;

    void Awake()
    {
        Instance = this;
    }

    public void Vibrate()
    {
        var freq = 0.4f;
        if (isHeavyVibration)
        {
            isHeavyVibration = false;
            freq = 0.9f;
        }
        //HapticPatterns.PlayEmphasis(freq, 0.0f); #8AC8FF
    }

    public void PlaySound(AudioClip clip)
    {
        source.clip = clip;
        source.Play();
    }
}