using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

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

    #region Utilities
    public bool DoesPlayerMeetChoiceRequirement(StoryEventChoice choice) {
        if (string.IsNullOrEmpty(choice.reqType) || string.IsNullOrEmpty(choice.reqName)) {
            return true;
        } else {
            if (choice.reqType == "Summon") {
                if (choice.reqName.Contains("|")) {
                    string[] summons = choice.reqName.Split('|');
                    return PlayerManager.Instance.player.HasAnySummon(summons);
                } else if (choice.reqName.Contains("Quantity")) {
                    int neededQuantity = System.Int32.Parse(choice.reqName.Substring(choice.reqName.IndexOf("_") + 1));
                    return PlayerManager.Instance.player.GetTotalSummonsCount() >= neededQuantity;
                } else {
                    return PlayerManager.Instance.player.HasAnySummon(choice.reqName);
                }
            } else if (choice.reqType == "Artifact") {
                if (choice.reqName.Contains("Quantity")) {
                    int neededQuantity = System.Int32.Parse(choice.reqName.Substring(choice.reqName.IndexOf("_") + 1));
                    return PlayerManager.Instance.player.GetTotalArtifactCount() >= neededQuantity;
                } else {
                    return PlayerManager.Instance.player.HasArtifact(choice.reqName);
                }
            } else if (choice.reqType == "Combat_Ability") {
                return PlayerManager.Instance.player.HasMinionWithCombatAbility((COMBAT_ABILITY)System.Enum.Parse(typeof(COMBAT_ABILITY), choice.reqName));
            } 
            //else if (choice.reqType == "Intervention_Ability") {
            //    return PlayerManager.Instance.player.HasMinionWithInterventionAbility((INTERVENTION_ABILITY) System.Enum.Parse(typeof(INTERVENTION_ABILITY), choice.reqName));
            //}
            else if (choice.reqType == "Minion") {
                if (choice.reqName.Contains("Quantity")) {
                    int neededQuantity = System.Int32.Parse(choice.reqName.Substring(choice.reqName.IndexOf("_") + 1));
                    return PlayerManager.Instance.player.GetCurrentMinionCount() >= neededQuantity;
                }
                return false;
            } else {
                Debug.LogWarning("There is no casing for requirement type: " + choice.reqType);
                return false;
            }
        }
    }
    public void ExecuteEffect(StoryEventEffect effect) {
        if (string.Equals(effect.effect, "Nothing", System.StringComparison.OrdinalIgnoreCase)) {
            //Nothing Happens
        } else if (string.Equals(effect.effect, "Gain", System.StringComparison.OrdinalIgnoreCase)) {
            //gain
            ExecuteGainEffect(effect);
        } else if (string.Equals(effect.effect, "Lose", System.StringComparison.OrdinalIgnoreCase)) {
            //lose
            ExecuteLoseEffect(effect);
        }
    }
    private void ExecuteGainEffect(StoryEventEffect effect) {
        if (string.Equals(effect.effectType, "Summon", System.StringComparison.OrdinalIgnoreCase)) {
            //Gain Summon
            SUMMON_TYPE type = (SUMMON_TYPE)System.Enum.Parse(typeof(SUMMON_TYPE), effect.effectValue);
            PlayerManager.Instance.player.GainSummon(type, showNewSummonUI: true);
        } else if (string.Equals(effect.effectType, "Artifact", System.StringComparison.OrdinalIgnoreCase)) {
            //Gain Artifact
            if (string.Equals(effect.effectValue, "Random", System.StringComparison.OrdinalIgnoreCase)) {
                ARTIFACT_TYPE[] types = Utilities.GetEnumValues<ARTIFACT_TYPE>();
                PlayerManager.Instance.player.GainArtifact(types[UnityEngine.Random.Range(1, types.Length)], true); //Started at 1 index to ignore None choice.
            } else {
                ARTIFACT_TYPE type = (ARTIFACT_TYPE)System.Enum.Parse(typeof(ARTIFACT_TYPE), effect.effectValue);
                PlayerManager.Instance.player.GainArtifact(type, true);
            }
        } else if (string.Equals(effect.effectType, "Intervention_Ability", System.StringComparison.OrdinalIgnoreCase)) {
            //Gain Ability
            if (effect.effectValue.ToLower() == "random") {
                PlayerManager.Instance.player.GainNewInterventionAbility(PlayerManager.Instance.CreateNewInterventionAbility(PlayerManager.Instance.allInterventionAbilities[UnityEngine.Random.Range(0, PlayerManager.Instance.allInterventionAbilities.Length)]), true);
            } else {
                INTERVENTION_ABILITY ability;
                ABILITY_TAG abilityTag;
                if (System.Enum.TryParse(effect.effectValue.ToUpper(), out ability)) {
                    //intervention ability
                    PlayerManager.Instance.player.GainNewInterventionAbility(ability, true);
                } else if (System.Enum.TryParse(effect.effectValue.ToUpper(), out abilityTag)) {
                    //ability tag
                    List<INTERVENTION_ABILITY> abilities = PlayerManager.Instance.GetInterventionAbilitiesWithTag(abilityTag);
                    if (abilities.Count > 0) {
                        PlayerManager.Instance.player.GainNewInterventionAbility(abilities[Random.Range(0, abilities.Count)], true);
                    } else {
                        Debug.LogWarning("There are no intervention abilities with tag " + abilityTag.ToString());
                    }
                }
            }
        } else if (string.Equals(effect.effectType, "Combat_Ability", System.StringComparison.OrdinalIgnoreCase)) {
            //Gain Ability
            if (effect.effectValue.ToLower() == "random") {
                PlayerManager.Instance.player.currentMinionLeader.SetCombatAbility(PlayerManager.Instance.CreateNewCombatAbility(PlayerManager.Instance.allCombatAbilities[UnityEngine.Random.Range(0, PlayerManager.Instance.allCombatAbilities.Length)]), true);
            } else {
                COMBAT_ABILITY ability;
                if (System.Enum.TryParse<COMBAT_ABILITY>(effect.effectValue.ToUpper(), out ability)) {
                    PlayerManager.Instance.player.currentMinionLeader.SetCombatAbility(ability, true);
                }
            }
        } else if (string.Equals(effect.effectType, "Minion", System.StringComparison.OrdinalIgnoreCase)) {
            //Gain Minion
            //effectValue = class name
            if (effect.effectValue.ToLower() == "random") {
                Minion newMinion = PlayerManager.Instance.player.CreateNewMinionRandomClass(RACE.DEMON);
                PlayerManager.Instance.player.AddMinion(newMinion);
            } else {
                Minion newMinion = PlayerManager.Instance.player.CreateNewMinion(effect.effectValue, RACE.DEMON);
                PlayerManager.Instance.player.AddMinion(newMinion);
            }
        } else if (string.Equals(effect.effectType, "Level", System.StringComparison.OrdinalIgnoreCase)) {
            //If effect value is a number, this means that the one that will be leveled up is the minion itself, if it is "Intervention Ability", "Combat Ability", "Summon", "Artifact", show Level Up UI
            //Gain Level
            int level = 0;
            bool isNumber = int.TryParse(effect.effectValue, out level);
            if (isNumber) {
                PlayerManager.Instance.player.currentMinionLeader.LevelUp(level);
            } else {
                PlayerUI.Instance.levelUpUI.ShowLevelUpUI(PlayerManager.Instance.player.currentMinionLeader, effect.effectValue);
            }
        } else if (string.Equals(effect.effectType, "All_Level", System.StringComparison.OrdinalIgnoreCase)) {
            //If effect value is a number, this means that the one that will be leveled up is the minion itself, if it is "Intervention Ability", "Combat Ability", "Summon", "Artifact", show Level Up UI
            //Gain Level
            int level = 0;
            bool isNumber = int.TryParse(effect.effectValue, out level);
            if (isNumber) {
                for (int i = 0; i < PlayerManager.Instance.player.minions.Length; i++) {
                    Minion minion = PlayerManager.Instance.player.minions[i];
                    if(minion != null) {
                        minion.LevelUp(level);
                    }
                }
            } else {
                if (effect.effectValue.ToLower() == "combat ability") {
                    for (int i = 0; i < PlayerManager.Instance.player.minions.Length; i++) {
                        Minion minion = PlayerManager.Instance.player.minions[i];
                        if (minion != null) {
                            minion.combatAbility.LevelUp();
                        }
                    }
                } 
                //else if (effect.effectValue.ToLower() == "intervention ability") {
                //    for (int i = 0; i < PlayerManager.Instance.player.minions.Length; i++) {
                //        Minion minion = PlayerManager.Instance.player.minions[i];
                //        if (minion != null) {
                //            for (int j = 0; j < minion.interventionAbilities.Length; j++) {
                //                if(minion.interventionAbilities[j] != null) {
                //                    minion.interventionAbilities[j].LevelUp();
                //                }
                //            }
                //        }
                //    }
                //}
                else if (effect.effectValue.ToLower() == "summon") {
                    List<Summon> summons = PlayerManager.Instance.player.GetAllSummons();
                    for (int i = 0; i < summons.Count; i++) {
                        summons[i].LevelUp();
                    }
                } else if (effect.effectValue.ToLower() == "artifact") {
                    for (int i = 0; i < PlayerManager.Instance.player.artifactSlots.Length; i++) {
                        if (PlayerManager.Instance.player.artifactSlots[i] != null) {
                            PlayerManager.Instance.player.artifactSlots[i].LevelUp();
                        }
                    }
                }
                PlayerUI.Instance.levelUpUI.ShowLevelUpUI(PlayerManager.Instance.player.currentMinionLeader, effect.effectValue);
            }
        } else if (string.Equals(effect.effectType, "Trait", System.StringComparison.OrdinalIgnoreCase)) {
            //Gain Trait
            PlayerManager.Instance.player.currentMinionLeader.AddTrait(effect.effectValue);
        } else if (string.Equals(effect.effectType, "All_Trait", System.StringComparison.OrdinalIgnoreCase)) {
            //Gain Trait for all minions.
            for (int i = 0; i < PlayerManager.Instance.player.minions.Length; i++) {
                Minion currMinion = PlayerManager.Instance.player.minions[i];
                if (currMinion != null) {
                    currMinion.AddTrait(effect.effectValue);
                }
            }
        } else if (string.Equals(effect.effectType, "Slot", System.StringComparison.OrdinalIgnoreCase)) {
            //Gain Slot
            if (string.Equals(effect.effectValue, "Summon", System.StringComparison.OrdinalIgnoreCase)) {
                PlayerManager.Instance.player.AdjustSummonSlot(1);
            } else if(string.Equals(effect.effectValue, "Artifact", System.StringComparison.OrdinalIgnoreCase)) {
                PlayerManager.Instance.player.AdjustArtifactSlot(1);
            }
        }
    }
    private void ExecuteLoseEffect(StoryEventEffect effect) {
        if (string.Equals(effect.effectType, "Summon", System.StringComparison.OrdinalIgnoreCase)) {
            //Lose Summon
            if (string.Equals(effect.effectValue, "Random", System.StringComparison.OrdinalIgnoreCase)) {
                Summon randomSummon = PlayerManager.Instance.player.GetRandomSummon();
                PlayerManager.Instance.player.RemoveSummon(randomSummon);
            } else {
                SUMMON_TYPE type = (SUMMON_TYPE) System.Enum.Parse(typeof(SUMMON_TYPE), effect.effectValue);
                PlayerManager.Instance.player.RemoveSummon(type);
            }
        } else if (string.Equals(effect.effectType, "Artifact", System.StringComparison.OrdinalIgnoreCase)) {
            //Lose Artifact
            if (string.Equals(effect.effectValue, "Random", System.StringComparison.OrdinalIgnoreCase)) {
                Artifact random = PlayerManager.Instance.player.GetRandomArtifact();
                PlayerManager.Instance.player.RemoveArtifact(random);
            } else {
                ARTIFACT_TYPE type = (ARTIFACT_TYPE) System.Enum.Parse(typeof(ARTIFACT_TYPE), effect.effectValue);
                PlayerManager.Instance.player.LoseArtifact(type);
            }
        } else if (string.Equals(effect.effectType, "Ability", System.StringComparison.OrdinalIgnoreCase)) {
            //Lose Ability
        } else if (string.Equals(effect.effectType, "Minion", System.StringComparison.OrdinalIgnoreCase)) {
            //Minion
            if (string.Equals(effect.effectValue, "Lead", System.StringComparison.OrdinalIgnoreCase)) {
                PlayerManager.Instance.player.RemoveMinion(PlayerManager.Instance.player.currentMinionLeader);
            }
        } else if (string.Equals(effect.effectType, "Level", System.StringComparison.OrdinalIgnoreCase)) {
            //Level
            //Only for minions, cannot level down summons, artifacts, combat abilities, and intervention abilities
            int level = 0;
            bool isNumber = int.TryParse(effect.effectValue, out level);
            if (isNumber) {
                PlayerManager.Instance.player.currentMinionLeader.LevelUp(-level);
            }
        } else if (string.Equals(effect.effectType, "All_Level", System.StringComparison.OrdinalIgnoreCase)) {
            //All Level
            //Only for minions, cannot level down summons, artifacts, combat abilities, and intervention abilities
            int level = 0;
            bool isNumber = int.TryParse(effect.effectValue, out level);
            if (isNumber) {
                for (int i = 0; i < PlayerManager.Instance.player.minions.Length; i++) {
                    Minion minion = PlayerManager.Instance.player.minions[i];
                    if (minion != null) {
                        minion.LevelUp(-level);
                    }
                }
            }
        } else if (string.Equals(effect.effectType, "All_Trait", System.StringComparison.OrdinalIgnoreCase)) {
            //Lose Trait for all minions.
            if (effect.effectValue.ToLower() == "negative") {
                for (int i = 0; i < PlayerManager.Instance.player.minions.Length; i++) {
                    Minion currMinion = PlayerManager.Instance.player.minions[i];
                    if (currMinion != null) {
                        for (int j = 0; j < currMinion.character.normalTraits.Count; j++) {
                            Trait currTrait = currMinion.character.normalTraits[j];
                            if(currTrait.effect == TRAIT_EFFECT.NEGATIVE) {
                                currMinion.character.RemoveTrait(currTrait);
                                j--;
                            }
                        }
                    }
                }
            } else if (effect.effectValue.ToLower() == "neutral") {
                for (int i = 0; i < PlayerManager.Instance.player.minions.Length; i++) {
                    Minion currMinion = PlayerManager.Instance.player.minions[i];
                    if (currMinion != null) {
                        for (int j = 0; j < currMinion.character.normalTraits.Count; j++) {
                            Trait currTrait = currMinion.character.normalTraits[j];
                            if (currTrait.effect == TRAIT_EFFECT.NEUTRAL) {
                                currMinion.character.RemoveTrait(currTrait);
                                j--;
                            }
                        }
                    }
                }
            } else if (effect.effectValue.ToLower() == "positive") {
                for (int i = 0; i < PlayerManager.Instance.player.minions.Length; i++) {
                    Minion currMinion = PlayerManager.Instance.player.minions[i];
                    if (currMinion != null) {
                        for (int j = 0; j < currMinion.character.normalTraits.Count; j++) {
                            Trait currTrait = currMinion.character.normalTraits[j];
                            if (currTrait.effect == TRAIT_EFFECT.POSITIVE) {
                                currMinion.character.RemoveTrait(currTrait);
                                j--;
                            }
                        }
                    }
                }
            } else {
                for (int i = 0; i < PlayerManager.Instance.player.minions.Length; i++) {
                    Minion currMinion = PlayerManager.Instance.player.minions[i];
                    if (currMinion != null) {
                        currMinion.character.RemoveTrait(effect.effectValue);
                    }
                }
            }
        } else if (string.Equals(effect.effectType, "Trait", System.StringComparison.OrdinalIgnoreCase)) {
            //Lose Trait for all minions.
            if (effect.effectValue.ToLower() == "negative") {
                for (int j = 0; j < PlayerManager.Instance.player.currentMinionLeader.character.normalTraits.Count; j++) {
                    Trait currTrait = PlayerManager.Instance.player.currentMinionLeader.character.normalTraits[j];
                    if (currTrait.effect == TRAIT_EFFECT.NEGATIVE) {
                        PlayerManager.Instance.player.currentMinionLeader.character.RemoveTrait(currTrait);
                        j--;
                    }
                }
            } else if (effect.effectValue.ToLower() == "neutral") {
                for (int j = 0; j < PlayerManager.Instance.player.currentMinionLeader.character.normalTraits.Count; j++) {
                    Trait currTrait = PlayerManager.Instance.player.currentMinionLeader.character.normalTraits[j];
                    if (currTrait.effect == TRAIT_EFFECT.NEUTRAL) {
                        PlayerManager.Instance.player.currentMinionLeader.character.RemoveTrait(currTrait);
                        j--;
                    }
                }
            } else if (effect.effectValue.ToLower() == "positive") {
                for (int j = 0; j < PlayerManager.Instance.player.currentMinionLeader.character.normalTraits.Count; j++) {
                    Trait currTrait = PlayerManager.Instance.player.currentMinionLeader.character.normalTraits[j];
                    if (currTrait.effect == TRAIT_EFFECT.POSITIVE) {
                        PlayerManager.Instance.player.currentMinionLeader.character.RemoveTrait(currTrait);
                        j--;
                    }
                }
            } else {
                PlayerManager.Instance.player.currentMinionLeader.character.RemoveTrait(effect.effectValue);
            }
        }
    }
    public void CollectEffects(StoryEvent storyEvent, out string additionalText, out List<StoryEventEffect> chosenEffects) {
        additionalText = string.Empty;
        chosenEffects = new List<StoryEventEffect>();
        WeightedDictionary<StoryEventEffect> pooledEffects = new WeightedDictionary<StoryEventEffect>();
        for (int i = 0; i < storyEvent.effects.Length; i++) {
            StoryEventEffect currEFfect = storyEvent.effects[i];
            if (currEFfect.effectChance == 100) {
                //add effect to chosen effects. These will be executed once the player has clicked a choice
                additionalText += " " + currEFfect.additionalText;
                chosenEffects.Add(currEFfect);
            } else {
                pooledEffects.AddElement(currEFfect, currEFfect.effectChance);
            }
        }
        //if any pooled effects. Give one.
        if (pooledEffects.GetTotalOfWeights() > 0) {
            StoryEventEffect currEffect = pooledEffects.PickRandomElementGivenWeights();
            additionalText += " " + currEffect.additionalText;
            chosenEffects.Add(currEffect);
        }
    }
    #endregion

    #region World Events
    private void LoadWorldEvents() {
        WORLD_EVENT[] events = Utilities.GetEnumValues<WORLD_EVENT>();
        worldEvents = new WorldEvent[events.Length];
        for (int i = 0; i < events.Length; i++) {
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
    private WorldEvent GetWorldEvent(WORLD_EVENT eventType) {
        for (int i = 0; i < worldEvents.Length; i++) {
            WorldEvent currEvent = worldEvents[i];
            if (currEvent.eventType == eventType) {
                return currEvent;
            }
        }
        return null;
    }
    private bool CanSpawnEventAt(WORLD_EVENT eventType, BaseLandmark landmark) {
        WorldEvent worldEvent = GetWorldEvent(eventType);
        if (worldEvent != null) {
            return worldEvent.CanSpawnEventAt(landmark);
        }
        return false;
    }
    public List<WorldEvent> GetEventsThatCanSpawnAt(BaseLandmark landmark) {
        List<WorldEvent> events = new List<WorldEvent>();
        for (int i = 0; i < worldEvents.Length; i++) {
            WorldEvent currEvent = worldEvents[i];
            if (currEvent.CanSpawnEventAt(landmark)) {
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
