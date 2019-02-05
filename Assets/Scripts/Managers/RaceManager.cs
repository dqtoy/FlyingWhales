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
                INTERACTION_TYPE.MOVE_TO_LOOT,
                INTERACTION_TYPE.MOVE_TO_TAME_BEAST,
                INTERACTION_TYPE.MOVE_TO_ABDUCT,
                //INTERACTION_TYPE.MOVE_TO_HUNT,
                //INTERACTION_TYPE.MOVE_TO_ARGUE,
                //INTERACTION_TYPE.MOVE_TO_CURSE,
                INTERACTION_TYPE.TORTURE_ACTION,
                //INTERACTION_TYPE.EAT_DEFENSELESS,
                INTERACTION_TYPE.CRAFT_ITEM,
                INTERACTION_TYPE.MOVE_TO_CHARM_FACTION,
                INTERACTION_TYPE.MOVE_TO_STEAL_FACTION,
                INTERACTION_TYPE.MOVE_TO_RECRUIT_FRIEND_FACTION,
                INTERACTION_TYPE.MOVE_TO_ASSASSINATE_FACTION,
                INTERACTION_TYPE.MOVE_TO_REANIMATE,
                INTERACTION_TYPE.MOVE_TO_SCAVENGE_FACTION,
                INTERACTION_TYPE.MOVE_TO_RAID_FACTION,
                INTERACTION_TYPE.MOVE_TO_OCCUPY_FACTION,
                INTERACTION_TYPE.MOVE_TO_MINE,
                INTERACTION_TYPE.MOVE_TO_HARVEST,
                INTERACTION_TYPE.SCRAP_ITEM,
                INTERACTION_TYPE.CONSUME_LIFE,
                INTERACTION_TYPE.PATROL_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_EXPLORE_FACTION,
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
    public List<INTERACTION_TYPE> GetNPCInteractionsOfRace(RACE race, INTERACTION_CATEGORY category, Character character = null) { 
        //If there is a character passed as parameter, this means that the list will be filtered by what the character can do
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        INTERACTION_TYPE[] interactionArray = _npcRaceInteractions[RACE.NONE];
        for (int i = 0; i < interactionArray.Length; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
            for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                if (interactionCategoryAndAlignment.categories[j] == category) {
                    if(character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character)) {
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
                        if (character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character)) {
                            interactions.Add(interactionArray[i]);
                        }
                        break;
                    }
                }
            }
        }
        return interactions;
    }
    public List<INTERACTION_TYPE> GetNPCInteractionsOfRace(RACE race, INTERACTION_ALIGNMENT alignment, Character character = null) {
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        INTERACTION_TYPE[] interactionArray = _npcRaceInteractions[RACE.NONE];
        for (int i = 0; i < interactionArray.Length; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
            if (interactionCategoryAndAlignment.alignment == alignment) {
                if (character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character)) {
                    interactions.Add(interactionArray[i]);
                }
            }
        }
        if (_npcRaceInteractions.ContainsKey(race)) {
            interactionArray = _npcRaceInteractions[race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
                if (interactionCategoryAndAlignment.alignment == alignment) {
                    if (character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character)) {
                        interactions.Add(interactionArray[i]);
                    }
                }
            }
        }
        return interactions;
    }
    public List<INTERACTION_TYPE> GetNPCInteractionsOfRace(RACE race, INTERACTION_CATEGORY category, INTERACTION_ALIGNMENT alignment, Character character = null) {
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        INTERACTION_TYPE[] interactionArray = _npcRaceInteractions[RACE.NONE];
        for (int i = 0; i < interactionArray.Length; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
            if (interactionCategoryAndAlignment.alignment == alignment) {
                for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                    if (interactionCategoryAndAlignment.categories[j] == category) {
                        if (character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character)) {
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
                            if (character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character)) {
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
