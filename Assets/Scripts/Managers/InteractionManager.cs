using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public class InteractionManager : MonoBehaviour {
    public static InteractionManager Instance = null;

    public delegate T ObjectActivator<T>(params object[] args);

    public static readonly string Supply_Cache_Reward_1 = "SupplyCacheReward1";
    public static readonly string Mana_Cache_Reward_1 = "ManaCacheReward1";
    public static readonly string Mana_Cache_Reward_2 = "ManaCacheReward2";
    public static readonly string Level_Reward_1 = "LevelReward1";
    public static readonly string Level_Reward_2 = "LevelReward2";

    public const string Goap_State_Success = "Success";
    public const string Goap_State_Fail = "Fail";

    public static readonly int Character_Action_Delay = 5;

    private string dailyInteractionSummary;

    public Dictionary<string, RewardConfig> rewardConfig = new Dictionary<string, RewardConfig>(){
        { Supply_Cache_Reward_1, new RewardConfig(){ rewardType = REWARD.SUPPLY, lowerRange = 50, higherRange = 250 } },
        { Mana_Cache_Reward_1, new RewardConfig(){ rewardType = REWARD.MANA, lowerRange = 5, higherRange = 30 } },
        { Mana_Cache_Reward_2, new RewardConfig(){ rewardType = REWARD.MANA, lowerRange = 30, higherRange = 50 } },
        { Level_Reward_1, new RewardConfig(){ rewardType = REWARD.LEVEL, lowerRange = 1, higherRange = 1 } },
        { Level_Reward_2, new RewardConfig(){ rewardType = REWARD.LEVEL, lowerRange = 2, higherRange = 2 } },
    };

    private void Awake() {
        Instance = this;
    }
    public void Initialize() {
        //Messenger.AddListener(Signals.TICK_ENDED_2, ExecuteInteractionsDefault); //TryExecuteInteractionsDefault

        //ConstructorInfo ctor = typeof(GoapAction).GetConstructors().First();
        //goapActionCreator = GetActivator<GoapAction>(ctor);
    }

    public GoapAction CreateNewGoapInteraction(INTERACTION_TYPE type, Character actor, IPointOfInterest target, bool willInitialize = true) {
        GoapAction goapAction = null;
        switch (type) {
            case INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION:
                goapAction = new ReleaseCharacter(actor, target);
                break;
            case INTERACTION_TYPE.EAT_PLANT:
                goapAction = new EatPlant(actor, target);
                break;
            case INTERACTION_TYPE.EAT_SMALL_ANIMAL:
                goapAction = new EatAnimal(actor, target);
                break;
            case INTERACTION_TYPE.EAT_DWELLING_TABLE:
                goapAction = new EatAtTable(actor, target);
                break;
            case INTERACTION_TYPE.CRAFT_ITEM:
                goapAction = new CraftItemGoap(actor, target);
                break;
            case INTERACTION_TYPE.PICK_ITEM:
                goapAction = new PickItemGoap(actor, target);
                break;
            case INTERACTION_TYPE.MINE_ACTION:
                goapAction = new MineGoap(actor, target);
                break;
            case INTERACTION_TYPE.SLEEP:
                goapAction = new Sleep(actor, target);
                break;
            case INTERACTION_TYPE.ASSAULT_ACTION_NPC:
                goapAction = new AssaultCharacter(actor, target);
                break;
            case INTERACTION_TYPE.ABDUCT_ACTION:
                goapAction = new AbductCharacter(actor, target);
                break;
            case INTERACTION_TYPE.CARRY_CHARACTER:
                goapAction = new CarryCharacter(actor, target);
                break;
            case INTERACTION_TYPE.DROP_CHARACTER:
                goapAction = new DropCharacter(actor, target);
                break;
            case INTERACTION_TYPE.DAYDREAM:
                goapAction = new Daydream(actor, target);
                break;
            case INTERACTION_TYPE.PLAY_GUITAR:
                goapAction = new PlayGuitar(actor, target);
                break;
            case INTERACTION_TYPE.CHAT_CHARACTER:
                goapAction = new ChatCharacter(actor, target);
                break;
            case INTERACTION_TYPE.ARGUE_CHARACTER:
                goapAction = new ArgueCharacter(actor, target);
                break;
            case INTERACTION_TYPE.STROLL:
                goapAction = new Stroll(actor, target);
                break;
            case INTERACTION_TYPE.RETURN_HOME:
                goapAction = new ReturnHome(actor, target);
                break;
            case INTERACTION_TYPE.DRINK:
                goapAction = new Drink(actor, target);
                break;
            case INTERACTION_TYPE.SLEEP_OUTSIDE:
                goapAction = new SleepOutside(actor, target);
                break;
            case INTERACTION_TYPE.EXPLORE:
                goapAction = new Explore(actor, target);
                break;
            case INTERACTION_TYPE.REMOVE_POISON_TABLE:
                goapAction = new TableRemovePoison(actor, target);
                break;
            case INTERACTION_TYPE.TABLE_POISON:
                goapAction = new TablePoison(actor, target);
                break;
            case INTERACTION_TYPE.PRAY:
                goapAction = new Pray(actor, target);
                break;
            case INTERACTION_TYPE.CHOP_WOOD:
                goapAction = new ChopWood(actor, target);
                break;
            case INTERACTION_TYPE.MAGIC_CIRCLE_PERFORM_RITUAL:
                goapAction = new MagicCirclePerformRitual(actor, target);
                break;
            case INTERACTION_TYPE.PATROL:
                goapAction = new Patrol(actor, target);
                break;
            case INTERACTION_TYPE.STEAL:
                goapAction = new Steal(actor, target);
                break;
            case INTERACTION_TYPE.SCRAP:
                goapAction = new Scrap(actor, target);
                break;
            case INTERACTION_TYPE.GET_SUPPLY:
                goapAction = new GetSupply(actor, target);
                break;
            case INTERACTION_TYPE.DROP_SUPPLY:
                goapAction = new DropSupply(actor, target);
                break;
            case INTERACTION_TYPE.TILE_OBJECT_DESTROY:
                goapAction = new TileObjectDestroy(actor, target);
                break;
            case INTERACTION_TYPE.ITEM_DESTROY:
                goapAction = new ItemDestroy(actor, target);
                break;
            case INTERACTION_TYPE.TRAVEL:
                goapAction = new Travel(actor, target);
                break;
            case INTERACTION_TYPE.RETURN_HOME_LOCATION:
                goapAction = new ReturnHomeLocation(actor, target);
                break;
            case INTERACTION_TYPE.HUNT_ACTION:
                goapAction = new Hunt(actor, target);
                break;
            case INTERACTION_TYPE.PLAY:
                goapAction = new Play(actor, target);
                break;
            case INTERACTION_TYPE.PATROL_ROAM:
                goapAction = new PatrolRoam(actor, target);
                break;
            case INTERACTION_TYPE.TRANSFORM_TO_WOLF:
                goapAction = new TransformToWolfForm(actor, target);
                break;
            case INTERACTION_TYPE.REVERT_TO_NORMAL:
                goapAction = new RevertToNormalForm(actor, target);
                break;
            case INTERACTION_TYPE.EAT_CORPSE:
                goapAction = new EatCorpse(actor, target);
                break;
            case INTERACTION_TYPE.RESTRAIN_CHARACTER:
                goapAction = new RestrainCharacter(actor, target);
                break;
            case INTERACTION_TYPE.FIRST_AID_CHARACTER:
                goapAction = new FirstAidCharacter(actor, target);
                break;
            case INTERACTION_TYPE.CURE_CHARACTER:
                goapAction = new CureCharacter(actor, target);
                break;
            case INTERACTION_TYPE.CURSE_CHARACTER:
                goapAction = new CurseCharacter(actor, target);
                break;
            case INTERACTION_TYPE.DISPEL_MAGIC:
                goapAction = new DispelMagic(actor, target);
                break;
            case INTERACTION_TYPE.JUDGE_CHARACTER:
                goapAction = new JudgeCharacter(actor, target);
                break;
            case INTERACTION_TYPE.REPORT_CRIME:
                goapAction = new ReportCrime(actor, target);
                break;
            case INTERACTION_TYPE.FEED:
                goapAction = new Feed(actor, target);
                break;
            case INTERACTION_TYPE.STEAL_CHARACTER:
                goapAction = new StealFromCharacter(actor, target);
                break;
            case INTERACTION_TYPE.DROP_ITEM:
                goapAction = new DropItemHome(actor, target);
                break;
            case INTERACTION_TYPE.DROP_ITEM_WAREHOUSE:
                goapAction = new DropItemWarehouse(actor, target);
                break;
            case INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER:
                goapAction = new AskForHelpSaveCharacter(actor, target);
                break;
            case INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE:
                goapAction = new AskForHelpRemovePoisonTable(actor, target);
                break;
            case INTERACTION_TYPE.STAND:
                goapAction = new Stand(actor, target);
                break;
            case INTERACTION_TYPE.SIT:
                goapAction = new Sit(actor, target);
                break;
            case INTERACTION_TYPE.NAP:
                goapAction = new Nap(actor, target);
                break;
            case INTERACTION_TYPE.BURY_CHARACTER:
                goapAction = new BuryCharacter(actor, target);
                break;
            case INTERACTION_TYPE.CARRY_CORPSE:
                goapAction = new CarryCorpse(actor, target);
                break;
            case INTERACTION_TYPE.REMEMBER_FALLEN:
                goapAction = new RememberFallen(actor, target);
                break;
            case INTERACTION_TYPE.SPIT:
                goapAction = new Spit(actor, target);
                break;
            case INTERACTION_TYPE.REPORT_HOSTILE:
                goapAction = new ReportHostile(actor, target);
                break;
            case INTERACTION_TYPE.INVITE_TO_MAKE_LOVE:
                goapAction = new InviteToMakeLove(actor, target);
                break;
            case INTERACTION_TYPE.MAKE_LOVE:
                goapAction = new MakeLove(actor, target);
                break;
            case INTERACTION_TYPE.DRINK_BLOOD:
                goapAction = new DrinkBlood(actor, target);
                break;
            case INTERACTION_TYPE.REPLACE_TILE_OBJECT:
                goapAction = new ReplaceTileObject(actor, target);
                break;
            case INTERACTION_TYPE.CRAFT_FURNITURE:
                goapAction = new CraftFurniture(actor, target);
                break;
            case INTERACTION_TYPE.TANTRUM:
                goapAction = new Tantrum(actor, target);
                break;
            case INTERACTION_TYPE.EAT_MUSHROOM:
                goapAction = new EatMushroom(actor, target);
                break;
            case INTERACTION_TYPE.SPREAD_RUMOR_REMOVE_FRIENDSHIP:
                goapAction = new SpreadRumorRemoveFriendship(actor, target);
                break;
            case INTERACTION_TYPE.SPREAD_RUMOR_REMOVE_LOVE:
                goapAction = new SpreadRumorRemoveLove(actor, target);
                break;
            case INTERACTION_TYPE.BREAK_UP:
                goapAction = new BreakUp(actor, target);
                break;
            case INTERACTION_TYPE.SHARE_INFORMATION:
                goapAction = new ShareInformation(actor, target);
                break;
            case INTERACTION_TYPE.WATCH:
                goapAction = new Watch(actor, target);
                break;
            case INTERACTION_TYPE.INSPECT:
                goapAction = new Inspect(actor, target);
                break;
            case INTERACTION_TYPE.EAT_CHARACTER:
                goapAction = new EatCharacter(actor, target);
                break;
            case INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD:
                goapAction = new HuntingToDrinkBlood(actor, target);
                break;
            case INTERACTION_TYPE.ROAMING_TO_STEAL:
                goapAction = new RoamingToSteal(actor, target);
                break;
            case INTERACTION_TYPE.PUKE:
                goapAction = new Puke(actor, target);
                break;
            case INTERACTION_TYPE.SEPTIC_SHOCK:
                goapAction = new SepticShock(actor, target);
                break;
        }
        if(goapAction != null && willInitialize) {
            goapAction.Initialize();
        }
        return goapAction;
    }
    public Reward GetReward(string rewardName) {
        if (rewardConfig.ContainsKey(rewardName)) {
            RewardConfig config = rewardConfig[rewardName];
            return new Reward { rewardType = config.rewardType, amount = UnityEngine.Random.Range(config.lowerRange, config.higherRange + 1) };
        }
        throw new System.Exception("There is no reward configuration with name " + rewardName);
    }
    //public void UnlockAllTokens() {
    //    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
    //        Character currCharacter = CharacterManager.Instance.allCharacters[i];
    //        if (!currCharacter.isDefender) {
    //            PlayerManager.Instance.player.AddToken(currCharacter.characterToken);
    //        }
    //    }
    //    for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
    //        Area currArea = LandmarkManager.Instance.allAreas[i];
    //        PlayerManager.Instance.player.AddToken(currArea.locationToken);
    //        PlayerManager.Instance.player.AddToken(currArea.defenderToken);
    //    }
    //    for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
    //        Faction currFaction = FactionManager.Instance.allFactions[i];
    //        PlayerManager.Instance.player.AddToken(currFaction.factionToken);
    //    }
    //}

    #region Move To Save
    private bool CanCreateMoveToSave(Character character) {
        /*
         * Trigger Criteria 1: There is at least one other area that his faction does not own that has at least one Abducted character 
         * that is not part of that area's faction that is either from this character's faction or from a Neutral, Friend or Ally factions
         */
        if (character.race == RACE.HUMANS || character.race == RACE.ELVES || character.race == RACE.SPIDER) {
            List<Area> otherAreas = new List<Area>(LandmarkManager.Instance.allAreas.Where(x => x.owner != null && x.owner.id != character.faction.id));
            for (int i = 0; i < otherAreas.Count; i++) {
                Area currArea = otherAreas[i];
                for (int j = 0; j < currArea.charactersAtLocation.Count; j++) {
                    Character currCharacter = currArea.charactersAtLocation[j];
                    Abducted abductedTrait = currCharacter.GetNormalTrait("Abducted") as Abducted;
                    if (abductedTrait != null && currArea.owner.id != currCharacter.faction.id) { //check if character is abducted and that the area he/she is in is not owned by their faction
                        if (currCharacter.faction.id == character.faction.id) {
                            return true;
                        } else {
                            FactionRelationship rel = character.faction.GetRelationshipWith(currCharacter.faction);
                            switch (rel.relationshipStatus) {
                                case FACTION_RELATIONSHIP_STATUS.NEUTRAL:
                                case FACTION_RELATIONSHIP_STATUS.FRIEND:
                                case FACTION_RELATIONSHIP_STATUS.ALLY:
                                    return true;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }
    #endregion

    #region Intel
    public Intel CreateNewIntel(IPointOfInterest poi) {
        switch (poi.poiType) {
            case POINT_OF_INTEREST_TYPE.ITEM:
            case POINT_OF_INTEREST_TYPE.TILE_OBJECT:
                return new TileObjectIntel(poi);
            default:
                return new Intel();
        }
    }
    public Intel CreateNewIntel(params object[] obj) {
        if (obj[0] is GoapPlan) {
            return new PlanIntel(obj[1] as Character, obj[0] as GoapPlan);
        } else if (obj[0] is GoapAction) {
            GoapAction action = obj[0] as GoapAction;
            switch (action.goapType) {
                case INTERACTION_TYPE.TABLE_POISON:
                    return new PoisonTableIntel(obj[1] as Character, obj[0] as GoapAction);
                default:
                    return new EventIntel(obj[1] as Character, obj[0] as GoapAction);
            }

            
        }
        return null;
    }
    #endregion

    #region Goap Action Utilities
    public LocationGridTile GetTargetLocationTile(ACTION_LOCATION_TYPE locationType, Character actor, LocationGridTile knownPOITargetLocation, params object[] other) {
        List<LocationGridTile> choices;
        LocationStructure specifiedStructure;
        LocationGridTile chosenTile = null;
        //Action Location says where the character will go to when performing the action:
        switch (locationType) {
            case ACTION_LOCATION_TYPE.IN_PLACE:
                //**In Place**: where he currently is
                chosenTile = actor.gridTileLocation;
                break;
            case ACTION_LOCATION_TYPE.NEARBY:
                //**Nearby**: an unoccupied tile within a 3 tile radius around the character
                if(actor.gridTileLocation != null) {
                    choices = actor.specificLocation.areaMap.GetTilesInRadius(actor.gridTileLocation, 3).Where(x => !x.isOccupied && x.structure != null).ToList();
                    if (choices.Count > 0) {
                        chosenTile = choices[Utilities.rng.Next(0, choices.Count)];
                    }
                }
                break;
            case ACTION_LOCATION_TYPE.RANDOM_LOCATION:
                //**Random Location**: chooses a random unoccupied tile in the specified structure
                specifiedStructure = other[0] as LocationStructure;
                choices = specifiedStructure.unoccupiedTiles.Where(x => x.reservedObjectType == TILE_OBJECT_TYPE.NONE).ToList();
                if (choices.Count > 0) {
                    chosenTile = choices[Utilities.rng.Next(0, choices.Count)];
                }
                break;
            case ACTION_LOCATION_TYPE.RANDOM_LOCATION_B:
                //**Random Location B**: chooses a random unoccupied tile in the specified structure that is also adjacent to one other unoccupied tile
                specifiedStructure = other[0] as LocationStructure;
                choices = specifiedStructure.unoccupiedTiles.Where(x => x.UnoccupiedNeighbours.Count > 0 && x.reservedObjectType == TILE_OBJECT_TYPE.NONE).ToList();
                if (choices.Count > 0) {
                    chosenTile = choices[Utilities.rng.Next(0, choices.Count)];
                }
                break;
            case ACTION_LOCATION_TYPE.NEAR_TARGET:
                //**Near Target**: adjacent unoccupied tile beside the target item, tile object, character
                if(actor.gridTileLocation != null) {
                    choices = knownPOITargetLocation.UnoccupiedNeighbours.Where(x => x.structure != null).OrderBy(x => Vector2.Distance(actor.gridTileLocation.localLocation, x.localLocation)).ToList();
                    if (choices.Where(x => x.charactersHere.Contains(actor)).Count() > 0) {
                        //if the actors current location is already part of the choices, stay in place
                        chosenTile = actor.gridTileLocation;
                    } else if (choices.Count > 0) {
                        //chosenTile = choices[Utilities.rng.Next(0, choices.Count)];
                        chosenTile = choices[0];
                    }
                }
                break;
            case ACTION_LOCATION_TYPE.ON_TARGET:
                //**On Target**: in the same tile as the target item or tile object
                //if(knownPOITargetLocation.occupant == null || knownPOITargetLocation.occupant == actor) {
                chosenTile = knownPOITargetLocation;
                //}
                break;
            default:
                break;
        }
        //if (chosenTile != null && chosenTile.occupant != null) {
        //    throw new Exception(actor.name + " is going to an occupied tile!");
        //}
        return chosenTile;
    }
    #endregion

    public int GetInitialPriority(JOB_TYPE jobType) {
        int priority = 0;
        switch (jobType) {
            case JOB_TYPE.BERSERK:
            case JOB_TYPE.TANTRUM:
            case JOB_TYPE.STEAL:
                priority = 10;
                break;
            case JOB_TYPE.KNOCKOUT:
            case JOB_TYPE.ABDUCT:
            case JOB_TYPE.UNDERMINE_ENEMY:
                priority = 20;
                break;
            case JOB_TYPE.HUNGER_RECOVERY_STARVING:
            case JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED:
                priority = 30;
                break;
            case JOB_TYPE.REPORT_HOSTILE:
            case JOB_TYPE.APPREHEND:
            case JOB_TYPE.REPORT_CRIME:
                priority = 40;
                break;
            case JOB_TYPE.REMOVE_TRAIT:
                priority = 50;
                break;
            case JOB_TYPE.RESTRAIN:
                priority = 60;
                break;
            case JOB_TYPE.REMOVE_POISON:
                priority = 70;
                break;
            case JOB_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE:
                priority = 80;
                break;
            case JOB_TYPE.SAVE_CHARACTER:
                priority = 90;
                break;
            case JOB_TYPE.ASK_FOR_HELP_SAVE_CHARACTER:
                priority = 90;
                break;
            case JOB_TYPE.HAPPINESS_RECOVERY_FORLORN:
                priority = 100;
                break;
            case JOB_TYPE.WATCH:
                priority = 109;
                break;
            case JOB_TYPE.FEED:
                priority = 110;
                break;
            case JOB_TYPE.BURY:
            case JOB_TYPE.CRAFT_TOOL:
            case JOB_TYPE.BREW_POTION:
            case JOB_TYPE.OBTAIN_SUPPLY:
                priority = 120;
                break;
            case JOB_TYPE.BREAK_UP:
                priority = 130;
                break;
            case JOB_TYPE.REPLACE_TILE_OBJECT:
                priority = 140;
                break;
            case JOB_TYPE.EXPLORE:
                priority = 150;
                break;
            case JOB_TYPE.DELIVER_TREASURE:
                priority = 160;
                break;
            case JOB_TYPE.PATROL:
                priority = 170;
                break;
            case JOB_TYPE.HUNGER_RECOVERY:
            //priority = 180;
            //break;
            case JOB_TYPE.TIREDNESS_RECOVERY:
            //priority = 190;
            //break;
            case JOB_TYPE.HAPPINESS_RECOVERY:
            //priority = 200;
            //break;
            case JOB_TYPE.SHARE_INFORMATION:
            //priority = 210;
            //break;
            case JOB_TYPE.JUDGEMENT:
                priority = 220;
                break;
            case JOB_TYPE.BUILD_FURNITURE:
            case JOB_TYPE.OBTAIN_ITEM:
                priority = 230;
                break;
        }
        return priority;
    }
}

public struct RewardConfig {
    public REWARD rewardType;
    public int lowerRange;
    public int higherRange;
}
public struct Reward {
    public REWARD rewardType;
    public int amount;
}
[System.Serializable]
public struct CharacterInteractionWeight {
    public INTERACTION_TYPE interactionType;
    public int weight;
}

public class InteractionAttributes {
    public INTERACTION_CATEGORY[] categories;
    public INTERACTION_ALIGNMENT alignment;
    public Precondition[] preconditions;
    public InteractionCharacterEffect[] actorEffect;
    public InteractionCharacterEffect[] targetCharacterEffect;
    public int cost;
}
public struct InteractionCharacterEffect {
    public INTERACTION_CHARACTER_EFFECT effect;
    public string effectString;
}