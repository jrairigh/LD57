using UnityEngine;
namespace LD57
{
    public class CameraFollow : MonoBehaviour
    {
        public Vector3 cameraOffset = new Vector3(0, 0, -10);

        private GameObject player;

        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        void Update()
        {
            if (player.scene.IsValid())
            {
                transform.position = player.transform.position + cameraOffset;
            }
        }
    }
}
