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
        ConstructNPCRaceInteractions();
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
            { RACE.HUMANS, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_RECRUIT_FRIEND_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_SCAVENGE_EVENT_FACTION,
                INTERACTION_TYPE.MOVE_TO_MINE_ACTION,
                //INTERACTION_TYPE.CRAFT_ITEM,
                INTERACTION_TYPE.MOVE_TO_OCCUPY_ACTION_FACTION,
                INTERACTION_TYPE.PATROL_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_EXPLORE_EVENT_FACTION,
                INTERACTION_TYPE.MOVE_TO_GIFT_ITEM,
            } },
            { RACE.ELVES, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_TAME_BEAST_ACTION,
                //INTERACTION_TYPE.CRAFT_ITEM,
                INTERACTION_TYPE.MOVE_TO_RECRUIT_FRIEND_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_ASSASSINATE_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_OCCUPY_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_MINE_ACTION,
                INTERACTION_TYPE.MOVE_TO_HARVEST_ACTION,
                INTERACTION_TYPE.PATROL_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_EXPLORE_EVENT_FACTION,
                INTERACTION_TYPE.MOVE_TO_COURTESY_CALL,
            } },
            { RACE.GOBLIN, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_LOOT_ACTION,
                INTERACTION_TYPE.MOVE_TO_TAME_BEAST_ACTION,
                INTERACTION_TYPE.MOVE_TO_ABDUCT_ACTION,
                INTERACTION_TYPE.TORTURE_ACTION,
                //INTERACTION_TYPE.CRAFT_ITEM,
                INTERACTION_TYPE.MOVE_TO_STEAL_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_SCAVENGE_EVENT_FACTION,
                INTERACTION_TYPE.MOVE_TO_RAID_EVENT_FACTION,
                INTERACTION_TYPE.MOVE_TO_OCCUPY_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_GIFT_BEAST,
            } },
            { RACE.FAERY, new INTERACTION_TYPE[] {
                //INTERACTION_TYPE.CRAFT_ITEM,
                INTERACTION_TYPE.MOVE_TO_CHARM_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_STEAL_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_OCCUPY_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_HARVEST_ACTION,
                INTERACTION_TYPE.SCRAP_ITEM,
                INTERACTION_TYPE.PATROL_ACTION_FACTION,
                INTERACTION_TYPE.MOVE_TO_EXPLORE_EVENT_FACTION,
            } },
            { RACE.SKELETON, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.TORTURE_ACTION,
                INTERACTION_TYPE.MOVE_TO_REANIMATE_ACTION,
                INTERACTION_TYPE.MOVE_TO_SCAVENGE_EVENT_FACTION,
                INTERACTION_TYPE.MOVE_TO_OCCUPY_ACTION_FACTION,
                INTERACTION_TYPE.PATROL_ACTION_FACTION,
                INTERACTION_TYPE.CONSUME_PRISONER_ACTION,
            } },
            { RACE.SPIDER, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_ABDUCT_ACTION,
                INTERACTION_TYPE.PATROL_ACTION_FACTION,
            } },
            { RACE.WOLF, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_LOOT_ACTION,
                INTERACTION_TYPE.PATROL_ACTION_FACTION,
            } },
            { RACE.DRAGON, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_EXPLORE_EVENT_FACTION,
                INTERACTION_TYPE.PATROL_ACTION_FACTION,
            } },
        };
    }
    public List<INTERACTION_TYPE> GetFactionInteractionsOfRace(Character character, INTERACTION_CATEGORY category) {
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        if (_factionRaceInteractions.ContainsKey(character.race)) {
            INTERACTION_TYPE[] interactionArray = _factionRaceInteractions[character.race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i], character);
                if (interactionCategoryAndAlignment == null) { continue; }
                if (character.faction.morality == MORALITY.GOOD && interactionCategoryAndAlignment.alignment == INTERACTION_ALIGNMENT.EVIL) {
                    //Alignment must be good or neutral, so if it is evil, skip it
                    continue;
                }else if (character.faction.morality == MORALITY.EVIL && interactionCategoryAndAlignment.alignment == INTERACTION_ALIGNMENT.GOOD) {
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
        if (CharacterManager.Instance.characterRoleInteractions.ContainsKey(character.role.roleType)) {
            INTERACTION_TYPE[] interactionArray = CharacterManager.Instance.characterRoleInteractions[character.role.roleType];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i], character);
                if (interactionCategoryAndAlignment == null) { continue; }
                if (character.faction.morality == MORALITY.GOOD && interactionCategoryAndAlignment.alignment == INTERACTION_ALIGNMENT.EVIL) {
                    //Alignment must be good or neutral, so if it is evil, skip it
                    continue;
                } else if (character.faction.morality == MORALITY.EVIL && interactionCategoryAndAlignment.alignment == INTERACTION_ALIGNMENT.GOOD) {
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
        for (int i = 0; i < character.currentInteractionTypes.Count; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(character.currentInteractionTypes[i], character);
            if (interactionCategoryAndAlignment == null) { continue; }
            if (character.faction.morality == MORALITY.GOOD && interactionCategoryAndAlignment.alignment == INTERACTION_ALIGNMENT.EVIL) {
                //Alignment must be good or neutral, so if it is evil, skip it
                continue;
            } else if (character.faction.morality == MORALITY.EVIL && interactionCategoryAndAlignment.alignment == INTERACTION_ALIGNMENT.GOOD) {
                //Alignment must be evil or neutral, so if it is good, skip it
                continue;
            }
            for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                if (interactionCategoryAndAlignment.categories[j] == category) {
                    interactions.Add(character.currentInteractionTypes[i]);
                    break;
                }
            }
        }
        return interactions;
    }
    #endregion

    #region NPC Interaction
    private void ConstructNPCRaceInteractions() {
        _npcRaceInteractions = new Dictionary<RACE, INTERACTION_TYPE[]>() {
             { RACE.HUMANS, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_VISIT,
                INTERACTION_TYPE.ARGUE_ACTION,
                INTERACTION_TYPE.CURSE_ACTION,
                INTERACTION_TYPE.HUNT_ACTION,
                INTERACTION_TYPE.TRANSFER_HOME,
                INTERACTION_TYPE.TORTURE_ACTION_NPC,
                INTERACTION_TYPE.USE_ITEM_ON_CHARACTER,
                INTERACTION_TYPE.REST_AT_HOME_ACTION,
                INTERACTION_TYPE.EAT_HOME_MEAL_ACTION,
                INTERACTION_TYPE.POISON_HOUSE_FOOD,
                INTERACTION_TYPE.FEED_PRISONER_ACTION,
                INTERACTION_TYPE.BOOBY_TRAP_HOUSE,
                INTERACTION_TYPE.STEAL_ACTION_NPC,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.CAMP_OUT_ACTION,
                INTERACTION_TYPE.FORAGE_ACTION,
                INTERACTION_TYPE.HANG_OUT_ACTION,
                INTERACTION_TYPE.MAKE_LOVE_ACTION,
                INTERACTION_TYPE.REMOVE_CURSE_ACTION,
                INTERACTION_TYPE.RESTRAIN_CRIMINAL_ACTION,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                INTERACTION_TYPE.FIRST_AID_ACTION,
                //INTERACTION_TYPE.EAT_DEFENSELESS,
                INTERACTION_TYPE.PROTECT_ACTION,
                INTERACTION_TYPE.LOCATE_MISSING,
                INTERACTION_TYPE.EAT_INN_MEAL,
                INTERACTION_TYPE.REST_AT_INN,
                INTERACTION_TYPE.HUNT_SMALL_ANIMALS,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                //INTERACTION_TYPE.CRAFT_TOOL,
                INTERACTION_TYPE.PICK_ITEM,
                //INTERACTION_TYPE.MINE_ACTION,
                INTERACTION_TYPE.EAT_PLANT,
                INTERACTION_TYPE.EAT_SMALL_ANIMAL,
                INTERACTION_TYPE.EAT_DWELLING_TABLE,
                INTERACTION_TYPE.SLEEP,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.ABDUCT_ACTION,
                INTERACTION_TYPE.CARRY_CHARACTER,
                INTERACTION_TYPE.DROP_CHARACTER,
                INTERACTION_TYPE.DAYDREAM,
                INTERACTION_TYPE.PLAY_GUITAR,
                INTERACTION_TYPE.CHAT_CHARACTER,
                INTERACTION_TYPE.ARGUE_CHARACTER,
                //INTERACTION_TYPE.CRAFT_ITEM,
                INTERACTION_TYPE.STROLL,
                INTERACTION_TYPE.RETURN_HOME,
                INTERACTION_TYPE.DRINK,
                INTERACTION_TYPE.SLEEP_OUTSIDE,
                INTERACTION_TYPE.EXPLORE,
                INTERACTION_TYPE.TABLE_REMOVE_POISON,
                INTERACTION_TYPE.TABLE_POISON,
                INTERACTION_TYPE.PRAY,
                //INTERACTION_TYPE.CHOP_WOOD,
                INTERACTION_TYPE.MAGIC_CIRCLE_PERFORM_RITUAL,
                //INTERACTION_TYPE.PATROL,
                INTERACTION_TYPE.STEAL,
                //INTERACTION_TYPE.SCRAP,
                //INTERACTION_TYPE.GET_SUPPLY,
                INTERACTION_TYPE.DROP_SUPPLY,
                INTERACTION_TYPE.TILE_OBJECT_DESTROY,
                INTERACTION_TYPE.ITEM_DESTROY,
                INTERACTION_TYPE.TRAVEL,
                INTERACTION_TYPE.TRANSFORM_TO_WOLF,
                INTERACTION_TYPE.REVERT_TO_NORMAL,
                INTERACTION_TYPE.REPORT_CRIME,
                INTERACTION_TYPE.RESTRAIN_CHARACTER,
                INTERACTION_TYPE.FIRST_AID_CHARACTER,
                INTERACTION_TYPE.CURE_CHARACTER,
                INTERACTION_TYPE.CURSE_CHARACTER,
                INTERACTION_TYPE.DISPEL_MAGIC,
                INTERACTION_TYPE.JUDGE_CHARACTER,
                INTERACTION_TYPE.FEED,
            } },
            { RACE.ELVES, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_VISIT,
                INTERACTION_TYPE.ARGUE_ACTION,
                INTERACTION_TYPE.CURSE_ACTION,
                INTERACTION_TYPE.HUNT_ACTION,
                INTERACTION_TYPE.TRANSFER_HOME,
                INTERACTION_TYPE.TORTURE_ACTION_NPC,
                INTERACTION_TYPE.USE_ITEM_ON_CHARACTER,
                INTERACTION_TYPE.REST_AT_HOME_ACTION,
                INTERACTION_TYPE.EAT_HOME_MEAL_ACTION,
                INTERACTION_TYPE.POISON_HOUSE_FOOD,
                INTERACTION_TYPE.FEED_PRISONER_ACTION,
                INTERACTION_TYPE.BOOBY_TRAP_HOUSE,
                INTERACTION_TYPE.STEAL_ACTION_NPC,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.CAMP_OUT_ACTION,
                INTERACTION_TYPE.FORAGE_ACTION,
                INTERACTION_TYPE.HANG_OUT_ACTION,
                INTERACTION_TYPE.MAKE_LOVE_ACTION,
                INTERACTION_TYPE.REMOVE_CURSE_ACTION,
                INTERACTION_TYPE.RESTRAIN_CRIMINAL_ACTION,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                INTERACTION_TYPE.FIRST_AID_ACTION,
                //INTERACTION_TYPE.EAT_DEFENSELESS,
                INTERACTION_TYPE.PROTECT_ACTION,
                INTERACTION_TYPE.LOCATE_MISSING,
                INTERACTION_TYPE.EAT_INN_MEAL,
                INTERACTION_TYPE.REST_AT_INN,
                INTERACTION_TYPE.HUNT_SMALL_ANIMALS,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                //INTERACTION_TYPE.CRAFT_TOOL,
                INTERACTION_TYPE.PICK_ITEM,
                //INTERACTION_TYPE.MINE_ACTION,
                INTERACTION_TYPE.EAT_PLANT,
                INTERACTION_TYPE.EAT_SMALL_ANIMAL,
                INTERACTION_TYPE.EAT_DWELLING_TABLE,
                INTERACTION_TYPE.SLEEP,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.ABDUCT_ACTION,
                INTERACTION_TYPE.CARRY_CHARACTER,
                INTERACTION_TYPE.DROP_CHARACTER,
                INTERACTION_TYPE.DAYDREAM,
                INTERACTION_TYPE.PLAY_GUITAR,
                INTERACTION_TYPE.CHAT_CHARACTER,
                INTERACTION_TYPE.ARGUE_CHARACTER,
                //INTERACTION_TYPE.CRAFT_ITEM,
                INTERACTION_TYPE.STROLL,
                INTERACTION_TYPE.RETURN_HOME,
                INTERACTION_TYPE.DRINK,
                INTERACTION_TYPE.SLEEP_OUTSIDE,
                INTERACTION_TYPE.EXPLORE,
                INTERACTION_TYPE.TABLE_REMOVE_POISON,
                INTERACTION_TYPE.TABLE_POISON,
                INTERACTION_TYPE.PRAY,
                //INTERACTION_TYPE.CHOP_WOOD,
                INTERACTION_TYPE.MAGIC_CIRCLE_PERFORM_RITUAL,
                //INTERACTION_TYPE.PATROL,
                INTERACTION_TYPE.STEAL,
                //INTERACTION_TYPE.SCRAP,
                //INTERACTION_TYPE.GET_SUPPLY,
                INTERACTION_TYPE.DROP_SUPPLY,
                INTERACTION_TYPE.TILE_OBJECT_DESTROY,
                INTERACTION_TYPE.ITEM_DESTROY,
                INTERACTION_TYPE.TRAVEL,
                INTERACTION_TYPE.TRANSFORM_TO_WOLF,
                INTERACTION_TYPE.REVERT_TO_NORMAL,
                INTERACTION_TYPE.REPORT_CRIME,
                INTERACTION_TYPE.RESTRAIN_CHARACTER,
                //INTERACTION_TYPE.FIRST_AID_CHARACTER,
                //INTERACTION_TYPE.CURE_CHARACTER,
                INTERACTION_TYPE.CURSE_CHARACTER,
                //INTERACTION_TYPE.DISPEL_MAGIC,
                INTERACTION_TYPE.JUDGE_CHARACTER,
                INTERACTION_TYPE.FEED,
            } },
            { RACE.GOBLIN, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_VISIT,
                INTERACTION_TYPE.ARGUE_ACTION,
                INTERACTION_TYPE.CURSE_ACTION,
                INTERACTION_TYPE.HUNT_ACTION,
                INTERACTION_TYPE.TRANSFER_HOME,
                INTERACTION_TYPE.TORTURE_ACTION_NPC,
                INTERACTION_TYPE.USE_ITEM_ON_CHARACTER,
                INTERACTION_TYPE.REST_AT_HOME_ACTION,
                INTERACTION_TYPE.EAT_HOME_MEAL_ACTION,
                INTERACTION_TYPE.POISON_HOUSE_FOOD,
                INTERACTION_TYPE.FEED_PRISONER_ACTION,
                INTERACTION_TYPE.BOOBY_TRAP_HOUSE,
                INTERACTION_TYPE.STEAL_ACTION_NPC,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.CAMP_OUT_ACTION,
                INTERACTION_TYPE.FORAGE_ACTION,
                INTERACTION_TYPE.HANG_OUT_ACTION,
                INTERACTION_TYPE.MAKE_LOVE_ACTION,
                INTERACTION_TYPE.REMOVE_CURSE_ACTION,
                INTERACTION_TYPE.RESTRAIN_CRIMINAL_ACTION,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                INTERACTION_TYPE.FIRST_AID_ACTION,
                INTERACTION_TYPE.EAT_DEFENSELESS,
                INTERACTION_TYPE.PROTECT_ACTION,
                INTERACTION_TYPE.LOCATE_MISSING,
                INTERACTION_TYPE.EAT_INN_MEAL,
                INTERACTION_TYPE.REST_AT_INN,
                INTERACTION_TYPE.HUNT_SMALL_ANIMALS,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                //INTERACTION_TYPE.CRAFT_TOOL,
                INTERACTION_TYPE.PICK_ITEM,
                //INTERACTION_TYPE.MINE_ACTION,
                INTERACTION_TYPE.EAT_PLANT,
                INTERACTION_TYPE.EAT_SMALL_ANIMAL,
                INTERACTION_TYPE.EAT_DWELLING_TABLE,
                INTERACTION_TYPE.SLEEP,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.ABDUCT_ACTION,
                INTERACTION_TYPE.CARRY_CHARACTER,
                INTERACTION_TYPE.DROP_CHARACTER,
                INTERACTION_TYPE.DAYDREAM,
                INTERACTION_TYPE.PLAY_GUITAR,
                INTERACTION_TYPE.CHAT_CHARACTER,
                INTERACTION_TYPE.ARGUE_CHARACTER,
                //INTERACTION_TYPE.CRAFT_ITEM,
                INTERACTION_TYPE.STROLL,
                INTERACTION_TYPE.RETURN_HOME,
                INTERACTION_TYPE.DRINK,
                INTERACTION_TYPE.SLEEP_OUTSIDE,
                INTERACTION_TYPE.EXPLORE,
                INTERACTION_TYPE.TABLE_REMOVE_POISON,
                INTERACTION_TYPE.TABLE_POISON,
                INTERACTION_TYPE.PRAY,
                //INTERACTION_TYPE.CHOP_WOOD,
                INTERACTION_TYPE.MAGIC_CIRCLE_PERFORM_RITUAL,
                //INTERACTION_TYPE.PATROL,
                INTERACTION_TYPE.STEAL,
                //INTERACTION_TYPE.SCRAP,
                //INTERACTION_TYPE.GET_SUPPLY,
                INTERACTION_TYPE.DROP_SUPPLY,
                INTERACTION_TYPE.TILE_OBJECT_DESTROY,
                INTERACTION_TYPE.ITEM_DESTROY,
                INTERACTION_TYPE.TRAVEL,
                INTERACTION_TYPE.TRANSFORM_TO_WOLF,
                INTERACTION_TYPE.REVERT_TO_NORMAL,
                INTERACTION_TYPE.REPORT_CRIME,
                INTERACTION_TYPE.RESTRAIN_CHARACTER,
                //INTERACTION_TYPE.FIRST_AID_CHARACTER,
                //INTERACTION_TYPE.CURE_CHARACTER,
                INTERACTION_TYPE.CURSE_CHARACTER,
                //INTERACTION_TYPE.DISPEL_MAGIC,
                INTERACTION_TYPE.JUDGE_CHARACTER,
                INTERACTION_TYPE.FEED,
            } },
            { RACE.FAERY, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_VISIT,
                INTERACTION_TYPE.ARGUE_ACTION,
                INTERACTION_TYPE.CURSE_ACTION,
                INTERACTION_TYPE.HUNT_ACTION,
                INTERACTION_TYPE.TRANSFER_HOME,
                INTERACTION_TYPE.TORTURE_ACTION_NPC,
                INTERACTION_TYPE.USE_ITEM_ON_CHARACTER,
                INTERACTION_TYPE.REST_AT_HOME_ACTION,
                INTERACTION_TYPE.EAT_HOME_MEAL_ACTION,
                INTERACTION_TYPE.POISON_HOUSE_FOOD,
                INTERACTION_TYPE.FEED_PRISONER_ACTION,
                INTERACTION_TYPE.BOOBY_TRAP_HOUSE,
                INTERACTION_TYPE.STEAL_ACTION_NPC,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.CAMP_OUT_ACTION,
                INTERACTION_TYPE.FORAGE_ACTION,
                INTERACTION_TYPE.HANG_OUT_ACTION,
                INTERACTION_TYPE.MAKE_LOVE_ACTION,
                INTERACTION_TYPE.REMOVE_CURSE_ACTION,
                INTERACTION_TYPE.RESTRAIN_CRIMINAL_ACTION,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                INTERACTION_TYPE.FIRST_AID_ACTION,
                //INTERACTION_TYPE.EAT_DEFENSELESS,
                INTERACTION_TYPE.PROTECT_ACTION,
                INTERACTION_TYPE.LOCATE_MISSING,
                INTERACTION_TYPE.EAT_INN_MEAL,
                INTERACTION_TYPE.REST_AT_INN,
                INTERACTION_TYPE.HUNT_SMALL_ANIMALS,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                //INTERACTION_TYPE.CRAFT_TOOL,
                INTERACTION_TYPE.PICK_ITEM,
                //INTERACTION_TYPE.MINE_ACTION,
                INTERACTION_TYPE.EAT_PLANT,
                INTERACTION_TYPE.EAT_SMALL_ANIMAL,
                INTERACTION_TYPE.EAT_DWELLING_TABLE,
                INTERACTION_TYPE.SLEEP,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.ABDUCT_ACTION,
                INTERACTION_TYPE.CARRY_CHARACTER,
                INTERACTION_TYPE.DROP_CHARACTER,
                INTERACTION_TYPE.DAYDREAM,
                INTERACTION_TYPE.PLAY_GUITAR,
                INTERACTION_TYPE.CHAT_CHARACTER,
                INTERACTION_TYPE.ARGUE_CHARACTER,
                //INTERACTION_TYPE.CRAFT_ITEM,
                INTERACTION_TYPE.STROLL,
                INTERACTION_TYPE.RETURN_HOME,
                INTERACTION_TYPE.DRINK,
                INTERACTION_TYPE.SLEEP_OUTSIDE,
                INTERACTION_TYPE.EXPLORE,
                INTERACTION_TYPE.TABLE_REMOVE_POISON,
                INTERACTION_TYPE.TABLE_POISON,
                INTERACTION_TYPE.PRAY,
                //INTERACTION_TYPE.CHOP_WOOD,
                INTERACTION_TYPE.MAGIC_CIRCLE_PERFORM_RITUAL,
                //INTERACTION_TYPE.PATROL,
                INTERACTION_TYPE.STEAL,
                //INTERACTION_TYPE.SCRAP,
                //INTERACTION_TYPE.GET_SUPPLY,
                INTERACTION_TYPE.DROP_SUPPLY,
                INTERACTION_TYPE.TILE_OBJECT_DESTROY,
                INTERACTION_TYPE.ITEM_DESTROY,
                INTERACTION_TYPE.TRAVEL,
                INTERACTION_TYPE.TRANSFORM_TO_WOLF,
                INTERACTION_TYPE.REVERT_TO_NORMAL,
                INTERACTION_TYPE.REPORT_CRIME,
                INTERACTION_TYPE.RESTRAIN_CHARACTER,
                //INTERACTION_TYPE.FIRST_AID_CHARACTER,
                //INTERACTION_TYPE.CURE_CHARACTER,
                INTERACTION_TYPE.CURSE_CHARACTER,
                //INTERACTION_TYPE.DISPEL_MAGIC,
                INTERACTION_TYPE.JUDGE_CHARACTER,
                INTERACTION_TYPE.FEED,
            } },
            { RACE.SKELETON, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_VISIT,
                INTERACTION_TYPE.ARGUE_ACTION,
                INTERACTION_TYPE.CURSE_ACTION,
                INTERACTION_TYPE.HUNT_ACTION,
                INTERACTION_TYPE.TRANSFER_HOME,
                INTERACTION_TYPE.TORTURE_ACTION_NPC,
                INTERACTION_TYPE.USE_ITEM_ON_CHARACTER,
                INTERACTION_TYPE.REST_AT_HOME_ACTION,
                INTERACTION_TYPE.EAT_HOME_MEAL_ACTION,
                INTERACTION_TYPE.POISON_HOUSE_FOOD,
                INTERACTION_TYPE.FEED_PRISONER_ACTION,
                INTERACTION_TYPE.BOOBY_TRAP_HOUSE,
                INTERACTION_TYPE.STEAL_ACTION_NPC,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.CAMP_OUT_ACTION,
                INTERACTION_TYPE.FORAGE_ACTION,
                INTERACTION_TYPE.HANG_OUT_ACTION,
                INTERACTION_TYPE.MAKE_LOVE_ACTION,
                INTERACTION_TYPE.REMOVE_CURSE_ACTION,
                INTERACTION_TYPE.RESTRAIN_CRIMINAL_ACTION,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                INTERACTION_TYPE.FIRST_AID_ACTION,
                //INTERACTION_TYPE.EAT_DEFENSELESS,
                INTERACTION_TYPE.PROTECT_ACTION,
                INTERACTION_TYPE.LOCATE_MISSING,
                INTERACTION_TYPE.EAT_INN_MEAL,
                INTERACTION_TYPE.REST_AT_INN,
                INTERACTION_TYPE.HUNT_SMALL_ANIMALS,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                INTERACTION_TYPE.CRAFT_TOOL,
                INTERACTION_TYPE.PICK_ITEM,
                //INTERACTION_TYPE.MINE_ACTION,
                INTERACTION_TYPE.EAT_PLANT,
                INTERACTION_TYPE.EAT_SMALL_ANIMAL,
                INTERACTION_TYPE.EAT_DWELLING_TABLE,
                INTERACTION_TYPE.SLEEP,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.ABDUCT_ACTION,
                INTERACTION_TYPE.CARRY_CHARACTER,
                INTERACTION_TYPE.DROP_CHARACTER,
                INTERACTION_TYPE.DAYDREAM,
                INTERACTION_TYPE.PLAY_GUITAR,
                INTERACTION_TYPE.CHAT_CHARACTER,
                INTERACTION_TYPE.ARGUE_CHARACTER,
                //INTERACTION_TYPE.CRAFT_ITEM,
                INTERACTION_TYPE.STROLL,
                INTERACTION_TYPE.RETURN_HOME,
                INTERACTION_TYPE.DRINK,
                INTERACTION_TYPE.SLEEP_OUTSIDE,
                INTERACTION_TYPE.EXPLORE,
                INTERACTION_TYPE.TABLE_REMOVE_POISON,
                INTERACTION_TYPE.TABLE_POISON,
                INTERACTION_TYPE.PRAY,
                //INTERACTION_TYPE.CHOP_WOOD,
                INTERACTION_TYPE.MAGIC_CIRCLE_PERFORM_RITUAL,
                //INTERACTION_TYPE.PATROL,
                INTERACTION_TYPE.STEAL,
                //INTERACTION_TYPE.SCRAP,
                //INTERACTION_TYPE.GET_SUPPLY,
                INTERACTION_TYPE.DROP_SUPPLY,
                INTERACTION_TYPE.TILE_OBJECT_DESTROY,
                INTERACTION_TYPE.ITEM_DESTROY,
                INTERACTION_TYPE.TRAVEL,
                INTERACTION_TYPE.REPORT_CRIME,
                INTERACTION_TYPE.RESTRAIN_CHARACTER,
                //INTERACTION_TYPE.FIRST_AID_CHARACTER,
                //INTERACTION_TYPE.CURE_CHARACTER,
                INTERACTION_TYPE.CURSE_CHARACTER,
                //INTERACTION_TYPE.DISPEL_MAGIC,
                INTERACTION_TYPE.JUDGE_CHARACTER,
                INTERACTION_TYPE.FEED,
            } },
            { RACE.SPIDER, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_VISIT,
                INTERACTION_TYPE.ARGUE_ACTION,
                INTERACTION_TYPE.CURSE_ACTION,
                INTERACTION_TYPE.HUNT_ACTION,
                INTERACTION_TYPE.TRANSFER_HOME,
                INTERACTION_TYPE.TORTURE_ACTION_NPC,
                INTERACTION_TYPE.USE_ITEM_ON_CHARACTER,
                INTERACTION_TYPE.REST_AT_HOME_ACTION,
                INTERACTION_TYPE.EAT_HOME_MEAL_ACTION,
                INTERACTION_TYPE.POISON_HOUSE_FOOD,
                INTERACTION_TYPE.FEED_PRISONER_ACTION,
                INTERACTION_TYPE.BOOBY_TRAP_HOUSE,
                INTERACTION_TYPE.STEAL_ACTION_NPC,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.CAMP_OUT_ACTION,
                INTERACTION_TYPE.FORAGE_ACTION,
                INTERACTION_TYPE.HANG_OUT_ACTION,
                INTERACTION_TYPE.MAKE_LOVE_ACTION,
                INTERACTION_TYPE.REMOVE_CURSE_ACTION,
                INTERACTION_TYPE.RESTRAIN_CRIMINAL_ACTION,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                INTERACTION_TYPE.FIRST_AID_ACTION,
                INTERACTION_TYPE.EAT_DEFENSELESS,
                INTERACTION_TYPE.PROTECT_ACTION,
                INTERACTION_TYPE.LOCATE_MISSING,
                INTERACTION_TYPE.EAT_INN_MEAL,
                INTERACTION_TYPE.REST_AT_INN,
                INTERACTION_TYPE.HUNT_SMALL_ANIMALS,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                //INTERACTION_TYPE.CRAFT_TOOL,
                INTERACTION_TYPE.PICK_ITEM,
                //INTERACTION_TYPE.MINE_ACTION,
                INTERACTION_TYPE.EAT_PLANT,
                INTERACTION_TYPE.EAT_SMALL_ANIMAL,
                //INTERACTION_TYPE.EAT_DWELLING_TABLE,
                //INTERACTION_TYPE.SLEEP,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.ABDUCT_ACTION,
                INTERACTION_TYPE.CARRY_CHARACTER,
                INTERACTION_TYPE.DROP_CHARACTER,
                INTERACTION_TYPE.PLAY_GUITAR,
                //INTERACTION_TYPE.CRAFT_ITEM,
                INTERACTION_TYPE.STROLL,
                INTERACTION_TYPE.RETURN_HOME,
                INTERACTION_TYPE.SLEEP_OUTSIDE,
                INTERACTION_TYPE.EXPLORE,
                //INTERACTION_TYPE.CHOP_WOOD,
                //INTERACTION_TYPE.PATROL,
                INTERACTION_TYPE.STEAL,
                INTERACTION_TYPE.TILE_OBJECT_DESTROY,
                INTERACTION_TYPE.ITEM_DESTROY,
                INTERACTION_TYPE.TRAVEL,
                INTERACTION_TYPE.HUNT_ACTION,
                INTERACTION_TYPE.PLAY,
                INTERACTION_TYPE.FEED,
            } },
            { RACE.WOLF, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_VISIT,
                INTERACTION_TYPE.ARGUE_ACTION,
                INTERACTION_TYPE.CURSE_ACTION,
                INTERACTION_TYPE.HUNT_ACTION,
                INTERACTION_TYPE.TRANSFER_HOME,
                INTERACTION_TYPE.TORTURE_ACTION_NPC,
                INTERACTION_TYPE.USE_ITEM_ON_CHARACTER,
                INTERACTION_TYPE.REST_AT_HOME_ACTION,
                INTERACTION_TYPE.EAT_HOME_MEAL_ACTION,
                INTERACTION_TYPE.POISON_HOUSE_FOOD,
                INTERACTION_TYPE.FEED_PRISONER_ACTION,
                INTERACTION_TYPE.BOOBY_TRAP_HOUSE,
                INTERACTION_TYPE.STEAL_ACTION_NPC,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.CAMP_OUT_ACTION,
                INTERACTION_TYPE.FORAGE_ACTION,
                INTERACTION_TYPE.HANG_OUT_ACTION,
                INTERACTION_TYPE.MAKE_LOVE_ACTION,
                INTERACTION_TYPE.REMOVE_CURSE_ACTION,
                INTERACTION_TYPE.RESTRAIN_CRIMINAL_ACTION,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                INTERACTION_TYPE.FIRST_AID_ACTION,
                INTERACTION_TYPE.EAT_DEFENSELESS,
                INTERACTION_TYPE.PROTECT_ACTION,
                INTERACTION_TYPE.LOCATE_MISSING,
                INTERACTION_TYPE.EAT_INN_MEAL,
                INTERACTION_TYPE.REST_AT_INN,
                INTERACTION_TYPE.HUNT_SMALL_ANIMALS,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                //INTERACTION_TYPE.CRAFT_TOOL,
                INTERACTION_TYPE.PICK_ITEM,
                //INTERACTION_TYPE.MINE_ACTION,
                INTERACTION_TYPE.EAT_PLANT,
                INTERACTION_TYPE.EAT_SMALL_ANIMAL,
                //INTERACTION_TYPE.EAT_DWELLING_TABLE,
                //INTERACTION_TYPE.SLEEP,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.ABDUCT_ACTION,
                INTERACTION_TYPE.CARRY_CHARACTER,
                INTERACTION_TYPE.DROP_CHARACTER,
                INTERACTION_TYPE.PLAY_GUITAR,
                //INTERACTION_TYPE.CRAFT_ITEM,
                INTERACTION_TYPE.STROLL,
                INTERACTION_TYPE.RETURN_HOME,
                INTERACTION_TYPE.SLEEP_OUTSIDE,
                INTERACTION_TYPE.EXPLORE,
                //INTERACTION_TYPE.CHOP_WOOD,
                //INTERACTION_TYPE.PATROL,
                INTERACTION_TYPE.STEAL,
                INTERACTION_TYPE.TILE_OBJECT_DESTROY,
                INTERACTION_TYPE.ITEM_DESTROY,
                INTERACTION_TYPE.TRAVEL,
                INTERACTION_TYPE.HUNT_ACTION,
                INTERACTION_TYPE.PLAY,
                INTERACTION_TYPE.FEED,
                INTERACTION_TYPE.REVERT_TO_NORMAL,
            } },
            { RACE.DRAGON, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MOVE_TO_VISIT,
                INTERACTION_TYPE.ARGUE_ACTION,
                INTERACTION_TYPE.CURSE_ACTION,
                INTERACTION_TYPE.HUNT_ACTION,
                INTERACTION_TYPE.TRANSFER_HOME,
                INTERACTION_TYPE.TORTURE_ACTION_NPC,
                INTERACTION_TYPE.USE_ITEM_ON_CHARACTER,
                INTERACTION_TYPE.REST_AT_HOME_ACTION,
                INTERACTION_TYPE.EAT_HOME_MEAL_ACTION,
                INTERACTION_TYPE.POISON_HOUSE_FOOD,
                INTERACTION_TYPE.FEED_PRISONER_ACTION,
                INTERACTION_TYPE.BOOBY_TRAP_HOUSE,
                INTERACTION_TYPE.STEAL_ACTION_NPC,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.CAMP_OUT_ACTION,
                INTERACTION_TYPE.FORAGE_ACTION,
                INTERACTION_TYPE.HANG_OUT_ACTION,
                INTERACTION_TYPE.MAKE_LOVE_ACTION,
                INTERACTION_TYPE.REMOVE_CURSE_ACTION,
                INTERACTION_TYPE.RESTRAIN_CRIMINAL_ACTION,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                INTERACTION_TYPE.FIRST_AID_ACTION,
                INTERACTION_TYPE.EAT_DEFENSELESS,
                INTERACTION_TYPE.PROTECT_ACTION,
                INTERACTION_TYPE.LOCATE_MISSING,
                INTERACTION_TYPE.EAT_INN_MEAL,
                INTERACTION_TYPE.REST_AT_INN,
                INTERACTION_TYPE.HUNT_SMALL_ANIMALS,
                INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION,
                //INTERACTION_TYPE.CRAFT_TOOL,
                INTERACTION_TYPE.PICK_ITEM,
                //INTERACTION_TYPE.MINE_ACTION,
                INTERACTION_TYPE.EAT_PLANT,
                INTERACTION_TYPE.EAT_SMALL_ANIMAL,
                //INTERACTION_TYPE.EAT_DWELLING_TABLE,
                //INTERACTION_TYPE.SLEEP,
                INTERACTION_TYPE.ASSAULT_ACTION_NPC,
                INTERACTION_TYPE.ABDUCT_ACTION,
                INTERACTION_TYPE.CARRY_CHARACTER,
                INTERACTION_TYPE.DROP_CHARACTER,
                INTERACTION_TYPE.PLAY_GUITAR,
                //INTERACTION_TYPE.CRAFT_ITEM,
                INTERACTION_TYPE.STROLL,
                INTERACTION_TYPE.RETURN_HOME,
                INTERACTION_TYPE.SLEEP_OUTSIDE,
                INTERACTION_TYPE.EXPLORE,
                //INTERACTION_TYPE.CHOP_WOOD,
                //INTERACTION_TYPE.PATROL,
                INTERACTION_TYPE.STEAL,
                INTERACTION_TYPE.TILE_OBJECT_DESTROY,
                INTERACTION_TYPE.ITEM_DESTROY,
                INTERACTION_TYPE.TRAVEL,
                INTERACTION_TYPE.HUNT_ACTION,
                INTERACTION_TYPE.PLAY,
                INTERACTION_TYPE.FEED,
            } },
        };
    }
    public List<INTERACTION_TYPE> GetNPCInteractionsOfRace(Character character) {
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        if (_npcRaceInteractions.ContainsKey(character.race)) {
            for (int i = 0; i < _npcRaceInteractions[character.race].Length; i++) {
                interactions.Add(_npcRaceInteractions[character.race][i]);
            }
        }
        if (character.role.allowedInteractions != null) {
            for (int i = 0; i < character.role.allowedInteractions.Length; i++) {
                interactions.Add(character.role.allowedInteractions[i]);
            }
        }
        for (int i = 0; i < character.currentInteractionTypes.Count; i++) {
            interactions.Add(character.currentInteractionTypes[i]);
        }
        return interactions;
    }
    public List<INTERACTION_TYPE> GetNPCInteractionsOfRace(Character character, INTERACTION_CATEGORY category, Character targetCharacter = null) { 
        //If there is a character passed as parameter, this means that the list will be filtered by what the character can do
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        if (_npcRaceInteractions.ContainsKey(character.race)) {
            INTERACTION_TYPE[] interactionArray = _npcRaceInteractions[character.race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i], character);
                if (interactionCategoryAndAlignment == null) { continue; }
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
        if (character.role.allowedInteractions != null) {
            INTERACTION_TYPE[] interactionArray = character.role.allowedInteractions;
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i], character);
                if (interactionCategoryAndAlignment == null) { continue; }
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
        for (int i = 0; i < character.currentInteractionTypes.Count; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(character.currentInteractionTypes[i], character);
            if (interactionCategoryAndAlignment == null) { continue; }
            for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                if (interactionCategoryAndAlignment.categories[j] == category) {
                    if (character != null && InteractionManager.Instance.CanCreateInteraction(character.currentInteractionTypes[i], character, targetCharacter)) {
                        interactions.Add(character.currentInteractionTypes[i]);
                    }
                    break;
                }
            }
        }
        return interactions;
    }
    public List<INTERACTION_TYPE> GetNPCInteractionsOfRace(Character character, INTERACTION_CATEGORY category, INTERACTION_CHARACTER_EFFECT characterEffect, string[] characterEffectStrings, bool checkTargetCharacterEffect, Character targetCharacter = null) {
        //If there is a character passed as parameter, this means that the list will be filtered by what the character can do
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        if (_npcRaceInteractions.ContainsKey(character.race)) {
            INTERACTION_TYPE[] interactionArray = _npcRaceInteractions[character.race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i], character);
                if (interactionCategoryAndAlignment == null) { continue; }
                for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                    if (interactionCategoryAndAlignment.categories[j] == category) {
                        bool canDoCharacterEffect = false;
                        InteractionCharacterEffect[] interactionCharacterEffect = interactionCategoryAndAlignment.targetCharacterEffect;
                        if (!checkTargetCharacterEffect) {
                            interactionCharacterEffect = interactionCategoryAndAlignment.actorEffect;
                        }
                        if (interactionCharacterEffect != null) {
                            for (int k = 0; k < interactionCharacterEffect.Length; k++) {
                                if (interactionCharacterEffect[k].effect != INTERACTION_CHARACTER_EFFECT.NONE &&
                                    interactionCharacterEffect[k].effect == characterEffect) {
                                    if (interactionCharacterEffect[k].effectString != null && interactionCharacterEffect[k].effectString != string.Empty) {
                                        if (characterEffectStrings.Contains(interactionCharacterEffect[k].effectString)) {
                                            canDoCharacterEffect = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (canDoCharacterEffect && character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character, targetCharacter)) {
                            interactions.Add(interactionArray[i]);
                        }
                        break;
                    }
                }
            }
        }
        if (character.role.allowedInteractions != null) {
            INTERACTION_TYPE[] interactionArray = character.role.allowedInteractions;
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i], character);
                if (interactionCategoryAndAlignment == null) { continue; }
                for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                    if (interactionCategoryAndAlignment.categories[j] == category) {
                        bool canDoCharacterEffect = false;
                        InteractionCharacterEffect[] interactionCharacterEffect = interactionCategoryAndAlignment.targetCharacterEffect;
                        if (!checkTargetCharacterEffect) {
                            interactionCharacterEffect = interactionCategoryAndAlignment.actorEffect;
                        }
                        if (interactionCharacterEffect != null) {
                            for (int k = 0; k < interactionCharacterEffect.Length; k++) {
                                if (interactionCharacterEffect[k].effect != INTERACTION_CHARACTER_EFFECT.NONE &&
                                    interactionCharacterEffect[k].effect == characterEffect) {
                                    if (interactionCharacterEffect[k].effectString != null && interactionCharacterEffect[k].effectString != string.Empty) {
                                        if (characterEffectStrings.Contains(interactionCharacterEffect[k].effectString)) {
                                            canDoCharacterEffect = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (canDoCharacterEffect && character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character, targetCharacter)) {
                            interactions.Add(interactionArray[i]);
                        }
                        break;
                    }
                }
            }
        }
        for (int i = 0; i < character.currentInteractionTypes.Count; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(character.currentInteractionTypes[i], character);
            if (interactionCategoryAndAlignment == null) { continue; }
            for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                if (interactionCategoryAndAlignment.categories[j] == category) {
                    bool canDoCharacterEffect = false;
                    InteractionCharacterEffect[] interactionCharacterEffect = interactionCategoryAndAlignment.targetCharacterEffect;
                    if (!checkTargetCharacterEffect) {
                        interactionCharacterEffect = interactionCategoryAndAlignment.actorEffect;
                    }
                    if (interactionCharacterEffect != null) {
                        for (int k = 0; k < interactionCharacterEffect.Length; k++) {
                            if (interactionCharacterEffect[k].effect != INTERACTION_CHARACTER_EFFECT.NONE &&
                                interactionCharacterEffect[k].effect == characterEffect) {
                                if (interactionCharacterEffect[k].effectString != null && interactionCharacterEffect[k].effectString != string.Empty) {
                                    if (characterEffectStrings.Contains(interactionCharacterEffect[k].effectString)) {
                                        canDoCharacterEffect = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (canDoCharacterEffect && character != null && InteractionManager.Instance.CanCreateInteraction(character.currentInteractionTypes[i], character, targetCharacter)) {
                        interactions.Add(character.currentInteractionTypes[i]);
                    }
                    break;
                }
            }
        }
        return interactions;
    }
    public List<INTERACTION_TYPE> GetNPCInteractionsOfRace(Character character, INTERACTION_CHARACTER_EFFECT characterEffect, string[] characterEffectStrings, bool checkTargetCharacterEffect, Character targetCharacter = null) {
        //If there is a character passed as parameter, this means that the list will be filtered by what the character can do
        List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
        if (_npcRaceInteractions.ContainsKey(character.race)) {
            INTERACTION_TYPE[] interactionArray = _npcRaceInteractions[character.race];
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i], character);
                if (interactionCategoryAndAlignment == null) { continue; }
                for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                    bool canDoCharacterEffect = false;
                    InteractionCharacterEffect[] interactionCharacterEffect = interactionCategoryAndAlignment.targetCharacterEffect;
                    if (!checkTargetCharacterEffect) {
                        interactionCharacterEffect = interactionCategoryAndAlignment.actorEffect;
                    }
                    if (interactionCharacterEffect != null) {
                        for (int k = 0; k < interactionCharacterEffect.Length; k++) {
                            if (interactionCharacterEffect[k].effect != INTERACTION_CHARACTER_EFFECT.NONE &&
                                interactionCharacterEffect[k].effect == characterEffect) {
                                if (interactionCharacterEffect[k].effectString != null && interactionCharacterEffect[k].effectString != string.Empty) {
                                    if (characterEffectStrings.Contains(interactionCharacterEffect[k].effectString)) {
                                        canDoCharacterEffect = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (canDoCharacterEffect && character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character, targetCharacter)) {
                        interactions.Add(interactionArray[i]);
                    }
                    break;
                }
            }
        }
        if (character.role.allowedInteractions != null) {
            INTERACTION_TYPE[] interactionArray = character.role.allowedInteractions;
            for (int i = 0; i < interactionArray.Length; i++) {
                InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionArray[i], character);
                if (interactionCategoryAndAlignment == null) { continue; }
                for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                    bool canDoCharacterEffect = false;
                    InteractionCharacterEffect[] interactionCharacterEffect = interactionCategoryAndAlignment.targetCharacterEffect;
                    if (!checkTargetCharacterEffect) {
                        interactionCharacterEffect = interactionCategoryAndAlignment.actorEffect;
                    }
                    if (interactionCharacterEffect != null) {
                        for (int k = 0; k < interactionCharacterEffect.Length; k++) {
                            if (interactionCharacterEffect[k].effect != INTERACTION_CHARACTER_EFFECT.NONE &&
                                interactionCharacterEffect[k].effect == characterEffect) {
                                if (interactionCharacterEffect[k].effectString != null && interactionCharacterEffect[k].effectString != string.Empty) {
                                    if (characterEffectStrings.Contains(interactionCharacterEffect[k].effectString)) {
                                        canDoCharacterEffect = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (canDoCharacterEffect && character != null && InteractionManager.Instance.CanCreateInteraction(interactionArray[i], character, targetCharacter)) {
                        interactions.Add(interactionArray[i]);
                    }
                    break;
                }
            }
        }
        for (int i = 0; i < character.currentInteractionTypes.Count; i++) {
            InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(character.currentInteractionTypes[i], character);
            if (interactionCategoryAndAlignment == null) { continue; }
            for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
                bool canDoCharacterEffect = false;
                InteractionCharacterEffect[] interactionCharacterEffect = interactionCategoryAndAlignment.targetCharacterEffect;
                if (!checkTargetCharacterEffect) {
                    interactionCharacterEffect = interactionCategoryAndAlignment.actorEffect;
                }
                if (interactionCharacterEffect != null) {
                    for (int k = 0; k < interactionCharacterEffect.Length; k++) {
                        if (interactionCharacterEffect[k].effect != INTERACTION_CHARACTER_EFFECT.NONE &&
                            interactionCharacterEffect[k].effect == characterEffect) {
                            if (interactionCharacterEffect[k].effectString != null && interactionCharacterEffect[k].effectString != string.Empty) {
                                if (characterEffectStrings.Contains(interactionCharacterEffect[k].effectString)) {
                                    canDoCharacterEffect = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (canDoCharacterEffect && character != null && InteractionManager.Instance.CanCreateInteraction(character.currentInteractionTypes[i], character, targetCharacter)) {
                    interactions.Add(character.currentInteractionTypes[i]);
                }
                break;
            }
        }
        return interactions;
    }
    public INTERACTION_TYPE CheckNPCInteractionOfRace(Character character, INTERACTION_TYPE interactionType, INTERACTION_CHARACTER_EFFECT characterEffect, string[] characterEffectStrings, bool checkTargetCharacterEffect, Character targetCharacter = null) {
        InteractionAttributes interactionCategoryAndAlignment = InteractionManager.Instance.GetCategoryAndAlignment(interactionType, character);
        if (interactionCategoryAndAlignment == null) { return INTERACTION_TYPE.NONE; }
        for (int j = 0; j < interactionCategoryAndAlignment.categories.Length; j++) {
            bool canDoCharacterEffect = false;
            InteractionCharacterEffect[] interactionCharacterEffect = interactionCategoryAndAlignment.targetCharacterEffect;
            if (!checkTargetCharacterEffect) {
                interactionCharacterEffect = interactionCategoryAndAlignment.actorEffect;
            }
            if (interactionCharacterEffect != null) {
                for (int k = 0; k < interactionCharacterEffect.Length; k++) {
                    if (interactionCharacterEffect[k].effect != INTERACTION_CHARACTER_EFFECT.NONE &&
                        interactionCharacterEffect[k].effect == characterEffect) {
                        if (interactionCharacterEffect[k].effectString != null && interactionCharacterEffect[k].effectString != string.Empty) {
                            if (characterEffectStrings.Contains(interactionCharacterEffect[k].effectString)) {
                                canDoCharacterEffect = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (canDoCharacterEffect && character != null && InteractionManager.Instance.CanCreateInteraction(interactionType, character, targetCharacter)) {
                return interactionType;
            }
            break;
        }
        return INTERACTION_TYPE.NONE;
    }
    #endregion
}
