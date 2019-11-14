using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Zapped : Trait {

        public Zapped() {
            name = "Zapped";
            description = "This character cannot move.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            daysDuration = 3;
            hindersMovement = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
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
                } else if (character.stateComponent.stateToDo != null) {
                    character.stateComponent.SetStateToDo(null);
                }
                character.CancelAllJobsAndPlans("Stopped by the player");
                //else if(character.currentAction != null) {
                //    character.currentAction.StopAction();
                //} else if (character.currentParty.icon.isTravelling) {
                //    if (character.currentParty.icon.travelLine == null) {
                //        character.marker.StopMovement();
                //    } else {
                //        character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
                //    }
                //}
                character.marker.ClearHostilesInRange(false);
                character.marker.ClearAvoidInRange(false);
                character.AdjustDoNotDisturb(1);
            }
            base.OnAddTrait(sourcePOI);
        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            if (sourcePOI is Character) {
                Character character = sourcePOI as Character;
                character.AdjustDoNotDisturb(-1);
                character.marker.ClearHostilesInRange(false);
                character.marker.ClearAvoidInRange(false);
            }
            base.OnRemoveTrait(sourcePOI, removedBy);
        }
        #endregion
    }
}
