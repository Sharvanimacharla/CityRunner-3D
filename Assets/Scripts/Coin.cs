using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public static readonly List<Coin> All = new List<Coin>();

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Update()
    {
        transform.Rotate(0f, 220f * Time.deltaTime, 0f, Space.World);
    }

    public void Collect()
    {
        gameObject.SetActive(false);
    }
}
