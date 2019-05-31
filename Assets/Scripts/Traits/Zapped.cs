using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zapped : Trait {

    public Zapped() {
        name = "Zapped";
        description = "This character cannot move.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEUTRAL;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = GameManager.Instance.GetTicksBasedOnMinutes(30);
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourcePOI) {
        if (sourcePOI is Character) {
            Character character = sourcePOI as Character;
            if (character.currentParty.icon.isTravelling) {
                if (character.currentParty.icon.travelLine == null) {
                    character.marker.StopMovement();
                } else {
                    character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
                }
            }
            if (character.stateComponent.currentState != null) {
                character.stateComponent.currentState.OnExitThisState();
                if (character.stateComponent.currentState != null) {
                    character.stateComponent.currentState.OnExitThisState();
                }
            }
            character.CancelAllJobsAndPlans();
            //else if(character.currentAction != null) {
            //    character.currentAction.StopAction();
            //} else if (character.currentParty.icon.isTravelling) {
            //    if (character.currentParty.icon.travelLine == null) {
            //        character.marker.StopMovement();
            //    } else {
            //        character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
            //    }
            //}
            character.AdjustDoNotDisturb(1);
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourcePOI) {
        if (sourcePOI is Character) {
            Character character = sourcePOI as Character;
            character.AdjustDoNotDisturb(-1);
        }
    }
    #endregion
}