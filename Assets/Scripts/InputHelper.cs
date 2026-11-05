using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

// Keyboard + mouse/touch swipe input. Works with the old Input Manager and the new Input System.
public static class InputHelper
{
    static int lastFrame = -1;
    static bool left, right, jump, slide, confirm;
    static bool dragging;
    static Vector2 dragStart;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { lastFrame = -1; dragging = false; }

    public static bool Left { get { Refresh(); return left; } }
    public static bool Right { get { Refresh(); return right; } }
    public static bool Jump { get { Refresh(); return jump; } }
    public static bool Slide { get { Refresh(); return slide; } }
    public static bool Confirm { get { Refresh(); return confirm; } }

    static void TrySwipe(Vector2 pos)
    {
        Vector2 d = pos - dragStart;
        if (d.magnitude < Screen.height * 0.06f) return;
        if (Mathf.Abs(d.x) > Mathf.Abs(d.y))
        {
            if (d.x > 0f) right = true; else left = true;
        }
        else
        {
            if (d.y > 0f) jump = true; else slide = true;
        }
        dragging = false;
    }

    static void Refresh()
    {
        if (lastFrame == Time.frameCount) return;
        lastFrame = Time.frameCount;
        left = right = jump = slide = confirm = false;
        bool tap = false;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) left = true;
            if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) right = true;
            if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame) jump = true;
            if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) slide = true;
            if (kb.enterKey.wasPressedThisFrame) confirm = true;
        }
        Pointer pointer = Pointer.current;
        if (pointer != null)
        {
            Vector2 pos = pointer.position.ReadValue();
            if (pointer.press.wasPressedThisFrame) { dragging = true; dragStart = pos; }
            else if (dragging && pointer.press.isPressed) TrySwipe(pos);
            if (pointer.press.wasReleasedThisFrame) { if (dragging) tap = true; dragging = false; }
        }
#else
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) left = true;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) right = true;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) jump = true;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) slide = true;
        if (Input.GetKeyDown(KeyCode.Return)) confirm = true;

        Vector2 mp = Input.mousePosition;
        if (Input.GetMouseButtonDown(0)) { dragging = true; dragStart = mp; }
        else if (dragging && Input.GetMouseButton(0)) TrySwipe(mp);
        if (Input.GetMouseButtonUp(0)) { if (dragging) tap = true; dragging = false; }
#endif
        if (tap) confirm = true;
        if (jump) confirm = true;
    }
}
