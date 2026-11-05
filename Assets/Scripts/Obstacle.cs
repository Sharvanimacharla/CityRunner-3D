using System.Collections.Generic;
using UnityEngine;

// Simple box description used for hit-testing against the player.
public class Obstacle : MonoBehaviour
{
    public static readonly List<Obstacle> All = new List<Obstacle>();

    public float HalfX = 1f;
    public float HalfZ = 1f;
    public float Bottom = 0f;
    public float Top = 1f;
    public float Vel = 0f;   // > 0 : drives toward the player once it gets close

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Update()
    {
        if (Vel <= 0f) return;
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Playing) return;
        if (transform.position.z - gm.Distance < 70f)
            transform.position += Vector3.back * Vel * Mathf.Min(Time.deltaTime, 0.05f);
    }
}
