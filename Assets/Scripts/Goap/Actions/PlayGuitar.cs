using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayGuitar : GoapAction {
    protected override string failActionState { get { return "Play Fail"; } }

    public PlayGuitar(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.PLAY_GUITAR, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
        };
        actionIconString = GoapActionStateDB.Entertain_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Play Success");
        } else {
            if (poiTarget.state == POI_STATE.INACTIVE) {
                SetState("Play Fail");
            } else {
                SetState("Target Missing");
            }
        }
    }
    protected override int GetCost() {
        //Cost:
        //- Actor is resident of the Guitar's Dwelling: 4-10
        //- Actor is not a resident but has a positive relationship with the Guitar's Dwelling resident: 7-12
        LocationGridTile knownLoc = actor.GetAwareness(poiTarget).knownGridLocation;
        if (actor.homeStructure == knownLoc.structure) {
            return Utilities.rng.Next(15, 30);
        } else {
            if (knownLoc.structure is Dwelling) {
                Dwelling dwelling = knownLoc.structure as Dwelling;
                if (dwelling.residents.Count > 0) {
                    for (int i = 0; i < dwelling.residents.Count; i++) {
                        Character currResident = dwelling.residents[i];
                        if (currResident.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE)) {
                            return Utilities.rng.Next(25, 40);
                        }
                    }
                    //the actor does NOT have any positive relations with any resident
                    return 99999; //NOTE: Should never reach here since Requirement prevents this.
                } else {
                    //in cases that the guitar is at a dwelling with no residents, always allow.
                    return Utilities.rng.Next(25, 35);
                }
            } else {
                return Utilities.rng.Next(25, 35);
            }
            
        }
        //return Utilities.rng.Next(3, 10);
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Play Fail");
    //}
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Play Success") {
            actor.AdjustDoNotGetLonely(-1);
            poiTarget.SetPOIState(POI_STATE.ACTIVE);
        }
    }
    #endregion

    #region State Effects
    public void PrePlaySuccess() {
        actor.AdjustDoNotGetLonely(1);
        poiTarget.SetPOIState(POI_STATE.INACTIVE);
    }
    public void PerTickPlaySuccess() {
        actor.AdjustHappiness(12);
    }
    public void AfterPlaySuccess() {
        actor.AdjustDoNotGetLonely(-1);
        poiTarget.SetPOIState(POI_STATE.ACTIVE);
    }
    public void PreTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    #region Requirement
    private bool Requirement() {
        if(poiTarget.state == POI_STATE.INACTIVE) {
            return false;
        }
        IAwareness awareness = actor.GetAwareness(poiTarget);
        if (awareness == null) {
            return false;
        }
        LocationGridTile knownLoc = awareness.knownGridLocation;
        //**Advertised To**: Residents of the dwelling or characters with a positive relationship with a Resident
        if (knownLoc.structure is Dwelling) {
            if (actor.homeStructure == knownLoc.structure) {
                return true;
            } else {
                Dwelling dwelling = knownLoc.structure as Dwelling;
                if (dwelling.residents.Count > 0) {
                    for (int i = 0; i < dwelling.residents.Count; i++) {
                        Character currResident = dwelling.residents[i];
                        if (currResident.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE)) {
                            return true;
                        }
                    }
                    //the actor does NOT have any positive relations with any resident
                    return false;
                } else {
                    //in cases that the guitar is at a dwelling with no residents, always allow.
                    return true;
                }
            }
        } else {
            //in cases that the guitar is not inside a dwelling, always allow.
            return true;
        }
    }
    #endregion
}
