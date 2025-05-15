using UnityEngine;
using System.Collections.Generic;
using TMPro;
public class ItemInteract : MonoBehaviour
{
    [Header("Misc. variables")]
    private List<List<string>>  Dialogue;

    [Header("UI Elements")] 
    public Canvas DialogueCanvas; // The canvas the dialogue is rendered on
    public TMP_Text TextObject; // The actual text being rendered on the text box
    public TMP_Text NameTextObject; // The name of the item
    
    [Header("Talking variables")] 
    public bool interactionStarted = false;
    public bool finishedRendering = false;
    public int dialogueNumber = 0; // Which paragraph to start reading from
    public int currentLine = 0; // Which line is being displayed
    public int currentCharacter = 0; // The character that is being displayed
    
    private void Start()
    {
        DialogueCanvas = GameObject.Find("Canvas").transform.GetChild(0).GetChild(2).gameObject.GetComponent<Canvas>();
        TextObject = DialogueCanvas.transform.GetChild(1).GetComponent<TMP_Text>();
        Dialogue = DialogueParser.ParseFromURL($"Dialogue/FullInventory");
        var parentName = transform.parent.name;
        Debug.Log(parentName);
        NameTextObject.text = parentName;
        // This should instead use the name of the object
    }

    // Update is called once per frame
    private void Update()
    {
        if (interactionStarted)
        {
            RenderNextChar();
        }
    }

    public void DisplayInventoryFull()
    {
        interactionStarted = true;
        TextObject.text = "";
        DialogueCanvas.gameObject.SetActive(true);
        finishedRendering = false;
    }

    public void EndInteraction()
    {
        TextObject.text = "";
        currentCharacter = 0;
        currentLine = 0;
        dialogueNumber = 0;
        finishedRendering = false;
        DialogueCanvas.gameObject.SetActive(false);
        interactionStarted = false;
    }
    
    private void RenderNextChar()
    {
        if (finishedRendering) return;
        
        // Debug.Log(Dialogue[dialogueNumber].Count);
        // Debug.Log(Dialogue[dialogueNumber][currentLine][currentCharacter]);
        // Debug.Log($"{currentCharacter}: {dialogueNumber}: {currentLine}");
        if (currentCharacter + 1 >= Dialogue[dialogueNumber][currentLine].Length) finishedRendering = true;
    
        
        TextObject.text += Dialogue[dialogueNumber][currentLine][currentCharacter];
        currentCharacter++;
    }
    
    public void IncreaseDialogueNumber() => dialogueNumber++;

    public void IncreaseLineNumber()
    {
        TextObject.text = "";
        currentCharacter = 0;
        finishedRendering = false;
        currentLine++;
    }

    // Returns true IF
    // 1. the current character is equal to or greater than the length of the current line
    // 2. the current line is the last one in the current dialogue
    public bool IsFinished() => 
        (currentCharacter >= Dialogue[dialogueNumber][currentLine].Length - 1) && 
        (currentLine >= Dialogue[dialogueNumber].Count - 1);
}

