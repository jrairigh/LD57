using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Monster : MonoBehaviour
{
    private List<GameObject> Targetables = new List<GameObject>();
    public float Speed = 1f;

    void Start()
    {
        Targetables.Add(GameObject.FindGameObjectWithTag("Player"));
        Targetables.AddRange(GameObject.FindGameObjectsWithTag("Targetable"));
    }

    void Update()
    {
        var target = SelectTarget();

        float angle = Mathf.Atan2(target.transform.position.y - transform.position.y, target.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * Speed);
    }

    GameObject SelectTarget()
    {
        var targetables = Targetables.Select(target => new TargetableGameObjects
        {
            Target = target,
            Distance = Vector3.Distance(target.transform.position, transform.position)
        });

        return targetables.Aggregate((currentMin, targetable) => targetable.Distance < currentMin.Distance ? targetable : currentMin).Target;
    }

    private class TargetableGameObjects
    {
        public GameObject Target;
        public float Distance;
    }
}
