using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using Events.World_Events;

public class WorldEventsManager : MonoBehaviour {

    public static WorldEventsManager Instance = null;
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
        var eventName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(eventType.ToString());
        string noSpacesTraitName = Utilities.RemoveAllWhiteSpace(eventName);
        string typeName = $"Events.World_Events.{ noSpacesTraitName }";
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
            if (currEvent.CanProvideAnyNeededEffects(effects) && currEvent.CanSpawnEventAt(region, spawner)) {
                events.Add(currEvent);
            }
        }
        return events;
    }
    public bool CanSpawnEventWithEffects(Region region, Character spawner, WORLD_EVENT_EFFECT[] effects) {
        for (int i = 0; i < worldEvents.Length; i++) {
            WorldEvent currEvent = worldEvents[i];
            if (currEvent.CanSpawnEventAt(region, spawner) && currEvent.CanProvideAnyNeededEffects(effects)) {
                return true;
            }
        }
        return false;
    }
    public bool DoesJobProduceWorldEvent(JOB_TYPE job) {
        return WorldEventsDB.jobEventsDB[job].neededEffects != null;
    }
    public WORLD_EVENT_EFFECT[] GetNeededEffectsOfJob(JOB_TYPE job) {
        return WorldEventsDB.jobEventsDB[job].neededEffects;
    }
    public Region GetValidRegionToMoveTo(JOB_TYPE job, Character character) {
        return WorldEventsDB.jobEventsDB[job].validRegionGetter.Invoke(character, job);
    }
    #endregion

    #region World State
    public void SetIsCultActive(bool state) {
        isCultActive = state;
    }
    #endregion
}
