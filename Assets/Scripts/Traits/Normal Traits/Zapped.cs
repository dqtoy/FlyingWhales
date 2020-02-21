using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Zapped : Trait {

        private GameObject electricEffectGO;
        public Zapped() {
            name = "Zapped";
            description = "This character cannot move.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnMinutes(15);
            hindersMovement = true;
            hindersWitness = true;
            hindersPerform = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            if(sourcePOI is IPointOfInterest) {
                electricEffectGO = GameManager.Instance.CreateElectricEffectAt(sourcePOI as IPointOfInterest);
            }
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
                    character.stateComponent.ExitCurrentState();
                }
                character.combatComponent.ClearHostilesInRange(false);
                character.combatComponent.ClearAvoidInRange(false);
                //character.AdjustCanPerform(1);
            }
            base.OnAddTrait(sourcePOI);
        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            if (electricEffectGO != null) {
                ObjectPoolManager.Instance.DestroyObject(electricEffectGO);
            }
            if (sourcePOI is Character) {
                Character character = sourcePOI as Character;
                //character.AdjustCanPerform(-1);
                if(character.marker != null) {
                    character.combatComponent.ClearHostilesInRange(false);
                    character.combatComponent.ClearAvoidInRange(false);
                }
            }
            base.OnRemoveTrait(sourcePOI, removedBy);
        }
        #endregion
    }
}
