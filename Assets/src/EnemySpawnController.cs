using UnityEngine;

namespace LD57
{
    public class EnemySpawnController : MonoBehaviour
    {
        [Tooltip("The prefab to spawn.")]
        public GameObject enemyPrefab;
        [Tooltip("The number of enemies to spawn.")]
        public int enemyCount = 5;
        [Tooltip("The spawn radius.")]
        public float spawnRadius = 5f;

        public void SpawnEnemies()
        {
            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 spawnPosition = spawnRadius * Random.insideUnitCircle;
                spawnPosition.z = 0;
                Instantiate(enemyPrefab, transform.position + spawnPosition, Quaternion.identity, transform);
            }
        }
    }
}