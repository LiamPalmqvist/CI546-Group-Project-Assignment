using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.SceneManagement.SceneManager;

public class LevelTransition : MonoBehaviour
{
    // Object Type "Scene" can't be assigned in the editor so we use a string name instead 
    public string nextScene;
    public string requiredItem = "";
    private void OnTriggerEnter(Collider triggerCollider)
    {
        Debug.Log("Entered" + triggerCollider.gameObject.tag);
        // check if the collider's tag == "Player"
        if (!triggerCollider.gameObject.CompareTag("Player")) 
            return;
        
        Debug.Log("Entered" + triggerCollider.gameObject.name);
        
        // Then check if the required item is set or null
        // (This determines whether the player needs an item to progress)
        if (requiredItem != "")
        {
            // if the required item isn't in the player's inventory
            if (!InventoryManager.instance.CheckInventoryForItem(requiredItem))
            {
                Debug.Log(requiredItem);
                Debug.Log($"{requiredItem} Not in player's inventory");
                return;
            }
            // return
        }

        // Otherwise, load the next scene
        LoadScene(nextScene);
        Debug.Log($"nextScene: {nextScene}");
    }
}
