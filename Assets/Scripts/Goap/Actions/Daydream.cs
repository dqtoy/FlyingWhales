using System.Collections;
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

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (_targetTile.tileState == LocationGridTile.Tile_State.Occupied) {
            SetState("Daydream Failed");
        } else {
            SetState("Daydream Success");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        //**Cost**: randomize between 3-10
        return Utilities.rng.Next(3, 10);
    }
    public override void DoAction(GoapPlan plan) {
        //**Movement**: Move Actor to a random unoccupied tile in current location Wilderness or Work Area.
        List<LocationStructure> choices = actor.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.WILDERNESS).Where(x => x.unoccupiedTiles.Count > 0).ToList();
        choices.AddRange(actor.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.WORK_AREA).Where(x => x.unoccupiedTiles.Count > 0));
        _targetStructure = choices[Random.Range(0, choices.Count)];
        _targetTile = _targetStructure.GetRandomUnoccupiedTile();
        base.DoAction(plan);
    }
    public override bool IsHalted() {
        TIME_IN_WORDS timeInWords = GameManager.GetCurrentTimeInWordsOfTick();
        if (timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            return true;
        }
        return false;
    }
    #endregion

    #region Effects
    private void DreDaydreamSuccess() {
        actor.AdjustDoNotGetLonely(1);
        actor.AdjustDoNotGetTired(1);
    }
    private void PerTickDaydreamSuccess() {
        actor.AdjustHappiness(3);
    }
    private void AfterDaydreamSuccess() {
        actor.AdjustDoNotGetLonely(-1);
        actor.AdjustDoNotGetTired(-1);
    }
    #endregion
}
