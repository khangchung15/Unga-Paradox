using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Default / Fallback")]
    public AudioClip defaultTrack;
    [Range(0f, 1f)] public float masterVolume = 0.8f;
    public float defaultFade = 1f;

    AudioSource srcA, srcB;       // for crossfades
    bool usingA = true;
    Coroutine fadeCo;

    // Keep a simple stack to remember what was playing before
    private Stack<AudioClip> trackStack = new Stack<AudioClip>();

    void Awake()
    {
        srcA = gameObject.AddComponent<AudioSource>();
        srcB = gameObject.AddComponent<AudioSource>();
        foreach (var s in new[] { srcA, srcB })
        {
            s.loop = true;
            s.playOnAwake = false;
            s.volume = 0f;
        }

        if (defaultTrack != null)
        {
            Play(defaultTrack, defaultFade, pushToStack:false); // start scene music
        }
    }

    AudioSource Active => usingA ? srcA : srcB;
    AudioSource Idle   => usingA ? srcB : srcA;

    public void Play(AudioClip clip, float fadeDuration = -1f, bool pushToStack = true)
    {
        if (clip == null) return;
        if (fadeDuration < 0f) fadeDuration = defaultFade;

        // avoid restarting same clip
        if (Active.clip == clip && Active.isPlaying) return;

        if (pushToStack && Active.clip != null)
            trackStack.Push(Active.clip);

        Idle.clip = clip;
        Idle.volume = 0f;
        Idle.Play();

        // crossfade
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(Crossfade(Active, Idle, masterVolume, fadeDuration));
        usingA = !usingA;
    }

    public void Revert(float fadeDuration = -1f)
    {
        if (fadeDuration < 0f) fadeDuration = defaultFade;

        AudioClip backTo = trackStack.Count > 0 ? trackStack.Pop() : defaultTrack;
        if (backTo == null) return;

        if (Active.clip == backTo && Active.isPlaying) return;

        Idle.clip = backTo;
        Idle.volume = 0f;
        Idle.Play();

        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(Crossfade(Active, Idle, masterVolume, fadeDuration));
        usingA = !usingA;
    }

    IEnumerator Crossfade(AudioSource from, AudioSource to, float targetVol, float dur)
    {
        float t = 0f;
        float fromStart = from.volume;
        float toStart = to.volume;

        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            from.volume = Mathf.Lerp(fromStart, 0f, k);
            to.volume   = Mathf.Lerp(toStart, targetVol, k);
            yield return null;
        }

        from.volume = 0f;
        from.Stop();
        to.volume = targetVol;
    }
}
