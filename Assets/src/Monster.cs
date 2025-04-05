using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Monster : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string TargetableTag = "Targetable";

    private List<GameObject> Targetables = new();
    public float Speed = 1f;
    public float RotationSpeed = 10000f;
    public float Damage = 20.0f;

    void Start()
    {
        UpdateTargetables();
    }

    void Update()
    {
        var target = SelectTarget();

        float angle = Mathf.Atan2(target.transform.position.y - transform.position.y, target.transform.position.x - transform.position.x) * Mathf.Rad2Deg;

        transform.SetPositionAndRotation(
            Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * Speed),
            Quaternion.RotateTowards(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * RotationSpeed));
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayerTag) ||  collision.gameObject.tag == TargetableTag)
        {
            collision.gameObject.ConvertTo<Killable>().Damage(Damage);
        }
    }

    private GameObject SelectTarget()
    {
        var targetables = Targetables.Select(target => new TargetableGameObjects
        {
            Target = target,
            Distance = Vector3.Distance(target.transform.position, transform.position)
        });

        return targetables.Aggregate((currentMin, targetable) => targetable.Distance < currentMin.Distance ? targetable : currentMin).Target;
    }

    private void UpdateTargetables()
    {
        Targetables.ForEach(x => x.ConvertTo<Killable>().OnKilled.RemoveListener(UpdateTargetables));

        Targetables.Add(GameObject.FindGameObjectWithTag(PlayerTag));
        Targetables.AddRange(GameObject.FindGameObjectsWithTag(TargetableTag));

        Targetables.ForEach(x => x.ConvertTo<Killable>().OnKilled.AddListener(UpdateTargetables));
    }

    private class TargetableGameObjects
    {
        public GameObject Target;
        public float Distance;
        public bool Killable;
    }
}
