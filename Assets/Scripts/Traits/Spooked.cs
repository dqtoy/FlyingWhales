using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spooked : Trait {
    public List<Character> terrifyingCharacters { get; private set; }

    public Spooked() {
        name = "Spooked";
        description = "This character fears anything.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEUTRAL;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = GameManager.Instance.GetTicksBasedOnMinutes(30);
        //effects = new List<TraitEffect>();
        terrifyingCharacters = new List<Character>();
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourcePOI) {
        if (sourcePOI is Character) {
            Character character = sourcePOI as Character;
            //if (character.stateComponent.currentState != null) {
            //    character.stateComponent.currentState.OnExitThisState();
            //    if (character.stateComponent.currentState != null) {
            //        character.stateComponent.currentState.OnExitThisState();
            //    }
            //} else if (character.stateComponent.stateToDo != null) {
            //    character.stateComponent.SetStateToDo(null);
            //} else 
            if (character.currentAction != null) {
                character.currentAction.StopAction();
            } else if (character.currentParty.icon.isTravelling) {
                if (character.currentParty.icon.travelLine == null) {
                    character.marker.StopMovement();
                } else {
                    character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
                }
            }
            //character.AdjustDoNotDisturb(1);

            if (!character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
                if (character.marker.inVisionPOIs.Count > 0) {
                    for (int i = 0; i < character.marker.inVisionPOIs.Count; i++) {
                        if (character.marker.inVisionPOIs[i] is Character) {
                            Character characterInVision = character.marker.inVisionPOIs[i] as Character;
                            AddTerrifyingCharacter(characterInVision);
                            //AddHostileInRange(characterInVision, CHARACTER_STATE.COMBAT, false);
                        }
                    }
                    if (terrifyingCharacters.Count > 0) {
                        if ((character.GetNormalTrait("Berserked") != null)
                            || (character.stateComponent.stateToDo != null && character.stateComponent.stateToDo.characterState == CHARACTER_STATE.BERSERKED && !character.stateComponent.stateToDo.isDone)) {
                            //If berserked
                        } else {
                            character.marker.AddAvoidsInRange(terrifyingCharacters, false);
                            Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, character);
                        }
                    }
                }
            }
        }
        base.OnAddTrait(sourcePOI);
    }
    public override void OnRemoveTrait(IPointOfInterest sourcePOI, Character removedBy) {
        if (sourcePOI is Character) {
            Character character = sourcePOI as Character;
            //character.AdjustDoNotDisturb(-1);
            for (int i = 0; i < terrifyingCharacters.Count; i++) {
                character.marker.RemoveAvoidInRange(terrifyingCharacters[i]);
                character.marker.RemoveTerrifyingObject(terrifyingCharacters[i]);
            }
            ClearTerrifyingCharacters();
        }
        base.OnRemoveTrait(sourcePOI, removedBy);
    }
    #endregion

    public void AddTerrifyingCharacter(Character character) {
        terrifyingCharacters.Add(character);
    }
    public void ClearTerrifyingCharacters() {
        terrifyingCharacters.Clear();
    }
}
