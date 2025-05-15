using System;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public float rotationSpeed;

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(Vector3.up, Time.deltaTime * rotationSpeed);
    }

    private void OnTriggerEnter(Collider c)
    {
        if (!c.gameObject.CompareTag("Player")) return;
        c.gameObject.GetComponent<PlayerControls>().inventoryManager.AddCoin();
        transform.position = new Vector3(0, -1000, 0);
    }
}
