using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

// Every dialogue will be split into a string followed by a newline
// Any new dialogue will be followed by two newlines
// This will make it more difficult to write dialogue and order it but,
// it should work

// Dialogue is stored as a List<List<string>>
// Where dialogue[i] is the index for which dialogue path to start
// and dialogue[i][j] is the index for which line to start using

public static class DialogueParser
{
    
    public static List<List<string>> ParseFromURL(string url)
    {
        Debug.Log(url);
        // 1. Create a text asset from the parsed URL
        var otherTextAsset = Resources.Load<TextAsset>(url);
        
        // 2. Split the string into paragraphs
        var paragraphs = otherTextAsset.text.Split("\n\n").ToList();
        
        // 3. Iterate through the paragraphs and add each line to the index of the
        //    paragraph
        var dialogue = paragraphs.Select(paragraph => paragraph.Split("\n").ToList()).ToList();

        // 4. Check if the last line is empty, if so, remove
        if (dialogue[^1][0].Length < 1) dialogue[^1].RemoveAt(dialogue[^1].Count - 1);
        
        Debug.Log(dialogue.Count);
        Debug.Log(dialogue[0].Count);
        
        // 5. Return the dialogue as a List<List<string>>
        return dialogue;
    }

    public static DialogueSystem ParseFromJson(string url)
    {
        var textAsset = Resources.Load<TextAsset>(url);
        
        Debug.Log(textAsset.text);
        var dialogue = JsonConvert.DeserializeObject<DialogueSystem>(textAsset.text);

        Debug.Log(dialogue.Dialogue["d_0_0"].Text);
        Debug.Log(dialogue.Response["r_0_0"].Choices[0].NextID);
        
        return dialogue;
    }
}

[Serializable]
public class DialogueSystem
{
    public Dictionary<string, DialogueEntry> Dialogue { get; set; }
    public Dictionary<string, ResponseEntry> Response { get; set; }
}

[Serializable]
public class DialogueEntry
{
    [JsonProperty("nextType")]
    public string NextType { get; set; }
    [JsonProperty("next")]
    public string NextID { get; set; }
    [JsonProperty("line")]
    public string Text { get; set; }
}

[Serializable]
public class ResponseEntry
{
    public List<DialogueEntry> Choices { get; set; }
}