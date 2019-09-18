using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

public class StoryEventsManager : MonoBehaviour {

    public static StoryEventsManager Instance = null;

    private Dictionary<TILE_TAG, StoryEventsContainer> eventsDatabse; //This is a dictionary of all events that can spawn per tile tag.
    //NOTE: Story events do not care what tile tag they are spawned in, so in order for an event to spawn in a specific tile tag, that event must be put in the Tile Tag XML File.
    
    public static StoryEventChoice continueChoice = new StoryEventChoice() { text = "Continue..."};

    private WorldEvent[] worldEvents;

    //world states
    public bool isCultActive { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public void Initialize() {
        //LoadEvents();
        LoadWorldEvents();
    }

    #region World Events
    private void LoadWorldEvents() {
        List<WORLD_EVENT> events = Utilities.GetEnumValues<WORLD_EVENT>().ToList();
        events.RemoveAt(0); //Do not include NONE
        worldEvents = new WorldEvent[events.Count];
        for (int i = 0; i < events.Count; i++) {
            WORLD_EVENT currType = events[i];
            WorldEvent eventObj = CreateNewWorldEvent(currType);
            if (eventObj != null) {
                worldEvents[i] = eventObj;
            } else {
                throw new System.Exception("Could not create world event class for type " + currType.ToString() + ". Make sure that it's class is named the EXACT same as its enum value");
            }
        }
    }
    private WorldEvent CreateNewWorldEvent(WORLD_EVENT eventType) {
        var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(eventType.ToString());
        return System.Activator.CreateInstance(System.Type.GetType(typeName)) as WorldEvent;
    }
    public WorldEvent GetWorldEvent(WORLD_EVENT eventType) {
        for (int i = 0; i < worldEvents.Length; i++) {
            WorldEvent currEvent = worldEvents[i];
            if (currEvent.eventType == eventType) {
                return currEvent;
            }
        }
        return null;
    }
    public List<WorldEvent> GetEventsThatCanProvideEffects(Region region, Character spawner, WORLD_EVENT_EFFECT[] effects) {
        List<WorldEvent> events = new List<WorldEvent>();
        for (int i = 0; i < worldEvents.Length; i++) {
            WorldEvent currEvent = worldEvents[i];
            if (currEvent.CanSpawnEventAt(region, spawner) && currEvent.CanProvideNeededEffects(effects)) {
                events.Add(currEvent);
            }
        }
        return events;
    }
    #endregion

    #region World State
    public void SetIsCultActive(bool state) {
        isCultActive = state;
    }
    #endregion
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
                        text = "Amazed by your Necronomicon, he decides to join you so he can study it.",
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
                    eventToExecute = new StoryEvent() {
                        text = "Upon browsing his books, he shows you a curious block of text.",
                        effects = new StoryEventEffect[] {
                            new StoryEventEffect() {
                                effect = "Gain",
                                effectType = "Intervention_Ability",
                                effectValue = "Provoke",
                                effectChance = 100,
                                additionalText = "It teaches you how to Provoke!"
                            }
                        }
                    }
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
