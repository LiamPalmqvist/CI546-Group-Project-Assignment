using System;
using UnityEngine;

public class AttackEntity : MonoBehaviour
{
    public GameObject origin;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Entity")) return;
        
        EntityHealth script = other.gameObject.GetComponent<EntityHealth>();
        script.Damage(10);
        script.ApplyPhysics(origin.transform.position);
    }
}
