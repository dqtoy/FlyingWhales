using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

public class StoryEventsManager : MonoBehaviour {

    public static StoryEventsManager Instance = null;

    private Dictionary<TILE_TAG, StoryEventsContainer> eventsDatabse; //This is a dictionary of all events that can spawn per tile tag.
    //NOTE: Story events do not care what tile tag they are spawned in, so in order for an event to spawn in a specific tile tag, that event must be put in the Tile Tag XML File.
    
    private void Awake() {
        Instance = this;
    }

    public void Initialize() {
        LoadEvents();
    }

    private void LoadEvents() {
        eventsDatabse = new Dictionary<TILE_TAG, StoryEventsContainer>();
        //Not sure if it is ok to load all the events upon starting the game as I don't think that it would scale well. Might transition to getting events from file every time. But for speed sake, will load them all.
        string basePath = Application.streamingAssetsPath + "/Story Events/";
        TILE_TAG[] tileTags = Utilities.GetEnumValues<TILE_TAG>();
        for (int i = 0; i < tileTags.Length; i++) {
            TILE_TAG currTag = tileTags[i];
            string path = basePath + currTag.ToString() + ".xml";
            var serializer = new XmlSerializer(typeof(StoryEventsContainer));
            var stream = new FileStream(path, FileMode.Open);
            StoryEventsContainer container = serializer.Deserialize(stream) as StoryEventsContainer;
            eventsDatabse.Add(currTag, container);
            stream.Close();
        }
    }

    public List<StoryEvent> GetPossibleEventsForTile(HexTile tile) {
        List<StoryEvent> events = new List<StoryEvent>();
        for (int i = 0; i < tile.tileTags.Count; i++) {
            TILE_TAG currTag = tile.tileTags[i];
            if (eventsDatabse.ContainsKey(currTag)) {
                events.AddRange(eventsDatabse[currTag].events);
            } else {
                Debug.LogWarning("There is no event database for tag " + currTag.ToString());
            }
        }
        return events;
    }
}

[XmlRoot("EventsCollection")]
public class StoryEventsContainer {
    [XmlArray("Events")]
    [XmlArrayItem("Event")]
    public List<StoryEvent> events = new List<StoryEvent>();

    public void Save(string path) {
        var serializer = new XmlSerializer(typeof(StoryEventsContainer));
        using (var stream = new FileStream(path, FileMode.Create)) {
            serializer.Serialize(stream, this);
        }
    }

    public static StoryEventsContainer Load(string path) {
        var serializer = new XmlSerializer(typeof(StoryEventsContainer));
        using (var stream = new FileStream(path, FileMode.Open)) {
            return serializer.Deserialize(stream) as StoryEventsContainer;
        }
    }

    //Loads the xml directly from the given string. Useful in combination with www.text.
    public static StoryEventsContainer LoadFromText(string text) {
        var serializer = new XmlSerializer(typeof(StoryEventsContainer));
        return serializer.Deserialize(new StringReader(text)) as StoryEventsContainer;
    }
}

//old code
/*
 * StoryEventsContainer container = new StoryEventsContainer();
        StoryEvent newEvent = new StoryEvent() {
            id = "AN_INCUBUS",
            name = "An Incubus",
            text = "While exploring you encounter an incubus. After conversing a bit you learn that he is very interested in ancient artifacts.",
            choices = new StoryEventChoice[] {
                new StoryEventChoice {
                    text = "Show him your Necronomicon.",
                    reqType = "Artifact",
                    reqName = "Necronomicon",
                    eventToExecute = new StoryEvent() {
                        effects = new StoryEventEffect[] {
                            new StoryEventEffect() {
                                effect = "Gain",
                                effectType = "Summon",
                                effectValue = "Incubus",
                                effectChance = 100
                            }
                        }
                    }
                },
                new StoryEventChoice {
                    text = "Ask him if he knows anything that may help you in conquering this region.",
                },
                new StoryEventChoice {
                    text = "Take your leave.",
                }
            }
        };
        container.events.Add(newEvent);
        TILE_TAG[] tileTags = Utilities.GetEnumValues<TILE_TAG>();
        for (int i = 0; i < tileTags.Length; i++) {
            container.Save(Application.streamingAssetsPath + "/Story Events/" + tileTags[i].ToString() + ".xml");
        }
*/
