using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

public class StoryEventsManager : MonoBehaviour {

    public static StoryEventsManager Instance = null;

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
