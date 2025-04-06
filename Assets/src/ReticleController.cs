using UnityEngine;

public class ReticleController : MonoBehaviour
{
    void Update()
    {
        var mouse_world_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse_world_pos.z = 0;
        transform.position = mouse_world_pos;
    }
}
