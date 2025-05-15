using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;

public class InventoryManager : MonoBehaviour
{
    // Enables sharing between scenes
    public static InventoryManager instance;
    // File location
    string uri;
    
    // TODO: Load inventory to and from save file
    // This is the inventory
    private List<Item> Items { get; } = new();
    private const int MaxInventoryLength = 25;
    
    // This is the amount of coins the player has in their inventory
    private int _coinCount = 0;

    // The item the player can use?
    // Unused right now
    public Item activeItem;
    // Unused
    public bool canDoubleJump;
    
    // public variable which is assigned to the index of the item in Items?
    // I honestly don't remember what this does
    public Item this[int index] => Items[index];
    
    private void Awake()
    {
        uri = Application.persistentDataPath + "/inventory.json";
        // Check if an instance of this class exists
        if (instance != null)
        {
            // if so, destroy it
            Destroy(gameObject); 
            return;
        }

        // Otherwise, set the instance to this
        instance = this;
        // and make persistent
        DontDestroyOnLoad(gameObject);

        
        // Check if the items file exists
        if (!File.Exists(uri)) return;
        
        // if so, load the items from the file
        var loadedItems = LoadFromFile();
        // and add them to the Items variable
        Items.AddRange(loadedItems);
        // Finally, update the GUI
        UpdateInventoryAllGUI();
    }

    private List<Item> LoadFromFile()
    {
        JsonSerializer serializer = new JsonSerializer();
        FileStream fileStream = new FileStream(uri, FileMode.Open);
        JsonTextReader reader = new JsonTextReader(new StreamReader(fileStream));
        List<Item> items = serializer.Deserialize<List<Item>>(reader);
        fileStream.Close();
        return items;
    }

    public void SaveToFile()
    {
        // Create a new Json Serializer and create/open the file to write to
        JsonSerializer serializer = new JsonSerializer();
        FileStream file = File.Open(uri, FileMode.OpenOrCreate, FileAccess.ReadWrite);

        // If the file already has content
        if (file.Length != 0)
        {
            // set the length to zero for writing
            file.SetLength(0);
        }
        
        // Start to write
        JsonWriter writer = new JsonTextWriter(TextWriter.Synchronized(new StreamWriter(file)));
        serializer.Serialize(writer, Items);
        writer.Flush();
        writer.Close();
        
        
        Debug.Log(Application.persistentDataPath + "/inventory.json");
    }
    
    // adding an item to the inventory is as simple as just
    // adding either a single item if the second parameter isn't passed
    // or adding the amount passed
    /// <summary>
    /// <para>Adds an item to the player's inventory. If the player's inventory is full, returns -1, otherwise returns 1</para>
    /// </summary>
    /// <param name="itemName">The name of the item to add</param>
    /// <param name="amount">The amount to add</param>
    /// <returns>`false` if full || `true` if not full</returns>
    public bool AddItem(string itemName, int amount)
    {
        // First, find the index of the item with its name
        // if it returns -1, the item does not exist and will be added
        // to the inventory
        var index = Items.FindIndex(item => item.GetName() == itemName);
        if (index == -1)
        {
            // Second, check if the current length of the inventory is less than
            // or equal to the MaxInventoryLength
            if (Items.Count >= MaxInventoryLength)
            {
                // Returns -1 if item could not be added to inventory
                return false;
            }
            // Otherwise, add item to the inventory
            Items.Add(new Item(itemName, amount));
        }
        else
        {
            // Otherwise, add item amount to the existing item
            Items[index].AddAmount(amount);
        }
        
        UpdateInventorySlotGUI(index);

        // Finally, return 1 for success
        return true;
    }
    
    // adding an item to the inventory is as simple as just
    // adding either a single item if the second parameter isn't passed
    // or adding the amount passed
    /// <summary>
    /// <para>Adds an item to the player's inventory. If the player's inventory is full, returns -1, otherwise returns 1</para>
    /// </summary>
    /// <param name="itemName">The name of the item to add</param>
    /// <returns>`false` if full || `true` if not full</returns>
    public bool AddItem(string itemName)
    {
        // First, find the index of the item with its name
        // if it returns -1, the item does not exist and will be added
        // to the inventory
        var index = Items.FindIndex(item => item.GetName() == itemName);
        if (index == -1)
        {
            // Second, check if the current length of the inventory is less than
            // or equal to the MaxInventoryLength
            if (Items.Count >= MaxInventoryLength)
            {
                // Returns -1 if item could not be added to inventory
                return false;
            }
            // Otherwise, add item to the inventory
            Items.Add(new Item(itemName, 1));
            // And update the inventory slot the item was added to
            UpdateInventorySlotGUI(Items.FindIndex(item => item.GetName() == itemName));
        }
        else
        {
            // Otherwise, add item amount to the existing item
            Items[index].AddAmount(1);
            UpdateInventorySlotGUI(index);
        }
        
        // Finally, return 1 for success
        return true;
    }

    // removing the item is as simple as either passing an index
    // and an amount or a name and an amount
    /// <summary><para>Removes an item from the inventory at index specified by the `index` parameter. If an amount is included, it will only remove the amount specified.</para></summary>
    /// <param name="index">The predicate delegate that specifies the index to remove the item at.</param>
    /// <param name="amount">The amount to remove</param>
    public void RemoveItem(int index, int amount)
    {
        // if the index of the item passed is bigger than the length of the current inventory
        // return
        if (Items.Count <= index)
        {
            Debug.LogError("ValueError: Inventory is smaller than index");
            return;
        }
        
        // If the amount to remove is less than the amount currently held, remove that amount
        if (amount < Items[index].GetAmount())
        {
            Items[index].RemoveAmount(amount);
            UpdateInventorySlotGUI(index);
            return;
        }
        
        // Otherwise, remove item entirely
        Items.RemoveAt(index);
        UpdateInventorySlotGUI(index);
    }

    /// <summary><para>Removes an item from the inventory that matches the `itemName` parameter. If an amount is included, it will only remove the amount specified.</para></summary>
    /// <param name="itemName">The predicate delegate that specifies the item to search for.</param>
    /// <param name="amount">The amount to remove</param>
    public void RemoveItem(string itemName, int amount)
    {
        var index = Items.FindIndex(item => item.GetName() == itemName);
        if (index == -1)
        {
            Debug.LogError($"NameError: Inventory does not contain item {itemName}");
            return;
        }
        // This will only make the amount 1 or higher
        if (amount < Items[index].GetAmount())
        {
            Items[index].RemoveAmount(amount);
            UpdateInventorySlotGUI(index);
        }
        // otherwise it will remove the item entirely
        else
        {
            Items.RemoveAt(index);
            UpdateInventorySlotGUI(index);
        }
    }
    
    // removing the item is as simple as either passing an index
    // and an amount or a name and an amount
    /// <summary><para>Removes an item from the inventory at index specified by the `index` parameter. If an amount is included, it will only remove the amount specified.</para></summary>
    /// <param name="index">The predicate delegate that specifies the index to remove the item at.</param>
    public void RemoveItem(int index)
    {
        // if the index of the item passed is bigger than the length of the current inventory
        // return
        if (Items.Count <= index)
        {
            Debug.LogError("ValueError: Inventory is smaller than index");
            return;
        }
        
        // If the amount to remove is less than the amount currently held, remove that amount
        if (1 < Items[index].GetAmount())
        {
            Items[index].RemoveAmount(1);
            UpdateInventorySlotGUI(index);
            return;
        }
        
        // Otherwise, remove item entirely
        Items.RemoveAt(index);
        UpdateInventorySlotGUI(index);
    }

    /// <summary><para>Removes an item from the inventory that matches the `itemName` parameter. If an amount is included, it will only remove the amount specified.</para></summary>
    /// <param name="itemName">The predicate delegate that specifies the item to search for.</param>
    public void RemoveItem(string itemName)
    {
        var index = Items.FindIndex(item => item.GetName() == itemName);
        if (index == -1)
        {
            Debug.LogError($"NameError: Inventory does not contain item {itemName}");
            return;
        }
        // This will only make the amount 1 or higher
        if (1 < Items[index].GetAmount())
        {
            Items[index].RemoveAmount(1);
            UpdateInventorySlotGUI(index);
        }
        // otherwise it will remove the item entirely
        else
        {
            Items.RemoveAt(index);
            UpdateInventorySlotGUI(index);
        }
    }

    /// <summary><para>checks if an item exists in the inventory that matches the `itemName` parameter.</para></summary>
    /// <param name="itemName">The predicate delegate that specifies the item to search for.</param>
    /// <returns>`false` if item is not in the player's inventory and `true` if it is</returns>
    public bool CheckInventoryForItem(string itemName) => Items.FindIndex(item => item.GetName() == itemName) != -1;

    /// <summary><para>checks if an item exists in the inventory that matches the `itemName` parameter.</para></summary>
    /// <param name="itemName">The predicate delegate that specifies the item to search for.</param>
    /// <param name="amount">The amount that the player needs.</param>
    public bool CheckInventoryForItem(string itemName, int amount)
    {
        try
        {
            var itemToFind = Items[Items.FindIndex(item => item.GetName() == itemName)];
            return itemToFind.GetAmount() >= amount;
        }
        catch (IndexOutOfRangeException)
        {
            return false;
        }
    } 
    
    /// <summary><para>checks if an item exists in the inventory at the `index` parameter.</para></summary>
    /// <param name="index">The predicate delegate that specifies the item to search for.</param>
    public bool CheckInventoryForItem(int index)
    {
        try
        {
            return Items[index].GetAmount() > 0;
        }
        catch (IndexOutOfRangeException)
        {
            return false;
        }
    }
    /// <summary><para>checks if an item exists in the inventory at the `index` parameter.</para></summary>
    /// <param name="index">The predicate delegate that specifies the item to search for.</param>
    /// <param name="amount">The amount that the player needs.</param>
    public bool CheckInventoryForItem(int index, int amount)
    {
        try
        {
            return Items[index].GetAmount() >= amount;
        }
        catch (IndexOutOfRangeException)
        {
            return false;
        }
    }
    

    public void UpdateInventoryAllGUI()
    {
        var inventory = GameObject.Find("InventoryBackground");
        if (Items.Count == 0) return;
        var children = inventory.transform.GetComponentsInChildren<Transform>();
        for (var i = 0; i < Items.Count; i++)
        {
            var item = inventory.transform.GetChild(i);
            var itemImage = inventory.transform.GetChild(i).GetChild(2).GetComponent<UnityEngine.UI.Image>();
            // TODO: FIX THIS, IMAGE LOADING WORKS BUT DOES NOT ASSIGN
            itemImage.sprite = Resources.Load<Sprite>($"Images/Items/{Items[i].GetName()}");
            item.GetComponentInChildren<TMP_Text>().text = Items[i].GetAmount().ToString();
        }
    }

    private void UpdateInventorySlotGUI(int index)
    {
        // Get the inventory as a gameObject
        var inventory = GameObject.Find("InventoryBackground");
        // Error check for if exists
        if (!inventory) return;

        // Get the image element on the inventory
        var item = inventory.transform.GetChild(index);
        var itemImage = item.transform.GetChild(2).GetComponent<UnityEngine.UI.Image>();
        // and update it
        // TODO: Get the image loading to work, everything else does
        itemImage.sprite = Resources.Load<Sprite>($"Images/Items/{Items[index].GetName()}");
        item.GetComponentInChildren<TMP_Text>().text = Items[index].GetAmount().ToString();
    }

    /// <summary>
    /// Updates the coin GUI whenever the _coinCount variable is modified
    /// </summary>
    private void UpdateCoinGUI()
    {
        // Get the inventory as a gameObject
        var coinCount = GameObject.Find("CoinAmount");
        Debug.Log("Coin counting!");
        // Error check for if exists
        if (!coinCount) return;
        
        // and update it
        // TODO: Get the image loading to work, everything else does
        coinCount.GetComponentInChildren<TMP_Text>().text = _coinCount.ToString();
    }

    /// <summary>
    /// Adds a single coin to the player's inventory and updates the GUI
    /// </summary>
    public void AddCoin()
    {
        _coinCount++;
        UpdateCoinGUI();
    }

    /// <summary>
    /// Adds a specified amount of coins to the player's inventory and updates the GUI
    /// </summary>
    /// <param name="amount">The amount of coins to add</param>
    public void AddCoin(int amount)
    {
        _coinCount += amount;
        UpdateCoinGUI();
    }

    /// <summary><para>checks if the player has more than 1 coin and if so, removes it and returns true as well as updates the GUI, otherwise returns false</para></summary>
    /// <returns>returns `true` if there is more than one coin in the player's inventory and false if not</returns>
    public bool RemoveCoin()
    {
        if (_coinCount <= 0)
        {
            _coinCount = 0;
            return false;
        }
        else
        {
            _coinCount--;
            UpdateCoinGUI();
            return true;
        }
    }
    /// <summary><para>checks if the player has more than 1 coin and if so, removes it and returns true as well as updates the GUI, otherwise returns false</para></summary>
    /// <param name="amount">The amount of coins to remove</param>
    /// <returns>returns `true` if there is more than the amount of coins attempted to be removed in the player's inventory and false if not</returns>
    public bool RemoveCoin(int amount)
    {
        if (_coinCount < amount) return false;
        _coinCount -= amount;
        return true;
    }
}

[Serializable]
public class Item
{
    [JsonProperty("Name")]
    private string Name { get; }
    [JsonProperty("Amount")]
    private int Amount { get; set;  }

    public Item(string name, int amount)
    {
        this.Name = name;
        this.Amount = amount;
    }
    
    public string GetName() { return this.Name; }
    public int GetAmount() { return Amount; }
    public void AddAmount(int amount) => Amount += amount;
    public void RemoveAmount(int amount) => Amount -= amount;
}

[Serializable]
public class Inventory
{
    public List<Item> Items { get; }

    public Inventory(List<Item> items)
    {
        Items = items;
    }
}