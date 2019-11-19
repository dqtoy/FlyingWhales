using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class DispelMagic : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public DispelMagic() : base(INTERACTION_TYPE.DISPEL_MAGIC) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.FirstAid_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Enchantment", false, GOAP_EFFECT_TARGET.TARGET ));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Dispel Magic Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 1;
    }
    public override GoapActionInvalidity IsInvalid(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(actor, poiTarget, otherData);
        if (goapActionInvalidity.isInvalid == false) {
            if ((poiTarget as Character).IsInOwnParty() == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return poiTarget.traitContainer.HasTraitOf(TRAIT_TYPE.ENCHANTMENT) && actor.specificLocation.GetTileObjectsOfType(TILE_OBJECT_TYPE.MAGIC_CIRCLE).Count > 0;
        }
        return false;
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        List<TileObject> magicCircle = goapNode.actor.specificLocation.GetTileObjectsOfType(TILE_OBJECT_TYPE.MAGIC_CIRCLE);
        TileObject chosen = magicCircle[Random.Range(0, magicCircle.Count)];
        return chosen;
    }
    #endregion

    #region State Effects
    public void PreDispelMagicSuccess(ActualGoapNode goapNode) {
        //**Pre Effect 1**: Prevent movement of Target
        //(poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(1);

    }
    public void AfterDispelMagicSuccess(ActualGoapNode goapNode) {
        //**After Effect 1**: Reduce all of target's Enchantment type traits
        goapNode.poiTarget.traitContainer.RemoveAllTraitsByType(goapNode.poiTarget, TRAIT_TYPE.ENCHANTMENT);
    }
    #endregion
}

public class DispelMagicData : GoapActionData {
    public DispelMagicData() : base(INTERACTION_TYPE.DISPEL_MAGIC) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, };
        requirementAction = Requirement;
    }
    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.traitContainer.GetNormalTrait("Reanimated", "Cursed") != null;
    }
}
