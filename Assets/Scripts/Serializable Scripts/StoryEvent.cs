using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

//NOTE: All of these should only contain data.
//Reference for XML Serialization: https://wiki.unity3d.com/index.php/Saving_and_Loading_Data:_XmlSerializer
[System.Serializable]
public class StoryEvent {
    [XmlAttribute("id")]
    public string id;
    [XmlAttribute("name")]
    public string name;
    public string text;
    [XmlArrayItem("choice")]
    public StoryEventChoice[] choices;
    [XmlArrayItem("effect")]
    public StoryEventEffect[] effects; //NOTE: All effects have individual chances. All effects that have 100% chance will be given, and all other effects that are below 100 will be pooled and only one of those will be given.
    [XmlAttribute("trigger")]
    public STORY_EVENT_TRIGGER trigger; //when should this event happen?

    //Misc Attributes
    [XmlAttribute("load")]
    public string loadEvent; //Used for when event to be used is a pre made event (Enter event id to load that event). This is meant to be used for choices
}
[System.Serializable]
public class StoryEventChoice {
    public string text;
    [XmlAttribute("reqType")]
    public string reqType; //Summon/Artifact
    [XmlAttribute("reqName")]
    public string reqName; //Summon Name/ Artifact Name
    [XmlElement("event")]
    public StoryEvent eventToExecute;
}
[System.Serializable]
public class StoryEventEffect {
    [XmlAttribute("effect")]
    public string effect; //Gain or Lose
    [XmlAttribute("type")]
    public string effectType; //Gain or Lose what? e.g. Summon/Artifact/Level
    [XmlAttribute("object")]
    public string effectValue; //Given the type, what of that type should be gained or lost e.g. Wolf/Necronomicon/1. NOTE: if no value was specified. Randomize.
    [XmlAttribute("chance")]
    public int effectChance; //The chance that this effect will execute.
    public string additionalText; //this is text that will be concatenated with the event text, should this effect happen

    /*
     Example:
     effect = "Gain"
     effectType = "Summon"
     effectValue = "Wolf"
     effectChance = 50
     This means that the player has a 50% chance to gain a wolf summon
     */
}

public enum STORY_EVENT_TRIGGER {
    IMMEDIATE,
    MID,
    END,
}