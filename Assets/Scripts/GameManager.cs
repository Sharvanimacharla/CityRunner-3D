using UnityEngine;

public enum GameState { Menu, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public const float StartSpeed = 14f;
    public const float MaxSpeed = 28f;
    public const float Acceleration = 0.25f;

    public GameState State { get; private set; }
    public int StateFrame { get; private set; }
    public float Speed { get; private set; }
    public int Coins { get; private set; }
    public int Best { get; private set; }
    public bool Shield { get; private set; }
    public bool CaughtByGuard { get; private set; }

    public bool Magnet { get { return magnetTime > 0f; } }
    public bool Jetpack { get { return jetpackTime > 0f; } }
    public bool Stumbled { get { return stumbleTimer > 0f; } }
    public bool Invulnerable { get { return invulnTime > 0f || jetpackTime > 0f; } }
    public float InvulnTime { get { return invulnTime; } }
    public int Multiplier { get { return multTime > 0f ? 2 : 1; } }
    public float Distance { get { return player != null ? player.transform.position.z : 0f; } }
    public int Score { get { return Mathf.FloorToInt(scoreAcc); } }

    PlayerMovement player;
    SegmentGenerator generator;
    CameraFollow cameraFollow;
    CitySkyline skyline;
    Chaser chaser;
    Camera cam;
    Light sun;

    float scoreAcc;
    float magnetTime, jetpackTime, multTime, stumbleTimer, invulnTime;
    float gameOverTime;
    bool newBest;
    int nextMilestone;
    string toast = "";
    float toastTime;

    int styleHeight = -1;
    GUIStyle bigC, midC, smallC, midL, midR, smallR, smallL;

    // day -> sunset -> night cycle
    // city haze tones: smoggy day, orange dusk over the skyline, dark navy night with city glow
    static readonly Color[] SkyC = { new Color(0.68f, 0.75f, 0.82f), new Color(0.95f, 0.60f, 0.42f), new Color(0.07f, 0.08f, 0.18f) };
    static readonly Color[] AmbC = { new Color(0.60f, 0.64f, 0.72f), new Color(0.58f, 0.46f, 0.50f), new Color(0.20f, 0.22f, 0.36f) };
    static readonly Color[] SunC = { new Color(1f, 0.96f, 0.88f), new Color(1f, 0.70f, 0.50f), new Color(0.55f, 0.65f, 1f) };
    static readonly float[] SunI = { 1.1f, 0.8f, 0.3f };

    void Awake()
    {
        Instance = this;
        SoundFx.Init(gameObject);
        SetupEnvironment();
        SetupCamera();

        player = new GameObject("Player").AddComponent<PlayerMovement>();
        generator = new GameObject("World").AddComponent<SegmentGenerator>();
        generator.target = player.transform;
        skyline = new GameObject("CitySkyline").AddComponent<CitySkyline>();
        skyline.target = player.transform;
        cameraFollow.target = player.transform;
        chaser = new GameObject("Guard").AddComponent<Chaser>();
        chaser.Init(player);

        Best = SaveLoad.LoadBest();
        ResetGame();
    }

    void SetupEnvironment()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 50f;
        RenderSettings.fogEndDistance = 250f; // long enough that the skyline stays visible as hazy silhouettes

        foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (l.type == LightType.Directional) { sun = l; break; }
        }
        if (sun == null)
        {
            sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
        }
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        sun.shadows = LightShadows.Soft;
        QualitySettings.shadowDistance = 60f;
    }

    void SetupCamera()
    {
        cam = Camera.main;
        if (cam == null)
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
        }
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.fieldOfView = 62f;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 400f;
        cameraFollow = cam.GetComponent<CameraFollow>();
        if (cameraFollow == null) cameraFollow = cam.gameObject.AddComponent<CameraFollow>();
    }

    void UpdateTimeOfDay()
    {
        float t = Mathf.Repeat(scoreAcc / 900f, 3f);
        int a = (int)t;
        int b = (a + 1) % 3;
        float f = Mathf.SmoothStep(0f, 1f, t - a);
        Color sky = Color.Lerp(SkyC[a], SkyC[b], f);
        RenderSettings.fogColor = sky;
        RenderSettings.ambientLight = Color.Lerp(AmbC[a], AmbC[b], f);
        if (cam != null) cam.backgroundColor = sky;
        if (skyline != null) skyline.SetNight(a == 2 ? 1f - f : (a == 1 ? f : 0f));
        if (sun != null)
        {
            sun.color = Color.Lerp(SunC[a], SunC[b], f);
            sun.intensity = Mathf.Lerp(SunI[a], SunI[b], f);
        }
    }

    void SetState(GameState s)
    {
        State = s;
        StateFrame = Time.frameCount;
    }

    void Toast(string text)
    {
        toast = text;
        toastTime = 2f;
    }

    public void ResetGame()
    {
        player.ResetPlayer();
        chaser.ResetChaser();
        generator.Rebuild();
        Coins = 0;
        scoreAcc = 0f;
        nextMilestone = 1000;
        Speed = StartSpeed;
        magnetTime = jetpackTime = multTime = stumbleTimer = invulnTime = 0f;
        Shield = false;
        CaughtByGuard = false;
        newBest = false;
        toastTime = 0f;
        SetState(GameState.Menu);
        UpdateTimeOfDay();
    }

    void StartRun()
    {
        Speed = StartSpeed;
        SetState(GameState.Playing);
    }

    public void AddCoin()
    {
        Coins++;
        scoreAcc += 10f * Multiplier;
        SoundFx.Play("coin");
    }

    public void Activate(PowerType type)
    {
        switch (type)
        {
            case PowerType.Magnet: magnetTime = 8f; Toast("MAGNET!"); break;
            case PowerType.Jetpack: jetpackTime = 6f; Toast("JETPACK!  Fly over everything"); break;
            case PowerType.Shield: Shield = true; Toast("SHIELD!  Absorbs one hit"); break;
            case PowerType.Multiplier: multTime = 10f; Toast("DOUBLE SCORE!"); break;
        }
        SoundFx.Play("power");
    }

    // Called by the player when an obstacle is hit.
    public void Hit()
    {
        if (State != GameState.Playing || Invulnerable) return;

        if (Shield)
        {
            Shield = false;
            invulnTime = 1.5f;
            Toast("SHIELD BROKE!");
            SoundFx.Play("stumble");
            cameraFollow.Shake(0.5f);
            return;
        }
        if (Stumbled)
        {
            Crash();
            return;
        }
        stumbleTimer = 8f;
        invulnTime = 1.5f;
        Speed = Mathf.Max(StartSpeed * 0.8f, Speed * 0.7f);
        Toast("STUMBLE!  The guard is chasing you");
        SoundFx.Play("stumble");
        cameraFollow.Shake(0.8f);
    }

    void Crash()
    {
        if (State != GameState.Playing) return;
        CaughtByGuard = true;
        SetState(GameState.GameOver);
        gameOverTime = Time.time;
        newBest = Score > Best;
        if (newBest) { Best = Score; SaveLoad.SaveBest(Best); }
        SaveLoad.AddCoins(Coins);
        SoundFx.Play("crash");
        cameraFollow.Shake(1f);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        switch (State)
        {
            case GameState.Menu:
                if (InputHelper.Confirm) StartRun();
                break;
            case GameState.Playing:
                TickPlaying(dt);
                break;
            case GameState.GameOver:
                if (Time.time - gameOverTime > 0.6f && InputHelper.Confirm)
                {
                    ResetGame();
                    StartRun();
                }
                break;
        }
        if (toastTime > 0f) toastTime -= dt;
        UpdateTimeOfDay();
    }

    void TickPlaying(float dt)
    {
        Speed = Mathf.Min(MaxSpeed, Speed + Acceleration * dt);
        scoreAcc += Speed * dt * Multiplier;

        if (magnetTime > 0f) magnetTime -= dt;
        if (multTime > 0f) multTime -= dt;
        if (invulnTime > 0f) invulnTime -= dt;
        if (jetpackTime > 0f)
        {
            jetpackTime -= dt;
            if (jetpackTime <= 0f) invulnTime = Mathf.Max(invulnTime, 1.8f); // safe landing
        }
        if (stumbleTimer > 0f)
        {
            stumbleTimer -= dt;
            if (stumbleTimer <= 0f) Toast("You lost the guard!");
        }
        if (Score >= nextMilestone)
        {
            Toast("SCORE " + nextMilestone + "!");
            nextMilestone += 1000;
        }
    }

    // ---------- HUD ----------
    GUIStyle MakeStyle(int size, TextAnchor anchor)
    {
        GUIStyle s = new GUIStyle(GUI.skin.label);
        s.fontSize = size;
        s.alignment = anchor;
        s.fontStyle = FontStyle.Bold;
        s.wordWrap = false;
        return s;
    }

    void EnsureStyles(float h)
    {
        if (styleHeight == Screen.height && bigC != null) return;
        styleHeight = Screen.height;
        int big = Mathf.RoundToInt(h / 9f), mid = Mathf.RoundToInt(h / 21f), small = Mathf.RoundToInt(h / 30f);
        bigC = MakeStyle(big, TextAnchor.MiddleCenter);
        midC = MakeStyle(mid, TextAnchor.MiddleCenter);
        smallC = MakeStyle(small, TextAnchor.MiddleCenter);
        midL = MakeStyle(mid, TextAnchor.UpperLeft);
        midR = MakeStyle(mid, TextAnchor.UpperRight);
        smallR = MakeStyle(small, TextAnchor.UpperRight);
        smallL = MakeStyle(small, TextAnchor.UpperLeft);
    }

    void Shadowed(Rect r, string text, GUIStyle style, Color color)
    {
        style.normal.textColor = new Color(0f, 0f, 0f, 0.6f);
        GUI.Label(new Rect(r.x + 2f, r.y + 2f, r.width, r.height), text, style);
        style.normal.textColor = color;
        GUI.Label(r, text, style);
    }

    void OnGUI()
    {
        float w = Screen.width, h = Screen.height;
        EnsureStyles(h);
        Color gold = new Color(1f, 0.85f, 0.2f);

        if (State != GameState.Menu)
        {
            string mult = Multiplier > 1 ? "   x2" : "";
            Shadowed(new Rect(24f, 12f, w * 0.5f, h * 0.1f), "Score  " + Score + mult, midL, Multiplier > 1 ? new Color(0.85f, 0.55f, 1f) : Color.white);
            Shadowed(new Rect(w * 0.5f - 24f, 12f, w * 0.5f, h * 0.1f), "Coins  " + Coins, midR, gold);
            Shadowed(new Rect(w * 0.5f - 24f, 12f + h * 0.06f, w * 0.5f, h * 0.1f), "Best  " + Best, smallR, Color.white);

            // active power-ups
            float y = 12f + h * 0.065f;
            float step = h * 0.04f;
            if (Jetpack) { Shadowed(new Rect(24f, y, w * 0.5f, step), "JETPACK  " + jetpackTime.ToString("0.0"), smallL, new Color(1f, 0.65f, 0.2f)); y += step; }
            if (Magnet) { Shadowed(new Rect(24f, y, w * 0.5f, step), "MAGNET  " + magnetTime.ToString("0.0"), smallL, new Color(1f, 0.4f, 0.4f)); y += step; }
            if (Multiplier > 1) { Shadowed(new Rect(24f, y, w * 0.5f, step), "x2 SCORE  " + multTime.ToString("0.0"), smallL, new Color(0.85f, 0.55f, 1f)); y += step; }
            if (Shield) { Shadowed(new Rect(24f, y, w * 0.5f, step), "SHIELD READY", smallL, new Color(0.45f, 0.75f, 1f)); y += step; }
            if (Stumbled && State == GameState.Playing) { Shadowed(new Rect(24f, y, w * 0.5f, step), "GUARD CHASING!  " + stumbleTimer.ToString("0.0"), smallL, new Color(1f, 0.35f, 0.35f)); }
        }

        if (toastTime > 0f && State == GameState.Playing)
        {
            float a = Mathf.Clamp01(toastTime * 2f);
            Shadowed(new Rect(0f, h * 0.22f, w, h * 0.1f), toast, midC, new Color(1f, 1f, 1f, a));
        }

        if (State == GameState.Menu)
        {
            Shadowed(new Rect(0f, h * 0.10f, w, h * 0.2f), "CITY RUNNER 3D", bigC, Color.white);
            float a = 0.6f + 0.4f * Mathf.Sin(Time.time * 4f);
            Shadowed(new Rect(0f, h * 0.32f, w, h * 0.1f), "Press SPACE or tap to start", midC, new Color(1f, 1f, 1f, a));
            if (Best > 0) Shadowed(new Rect(0f, h * 0.42f, w, h * 0.08f), "Best score  " + Best, smallC, gold);
            Shadowed(new Rect(0f, h * 0.74f, w, h * 0.06f), "Hit something once = stumble.  Twice = the guard catches you!", smallC, new Color(1f, 0.6f, 0.6f));
            Shadowed(new Rect(0f, h * 0.79f, w, h * 0.06f), "Grab MAGNET, JETPACK, SHIELD and x2 power-ups. Taxis drive at you!", smallC, new Color(1f, 0.95f, 0.6f));
            Shadowed(new Rect(0f, h * 0.85f, w, h * 0.06f), "A / D  or  Left / Right : change lane", smallC, Color.white);
            Shadowed(new Rect(0f, h * 0.90f, w, h * 0.06f), "W / Up / Space : jump      S / Down : slide      (swipe on touch)", smallC, Color.white);
        }
        else if (State == GameState.GameOver)
        {
            Shadowed(new Rect(0f, h * 0.16f, w, h * 0.2f), "BUSTED!", bigC, new Color(1f, 0.35f, 0.35f));
            Shadowed(new Rect(0f, h * 0.32f, w, h * 0.08f), "The guard caught you", smallC, Color.white);
            Shadowed(new Rect(0f, h * 0.39f, w, h * 0.1f), "Score  " + Score + "     Coins  " + Coins, midC, Color.white);
            if (newBest) Shadowed(new Rect(0f, h * 0.47f, w, h * 0.1f), "New best score!", midC, gold);
            Shadowed(new Rect(0f, h * 0.60f, w, h * 0.08f), "Press SPACE or tap to run again", smallC, Color.white);
        }
    }
}
