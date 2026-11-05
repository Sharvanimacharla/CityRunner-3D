using System.Collections.Generic;
using UnityEngine;

// Tiny procedural sound effects (no audio files needed).
public static class SoundFx
{
    static AudioSource source;
    static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { source = null; clips.Clear(); }

    public static void Init(GameObject host)
    {
        source = host.AddComponent<AudioSource>();
        source.playOnAwake = false;
        clips.Clear();
        clips["coin"] = Tone("coin", 1200f, 1800f, 0.12f, 0.30f, false);
        clips["jump"] = Tone("jump", 280f, 600f, 0.18f, 0.30f, false);
        clips["slide"] = Tone("slide", 0f, 0f, 0.18f, 0.12f, true);
        clips["power"] = Tone("power", 500f, 1100f, 0.40f, 0.35f, false);
        clips["stumble"] = Tone("stumble", 150f, 60f, 0.30f, 0.60f, false);
        clips["crash"] = Tone("crash", 0f, 0f, 0.60f, 0.55f, true);
    }

    static AudioClip Tone(string name, float f0, float f1, float duration, float volume, bool noise)
    {
        int rate = 44100;
        int n = Mathf.Max(1, (int)(rate * duration));
        float[] data = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n;
            float f = Mathf.Lerp(f0, f1, t);
            phase += 2f * Mathf.PI * f / rate;
            float env = (1f - t) * (1f - t);
            float sample = noise ? Random.Range(-1f, 1f) : Mathf.Sin(phase);
            data[i] = sample * env * volume;
        }
        AudioClip clip = AudioClip.Create(name, n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public static void Play(string name)
    {
        AudioClip clip;
        if (source != null && clips.TryGetValue(name, out clip)) source.PlayOneShot(clip);
    }
}
