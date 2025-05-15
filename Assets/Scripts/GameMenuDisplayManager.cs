using UnityEngine;
using UnityEngine.Serialization;

public class GameMenuDisplayManager : MonoBehaviour
{
    private static readonly int Open = Animator.StringToHash("Open");
    public GameObject inventory;
    public GameObject mainMenu;
    public InventoryManager inventoryManager;
    
    private Animator _inventoryAnimation;
    private Animator _mainMenuAnimation;
    public void Start()
    {
        Debug.Log("InventoryDisplayManager");
        _inventoryAnimation = inventory.GetComponent<Animator>();
        _mainMenuAnimation = mainMenu.GetComponent<Animator>();
        inventoryManager = InventoryManager.instance;
        inventoryManager.UpdateInventoryAllGUI();
        Cursor.lockState = CursorLockMode.Locked;
        // Update the inventory once when the game loads
    }

    public void OpenInventory()
    {
        _inventoryAnimation.SetBool(Open, true);
        Debug.Log("INVENTORY OPEN");
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseInventory()
    {
        _inventoryAnimation.SetBool(Open, false);
        Debug.Log("INVENTORY CLOSE");
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OpenMainMenu()
    {
        _mainMenuAnimation.SetBool(Open, true);
        Debug.Log("MENU OPEN");
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseMainMenu()
    {
        _mainMenuAnimation.SetBool(Open, false);
        Debug.Log("MENU CLOSE");
        Cursor.lockState = CursorLockMode.Locked;
    }
}