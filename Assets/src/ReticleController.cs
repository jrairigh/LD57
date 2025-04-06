using TMPro;
using UnityEngine;

public class ReticleController : MonoBehaviour
{
    private Camera m_camera;

    private void Awake()
    {
        m_camera = Camera.main;
    }

    void Update()
    {
        var mouse_world_pos = m_camera.ScreenToWorldPoint(Input.mousePosition);
        mouse_world_pos.z = 0;
        transform.position = mouse_world_pos;
    }
}
