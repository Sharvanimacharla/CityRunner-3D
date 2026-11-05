using System.Collections.Generic;
using UnityEngine;

// Endlessly spawns city road segments (road, buildings, lamps, traffic, coins, power-ups) ahead of the player.
public class SegmentGenerator : MonoBehaviour
{
    public const float SegmentLength = 30f;
    public Transform target;

    class Segment { public GameObject root; public float endZ; }
    readonly List<Segment> segments = new List<Segment>();
    float nextZ;
    int index;

    static readonly Color[] BuildingColors =
    {
        // urban palette: brick, blue glass, sandstone, concrete, steel, off-white, slate
        new Color(0.62f, 0.38f, 0.33f), new Color(0.42f, 0.55f, 0.70f), new Color(0.74f, 0.68f, 0.56f),
        new Color(0.55f, 0.57f, 0.60f), new Color(0.36f, 0.42f, 0.52f), new Color(0.82f, 0.82f, 0.84f),
        new Color(0.30f, 0.33f, 0.40f)
    };

    public void Rebuild()
    {
        foreach (Segment s in segments)
        {
            if (s.root != null) { s.root.SetActive(false); Destroy(s.root); }
        }
        segments.Clear();
        nextZ = -SegmentLength;
        index = 0;
        Fill();
    }

    void Update()
    {
        if (target == null) return;
        Fill();
        while (segments.Count > 0 && segments[0].endZ < target.position.z - 40f)
        {
            segments[0].root.SetActive(false);
            Destroy(segments[0].root);
            segments.RemoveAt(0);
        }
    }

    void Fill()
    {
        if (target == null) return;
        // stop spawning before the skyline wall (205 ahead) so road ends/pop-in stay hidden behind it
        while (nextZ < target.position.z + 165f) Spawn();
    }

    void Spawn()
    {
        GameObject root = new GameObject("Segment_" + index);
        root.transform.SetParent(transform, false);
        root.transform.position = new Vector3(0f, 0f, nextZ);
        BuildRoad(root.transform);
        BuildSides(root.transform);
        if (index >= 2) BuildTraffic(root.transform);
        segments.Add(new Segment { root = root, endZ = nextZ + SegmentLength });
        nextZ += SegmentLength;
        index++;
    }

    void BuildRoad(Transform p)
    {
        float L = SegmentLength;
        float lw = PlayerMovement.LaneWidth;
        float roadW = lw * 3f + 1.2f;
        Color white = new Color(0.95f, 0.95f, 0.95f);

        ShapeFactory.Cube("Road", p, new Vector3(0f, -0.25f, L * 0.5f), new Vector3(roadW, 0.5f, L), new Color(0.20f, 0.20f, 0.23f));
        for (int k = 0; k < 5; k++)
        {
            float z = k * 6f + 3f;
            ShapeFactory.Cube("Dash", p, new Vector3(-lw * 0.5f, 0.015f, z), new Vector3(0.14f, 0.03f, 2.8f), white);
            ShapeFactory.Cube("Dash", p, new Vector3(lw * 0.5f, 0.015f, z), new Vector3(0.14f, 0.03f, 2.8f), white);
        }
        ShapeFactory.Cube("EdgeL", p, new Vector3(-(roadW * 0.5f - 0.3f), 0.015f, L * 0.5f), new Vector3(0.12f, 0.03f, L), new Color(1f, 0.85f, 0.2f));
        ShapeFactory.Cube("EdgeR", p, new Vector3((roadW * 0.5f - 0.3f), 0.015f, L * 0.5f), new Vector3(0.12f, 0.03f, L), new Color(1f, 0.85f, 0.2f));

        float sideX = roadW * 0.5f + 1.5f;
        ShapeFactory.Cube("SidewalkL", p, new Vector3(-sideX, -0.05f, L * 0.5f), new Vector3(3f, 0.3f, L), new Color(0.62f, 0.62f, 0.65f));
        ShapeFactory.Cube("SidewalkR", p, new Vector3(sideX, -0.05f, L * 0.5f), new Vector3(3f, 0.3f, L), new Color(0.62f, 0.62f, 0.65f));
        ShapeFactory.Cube("GroundL", p, new Vector3(-27.5f, -0.3f, L * 0.5f), new Vector3(40f, 0.5f, L), new Color(0.42f, 0.44f, 0.48f));
        ShapeFactory.Cube("GroundR", p, new Vector3(27.5f, -0.3f, L * 0.5f), new Vector3(40f, 0.5f, L), new Color(0.42f, 0.44f, 0.48f));
    }

    void BuildSides(Transform p)
    {
        for (int s = -1; s <= 1; s += 2)
        {
            for (int k = 0; k < 3; k++)
            {
                float z = k * 10f + 5f;
                float w = Random.Range(5f, 8f);
                float d = Random.Range(7f, 10f);
                float h = Random.Range(8f, 30f);
                float gap = Random.Range(0.5f, 2f);
                float faceX = s * (7.5f + gap);
                Color c = BuildingColors[Random.Range(0, BuildingColors.Length)];

                ShapeFactory.Cube("Building", p, new Vector3(faceX + s * d * 0.5f, h * 0.5f - 0.05f, z), new Vector3(d, h, w), c);
                ShapeFactory.Cube("Roof", p, new Vector3(faceX + s * d * 0.5f, h + 0.3f, z), new Vector3(d * 0.7f, 0.6f, w * 0.7f), c * 0.75f);

                for (float wy = 2.5f; wy < h - 1f; wy += 3f)
                {
                    Vector3 wp = new Vector3(faceX - s * 0.03f, wy, z);
                    Vector3 ws = new Vector3(0.06f, 0.9f, w * 0.8f);
                    if (Random.value < 0.35f)
                        ShapeFactory.Unlit(PrimitiveType.Cube, "WindowLit", p, wp, ws, new Color(1f, 0.92f, 0.55f));
                    else
                        ShapeFactory.Cube("Window", p, wp, ws, new Color(0.22f, 0.30f, 0.42f));
                }
            }
            for (int k = 0; k < 2; k++)
            {
                float z = k * 15f + 7.5f;
                float lx = s * 4.9f;
                ShapeFactory.Create(PrimitiveType.Cylinder, "LampPole", p, new Vector3(lx, 2.6f, z), new Vector3(0.12f, 2.6f, 0.12f), new Color(0.3f, 0.3f, 0.34f));
                ShapeFactory.Unlit(PrimitiveType.Cube, "LampHead", p, new Vector3(lx - s * 0.4f, 5.3f, z), new Vector3(0.9f, 0.15f, 0.35f), new Color(1f, 0.95f, 0.7f));
            }
        }
    }

    static int[] ShuffledLanes()
    {
        int[] a = { -1, 0, 1 };
        for (int i = 2; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int t = a[i]; a[i] = a[j]; a[j] = t;
        }
        return a;
    }

    void BuildTraffic(Transform p)
    {
        float difficulty = Mathf.Clamp01(index / 40f);
        float emptyChance = Mathf.Lerp(0.35f, 0.12f, difficulty);
        Row(p, 15f, emptyChance); // one row per segment = 30 units of recovery space
    }

    void Row(Transform p, float z, float emptyChance)
    {
        int[] lanes = ShuffledLanes();
        if (Random.value < emptyChance)
        {
            CoinLine(p, lanes[0], z, 1f);
        }
        else
        {
            float kind = Random.value;
            if (kind < 0.25f)
            {
                int n = Random.Range(1, 3);
                for (int i = 0; i < n; i++) Van(p, lanes[i], z);
                if (Random.value < 0.7f) CoinLine(p, lanes[n], z, 1f);
            }
            else if (kind < 0.45f)
            {
                int n = Random.Range(1, 4);
                for (int i = 0; i < n; i++) Barrier(p, lanes[i], z);
                CoinArc(p, lanes[0], z);
            }
            else if (kind < 0.65f)
            {
                int n = Random.Range(1, 4);
                for (int i = 0; i < n; i++) Gantry(p, lanes[i], z);
                CoinLine(p, lanes[0], z, 0.6f);
            }
            else
            {
                // oncoming taxis that drive toward you (jump over them or change lane)
                int n = Random.Range(1, 3);
                for (int i = 0; i < n; i++) Taxi(p, lanes[i], z);
                if (Random.value < 0.7f) CoinLine(p, lanes[n], z, 1f);
            }
        }

        if (index >= 3 && Random.value < 0.22f)
            MakePowerUp(p, lanes[Random.Range(0, 3)], z + 8f, (PowerType)Random.Range(0, 4));
    }

    Transform NewObstacle(Transform p, string name, int lane, float z, float halfX, float halfZ, float bottom, float top)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(p, false);
        go.transform.localPosition = new Vector3(lane * PlayerMovement.LaneWidth, 0f, z);
        Obstacle o = go.AddComponent<Obstacle>();
        o.HalfX = halfX; o.HalfZ = halfZ; o.Bottom = bottom; o.Top = top;
        return go.transform;
    }

    void Wheels(Transform v, float x, float[] zs)
    {
        for (int i = 0; i < zs.Length; i++)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                GameObject w = ShapeFactory.Create(PrimitiveType.Cylinder, "Wheel", v, new Vector3(s * x, 0.35f, zs[i]), new Vector3(0.7f, 0.12f, 0.7f), new Color(0.08f, 0.08f, 0.1f));
                w.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }
    }

    void Van(Transform p, int lane, float z)
    {
        Transform v = NewObstacle(p, "GreenVan", lane, z, 0.95f, 2.1f, 0f, 2.2f);
        Color body = new Color(0.20f, 0.75f, 0.35f);
        Color dark = new Color(0.14f, 0.55f, 0.26f);
        ShapeFactory.Cube("Cargo", v, new Vector3(0f, 1.25f, 0.6f), new Vector3(1.9f, 1.9f, 2.6f), body);
        ShapeFactory.Cube("Stripe", v, new Vector3(0f, 1.25f, 0.6f), new Vector3(1.94f, 0.3f, 2.64f), Color.white);
        ShapeFactory.Cube("Cab", v, new Vector3(0f, 0.9f, -1.45f), new Vector3(1.9f, 1.2f, 1.5f), dark);
        ShapeFactory.Cube("Windshield", v, new Vector3(0f, 1.2f, -2.21f), new Vector3(1.5f, 0.5f, 0.04f), new Color(0.15f, 0.25f, 0.4f));
        ShapeFactory.Unlit(PrimitiveType.Cube, "LightL", v, new Vector3(-0.65f, 0.65f, -2.21f), new Vector3(0.3f, 0.2f, 0.04f), new Color(1f, 0.95f, 0.5f));
        ShapeFactory.Unlit(PrimitiveType.Cube, "LightR", v, new Vector3(0.65f, 0.65f, -2.21f), new Vector3(0.3f, 0.2f, 0.04f), new Color(1f, 0.95f, 0.5f));
        Wheels(v, 0.95f, new[] { -1.3f, 1.3f });
    }

    void Taxi(Transform p, int lane, float z)
    {
        Transform t = NewObstacle(p, "Taxi", lane, z, 0.85f, 1.7f, 0f, 1.55f);
        t.GetComponent<Obstacle>().Vel = 10f;
        Color yellow = new Color(1f, 0.82f, 0.1f);
        ShapeFactory.Cube("Body", t, new Vector3(0f, 0.65f, 0f), new Vector3(1.7f, 0.7f, 3.4f), yellow);
        ShapeFactory.Cube("Cabin", t, new Vector3(0f, 1.25f, 0.2f), new Vector3(1.5f, 0.6f, 1.8f), yellow);
        ShapeFactory.Cube("Windshield", t, new Vector3(0f, 1.28f, -0.71f), new Vector3(1.4f, 0.45f, 0.04f), new Color(0.15f, 0.25f, 0.4f));
        ShapeFactory.Cube("Sign", t, new Vector3(0f, 1.65f, 0.2f), new Vector3(0.5f, 0.2f, 0.3f), Color.white);
        ShapeFactory.Unlit(PrimitiveType.Cube, "LightL", t, new Vector3(-0.6f, 0.65f, -1.71f), new Vector3(0.3f, 0.18f, 0.04f), new Color(1f, 0.95f, 0.6f));
        ShapeFactory.Unlit(PrimitiveType.Cube, "LightR", t, new Vector3(0.6f, 0.65f, -1.71f), new Vector3(0.3f, 0.18f, 0.04f), new Color(1f, 0.95f, 0.6f));
        Wheels(t, 0.85f, new[] { -1.1f, 1.1f });
    }

    void Barrier(Transform p, int lane, float z)
    {
        Transform b = NewObstacle(p, "Barrier", lane, z, 1.15f, 0.2f, 0f, 0.7f);
        ShapeFactory.Cube("BarLow", b, new Vector3(0f, 0.25f, 0f), new Vector3(2.3f, 0.3f, 0.35f), new Color(0.95f, 0.95f, 0.95f));
        ShapeFactory.Cube("BarHigh", b, new Vector3(0f, 0.55f, 0f), new Vector3(2.3f, 0.3f, 0.35f), new Color(0.9f, 0.2f, 0.2f));
        ShapeFactory.Cube("LegL", b, new Vector3(-1f, 0.25f, 0f), new Vector3(0.15f, 0.5f, 0.15f), new Color(0.3f, 0.3f, 0.34f));
        ShapeFactory.Cube("LegR", b, new Vector3(1f, 0.25f, 0f), new Vector3(0.15f, 0.5f, 0.15f), new Color(0.3f, 0.3f, 0.34f));
    }

    void Gantry(Transform p, int lane, float z)
    {
        Transform g = NewObstacle(p, "Gantry", lane, z, 1.25f, 0.25f, 1.4f, 2.3f);
        ShapeFactory.Cube("PostL", g, new Vector3(-1.15f, 1.2f, 0f), new Vector3(0.2f, 2.4f, 0.2f), new Color(0.4f, 0.4f, 0.45f));
        ShapeFactory.Cube("PostR", g, new Vector3(1.15f, 1.2f, 0f), new Vector3(0.2f, 2.4f, 0.2f), new Color(0.4f, 0.4f, 0.45f));
        ShapeFactory.Cube("Beam", g, new Vector3(0f, 1.85f, 0f), new Vector3(2.5f, 0.9f, 0.4f), new Color(1f, 0.6f, 0.1f));
        ShapeFactory.Cube("BeamStrip", g, new Vector3(0f, 1.55f, 0f), new Vector3(2.52f, 0.2f, 0.42f), new Color(0.15f, 0.15f, 0.15f));
    }

    void MakeCoin(Transform p, Vector3 localPos)
    {
        GameObject c = new GameObject("Coin");
        c.transform.SetParent(p, false);
        c.transform.localPosition = localPos;
        c.AddComponent<Coin>();
        GameObject disc = ShapeFactory.Unlit(PrimitiveType.Cylinder, "Disc", c.transform, Vector3.zero, new Vector3(0.7f, 0.05f, 0.7f), new Color(1f, 0.8f, 0.1f));
        disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void CoinLine(Transform p, int lane, float z, float y)
    {
        for (int i = -2; i <= 2; i++)
            MakeCoin(p, new Vector3(lane * PlayerMovement.LaneWidth, y, z + i * 2f));
    }

    void CoinArc(Transform p, int lane, float z)
    {
        float[] ys = { 1.0f, 1.8f, 2.3f, 1.8f, 1.0f };
        for (int i = 0; i < 5; i++)
            MakeCoin(p, new Vector3(lane * PlayerMovement.LaneWidth, ys[i], z + (i - 2) * 1.5f));
    }

    void MakePowerUp(Transform p, int lane, float z, PowerType type)
    {
        GameObject root = new GameObject("PowerUp_" + type);
        root.transform.SetParent(p, false);
        root.transform.localPosition = new Vector3(lane * PlayerMovement.LaneWidth, 1.4f, z);
        PowerUp pu = root.AddComponent<PowerUp>();
        pu.Type = type;
        Transform t = root.transform;
        Color c;

        switch (type)
        {
            case PowerType.Magnet:
            {
                c = new Color(0.95f, 0.2f, 0.2f);
                ShapeFactory.Unlit(PrimitiveType.Cube, "L", t, new Vector3(-0.28f, 0.05f, 0f), new Vector3(0.2f, 0.6f, 0.2f), c);
                ShapeFactory.Unlit(PrimitiveType.Cube, "R", t, new Vector3(0.28f, 0.05f, 0f), new Vector3(0.2f, 0.6f, 0.2f), c);
                ShapeFactory.Unlit(PrimitiveType.Cube, "Bottom", t, new Vector3(0f, -0.3f, 0f), new Vector3(0.76f, 0.2f, 0.2f), c);
                ShapeFactory.Unlit(PrimitiveType.Cube, "TipL", t, new Vector3(-0.28f, 0.4f, 0f), new Vector3(0.2f, 0.2f, 0.2f), Color.white);
                ShapeFactory.Unlit(PrimitiveType.Cube, "TipR", t, new Vector3(0.28f, 0.4f, 0f), new Vector3(0.2f, 0.2f, 0.2f), Color.white);
                break;
            }
            case PowerType.Jetpack:
            {
                c = new Color(1f, 0.6f, 0.1f);
                ShapeFactory.Unlit(PrimitiveType.Cylinder, "PodL", t, new Vector3(-0.22f, 0f, 0f), new Vector3(0.3f, 0.45f, 0.3f), c);
                ShapeFactory.Unlit(PrimitiveType.Cylinder, "PodR", t, new Vector3(0.22f, 0f, 0f), new Vector3(0.3f, 0.45f, 0.3f), c);
                ShapeFactory.Unlit(PrimitiveType.Sphere, "FlameL", t, new Vector3(-0.22f, -0.55f, 0f), new Vector3(0.2f, 0.3f, 0.2f), new Color(1f, 0.9f, 0.3f));
                ShapeFactory.Unlit(PrimitiveType.Sphere, "FlameR", t, new Vector3(0.22f, -0.55f, 0f), new Vector3(0.2f, 0.3f, 0.2f), new Color(1f, 0.9f, 0.3f));
                break;
            }
            case PowerType.Shield:
            {
                c = new Color(0.3f, 0.65f, 1f);
                ShapeFactory.Translucent(PrimitiveType.Sphere, "Bubble", t, Vector3.zero, Vector3.one * 0.9f, new Color(0.3f, 0.65f, 1f, 0.55f));
                ShapeFactory.Unlit(PrimitiveType.Sphere, "Core", t, Vector3.zero, Vector3.one * 0.4f, new Color(0.8f, 0.95f, 1f));
                break;
            }
            default:
            {
                c = new Color(0.7f, 0.3f, 1f);
                GameObject cube = ShapeFactory.Unlit(PrimitiveType.Cube, "Gem", t, Vector3.zero, Vector3.one * 0.6f, c);
                cube.transform.localRotation = Quaternion.Euler(45f, 0f, 45f);
                ShapeFactory.Unlit(PrimitiveType.Sphere, "Core", t, Vector3.zero, Vector3.one * 0.35f, new Color(1f, 0.85f, 0.2f));
                break;
            }
        }
        ShapeFactory.Translucent(PrimitiveType.Sphere, "Glow", t, Vector3.zero, Vector3.one * 1.5f, new Color(c.r, c.g, c.b, 0.25f));
    }
}
