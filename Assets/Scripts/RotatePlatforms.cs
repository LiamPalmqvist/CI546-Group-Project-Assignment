using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.Rendering;

public class RotatePlatforms : MonoBehaviour
{
    public GameObject platformPrefab; // The prefab to spawn
    public float rotationSpeed; // The speed of the rotation
    public int platformCount; // The amount of platforms to spawn
    // Booleans that decide which direction the platform will rotate
    public bool rotateX;
    public bool rotateY;
    public bool rotateZ;

    public List<Transform> children = new();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        var positions = points_on_circle(platformCount);
        platformPrefab.transform.localScale = Vector3.one;
        
        for (var i = 0; i < positions.Count; i++)
        {
            var platform = Instantiate(platformPrefab, new Vector3(positions[i].x, positions[i].y, 0) * 5 + transform.position, transform.rotation, transform);
            children.Add(platform.transform);
        }
    }

    private static List<Vector2> points_on_circle(int samples)
    {
        List<Vector2> points = new();
        
        // calculate the spacing in radians
        var spacing = 360 / (float)samples * (math.PI/180);
        
        // iterate through number of samples
        for (var i = 0; i < samples; i++)
        {
            // sin(spacing * i), cos(spacing * i)
            points.Add(new Vector2(math.sin(spacing*i), math.cos(spacing*i)));
        }

        return points;
    }
    

    // Update is called once per frame
    private void Update()
    {
        foreach (var child in children)
        {
            if (rotateX)
            {
                child.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
                child.rotation = new Quaternion();
            } 
            
            if (rotateY)
            {
                child.RotateAround(transform.position, Vector3.right, rotationSpeed * Time.deltaTime);
                child.rotation = new Quaternion();
            } 
            
            if (rotateZ)
            {
                child.RotateAround(transform.position, Vector3.forward, rotationSpeed * Time.deltaTime);
                child.rotation = new Quaternion();
            }
        }
    }
}
