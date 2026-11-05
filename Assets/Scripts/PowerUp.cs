using System.Collections.Generic;
using UnityEngine;

public enum PowerType { Magnet, Jetpack, Shield, Multiplier }

public class PowerUp : MonoBehaviour
{
    public static readonly List<PowerUp> All = new List<PowerUp>();
    public PowerType Type;
    float baseY;

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }
    void Start() { baseY = transform.localPosition.y; }

    void Update()
    {
        transform.Rotate(0f, 120f * Time.deltaTime, 0f, Space.World);
        Vector3 lp = transform.localPosition;
        lp.y = baseY + Mathf.Sin(Time.time * 3f) * 0.15f;
        transform.localPosition = lp;
    }

    public void Collect() { gameObject.SetActive(false); }
}
