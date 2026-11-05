using UnityEngine;

// The guard who chases you after a stumble. Hit something again and he catches you.
public class Chaser : MonoBehaviour
{
    PlayerMovement player;
    GameObject model;
    Transform legL, legR, armL, armR;
    float dist = 25f;
    float gx;

    Transform Limb(string name, Vector3 pivotPos, float length, float thick, Color col)
    {
        Transform pv = new GameObject(name).transform;
        pv.SetParent(model.transform, false);
        pv.localPosition = pivotPos;
        ShapeFactory.Cube("Mesh", pv, new Vector3(0f, -length * 0.5f, 0f), new Vector3(thick, length, thick), col);
        return pv;
    }

    public void Init(PlayerMovement p)
    {
        player = p;
        model = new GameObject("Model");
        model.transform.SetParent(transform, false);
        model.transform.localScale = Vector3.one * 1.1f;

        Color navy = new Color(0.12f, 0.18f, 0.45f);
        Color dark = new Color(0.08f, 0.10f, 0.25f);
        Color skin = new Color(0.90f, 0.70f, 0.55f);

        ShapeFactory.Cube("Torso", model.transform, new Vector3(0f, 1.1f, 0f), new Vector3(0.8f, 0.85f, 0.45f), navy);
        ShapeFactory.Create(PrimitiveType.Sphere, "Head", model.transform, new Vector3(0f, 1.7f, 0f), new Vector3(0.52f, 0.52f, 0.52f), skin);
        ShapeFactory.Cube("Hat", model.transform, new Vector3(0f, 1.95f, 0f), new Vector3(0.6f, 0.2f, 0.6f), dark);
        ShapeFactory.Cube("Brim", model.transform, new Vector3(0f, 1.88f, 0.35f), new Vector3(0.5f, 0.05f, 0.3f), dark);
        ShapeFactory.Unlit(PrimitiveType.Cube, "Badge", model.transform, new Vector3(0.2f, 1.25f, 0.23f), new Vector3(0.15f, 0.15f, 0.04f), new Color(1f, 0.85f, 0.2f));
        legL = Limb("LegL", new Vector3(-0.2f, 0.7f, 0f), 0.7f, 0.28f, dark);
        legR = Limb("LegR", new Vector3(0.2f, 0.7f, 0f), 0.7f, 0.28f, dark);
        armL = Limb("ArmL", new Vector3(-0.52f, 1.45f, 0f), 0.65f, 0.2f, navy);
        armR = Limb("ArmR", new Vector3(0.52f, 1.45f, 0f), 0.65f, 0.2f, navy);
        model.SetActive(false);
    }

    public void ResetChaser()
    {
        dist = 25f;
        if (player != null) gx = player.X;
        if (model != null) model.SetActive(false);
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || player == null || model == null) return;
        float dt = Mathf.Min(Time.deltaTime, 0.05f);

        float target = gm.CaughtByGuard ? 1.4f : (gm.Stumbled ? 3.5f : 25f);
        float rate = gm.CaughtByGuard ? 8f : 2.5f;
        dist = Mathf.Lerp(dist, target, 1f - Mathf.Exp(-rate * dt));

        bool show = dist < 20f;
        if (model.activeSelf != show) model.SetActive(show);

        gx = Mathf.Lerp(gx, player.X, 1f - Mathf.Exp(-6f * dt));
        transform.position = new Vector3(gx, 0f, player.transform.position.z - dist);

        if (show)
        {
            float sw = Mathf.Sin(Time.time * (gm.CaughtByGuard ? 3f : 14f)) * (gm.CaughtByGuard ? 10f : 50f);
            legL.localRotation = Quaternion.Euler(sw, 0f, 0f);
            legR.localRotation = Quaternion.Euler(-sw, 0f, 0f);
            armL.localRotation = Quaternion.Euler(-sw * 0.9f - (gm.CaughtByGuard ? 90f : 0f), 0f, 0f);
            armR.localRotation = Quaternion.Euler(sw * 0.9f - (gm.CaughtByGuard ? 90f : 0f), 0f, 0f);
        }
    }
}
