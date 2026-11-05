using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public const float LaneWidth = 2.6f;
    const float Gravity = -32f;
    const float JumpVelocity = 12.5f;
    const float StandHeight = 1.8f;
    const float SlideHeight = 0.8f;
    const float SlideTime = 0.7f;
    const float JetHeight = 6f;

    public int Lane { get; private set; }
    public float X { get { return x; } }
    public float Height { get { return slideTimer > 0f ? SlideHeight : StandHeight; } }

    float x, y, vy, slideTimer, runTime, leanZ;
    bool grounded = true;
    Transform visual, armL, armR, legL, legR, aura1, aura2, auraRoot, flameL, flameR;
    GameObject jetpackObj, shieldBubble, magnetRing;

    void Awake() { Build(); }

    Transform MakeLimb(string name, Vector3 pivotPos, float length, float thickness, Color color)
    {
        Transform pivot = new GameObject(name).transform;
        pivot.SetParent(visual, false);
        pivot.localPosition = pivotPos;
        ShapeFactory.Cube("Mesh", pivot, new Vector3(0f, -length * 0.5f, 0f), new Vector3(thickness, length, thickness), color);
        return pivot;
    }

    void Build()
    {
        visual = new GameObject("Visual").transform;
        visual.SetParent(transform, false);

        Color skin = new Color(0.96f, 0.76f, 0.62f);
        Color hoodie = new Color(0.90f, 0.12f, 0.50f); // magenta - stands out against the grey city
        Color jeans = new Color(0.14f, 0.15f, 0.20f);  // charcoal
        Color cap = new Color(0.96f, 0.96f, 0.98f);    // white
        Color pack = new Color(0.15f, 0.85f, 0.95f);   // cyan

        ShapeFactory.Cube("Torso", visual, new Vector3(0f, 1.05f, 0f), new Vector3(0.7f, 0.75f, 0.4f), hoodie);
        ShapeFactory.Cube("Backpack", visual, new Vector3(0f, 1.08f, -0.3f), new Vector3(0.5f, 0.55f, 0.22f), pack);
        ShapeFactory.Create(PrimitiveType.Sphere, "Head", visual, new Vector3(0f, 1.6f, 0f), new Vector3(0.5f, 0.5f, 0.5f), skin);
        ShapeFactory.Cube("Cap", visual, new Vector3(0f, 1.8f, 0f), new Vector3(0.52f, 0.14f, 0.54f), cap);
        ShapeFactory.Cube("Visor", visual, new Vector3(0f, 1.74f, 0.3f), new Vector3(0.4f, 0.05f, 0.25f), cap);
        legL = MakeLimb("LegL", new Vector3(-0.17f, 0.7f, 0f), 0.7f, 0.24f, jeans);
        legR = MakeLimb("LegR", new Vector3(0.17f, 0.7f, 0f), 0.7f, 0.24f, jeans);
        armL = MakeLimb("ArmL", new Vector3(-0.46f, 1.4f, 0f), 0.62f, 0.18f, hoodie);
        armR = MakeLimb("ArmR", new Vector3(0.46f, 1.4f, 0f), 0.62f, 0.18f, hoodie);

        // Jetpack (only visible while flying)
        jetpackObj = new GameObject("Jetpack");
        jetpackObj.transform.SetParent(visual, false);
        Color metal = new Color(0.55f, 0.58f, 0.65f);
        ShapeFactory.Create(PrimitiveType.Cylinder, "PodL", jetpackObj.transform, new Vector3(-0.17f, 1.1f, -0.5f), new Vector3(0.22f, 0.38f, 0.22f), metal);
        ShapeFactory.Create(PrimitiveType.Cylinder, "PodR", jetpackObj.transform, new Vector3(0.17f, 1.1f, -0.5f), new Vector3(0.22f, 0.38f, 0.22f), metal);
        flameL = ShapeFactory.Unlit(PrimitiveType.Sphere, "FlameL", jetpackObj.transform, new Vector3(-0.17f, 0.6f, -0.5f), new Vector3(0.22f, 0.55f, 0.22f), new Color(1f, 0.65f, 0.1f)).transform;
        flameR = ShapeFactory.Unlit(PrimitiveType.Sphere, "FlameR", jetpackObj.transform, new Vector3(0.17f, 0.6f, -0.5f), new Vector3(0.22f, 0.55f, 0.22f), new Color(1f, 0.65f, 0.1f)).transform;
        jetpackObj.SetActive(false);

        // Pink aura around the hero
        auraRoot = new GameObject("Aura").transform;
        auraRoot.SetParent(transform, false);
        auraRoot.localPosition = new Vector3(0f, 0.95f, 0f);
        aura1 = ShapeFactory.Translucent(PrimitiveType.Sphere, "AuraOuter", auraRoot, Vector3.zero, Vector3.one * 2.8f, new Color(1f, 0.35f, 0.75f, 0.16f)).transform;
        aura2 = ShapeFactory.Translucent(PrimitiveType.Sphere, "AuraInner", auraRoot, Vector3.zero, Vector3.one * 2.1f, new Color(1f, 0.45f, 0.8f, 0.22f)).transform;
        Light glow = auraRoot.gameObject.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = new Color(1f, 0.45f, 0.8f);
        glow.range = 7f;
        glow.intensity = 1.5f;

        // Shield bubble + magnet ring
        shieldBubble = ShapeFactory.Translucent(PrimitiveType.Sphere, "ShieldBubble", auraRoot, Vector3.zero, Vector3.one * 3.5f, new Color(0.3f, 0.6f, 1f, 0.30f));
        shieldBubble.SetActive(false);
        magnetRing = ShapeFactory.Translucent(PrimitiveType.Cylinder, "MagnetRing", transform, new Vector3(0f, 0.06f, 0f), new Vector3(7f, 0.02f, 7f), new Color(1f, 0.3f, 0.3f, 0.20f));
        magnetRing.SetActive(false);
    }

    public void ResetPlayer()
    {
        Lane = 0; x = 0f; y = 0f; vy = 0f; grounded = true;
        slideTimer = 0f; runTime = 0f; leanZ = 0f;
        transform.position = Vector3.zero;
        visual.gameObject.SetActive(true);
        visual.localRotation = Quaternion.identity;
        visual.localScale = Vector3.one;
        visual.localPosition = Vector3.zero;
        jetpackObj.SetActive(false);
        shieldBubble.SetActive(false);
        magnetRing.SetActive(false);
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;
        float dt = Mathf.Min(Time.deltaTime, 0.05f);
        bool playing = gm.State == GameState.Playing;
        bool jet = playing && gm.Jetpack;
        Vector3 pos = transform.position;

        if (playing)
        {
            if (gm.StateFrame != Time.frameCount)
            {
                if (InputHelper.Left) Lane = Mathf.Max(-1, Lane - 1);
                if (InputHelper.Right) Lane = Mathf.Min(1, Lane + 1);
                if (!jet)
                {
                    if (InputHelper.Jump && grounded)
                    {
                        vy = JumpVelocity; grounded = false; slideTimer = 0f;
                        SoundFx.Play("jump");
                    }
                    if (InputHelper.Slide)
                    {
                        slideTimer = SlideTime;
                        if (!grounded) vy = -22f;
                        SoundFx.Play("slide");
                    }
                }
            }
            pos.z += gm.Speed * dt;
            if (jet)
            {
                y = Mathf.Lerp(y, JetHeight, 1f - Mathf.Exp(-3f * dt));
                vy = 0f; grounded = false; slideTimer = 0f;
            }
            else if (!grounded)
            {
                vy += Gravity * dt;
                y += vy * dt;
                if (y <= 0f) { y = 0f; vy = 0f; grounded = true; }
            }
            if (slideTimer > 0f) slideTimer -= dt;
            runTime += dt * gm.Speed * 0.75f;
        }
        else if (gm.State == GameState.Menu)
        {
            runTime += dt * 6f;
        }

        float targetX = Lane * LaneWidth;
        x = Mathf.Lerp(x, targetX, 1f - Mathf.Exp(-14f * dt));
        pos.x = x;
        pos.y = y;
        transform.position = pos;

        Animate(gm, playing, jet, dt, targetX);

        if (playing)
        {
            CollectCoins(gm, pos, dt, jet);
            CollectPowerUps(gm, pos, jet);
            if (!jet) CheckCollisions(gm, pos);
        }
    }

    void Animate(GameManager gm, bool playing, bool jet, float dt, float targetX)
    {
        float swing = Mathf.Sin(runTime) * 55f;
        if (jet)
        {
            legL.localRotation = Quaternion.Euler(10f, 0f, 0f);
            legR.localRotation = Quaternion.Euler(-10f, 0f, 0f);
            armL.localRotation = Quaternion.Euler(-100f, 0f, -15f);
            armR.localRotation = Quaternion.Euler(-100f, 0f, 15f);
        }
        else if (!grounded && playing)
        {
            legL.localRotation = Quaternion.Euler(35f, 0f, 0f);
            legR.localRotation = Quaternion.Euler(-25f, 0f, 0f);
            armL.localRotation = Quaternion.Euler(-160f, 0f, 0f);
            armR.localRotation = Quaternion.Euler(-160f, 0f, 0f);
        }
        else
        {
            legL.localRotation = Quaternion.Euler(swing, 0f, 0f);
            legR.localRotation = Quaternion.Euler(-swing, 0f, 0f);
            armL.localRotation = Quaternion.Euler(-swing * 0.9f, 0f, 0f);
            armR.localRotation = Quaternion.Euler(swing * 0.9f, 0f, 0f);
        }

        bool sliding = slideTimer > 0f;
        if (gm.State == GameState.GameOver)
        {
            visual.localRotation = Quaternion.Slerp(visual.localRotation, Quaternion.Euler(-85f, 0f, 0f), dt * 8f);
            visual.localScale = Vector3.one;
        }
        else
        {
            leanZ = Mathf.Lerp(leanZ, Mathf.Clamp((x - targetX) * 8f, -20f, 20f), dt * 10f);
            float pitch = jet ? 35f : (sliding ? 25f : 0f);
            visual.localRotation = Quaternion.Euler(pitch, 0f, leanZ);
            Vector3 s = sliding ? new Vector3(1f, 0.5f, 1f) : Vector3.one;
            visual.localScale = Vector3.Lerp(visual.localScale, s, 1f - Mathf.Exp(-20f * dt));
            float bob = (grounded && playing) ? Mathf.Abs(Mathf.Sin(runTime)) * 0.12f : 0f;
            visual.localPosition = new Vector3(0f, bob, 0f);
        }

        // blink while invulnerable after a stumble / shield break
        bool blink = playing && !jet && gm.InvulnTime > 0f && Mathf.Sin(Time.time * 40f) > 0f;
        if (visual.gameObject.activeSelf == blink) visual.gameObject.SetActive(!blink);

        // power-up visuals
        jetpackObj.SetActive(jet);
        if (jet)
        {
            float fl = 0.45f + Mathf.Abs(Mathf.Sin(Time.time * 35f)) * 0.35f;
            flameL.localScale = new Vector3(0.22f, fl, 0.22f);
            flameR.localScale = new Vector3(0.22f, fl, 0.22f);
        }
        shieldBubble.SetActive(gm.Shield && gm.State != GameState.Menu);
        bool mag = playing && gm.Magnet;
        magnetRing.SetActive(mag);
        if (mag)
        {
            float mp = 7f + Mathf.Sin(Time.time * 6f) * 0.6f;
            magnetRing.transform.localScale = new Vector3(mp, 0.02f, mp);
            magnetRing.transform.localPosition = new Vector3(0f, -y + 0.06f, 0f);
        }

        float pulse = 1f + 0.06f * Mathf.Sin(Time.time * 5f);
        aura1.localScale = Vector3.one * 2.8f * pulse;
        aura2.localScale = Vector3.one * 2.1f * (2f - pulse);
        auraRoot.localPosition = new Vector3(0f, Height * 0.5f + 0.05f, 0f);
    }

    void CollectCoins(GameManager gm, Vector3 p, float dt, bool jet)
    {
        float h = Height;
        for (int i = Coin.All.Count - 1; i >= 0; i--)
        {
            if (i >= Coin.All.Count) continue;
            Coin c = Coin.All[i];
            Vector3 cp = c.transform.position;

            if (gm.Magnet && cp.z > p.z - 1f && Vector3.Distance(cp, p) < 10f)
            {
                cp = Vector3.MoveTowards(cp, p + Vector3.up * 1f, 45f * dt);
                c.transform.position = cp;
            }

            float xTol = jet ? 1.4f : 0.8f;
            bool yOk = jet ? cp.y < 8f : (cp.y > p.y - 0.2f && cp.y < p.y + h + 0.2f);
            if (yOk && Mathf.Abs(cp.z - p.z) < 0.9f && Mathf.Abs(cp.x - p.x) < xTol)
            {
                c.Collect();
                gm.AddCoin();
            }
        }
    }

    void CollectPowerUps(GameManager gm, Vector3 p, bool jet)
    {
        float h = Height;
        for (int i = PowerUp.All.Count - 1; i >= 0; i--)
        {
            if (i >= PowerUp.All.Count) continue;
            PowerUp pu = PowerUp.All[i];
            Vector3 pp = pu.transform.position;
            float xTol = jet ? 1.5f : 1.0f;
            bool yOk = jet ? pp.y < 8f : (pp.y > p.y - 0.4f && pp.y < p.y + h + 0.4f);
            if (yOk && Mathf.Abs(pp.z - p.z) < 1.1f && Mathf.Abs(pp.x - p.x) < xTol)
            {
                PowerType t = pu.Type;
                pu.Collect();
                gm.Activate(t);
            }
        }
    }

    void CheckCollisions(GameManager gm, Vector3 p)
    {
        if (gm.Invulnerable) return;
        float h = Height;
        for (int i = 0; i < Obstacle.All.Count; i++)
        {
            Obstacle o = Obstacle.All[i];
            Vector3 op = o.transform.position;
            if (Mathf.Abs(op.z - p.z) > o.HalfZ + 0.2f) continue;
            if (Mathf.Abs(op.x - p.x) > o.HalfX + 0.15f) continue;
            if (p.y + 0.2f < o.Top && p.y + h - 0.15f > o.Bottom)
            {
                gm.Hit();
                return;
            }
        }
    }
}
