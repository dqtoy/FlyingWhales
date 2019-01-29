using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class RaceManager : MonoBehaviour {
    public static RaceManager Instance;

    private Dictionary<string, RaceSetting> _racesDictionary;
    private Dictionary<RACE, INTERACTION_TYPE[]> _racesInteractions;

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
        ConstructRacesInteractions();
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
    private void ConstructRacesInteractions() {
        _racesInteractions = new Dictionary<RACE, INTERACTION_TYPE[]>() {
            { RACE.HUMANS, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_LOOT,
                INTERACTION_TYPE.MOVE_TO_TAME_BEAST,
                INTERACTION_TYPE.MOVE_TO_ABDUCT,
                INTERACTION_TYPE.MOVE_TO_HUNT,
                INTERACTION_TYPE.MOVE_TO_ARGUE,
                INTERACTION_TYPE.MOVE_TO_CURSE,
                INTERACTION_TYPE.TORTURE_ACTION,
                INTERACTION_TYPE.EAT_DEFENSELESS,
            } },
            { RACE.ELVES, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_LOOT,
                INTERACTION_TYPE.MOVE_TO_TAME_BEAST,
                INTERACTION_TYPE.MOVE_TO_ABDUCT,
                INTERACTION_TYPE.MOVE_TO_HUNT,
                INTERACTION_TYPE.MOVE_TO_ARGUE,
                INTERACTION_TYPE.MOVE_TO_CURSE,
                INTERACTION_TYPE.TORTURE_ACTION,
                INTERACTION_TYPE.EAT_DEFENSELESS,
            } },
            { RACE.GOBLIN, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_LOOT,
                INTERACTION_TYPE.MOVE_TO_TAME_BEAST,
                INTERACTION_TYPE.MOVE_TO_ABDUCT,
                INTERACTION_TYPE.MOVE_TO_HUNT,
                INTERACTION_TYPE.MOVE_TO_ARGUE,
                INTERACTION_TYPE.MOVE_TO_CURSE,
                INTERACTION_TYPE.TORTURE_ACTION,
                INTERACTION_TYPE.EAT_DEFENSELESS,
            } },
            { RACE.FAERY, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_LOOT,
                INTERACTION_TYPE.MOVE_TO_TAME_BEAST,
                INTERACTION_TYPE.MOVE_TO_ABDUCT,
                INTERACTION_TYPE.MOVE_TO_HUNT,
                INTERACTION_TYPE.MOVE_TO_ARGUE,
                INTERACTION_TYPE.MOVE_TO_CURSE,
                INTERACTION_TYPE.TORTURE_ACTION,
                INTERACTION_TYPE.EAT_DEFENSELESS,
            } },
            { RACE.SKELETON, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_LOOT,
                INTERACTION_TYPE.MOVE_TO_TAME_BEAST,
                INTERACTION_TYPE.MOVE_TO_ABDUCT,
                INTERACTION_TYPE.MOVE_TO_HUNT,
                INTERACTION_TYPE.MOVE_TO_ARGUE,
                INTERACTION_TYPE.MOVE_TO_CURSE,
                INTERACTION_TYPE.TORTURE_ACTION,
                INTERACTION_TYPE.EAT_DEFENSELESS,
            } },
            { RACE.SPIDER, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_LOOT,
                INTERACTION_TYPE.MOVE_TO_TAME_BEAST,
                INTERACTION_TYPE.MOVE_TO_ABDUCT,
                INTERACTION_TYPE.MOVE_TO_HUNT,
                INTERACTION_TYPE.MOVE_TO_ARGUE,
                INTERACTION_TYPE.MOVE_TO_CURSE,
                INTERACTION_TYPE.TORTURE_ACTION,
                INTERACTION_TYPE.EAT_DEFENSELESS,
            } },
            { RACE.WOLF, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_LOOT,
                INTERACTION_TYPE.MOVE_TO_TAME_BEAST,
                INTERACTION_TYPE.MOVE_TO_ABDUCT,
                INTERACTION_TYPE.MOVE_TO_HUNT,
                INTERACTION_TYPE.MOVE_TO_ARGUE,
                INTERACTION_TYPE.MOVE_TO_CURSE,
                INTERACTION_TYPE.TORTURE_ACTION,
                INTERACTION_TYPE.EAT_DEFENSELESS,
            } },
            { RACE.DRAGON, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_LOOT,
                INTERACTION_TYPE.MOVE_TO_TAME_BEAST,
                INTERACTION_TYPE.MOVE_TO_ABDUCT,
                INTERACTION_TYPE.MOVE_TO_HUNT,
                INTERACTION_TYPE.MOVE_TO_ARGUE,
                INTERACTION_TYPE.MOVE_TO_CURSE,
                INTERACTION_TYPE.TORTURE_ACTION,
                INTERACTION_TYPE.EAT_DEFENSELESS,
            } },
        };
    }
    public List<INTERACTION_TYPE> GetInteractionsOfRace(RACE race) {
        if (_racesInteractions.ContainsKey(race)) {
            return _racesInteractions[race].ToList();
        }
        return null;
    }
    public List<INTERACTION_TYPE> GetInteractionsOfRace(RACE race, INTERACTION_CATEGORY category) {
        if (_racesInteractions.ContainsKey(race)) {
            List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>();
            INTERACTION_TYPE[] interactionArray = _racesInteractions[race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionCategoryAndAlignment interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
                for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                    if(interactionCategoryAndAlignment.categories[j] == category) {
                        interactions.Add(interactionArray[i]);
                        break;
                    }
                }
            }
            return interactions;
        }
        return null;
    }
    public List<INTERACTION_TYPE> GetInteractionsOfRace(RACE race, INTERACTION_ALIGNMENT alignment) {
        if (_racesInteractions.ContainsKey(race)) {
            List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>();
            INTERACTION_TYPE[] interactionArray = _racesInteractions[race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionCategoryAndAlignment interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
                if (interactionCategoryAndAlignment.alignment == alignment) {
                    interactions.Add(interactionArray[i]);
                }
            }
            return interactions;
        }
        return null;
    }
    public List<INTERACTION_TYPE> GetInteractionsOfRace(RACE race, INTERACTION_CATEGORY category, INTERACTION_ALIGNMENT alignment) {
        if (_racesInteractions.ContainsKey(race)) {
            List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>();
            INTERACTION_TYPE[] interactionArray = _racesInteractions[race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionCategoryAndAlignment interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i]);
                if (interactionCategoryAndAlignment.alignment == alignment) {
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
        return null;
    }
}
