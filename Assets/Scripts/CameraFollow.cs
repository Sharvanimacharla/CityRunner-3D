using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 5.2f, -8.5f);
    public float lookAhead = 7f;
    public float baseFov = 62f;

    Camera cam;
    float shake;

    void Awake() { cam = GetComponent<Camera>(); }

    public void Shake(float amount) { shake = amount; }

    void LateUpdate()
    {
        if (target == null) return;
        float dt = Time.deltaTime;
        Vector3 tp = target.position;
        Vector3 p = transform.position;

        float k = 1f - Mathf.Exp(-10f * dt);
        p.x = Mathf.Lerp(p.x, tp.x * 0.55f, k);
        p.y = Mathf.Lerp(p.y, offset.y + tp.y * 0.35f, k);
        p.z = tp.z + offset.z;
        transform.position = p;
        transform.LookAt(new Vector3(tp.x * 0.35f, 1.2f, tp.z + lookAhead));

        if (shake > 0f)
        {
            transform.position += Random.insideUnitSphere * 0.35f * shake;
            shake -= dt * 2f;
        }

        if (cam != null && GameManager.Instance != null)
        {
            float targetFov = baseFov + (GameManager.Instance.Speed - GameManager.StartSpeed) * 0.4f;
            if (GameManager.Instance.Jetpack) targetFov += 8f;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, dt * 2f);
        }
    }
}
