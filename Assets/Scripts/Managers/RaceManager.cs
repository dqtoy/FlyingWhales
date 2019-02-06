using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class RaceManager : MonoBehaviour {
    public static RaceManager Instance;

    private Dictionary<string, RaceSetting> _racesDictionary;
    private Dictionary<RACE, INTERACTION_TYPE[]> _factionRaceInteractions;
    private Dictionary<RACE, INTERACTION_TYPE[]> _npcRaceInteractions;

    #region getters/setters
    public Dictionary<string, RaceSetting> racesDictionary {
        get { return _racesDictionary; }
    }
    #endregion

    void Awake() {
        Instance = this;    
    }

    public void Initialize() {
        ConstructAllRaces();
        ConstructFactionRaceInteractions();
    }

    private void ConstructAllRaces() {
        _racesDictionary = new Dictionary<string, RaceSetting>();
        string path = Utilities.dataPath + "RaceSettings/";
        string[] races = System.IO.Directory.GetFiles(path, "*.json");
        for (int i = 0; i < races.Length; i++) {
            RaceSetting currentRace = JsonUtility.FromJson<RaceSetting>(System.IO.File.ReadAllText(races[i]));
            _racesDictionary.Add(currentRace.race.ToString(), currentRace);
        }
    }

    #region Faction Interactions
    private void ConstructFactionRaceInteractions() {
        _factionRaceInteractions = new Dictionary<RACE, INTERACTION_TYPE[]>() {
            { RACE.NONE, new INTERACTION_TYPE[] { //All races
                INTERACTION_TYPE.MOVE_TO_LOOT_ACTION,
                INTERACTION_TYPE.MOVE_TO_TAME_BEAST_ACTION,
                INTERACTION_TYPE.MOVE_TO_ABDUCT_ACTION,
                //INTERACTION_TYPE.MOVE_TO_HUNT,
                //INTERACTION_TYPE.MOVE_TO_ARGUE,
                //INTERACTION_TYPE.MOVE_TO_CURSE,
                INTERACTION_TYPE.TORTURE_ACTION,
                //INTERACTION_TYPE.EAT_DEFENSELESS,
                INTERACTION_TYPE.CRAFT_ITEM,
                INTERACTION_TYPE.MOVE_TO_CHARM_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_STEAL_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_RECRUIT_FRIEND_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_ASSASSINATE_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_REANIMATE_ACTION,
                INTERACTION_TYPE.MOVE_TO_SCAVENGE_EVENT_FACTION,
                INTERACTION_TYPE.MOVE_TO_RAID_EVENT_FACTION,
                INTERACTION_TYPE.MOVE_TO_OCCUPY_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_MINE_ACTION,
                INTERACTION_TYPE.MOVE_TO_HARVEST_ACTION,
                INTERACTION_TYPE.SCRAP_ITEM,
                INTERACTION_TYPE.CONSUME_LIFE,
                INTERACTION_TYPE.PATROL_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_EXPLORE_EVENT_FACTION,
            } },
        };
    }
    public List<INTERACTION_TYPE> GetFactionInteractionsOfRace(RACE race) {
        List<INTERACTION_TYPE> interactions = _factionRaceInteractions[RACE.NONE].ToList(); //Get interactions of all races first
        if (_factionRaceInteractions.ContainsKey(race)) {
            for (int i = 0; i < _factionRaceInteractions[race].Length; i++) {
                interactions.Add(_factionRaceInteractions[race][i]);
            }
        }
        return interactions;
    }
    public List<INTERACTION_TYPE> GetFactionInteractionsOfRace(RACE race, INTERACTION_CATEGORY category) {
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        INTERACTION_TYPE[] interactionArray = _factionRaceInteractions[RACE.NONE];
        for (int i = 0; i < interactionArray.Length; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
            for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                if (interactionCategoryAndAlignment.categories[j] == category) {
                    interactions.Add(interactionArray[i]);
                    break;
                }
            }
        }
        if (_factionRaceInteractions.ContainsKey(race)) {
            interactionArray = _factionRaceInteractions[race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
                for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                    if(interactionCategoryAndAlignment.categories[j] == category) {
                        interactions.Add(interactionArray[i]);
                        break;
                    }
                }
            }
        }
        return interactions;
    }
    public List<INTERACTION_TYPE> GetFactionInteractionsOfRace(RACE race, INTERACTION_ALIGNMENT alignment) {
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        INTERACTION_TYPE[] interactionArray = _factionRaceInteractions[RACE.NONE];
        for (int i = 0; i < interactionArray.Length; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
            if (interactionCategoryAndAlignment.alignment == alignment) {
                interactions.Add(interactionArray[i]);
            }
        }
        if (_factionRaceInteractions.ContainsKey(race)) {
            interactionArray = _factionRaceInteractions[race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
                if (interactionCategoryAndAlignment.alignment == alignment) {
                    interactions.Add(interactionArray[i]);
                }
            }
        }
        return interactions;
    }
    public List<INTERACTION_TYPE> GetFactionInteractionsOfRace(RACE race, INTERACTION_CATEGORY category, INTERACTION_ALIGNMENT alignment) {
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        INTERACTION_TYPE[] interactionArray = _factionRaceInteractions[RACE.NONE];
        for (int i = 0; i < interactionArray.Length; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
            if (interactionCategoryAndAlignment.alignment == alignment) {
                for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                    if (interactionCategoryAndAlignment.categories[j] == category) {
                        interactions.Add(interactionArray[i]);
                        break;
                    }
                }
            }
        }
        if (_factionRaceInteractions.ContainsKey(race)) {
            interactionArray = _factionRaceInteractions[race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
                if (interactionCategoryAndAlignment.alignment == alignment) {
                    for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                        if (interactionCategoryAndAlignment.categories[j] == category) {
                            interactions.Add(interactionArray[i]);
                            break;
                        }
                    }
                }
            }
        }
        return interactions;
    }
    public List<INTERACTION_TYPE> GetFactionInteractionsOfRace(RACE race, INTERACTION_CATEGORY category, MORALITY factionMorality) {
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        INTERACTION_TYPE[] interactionArray = _factionRaceInteractions[RACE.NONE];
        for (int i = 0; i < interactionArray.Length; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
            if (factionMorality == MORALITY.GOOD && interactionCategoryAndAlignment.alignment == INTERACTION_ALIGNMENT.EVIL) {
                //Alignment must be good or neutral, so if it is evil, skip it
                continue;
            } else if (factionMorality == MORALITY.EVIL && interactionCategoryAndAlignment.alignment == INTERACTION_ALIGNMENT.GOOD) {
                //Alignment must be evil or neutral, so if it is good, skip it
                continue;
            }
            for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                if (interactionCategoryAndAlignment.categories[j] == category) {
                    interactions.Add(interactionArray[i]);
                    break;
                }
            }
        }
        if (_factionRaceInteractions.ContainsKey(race)) {
            interactionArray = _factionRaceInteractions[race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
                if (factionMorality == MORALITY.GOOD && interactionCategoryAndAlignment.alignment == INTERACTION_ALIGNMENT.EVIL) {
                    //Alignment must be good or neutral, so if it is evil, skip it
                    continue;
                }else if (factionMorality == MORALITY.EVIL && interactionCategoryAndAlignment.alignment == INTERACTION_ALIGNMENT.GOOD) {
                    //Alignment must be evil or neutral, so if it is good, skip it
                    continue;
                }
                for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                    if (interactionCategoryAndAlignment.categories[j] == category) {
                        interactions.Add(interactionArray[i]);
                        break;
                    }
                }
            }
        }
        return interactions;
    }
    #endregion

    #region NPC Interaction
    private void ConstructNPCRaceInteractions() {
        _npcRaceInteractions = new Dictionary<RACE, INTERACTION_TYPE[]>() {
            { RACE.NONE, new INTERACTION_TYPE[] { //All races
                INTERACTION_TYPE.MOVE_TO_VISIT,
                INTERACTION_TYPE.ABDUCT_ACTION,
                INTERACTION_TYPE.ARGUE_ACTION,
                INTERACTION_TYPE.CURSE_ACTION,
                INTERACTION_TYPE.HUNT_ACTION,
                INTERACTION_TYPE.TRANSFER_HOME,
                INTERACTION_TYPE.TORTURE_ACTION_NPC,
            } },
        };
    }
    public List<INTERACTION_TYPE> GetNPCInteractionsOfRace(RACE race) {
        List<INTERACTION_TYPE> interactions = _npcRaceInteractions[RACE.NONE].ToList(); //Get interactions of all races first
        if (_npcRaceInteractions.ContainsKey(race)) {
            for (int i = 0; i < _npcRaceInteractions[race].Length; i++) {
                interactions.Add(_npcRaceInteractions[race][i]);
            }
        }
        return interactions;
    }
    public List<INTERACTION_TYPE> GetNPCInteractionsOfRace(RACE race, INTERACTION_CATEGORY category, Character character = null, Character targetCharacter = null) { 
        //If there is a character passed as parameter, this means that the list will be filtered by what the character can do
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        INTERACTION_TYPE[] interactionArray = _npcRaceInteractions[RACE.NONE];
        for (int i = 0; i < interactionArray.Length; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
            for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                if (interactionCategoryAndAlignment.categories[j] == category) {
                    if(character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character, targetCharacter)) {
                        interactions.Add(interactionArray[i]);
                    }
                    break;
                }
            }
        }
        if (_npcRaceInteractions.ContainsKey(race)) {
            interactionArray = _npcRaceInteractions[race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
                for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                    if (interactionCategoryAndAlignment.categories[j] == category) {
                        if (character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character, targetCharacter)) {
                            interactions.Add(interactionArray[i]);
                        }
                        break;
                    }
                }
            }
        }
        return interactions;
    }
    public List<INTERACTION_TYPE> GetNPCInteractionsOfRaceTarget(RACE race, INTERACTION_CATEGORY category, InteractionCharacterEffect interactionTargetCharacterEffect, Character character = null, Character targetCharacter = null) {
        //If there is a character passed as parameter, this means that the list will be filtered by what the character can do
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        INTERACTION_TYPE[] interactionArray = _npcRaceInteractions[RACE.NONE];
        for (int i = 0; i < interactionArray.Length; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
            for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                if (interactionCategoryAndAlignment.categories[j] == category) {
                    bool canDoTargetCharacterEffect = false;
                    if(interactionCategoryAndAlignment.targetCharacterEffect != null) {
                        for (int k = 0; k < interactionCategoryAndAlignment.targetCharacterEffect.Length; k++) {
                            if (interactionCategoryAndAlignment.targetCharacterEffect[k].effect != INTERACTION_CHARACTER_EFFECT.NONE &&
                                interactionCategoryAndAlignment.targetCharacterEffect[k].effect == interactionTargetCharacterEffect.effect) {
                                if (interactionCategoryAndAlignment.targetCharacterEffect[k].effectString != null && interactionTargetCharacterEffect.effectString != null) {
                                    bool canDoEffectString = false;
                                    for (int l = 0; l < interactionCategoryAndAlignment.targetCharacterEffect[k].effectString.Length; l++) {
                                        string effectString = interactionCategoryAndAlignment.targetCharacterEffect[k].effectString[l];
                                        if (interactionTargetCharacterEffect.effectString.Contains(effectString)) {
                                            canDoEffectString = true;
                                            break;
                                        }
                                    }
                                    if (canDoEffectString) {
                                        canDoTargetCharacterEffect = true;
                                        break;
                                    }
                                } else {
                                    canDoTargetCharacterEffect = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (canDoTargetCharacterEffect && character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character, targetCharacter)) {
                        interactions.Add(interactionArray[i]);
                    }
                    break;
                }
            }
        }
        if (_npcRaceInteractions.ContainsKey(race)) {
            interactionArray = _npcRaceInteractions[race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
                for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                    if (interactionCategoryAndAlignment.categories[j] == category) {
                        bool canDoTargetCharacterEffect = false;
                        if (interactionCategoryAndAlignment.targetCharacterEffect != null) {
                            for (int k = 0; k < interactionCategoryAndAlignment.targetCharacterEffect.Length; k++) {
                                if (interactionCategoryAndAlignment.targetCharacterEffect[k].effect != INTERACTION_CHARACTER_EFFECT.NONE &&
                                    interactionCategoryAndAlignment.targetCharacterEffect[k].effect == interactionTargetCharacterEffect.effect) {
                                    if (interactionCategoryAndAlignment.targetCharacterEffect[k].effectString != null && interactionTargetCharacterEffect.effectString != null) {
                                        bool canDoEffectString = false;
                                        for (int l = 0; l < interactionCategoryAndAlignment.targetCharacterEffect[k].effectString.Length; l++) {
                                            string effectString = interactionCategoryAndAlignment.targetCharacterEffect[k].effectString[l];
                                            if (interactionTargetCharacterEffect.effectString.Contains(effectString)) {
                                                canDoEffectString = true;
                                                break;
                                            }
                                        }
                                        if (canDoEffectString) {
                                            canDoTargetCharacterEffect = true;
                                            break;
                                        }
                                    } else {
                                        canDoTargetCharacterEffect = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (canDoTargetCharacterEffect && character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character, targetCharacter)) {
                            interactions.Add(interactionArray[i]);
                        }
                        break;
                    }
                }
            }
        }
        return interactions;
    }
    public List<INTERACTION_TYPE> GetNPCInteractionsOfRaceActor(RACE race, INTERACTION_CATEGORY category, InteractionCharacterEffect interactionActorCharacterEffect, Character character = null, Character targetCharacter = null) {
        //If there is a character passed as parameter, this means that the list will be filtered by what the character can do
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        INTERACTION_TYPE[] interactionArray = _npcRaceInteractions[RACE.NONE];
        for (int i = 0; i < interactionArray.Length; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
            for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                if (interactionCategoryAndAlignment.categories[j] == category) {
                    bool canDoActorEffect = false;
                    if (interactionCategoryAndAlignment.actorEffect != null) {
                        for (int k = 0; k < interactionCategoryAndAlignment.actorEffect.Length; k++) {
                            if (interactionCategoryAndAlignment.actorEffect[k].effect != INTERACTION_CHARACTER_EFFECT.NONE &&
                                interactionCategoryAndAlignment.actorEffect[k].effect == interactionActorCharacterEffect.effect) {
                                if (interactionCategoryAndAlignment.actorEffect[k].effectString != null && interactionActorCharacterEffect.effectString != null) {
                                    bool canDoEffectString = false;
                                    for (int l = 0; l < interactionCategoryAndAlignment.actorEffect[k].effectString.Length; l++) {
                                        string effectString = interactionCategoryAndAlignment.actorEffect[k].effectString[l];
                                        if (interactionActorCharacterEffect.effectString.Contains(effectString)) {
                                            canDoEffectString = true;
                                            break;
                                        }
                                    }
                                    if (canDoEffectString) {
                                        canDoActorEffect = true;
                                        break;
                                    }
                                } else {
                                    canDoActorEffect = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (canDoActorEffect && character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character, targetCharacter)) {
                        interactions.Add(interactionArray[i]);
                    }
                    break;
                }
            }
        }
        if (_npcRaceInteractions.ContainsKey(race)) {
            interactionArray = _npcRaceInteractions[race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
                for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                    if (interactionCategoryAndAlignment.categories[j] == category) {
                        bool canDoactorEffect = false;
                        if (interactionCategoryAndAlignment.actorEffect != null) {
                            for (int k = 0; k < interactionCategoryAndAlignment.actorEffect.Length; k++) {
                                if (interactionCategoryAndAlignment.actorEffect[k].effect != INTERACTION_CHARACTER_EFFECT.NONE &&
                                    interactionCategoryAndAlignment.actorEffect[k].effect == interactionActorCharacterEffect.effect) {
                                    if (interactionCategoryAndAlignment.actorEffect[k].effectString != null && interactionActorCharacterEffect.effectString != null) {
                                        bool canDoEffectString = false;
                                        for (int l = 0; l < interactionCategoryAndAlignment.actorEffect[k].effectString.Length; l++) {
                                            string effectString = interactionCategoryAndAlignment.actorEffect[k].effectString[l];
                                            if (interactionActorCharacterEffect.effectString.Contains(effectString)) {
                                                canDoEffectString = true;
                                                break;
                                            }
                                        }
                                        if (canDoEffectString) {
                                            canDoactorEffect = true;
                                            break;
                                        }
                                    } else {
                                        canDoactorEffect = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (canDoactorEffect && character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character, targetCharacter)) {
                            interactions.Add(interactionArray[i]);
                        }
                        break;
                    }
                }
            }
        }
        return interactions;
    }
    public List<INTERACTION_TYPE> GetNPCInteractionsOfRace(RACE race, INTERACTION_ALIGNMENT alignment, Character character = null, Character targetCharacter = null) {
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        INTERACTION_TYPE[] interactionArray = _npcRaceInteractions[RACE.NONE];
        for (int i = 0; i < interactionArray.Length; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
            if (interactionCategoryAndAlignment.alignment == alignment) {
                if (character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character, targetCharacter)) {
                    interactions.Add(interactionArray[i]);
                }
            }
        }
        if (_npcRaceInteractions.ContainsKey(race)) {
            interactionArray = _npcRaceInteractions[race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
                if (interactionCategoryAndAlignment.alignment == alignment) {
                    if (character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character, targetCharacter)) {
                        interactions.Add(interactionArray[i]);
                    }
                }
            }
        }
        return interactions;
    }
    public List<INTERACTION_TYPE> GetNPCInteractionsOfRace(RACE race, INTERACTION_CATEGORY category, INTERACTION_ALIGNMENT alignment, Character character = null, Character targetCharacter = null) {
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        INTERACTION_TYPE[] interactionArray = _npcRaceInteractions[RACE.NONE];
        for (int i = 0; i < interactionArray.Length; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
            if (interactionCategoryAndAlignment.alignment == alignment) {
                for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                    if (interactionCategoryAndAlignment.categories[j] == category) {
                        if (character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character, targetCharacter)) {
                            interactions.Add(interactionArray[i]);
                        }
                        break;
                    }
                }
            }
        }
        if (_npcRaceInteractions.ContainsKey(race)) {
            interactionArray = _npcRaceInteractions[race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
                if (interactionCategoryAndAlignment.alignment == alignment) {
                    for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                        if (interactionCategoryAndAlignment.categories[j] == category) {
                            if (character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character, targetCharacter)) {
                                interactions.Add(interactionArray[i]);
                            }
                            break;
                        }
                    }
                }
            }
        }
        return interactions;
    }
    #endregion
}
