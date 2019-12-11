public class ClaimRegion : GoapAction {
    
    public ClaimRegion() : base(INTERACTION_TYPE.CLAIM_REGION) {
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REGION_OWNED_BY_ACTOR_FACTION, string.Empty, false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Claim Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 1;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        log.AddToFillers(node.poiTarget.gridTileLocation.parentMap.location.coreTile.region, node.poiTarget.gridTileLocation.parentMap.location.coreTile.region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            //**Requirements:** Actor has Purifier trait. Region is corrupted. Region does not have a landmark.
            var region = poiTarget.gridTileLocation.parentMap.location.coreTile.region;
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null && region.owner == null && region.coreTile.isCorrupted == false;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreClaimSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(goapNode.actor.faction, goapNode.actor.faction.name, LOG_IDENTIFIER.FACTION_1);
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.parentMap.location.coreTile.region, goapNode.poiTarget.gridTileLocation.parentMap.location.coreTile.region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterClaimSuccess(ActualGoapNode goapNode) {
        var region = goapNode.poiTarget.gridTileLocation.parentMap.location.coreTile.region;
        LandmarkManager.Instance.OwnRegion(goapNode.actor.faction, goapNode.actor.faction.race, region);
    }
    #endregion
}