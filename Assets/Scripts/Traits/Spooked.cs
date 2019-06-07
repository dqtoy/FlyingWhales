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
        effects = new List<TraitEffect>();
        terrifyingCharacters = new List<Character>();
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourcePOI) {
        if (sourcePOI is Character) {
            Character character = sourcePOI as Character;
            if (character.stateComponent.currentState != null) {
                character.stateComponent.currentState.OnExitThisState();
                if (character.stateComponent.currentState != null) {
                    character.stateComponent.currentState.OnExitThisState();
                }
            } else if (character.currentAction != null) {
                character.currentAction.StopAction();
            } else if (character.currentParty.icon.isTravelling) {
                if (character.currentParty.icon.travelLine == null) {
                    character.marker.StopMovement();
                } else {
                    character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
                }
            }
            character.AdjustDoNotDisturb(1);
        }
        base.OnAddTrait(sourcePOI);
    }
    public override void OnRemoveTrait(IPointOfInterest sourcePOI) {
        if (sourcePOI is Character) {
            Character character = sourcePOI as Character;
            character.AdjustDoNotDisturb(-1);
            for (int i = 0; i < terrifyingCharacters.Count; i++) {
                character.marker.RemoveAvoidInRange(terrifyingCharacters[i]);
                character.marker.RemoveTerrifyingCharacter(terrifyingCharacters[i]);
            }
            ClearTerrifyingCharacters();
        }
        base.OnRemoveTrait(sourcePOI);
    }
    #endregion

    public void AddTerrifyingCharacter(Character character) {
        terrifyingCharacters.Add(character);
    }
    public void ClearTerrifyingCharacters() {
        terrifyingCharacters.Clear();
    }
}
