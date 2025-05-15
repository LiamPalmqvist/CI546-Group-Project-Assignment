using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EntityInteract : MonoBehaviour
{
    [Header("Dialogue variables")]
    private DialogueSystem _dialogue; // Dialogue for before the player has retrieved the required item.
    private DialogueSystem _dialogue1; // Dialogue for after the player has retrieved the required item.
    private DialogueEntry _currentDialogueEntry;
    private ResponseEntry _currentResponseEntry;
    private DialogueSystem _currentDialogue; // The currently playing dialogue
    public bool responding;
    public int responseChoice;
    public GameObject responsePrefab;
    public List<GameObject> responses = new();
    public string requiredItem;
    
    [Header("UI Elements")]
    public GameObject textBox; // The actual rendered text box
    public GameObject button; // The A-Button
    public GameObject textCanvas; // The Canvas the text is rendered on
    public TMP_Text textObject; // The actual text being rendered on the text box
    public TMP_Text nameTextObject; // The text object for the name label
    public GameObject selectorImage; // The box that is used to distinguish the player's current selected option
    
    [Header("Talking variables")] 
    public bool interactionStarted;
    public int currentCharacter; // The character that is being displayed
    public bool finishedRendering;

    private void Start()
    {
        var parentName = transform.parent.name;
        _dialogue = DialogueParser.ParseFromJson($"Dialogue/{parentName}");
        _dialogue1 = DialogueParser.ParseFromJson(requiredItem != "" ? $"Dialogue/{parentName}1" : $"Dialogue/{parentName}");
        nameTextObject.text = parentName.Replace("_", " ");

    // This should instead use the name of the object
    }

    // Update is called once per frame
    private void Update()
    {
        if (!interactionStarted || responding) return;
        
        RenderNextChar();
    }

    private void RenderNextChar()
    {
        if (finishedRendering) return;
        
        if (!responding)
            if (currentCharacter + 1 >= _currentDialogueEntry.Text.Length) finishedRendering = true;
        
        textObject.text += _currentDialogueEntry.Text[currentCharacter];
        currentCharacter++;
    }

    public void StartInteraction()
    {
        // Set initial variables when interaction starts
        interactionStarted = true;
        button.SetActive(false);
        textBox.SetActive(true);
        textCanvas.SetActive(true);
        responses = DestroyChoices(responses);
        responding = false;
        finishedRendering = false;
        _currentDialogue = InventoryManager.instance.CheckInventoryForItem(requiredItem) ? _dialogue1 : _dialogue;
        
        _currentDialogueEntry = _currentDialogue.Dialogue["d_0_0"];
        _currentResponseEntry = _currentDialogue.Response["r_0_0"];
    
        currentCharacter = 0;
    }

    public void EndInteraction()
    {
        // disable any UI object enabled when started
        interactionStarted = false;
        selectorImage.SetActive(false);
        button.SetActive(true);
        textBox.SetActive(false);
        textCanvas.SetActive(false);
        textObject.text = "";
        responseChoice = 0;
    }

    public void NextLine()
    {
        // Check if there are previous responses
        if (responses.Count > 0) responses = DestroyChoices(responses);
        
        // Check if player is responding to a dialogue box
        var nextType = responding ? _currentResponseEntry.Choices[responseChoice].NextType : _currentDialogueEntry.NextType;
        var nextID = responding ? _currentResponseEntry.Choices[responseChoice].NextID : _currentDialogueEntry.NextID;
        
        switch (nextType)
        {
            case "d":
                _currentDialogueEntry = _currentDialogue.Dialogue[nextID];
                responding = false;
                finishedRendering = false;
                textObject.text = "";
                break;
            case "r":
                _currentResponseEntry = _currentDialogue.Response[nextID];
                responses = ShowChoices(_currentDialogue.Response[_currentDialogueEntry.NextID]);
                responding = true;
                finishedRendering = true;
                break;
            case "e":
                Debug.Log("END");
                EndInteraction();
                break;
        }
        
        currentCharacter = 0;
    }
    
    private List<GameObject> ShowChoices(ResponseEntry choices)
    {
        List<GameObject> choiceBoxes = new();

        // Set constants for positioning
        const float verticalSpacer = 120;

        // Assign initial height
        Vector3 position = new();

        foreach (var choice in choices.Choices)
        {
            var choiceInstance = Instantiate(responsePrefab, textCanvas.transform, false);
            choiceInstance.GetComponentInChildren<TMP_Text>().text = choice.Text;
            choiceBoxes.Add(choiceInstance);
            choiceInstance.transform.localPosition += position;
            position.y -= verticalSpacer;
        }
        
        selectorImage.SetActive(true);
        selectorImage.transform.localPosition = new Vector3(400, -200, 0);
        responseChoice = 0;
        return choiceBoxes;
    }

    private List<GameObject> DestroyChoices(List<GameObject> choices)
    {
        selectorImage.SetActive(false);
        selectorImage.transform.localPosition -= new Vector3(400, -200, 0);
        // Destroy each instantiated choice
        foreach (var choice in choices) Destroy(choice);
        // return a new list
        return new List<GameObject>();
    }

    public void SubmitChoice()
    {
        _currentDialogueEntry = _currentDialogue.Dialogue[_currentResponseEntry.Choices[responseChoice].NextID];
        responses = DestroyChoices(responses);
    }

    public bool IsFinished()
    {
        // Check if the player is responding
        if (!responding) return (currentCharacter >= _currentDialogueEntry.Text.Length - 1 && _currentDialogueEntry.NextType == "e");
        // if so, check that the currently selected option's next type is "e"
        if (_currentResponseEntry.Choices[responseChoice].NextType == "e")
        {
            // if so, player is done speaking to NPC and can exit
            // NOTE: THIS SHOULD BE CALLED WHEN THE PLAYER PRESSES A BUTTON
            // TO ADVANCE TEXT WHEN INTERACTING
            return true;
        }
        // if not, check if player's current dialogue box text length is as long as the current line's length
        // and that the next type of line is of type "e"
        var boolean = (currentCharacter >= _currentDialogueEntry.Text.Length - 1 && _currentDialogueEntry.NextType == "e");
        Debug.Log(boolean);
        return boolean;
    }

    public void ChangeSelection(float f)
    {
        // Increase or decrease the count depending on the player's input
        switch (f)
        {
            case < 0:
                responseChoice++;
                break;
            case > 0:
                responseChoice--;
                break;
        }

        // loop over if at top or bottom choice
        if (responseChoice < 0) responseChoice = responses.Count - 1;
        else if (responseChoice >= responses.Count) responseChoice = 0;
        
        // set selector's position to -200 minus 120 multiplied by the current selection's index
        selectorImage.transform.localPosition = new Vector3(400, -200 - 120 * responseChoice, 0);
    }

    public void GetHurt()
    {
        Debug.Log("OW");
    }
}