using UnityEngine;

public class AnimateWater : MonoBehaviour
{
    public Texture2D[] textureArray;
    public Renderer m_Renderer;
    public float changeInterval = 0.33F;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // If there aren't any textures in the array, return
        if (textureArray.Length == 0) return;
        
        // The index becomes the Time since start divided by the change interval
        int index = Mathf.FloorToInt(Time.time / changeInterval);
        // Which then gets assigned to a number between 0 and the length of the texture array
        index %= textureArray.Length;
        // Which is then set as the main texture
        m_Renderer.material.mainTexture = textureArray[index];
    }
}
