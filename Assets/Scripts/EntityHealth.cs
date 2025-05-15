using System;
using UnityEngine;

public class EntityHealth : MonoBehaviour
{
    public int health;
    public int maxHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void Damage(int damage)
    {
        health -= damage;
    }

    public void ApplyPhysics(Vector3 hitPoint)
    {
        Vector3 hitNormal = new(hitPoint.x - transform.position.x, 1, hitPoint.z - transform.position.z);
        Debug.DrawRay(transform.position, hitNormal*100, Color.red, 100f);
        hitNormal.Normalize();
        GetComponent<Rigidbody>().AddForce(-hitNormal * 10, ForceMode.Impulse);
    }
}
