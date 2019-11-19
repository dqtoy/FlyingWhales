using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public class InteractionManager : MonoBehaviour {
    public static InteractionManager Instance = null;

    public const string Goap_State_Success = "Success";
    public const string Goap_State_Fail = "Fail";

    public static readonly int Character_Action_Delay = 5;

    private string dailyInteractionSummary;
    public Dictionary<INTERACTION_TYPE, GoapAction> goapActionData { get; private set; }
    public Dictionary<POINT_OF_INTEREST_TYPE, List<GoapAction>> allGoapActionAdvertisements { get; private set; }

    private void Awake() {
        Instance = this;
    }
    public void Initialize() {
        //Messenger.AddListener(Signals.TICK_ENDED_2, ExecuteInteractionsDefault); //TryExecuteInteractionsDefault

        //ConstructorInfo ctor = typeof(GoapAction).GetConstructors().First();
        //goapActionCreator = GetActivator<GoapAction>(ctor);
        ConstructGoapActionData();
    }

    //public GoapAction CreateNewGoapInteraction(INTERACTION_TYPE type, Character actor, IPointOfInterest target, bool willInitialize = true) {
    //    var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(type.ToString());
    //    System.Type systemType = System.Type.GetType(typeName);
    //    GoapAction goapAction = null;
    //    if (systemType != null) {
    //        goapAction = System.Activator.CreateInstance(systemType, actor, target) as GoapAction;
    //    }
    //    if (goapAction == null) {
    //        throw new Exception("There is no goap action: " + type.ToString());
    //    }
    //    if (goapAction != null && willInitialize) {
    //        goapAction.Initialize();
    //    }
    //    return goapAction;
    //}
    private void ConstructGoapActionData() {
        goapActionData = new Dictionary<INTERACTION_TYPE, GoapAction>();
        INTERACTION_TYPE[] allGoapActions = Utilities.GetEnumValues<INTERACTION_TYPE>();
        for (int i = 0; i < allGoapActions.Length; i++) {
            INTERACTION_TYPE currType = allGoapActions[i];
            var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(currType.ToString());
            System.Type type = System.Type.GetType(typeName);
            if(type != null) {
                GoapAction data = System.Activator.CreateInstance(type) as GoapAction;
                goapActionData.Add(currType, data);
            }
        }
    }
    public bool CanSatisfyGoapActionRequirements(INTERACTION_TYPE goapType, Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (goapActionData.ContainsKey(goapType)) {
            return goapActionData[goapType].CanSatisfyRequirements(actor, poiTarget, otherData);
        }
        throw new Exception("No Goap Action Data for " + goapType.ToString());
    }
    //public bool CanSatisfyGoapActionRequirementsOnBuildTree(INTERACTION_TYPE goapType, Character actor, IPointOfInterest poiTarget, object[] otherData) {
    //    if (goapActionData.ContainsKey(goapType)) {
    //        return goapActionData[goapType].CanSatisfyRequirementOnBuildGoapTree(actor, poiTarget, otherData);
    //    }
    //    throw new Exception("No Goap Action Data for " + goapType.ToString());
    //}

    #region Intel
    public Intel CreateNewIntel(params object[] obj) {
        if (obj[0] is GoapAction) {
            GoapAction action = obj[0] as GoapAction;
            switch (action.goapType) {
                case INTERACTION_TYPE.POISON:
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
            //case ACTION_LOCATION_TYPE.ON_TARGET:
            //    //**On Target**: in the same tile as the target item or tile object
            //    //if(knownPOITargetLocation.occupant == null || knownPOITargetLocation.occupant == actor) {
            //    chosenTile = knownPOITargetLocation;
            //    //}
            //    break;
            default:
                break;
        }
        //if (chosenTile != null && chosenTile.occupant != null) {
        //    throw new Exception(actor.name + " is going to an occupied tile!");
        //}
        return chosenTile;
    }
    /// <summary>
    /// Utility function to determine if the action type can be advertised, 
    /// even if the advertising actor is inactive.
    /// </summary>
    /// <param name="type">The action type</param>
    /// <returns>true or false</returns>
    public bool CanBeAdvertisedWhileCharacterIsUnavailable(INTERACTION_TYPE type) {
        switch (type) {
            case INTERACTION_TYPE.LAUGH_AT:
            case INTERACTION_TYPE.TEASE:
            case INTERACTION_TYPE.FEELING_CONCERNED:
                return true;
            default:
                return false;
        }
    }
    #endregion

    public int GetInitialPriority(JOB_TYPE jobType) {
        int priority = 0;
        switch (jobType) {
            case JOB_TYPE.CHEAT:
            case JOB_TYPE.HAVE_AFFAIR:
            case JOB_TYPE.TRIGGER_FLAW:
            case JOB_TYPE.MISC:
            case JOB_TYPE.RETURN_HOME:
            case JOB_TYPE.CORRUPT_CULTIST:
            case JOB_TYPE.DESTROY_FOOD:
            case JOB_TYPE.DESTROY_SUPPLY:
            case JOB_TYPE.SABOTAGE_FACTION:
            case JOB_TYPE.SCREAM:
            case JOB_TYPE.HUNT_SERIAL_KILLER_VICTIM:
                priority = 5;
                break;
            case JOB_TYPE.TANTRUM:
                priority = 6;
                break;
            case JOB_TYPE.DEATH:
            case JOB_TYPE.BERSERK:
            case JOB_TYPE.STEAL:
            case JOB_TYPE.RESOLVE_CONFLICT:
            case JOB_TYPE.DESTROY:
                priority = 10;
                break;
            case JOB_TYPE.KNOCKOUT:
            case JOB_TYPE.ABDUCT:
            case JOB_TYPE.SEDUCE:
            case JOB_TYPE.UNDERMINE_ENEMY:
                priority = 20;
                break;
            case JOB_TYPE.HUNGER_RECOVERY_STARVING:
            case JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED:
                priority = 30;
                break;
            case JOB_TYPE.APPREHEND:
            case JOB_TYPE.ATTEMPT_TO_STOP_JOB:
            case JOB_TYPE.REMOVE_FIRE:
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
            case JOB_TYPE.FEED:
                priority = 110;
                break;
            case JOB_TYPE.BURY:
            case JOB_TYPE.REPAIR:
            case JOB_TYPE.WATCH:
            case JOB_TYPE.BUILD_TILE_OBJECT:
            case JOB_TYPE.BUILD_GODDESS_STATUE:
            case JOB_TYPE.DESTROY_PROFANE_LANDMARK:
            case JOB_TYPE.PERFORM_HOLY_INCANTATION:
            case JOB_TYPE.PRAY_GODDESS_STATUE:
            case JOB_TYPE.REACT_TO_SCREAM:
            case JOB_TYPE.CHAT:
                priority = 120;
                break;
            case JOB_TYPE.BREAK_UP:
                priority = 130;
                break;
            case JOB_TYPE.REPLACE_TILE_OBJECT:
                priority = 140;
                break;
            //case JOB_TYPE.EXPLORE:
            //    priority = 150;
            //    break;
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
            case JOB_TYPE.SUICIDE:
                priority = 230;
                break;
            case JOB_TYPE.CRAFT_TOOL:
            case JOB_TYPE.BREW_POTION:
            case JOB_TYPE.OBTAIN_SUPPLY:
            case JOB_TYPE.OBTAIN_FOOD:
            case JOB_TYPE.DROP:
            case JOB_TYPE.INSPECT:
                priority = 240;
                break;
            //case JOB_TYPE.WATCH:
            //    priority = 250;
            //    break;
            case JOB_TYPE.BUILD_FURNITURE:
            case JOB_TYPE.OBTAIN_ITEM:
            case JOB_TYPE.MOVE_OUT:
            case JOB_TYPE.OBTAIN_FOOD_OUTSIDE:
            case JOB_TYPE.OBTAIN_SUPPLY_OUTSIDE:
            case JOB_TYPE.IMPROVE:
            case JOB_TYPE.EXPLORE:
            case JOB_TYPE.COMBAT:
            case JOB_TYPE.SEARCHING_WORLD_EVENT:
                priority = 300;
                break;
        }
        return priority;
    }

    #region Job Checkers
    public bool CanDoPatrolAndExplore(Character character, JobQueueItem job) {
        return character.traitContainer.GetNormalTrait("Injured") == null;
    }
    public bool IsSuicideJobStillValid(Character character, JobQueueItem item) {
        return character.traitContainer.GetNormalTrait("Forlorn") != null;
    }
    public bool CanMoveOut(Character character, JobQueueItem item) {
        TIME_IN_WORDS time = TIME_IN_WORDS.MORNING;
        if (character.traitContainer.GetNormalTrait("Nocturnal") != null) {
            //if nocturnal get after midnight
            time = TIME_IN_WORDS.AFTER_MIDNIGHT;
        }
        return character.role.roleType != CHARACTER_ROLE.LEADER && GameManager.GetTimeInWordsOfTick(GameManager.Instance.tick) == time; //Only non-leaders can take move out job, and it must also be in the morning time.
    }
    public bool CanDoCraftFurnitureJob(Character character, JobQueueItem item) {
        object[] otherData = (item as GoapPlanJob).otherData[INTERACTION_TYPE.CRAFT_FURNITURE];
        FURNITURE_TYPE furnitureToCreate = (FURNITURE_TYPE) otherData[1];
        return furnitureToCreate.ConvertFurnitureToTileObject().CanBeCraftedBy(character);
    }
    public bool CanDoDestroyProfaneJob(Character character, JobQueueItem item) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanDoCombatJob(Character character, JobQueueItem item) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanDoObtainFoodOutsideJob(Character character, JobQueueItem item) {
        return character.role.roleType == CHARACTER_ROLE.CIVILIAN;
    }
    public bool CanDoObtainSupplyOutsideJob(Character character, JobQueueItem item) {
        return character.role.roleType == CHARACTER_ROLE.CIVILIAN;
    }
    public bool CanDoHolyIncantationJob(Character character, JobQueueItem item) {
        return character.role.roleType == CHARACTER_ROLE.ADVENTURER;
    }
    public bool CanDoExploreJob(Character character, JobQueueItem item) {
        return character.role.roleType == CHARACTER_ROLE.ADVENTURER;
    }
    public bool CanDoJudgementJob(Character character, JobQueueItem job) {
        return character.role.roleType == CHARACTER_ROLE.NOBLE || character.role.roleType == CHARACTER_ROLE.LEADER;
    }
    public bool CanDoSabotageFactionJob(Character character, JobQueueItem item) {
        return character.traitContainer.GetNormalTrait("Cultist") != null;
    }
    public bool CanCraftTool(Character character, JobQueueItem job) {
        //return character.HasExtraTokenInInventory(SPECIAL_TOKEN.TOOL);
        return SPECIAL_TOKEN.TOOL.CanBeCraftedBy(character);
    }
    public bool CanDoObtainSupplyJob(Character character, JobQueueItem job) {
        return character.role.roleType == CHARACTER_ROLE.CIVILIAN;
    }
    public bool CanCharacterTakeBuildGoddessStatueJob(Character character, JobQueueItem item) {
        return character.traitContainer.GetNormalTrait("Craftsman") != null;
    }
    public bool CanBrewPotion(Character character, JobQueueItem job) {
        //return character.HasExtraTokenInInventory(SPECIAL_TOKEN.HEALING_POTION);
        return SPECIAL_TOKEN.HEALING_POTION.CanBeCraftedBy(character);
    }
    public bool CanTakeBuryJob(Character character, JobQueueItem job) {
        if (!character.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL) && character.isAtHomeRegion && character.isPartOfHomeFaction
                && character.role.roleType != CHARACTER_ROLE.BEAST) {
            return character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.CIVILIAN;
        }
        return false;
    }
    public bool CanCharacterTakeRemoveTraitJob(Character character, Character targetCharacter, JobQueueItem job) {
        if (character != targetCharacter && character.faction == targetCharacter.faction && character.isAtHomeRegion) {
            if(job != null) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (targetCharacter.traitContainer.GetNormalTrait((string) goapJob.goal.conditionKey).IsResponsibleForTrait(character)) {
                    return false;
                }
            }
            if (character.faction.id == FactionManager.Instance.neutralFaction.id) {
                return character.race == targetCharacter.race && character.homeArea == targetCharacter.homeArea && !targetCharacter.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TRAIT.ENEMY);
            }
            return !character.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY);
        }
        return false;
    }
    public bool CanCharacterTakeRemoveIllnessesJob(Character character, Character targetCharacter, JobQueueItem job) {
        if (character != targetCharacter && character.faction == targetCharacter.faction && character.isAtHomeRegion) {
            if (job != null) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (targetCharacter.traitContainer.GetNormalTrait((string) goapJob.goal.conditionKey).IsResponsibleForTrait(character)) {
                    return false;
                }
                //try {
                //} catch {
                //    throw new Exception("Problem with CanCharacterTakeRemoveIllnessesJob of " + character.name + ". Target character is " + (targetCharacter?.name ?? "Null") + ". Job is " + (goapJob?.name ?? "Null"));
                //}
            }
            if (character.faction.id == FactionManager.Instance.neutralFaction.id) {
                return character.race == targetCharacter.race && character.homeArea == targetCharacter.homeArea && !targetCharacter.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TRAIT.ENEMY);
            }
            return !character.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY); //&& character.traitContainer.GetNormalTrait("Doctor") != null;
        }
        return false;
    }
    public bool CanCharacterTakeRemoveSpecialIllnessesJob(Character character, Character targetCharacter, JobQueueItem job) {
        if (character != targetCharacter && character.faction == targetCharacter.faction && character.isAtHomeRegion) {
            if (job != null) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (targetCharacter.traitContainer.GetNormalTrait((string) goapJob.goal.conditionKey).IsResponsibleForTrait(character)) {
                    return false;
                }
                //try {
                //} catch {
                //    return false;
                //    //throw new Exception("Problem with CanCharacterTakeRemoveSpecialIllnessesJob of " + character.name + ". Target character is " + (targetCharacter?.name ?? "Null") + ". Job is " + (goapJob?.name ?? "Null"));
                //}
                
            }
            if (character.faction.id == FactionManager.Instance.neutralFaction.id) {
                return character.race == targetCharacter.race && character.homeArea == targetCharacter.homeArea && !targetCharacter.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TRAIT.ENEMY);
            }
            return !character.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY) && character.traitContainer.GetNormalTrait("Doctor") != null;
        }
        return false;
    }
    public bool CanCharacterTakeApprehendJob(Character character, Character targetCharacter, JobQueueItem job) {
        if (character.isAtHomeRegion && !character.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL) && character.traitContainer.GetNormalTrait("Coward") == null) {
            return character.role.roleType == CHARACTER_ROLE.SOLDIER && character.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) != RELATIONSHIP_EFFECT.POSITIVE;
        }
        return false;
    }
    public bool CanCharacterTakeRestrainJob(Character character, Character targetCharacter, JobQueueItem job) {
        return targetCharacter.faction != character.faction && character.isAtHomeRegion && character.isPartOfHomeFaction
            && (character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.CIVILIAN || character.role.roleType == CHARACTER_ROLE.ADVENTURER)
            && character.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) != RELATIONSHIP_EFFECT.POSITIVE && !character.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL);
    }
    public bool CanCharacterTakeRepairJob(Character character, JobQueueItem job) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.CIVILIAN || character.role.roleType == CHARACTER_ROLE.ADVENTURER;
    }
    public bool CanCharacterTakeReplaceTileObjectJob(Character character, JobQueueItem job) {
        object[] otherData = (job as GoapPlanJob).otherData[INTERACTION_TYPE.REPLACE_TILE_OBJECT];
        TileObject removedObj = otherData[0] as TileObject;
        return removedObj.tileObjectType.CanBeCraftedBy(character);
    }
    public bool CanCharacterTakeParalyzedFeedJob(Character sourceCharacter, Character character, JobQueueItem job) {
        return sourceCharacter != character && sourceCharacter.faction == character.faction && sourceCharacter.relationshipContainer.GetRelationshipEffectWith(character.currentAlterEgo) != RELATIONSHIP_EFFECT.NEGATIVE;
    }
    public bool CanCharacterTakeRestrainedFeedJob(Character sourceCharacter, Character character, JobQueueItem job) {
        if (sourceCharacter.specificLocation.region.IsResident(character)) {
            if (character.faction.id != FactionManager.Instance.neutralFaction.id) {
                return character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.CIVILIAN;
            } else {
                return character.role.roleType != CHARACTER_ROLE.BEAST && sourceCharacter.currentStructure.structureType.IsOpenSpace();
            }
        }
        return false;
    }
    public bool CanCharacterTakeDropJob(Character sourceCharacter, Character character, JobQueueItem job) {
        return sourceCharacter != character && sourceCharacter.faction == character.faction && character.relationshipContainer.GetRelationshipEffectWith(sourceCharacter.currentAlterEgo) != RELATIONSHIP_EFFECT.NEGATIVE;
    }
    public bool CanCharacterTakeKnockoutJob(Character character, Character targetCharacter, JobQueueItem job) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.ADVENTURER; // && !HasRelationshipOfEffectWith(targetCharacter, TRAIT_EFFECT.POSITIVE)
    }
    public void OnTakeBrewPotion(Character character, JobQueueItem job) {
        GoapPlanJob j = job as GoapPlanJob;
        //j.ClearForcedActions();
        //j.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.HEALING_POTION.ToString(), targetPOI = character }, INTERACTION_TYPE.CRAFT_ITEM);
    }
    public void OnTakeCraftTool(Character character, JobQueueItem job) {
        GoapPlanJob j = job as GoapPlanJob;
        //j.ClearForcedActions();
        //j.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.TOOL.ToString(), targetPOI = character }, INTERACTION_TYPE.CRAFT_ITEM);
    }
    #endregion

    #region Precondition Resolvers
    public bool TargetHasNegativeTraitEffect(Character actor, IPointOfInterest target) {
        return target.traitContainer.HasTraitOf(TRAIT_EFFECT.NEGATIVE);
    }
    #endregion
}