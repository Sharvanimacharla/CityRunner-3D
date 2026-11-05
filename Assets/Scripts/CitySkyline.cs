using UnityEngine;

// Distant city backdrop: layered skyscrapers flanking the road, a continuous skyline wall on the horizon
// and a far ground plane, so there is never open sky/void - the whole background is city.
// Windows light up at night (SetNight is driven by GameManager's day/sunset/night cycle).
public class CitySkyline : MonoBehaviour
{
    public Transform target;

    const float Period = 160f;   // side towers repeat every Period units (snapped, so the swap is invisible)
    const int Copies = 3;

    static readonly Color[] Tones =
    {
        new Color(0.38f, 0.48f, 0.60f), // blue glass
        new Color(0.58f, 0.60f, 0.64f), // concrete
        new Color(0.28f, 0.31f, 0.38f), // dark slate
        new Color(0.55f, 0.36f, 0.32f), // brick
        new Color(0.72f, 0.66f, 0.55f), // sandstone
        new Color(0.20f, 0.22f, 0.27f)  // charcoal
    };
    static readonly Color WindowGlow = new Color(1f, 0.88f, 0.50f);

    Transform sideRig, horizonRig, farGround;
    GameObject sideWindows, horizonWindows;
    System.Random rng;

    void Awake()
    {
        rng = new System.Random(2024);

        farGround = ShapeFactory.Cube("FarGround", transform, Vector3.zero, new Vector3(520f, 1f, 640f), new Color(0.36f, 0.38f, 0.42f)).transform;

        sideRig = new GameObject("SideTowers").transform;
        sideRig.SetParent(transform, false);
        sideWindows = new GameObject("Windows");
        sideWindows.transform.SetParent(sideRig, false);
        BuildSideTowers();

        horizonRig = new GameObject("HorizonWall").transform;
        horizonRig.SetParent(transform, false);
        horizonWindows = new GameObject("Windows");
        horizonWindows.transform.SetParent(horizonRig, false);
        BuildHorizon(205f, 14f, 40f, 90f);
        BuildHorizon(228f, 16f, 70f, 150f);
    }

    float R(float a, float b) { return a + (float)rng.NextDouble() * (b - a); }

    Color Tone() { return Tones[rng.Next(Tones.Length)]; }

    void AddSpire(Transform parent, Vector3 top)
    {
        ShapeFactory.Create(PrimitiveType.Cylinder, "Spire", parent, top + new Vector3(0f, 4f, 0f), new Vector3(0.4f, 4f, 0.4f), new Color(0.25f, 0.25f, 0.28f));
    }

    void BuildSideTowers()
    {
        // one period of towers, instantiated Copies times
        for (int c = 0; c < Copies; c++)
        {
            rng = new System.Random(77); // same seed per copy => identical pattern every Period
            for (int s = -1; s <= 1; s += 2)
            {
                for (int band = 0; band < 2; band++)
                {
                    float x0 = band == 0 ? 52f : 100f;
                    for (int i = 0; i < 6; i++)
                    {
                        float z = c * Period + i * (Period / 6f) + R(-4f, 4f);
                        float w = R(14f, 22f);
                        float d = R(14f, 24f);
                        float h = band == 0 ? R(25f, 70f) : R(50f, 130f);
                        float x = s * (x0 + d * 0.5f + R(0f, 8f));
                        Color col = Tone();

                        ShapeFactory.Cube("Tower", sideRig, new Vector3(x, h * 0.5f - 0.5f, z), new Vector3(d, h, w), col);
                        ShapeFactory.Cube("Crown", sideRig, new Vector3(x, h + 0.8f, z), new Vector3(d * 0.6f, 1.6f, w * 0.6f), col * 0.7f);
                        if (R(0f, 1f) < 0.4f) AddSpire(sideRig, new Vector3(x, h + 1.6f, z));

                        float faceX = x - s * d * 0.5f;
                        for (float wy = 6f; wy < h - 3f; wy += 7f)
                        {
                            if (R(0f, 1f) < 0.55f)
                                ShapeFactory.Unlit(PrimitiveType.Cube, "Win", sideWindows.transform, new Vector3(faceX - s * 0.05f, wy, z), new Vector3(0.1f, 1.8f, w * 0.8f), WindowGlow);
                        }
                    }
                }
            }
        }
    }

    // Continuous row of towers across the whole view at a fixed distance ahead of the player.
    void BuildHorizon(float relZ, float depth, float minH, float maxH)
    {
        float cursor = -190f;
        while (cursor < 190f)
        {
            float w = R(14f, 24f);
            float h = R(minH, maxH);
            float x = cursor + w * 0.5f;
            Color col = Tone();

            ShapeFactory.Cube("Tower", horizonRig, new Vector3(x, h * 0.5f - 0.5f, relZ), new Vector3(w, h, depth), col);
            ShapeFactory.Cube("Crown", horizonRig, new Vector3(x, h + 0.8f, relZ), new Vector3(w * 0.6f, 1.6f, depth * 0.6f), col * 0.7f);
            if (R(0f, 1f) < 0.4f) AddSpire(horizonRig, new Vector3(x, h + 1.6f, relZ));

            for (float wy = 6f; wy < h - 3f; wy += 7f)
            {
                if (R(0f, 1f) < 0.5f)
                    ShapeFactory.Unlit(PrimitiveType.Cube, "Win", horizonWindows.transform, new Vector3(x, wy, relZ - depth * 0.5f - 0.05f), new Vector3(w * 0.8f, 1.8f, 0.1f), WindowGlow);
            }
            cursor += w - 1f;
        }
    }

    // 0 = full day, 1 = full night
    public void SetNight(float n)
    {
        bool on = n > 0.3f;
        if (sideWindows != null && sideWindows.activeSelf != on) sideWindows.SetActive(on);
        if (horizonWindows != null && horizonWindows.activeSelf != on) horizonWindows.SetActive(on);
    }

    void LateUpdate()
    {
        if (target == null) return;
        float pz = target.position.z;

        horizonRig.position = new Vector3(0f, 0f, pz);
        farGround.position = new Vector3(0f, -0.8f, pz + 200f);

        // snap the side rig in whole periods; the pattern repeats exactly so the jump is invisible
        float snapped = Mathf.Floor((pz - 50f) / Period) * Period;
        sideRig.position = new Vector3(0f, 0f, snapped);
    }
}
