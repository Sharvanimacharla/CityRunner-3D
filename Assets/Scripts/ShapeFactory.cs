using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Creates low-poly shapes (no colliders) with cached flat-colour materials.
public static class ShapeFactory
{
    static readonly Dictionary<PrimitiveType, Mesh> meshes = new Dictionary<PrimitiveType, Mesh>();
    static readonly Dictionary<int, Material> solid = new Dictionary<int, Material>();
    static readonly Dictionary<int, Material> glass = new Dictionary<int, Material>();
    static readonly Dictionary<int, Material> unlit = new Dictionary<int, Material>();
    static Shader litShader;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        meshes.Clear(); solid.Clear(); glass.Clear(); unlit.Clear(); litShader = null;
    }

    static Mesh GetMesh(PrimitiveType type)
    {
        Mesh mesh;
        if (meshes.TryGetValue(type, out mesh) && mesh != null) return mesh;
        GameObject temp = GameObject.CreatePrimitive(type);
        mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Object.Destroy(temp);
        meshes[type] = mesh;
        return mesh;
    }

    static Shader LitShader()
    {
        if (litShader != null) return litShader;
        string[] names;
        if (GraphicsSettings.currentRenderPipeline != null)
            names = new[] { "Universal Render Pipeline/Lit", "Standard", "Unlit/Color", "Sprites/Default" };
        else
            names = new[] { "Standard", "Unlit/Color", "Sprites/Default" };
        foreach (string n in names)
        {
            Shader s = Shader.Find(n);
            if (s != null) { litShader = s; break; }
        }
        return litShader;
    }

    static int Key(Color c)
    {
        Color32 k = c;
        return (k.r << 24) | (k.g << 16) | (k.b << 8) | k.a;
    }

    public static Material GetMaterial(Color c)
    {
        int key = Key(c);
        Material m;
        if (solid.TryGetValue(key, out m) && m != null) return m;
        m = new Material(LitShader());
        m.color = c;
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.15f);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.15f);
        solid[key] = m;
        return m;
    }

    static Material GetGlass(Color c)
    {
        int key = Key(c);
        Material m;
        if (glass.TryGetValue(key, out m) && m != null) return m;
        Shader s = Shader.Find("Sprites/Default");
        if (s == null) s = LitShader();
        m = new Material(s);
        m.color = c;
        glass[key] = m;
        return m;
    }

    static Material GetUnlit(Color c)
    {
        int key = Key(c);
        Material m;
        if (unlit.TryGetValue(key, out m) && m != null) return m;
        Shader s = Shader.Find("Unlit/Color");
        if (s == null) s = Shader.Find("Sprites/Default");
        if (s == null) s = LitShader();
        m = new Material(s);
        m.color = c;
        unlit[key] = m;
        return m;
    }

    static GameObject Make(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject go = new GameObject(name);
        go.AddComponent<MeshFilter>().sharedMesh = GetMesh(type);
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        return go;
    }

    public static GameObject Create(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Color color)
    {
        return Make(type, name, parent, pos, scale, GetMaterial(color));
    }

    public static GameObject Cube(string name, Transform parent, Vector3 pos, Vector3 scale, Color color)
    {
        return Make(PrimitiveType.Cube, name, parent, pos, scale, GetMaterial(color));
    }

    public static GameObject Translucent(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Color color)
    {
        return Make(type, name, parent, pos, scale, GetGlass(color));
    }

    // Self-lit (glows, even at night): coins, lamps, lit windows, headlights, power-ups
    public static GameObject Unlit(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Color color)
    {
        return Make(type, name, parent, pos, scale, GetUnlit(color));
    }
}
