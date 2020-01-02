using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Inner_Maps;
using UnityEngine;

public partial class InteractionManager : MonoBehaviour {
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
        ConstructGoapActionData();
        ConstructAllGoapActionAdvertisements();
    }

    private void ConstructAllGoapActionAdvertisements() {
        POINT_OF_INTEREST_TYPE[] poiTypes = Utilities.GetEnumValues<POINT_OF_INTEREST_TYPE>();
        allGoapActionAdvertisements = new Dictionary<POINT_OF_INTEREST_TYPE, List<GoapAction>>();
        for (int i = 0; i < poiTypes.Length; i++) {
            POINT_OF_INTEREST_TYPE currType = poiTypes[i];
            allGoapActionAdvertisements.Add(currType, new List<GoapAction>());
        }
        for (int i = 0; i < goapActionData.Values.Count; i++) {
            GoapAction currAction = goapActionData.Values.ElementAt(i);
            for (int j = 0; j < currAction.advertisedBy.Length; j++) {
                POINT_OF_INTEREST_TYPE currType = currAction.advertisedBy[j];
                allGoapActionAdvertisements[currType].Add(currAction);
            }
        }
    }
    private void ConstructGoapActionData() {
        goapActionData = new Dictionary<INTERACTION_TYPE, GoapAction>();
        INTERACTION_TYPE[] allGoapActions = Utilities.GetEnumValues<INTERACTION_TYPE>();
        for (int i = 0; i < allGoapActions.Length; i++) {
            INTERACTION_TYPE currType = allGoapActions[i];
            var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(currType.ToString());
            System.Type type = System.Type.GetType(typeName);
            if (type != null) {
                GoapAction data = System.Activator.CreateInstance(type) as GoapAction;
                goapActionData.Add(currType, data);
            } else {
                Debug.LogWarning(currType.ToString() + " has no data!");
            }
        }
    }
    public bool CanSatisfyGoapActionRequirements(INTERACTION_TYPE goapType, Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (goapActionData.ContainsKey(goapType)) {
            return goapActionData[goapType].CanSatisfyRequirements(actor, poiTarget, otherData);
        }
        throw new Exception("No Goap Action Data for " + goapType.ToString());
    }

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
    private bool CanRegionAdvertiseActionTo(Region region, Character actor, INTERACTION_TYPE interactionType) {
        GoapAction action = goapActionData[interactionType];
        return action.CanSatisfyRequirements(actor, region.regionTileObject, null);
    }
    public Region GetRandomRegionTarget(Character actor, INTERACTION_TYPE interactionType) {
        List<Region> choices = new List<Region>();
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (CanRegionAdvertiseActionTo(currRegion, actor, interactionType)) {
                choices.Add(currRegion);
            }
        }
        if (choices.Count > 0) {
            return Utilities.GetRandomElement(choices);    
        }
        Debug.LogWarning($"{actor.name} cannot find a region to target with action {interactionType.ToString()}");
        return null;
    }
    #endregion

    public int GetInitialPriority(JOB_TYPE jobType) {
        int priority = 0;
        switch (jobType) {
            case JOB_TYPE.STOP_TORNADO:
            case JOB_TYPE.INTERRUPTION:
                priority = 2;
                break;
            case JOB_TYPE.COMBAT:
                priority = 3;
                break;
            case JOB_TYPE.TRIGGER_FLAW:
                priority = 4;
                break;
            case JOB_TYPE.MISC:
            case JOB_TYPE.RETURN_HOME:
            case JOB_TYPE.CORRUPT_CULTIST:
            case JOB_TYPE.DESTROY_FOOD:
            case JOB_TYPE.DESTROY_SUPPLY:
            case JOB_TYPE.SABOTAGE_FACTION:
            case JOB_TYPE.SCREAM:
            case JOB_TYPE.HUNT_SERIAL_KILLER_VICTIM:
            //case JOB_TYPE.INTERRUPTION:
                priority = 5;
                break;
            case JOB_TYPE.TANTRUM:
            case JOB_TYPE.CLAIM_REGION:
            case JOB_TYPE.CLEANSE_REGION:
            case JOB_TYPE.ATTACK_DEMONIC_REGION:
            case JOB_TYPE.ATTACK_NON_DEMONIC_REGION:
            case JOB_TYPE.INVADE_REGION:
                priority = 6;
                break;
            // case JOB_TYPE.IDLE:
            //     priority = 7;
            //     break;
            case JOB_TYPE.DEATH:
            case JOB_TYPE.BERSERK:
            case JOB_TYPE.STEAL:
            case JOB_TYPE.RESOLVE_CONFLICT:
            case JOB_TYPE.DESTROY:
                priority = 10;
                break;
            case JOB_TYPE.KNOCKOUT:
            case JOB_TYPE.SEDUCE:
            case JOB_TYPE.UNDERMINE_ENEMY:
                priority = 20;
                break;
            case JOB_TYPE.HUNGER_RECOVERY_STARVING:
            case JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED:
                priority = 30;
                break;
            case JOB_TYPE.APPREHEND:
            case JOB_TYPE.DOUSE_FIRE:
                priority = 40;
                break;
            case JOB_TYPE.REMOVE_TRAIT:
                priority = 50;
                break;
            case JOB_TYPE.RESTRAIN:
                priority = 60;
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
            case JOB_TYPE.DESTROY_PROFANE_LANDMARK:
            case JOB_TYPE.PERFORM_HOLY_INCANTATION:
            case JOB_TYPE.PRAY_GODDESS_STATUE:
            case JOB_TYPE.REACT_TO_SCREAM:
                priority = 120;
                break;
            case JOB_TYPE.BREAK_UP:
                priority = 130;
                break;
            case JOB_TYPE.PATROL:
                priority = 170;
                break;
            case JOB_TYPE.JUDGEMENT:
                priority = 220;
                break;
            case JOB_TYPE.SUICIDE:
            case JOB_TYPE.HAUL:
                priority = 230;
                break;
            case JOB_TYPE.CRAFT_OBJECT:
            case JOB_TYPE.PRODUCE_FOOD:
            case JOB_TYPE.PRODUCE_WOOD:
            case JOB_TYPE.PRODUCE_STONE:
            case JOB_TYPE.PRODUCE_METAL:
            case JOB_TYPE.TAKE_PERSONAL_FOOD:
            case JOB_TYPE.DROP:
            case JOB_TYPE.INSPECT:
            case JOB_TYPE.PLACE_BLUEPRINT:
            case JOB_TYPE.BUILD_BLUEPRINT:
            case JOB_TYPE.OBTAIN_PERSONAL_ITEM:
                priority = 240;
                break;
            case JOB_TYPE.HUNGER_RECOVERY:
            case JOB_TYPE.TIREDNESS_RECOVERY:
            case JOB_TYPE.HAPPINESS_RECOVERY:
                priority = 270;
                break;
            case JOB_TYPE.STROLL:
            case JOB_TYPE.IDLE:
                priority = 290;
                break;
            case JOB_TYPE.IMPROVE:
            case JOB_TYPE.EXPLORE:
                priority = 300;
                break;
        }
        return priority;
    }

    #region Precondition Resolvers
    public bool TargetHasNegativeTraitEffect(Character actor, IPointOfInterest target) {
        return target.traitContainer.HasTraitOf(TRAIT_EFFECT.NEGATIVE);
    }
    #endregion
}