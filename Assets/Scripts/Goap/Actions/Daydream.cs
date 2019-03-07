﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Daydream : GoapAction {

    private LocationStructure _targetStructure;
    private LocationGridTile _targetTile;
    public override LocationStructure targetStructure { get { return _targetStructure; } }
    public override LocationGridTile targetTile { get { return _targetTile; } }

    public Daydream(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DAYDREAM, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
    }

    //#region Overrides
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = actor });
    //}
    //public override bool PerformActualAction() {
    //    if (base.PerformActualAction()) {
    //        if (_targetTile.tileState == LocationGridTile.Tile_State.Occupied) {
    //            SetState("Daydream Failed");
    //        } else {
    //            SetState("Daydream Success");
    //        }
    //        return true;
    //    }
    //    return false;
    //}
    //protected override int GetCost() {
    //    //**Cost**: randomize between 3-10
    //    return Random.Range(3, 11);
    //}
    //public override void DoAction(GoapPlan plan) {
    //    //**Movement**: Move Actor to a random unoccupied tile in current location Wilderness or Work Area.
    //    List<LocationStructure> choices = actor.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.WILDERNESS).Where(x => x.unoccupiedTiles.Count > 0).ToList();
    //    choices.AddRange(actor.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.WORK_AREA).Where(x => x.unoccupiedTiles.Count > 0));
    //    _targetStructure = choices[Random.Range(0, choices.Count)];
    //    _targetTile = _targetStructure.GetRandomUnoccupiedTile();
    //    base.DoAction(plan);
    //}
    //#endregion

    //#region Effects
    //private void PerDayDreamSuccess() {
    //    actor.AdjustHappiness(3);
    //}
    //#endregion
}