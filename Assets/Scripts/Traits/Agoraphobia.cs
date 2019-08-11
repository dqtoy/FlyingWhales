using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agoraphobia : Trait {

    public Agoraphobia() {
        name = "Agoraphobia";
        description = "This is afraid of crowds.";
        type = TRAIT_TYPE.SPECIAL;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 50;
        //effects = new List<TraitEffect>();
    }

    #region Overrides
    protected override void OnChangeLevel() {
        base.OnChangeLevel();
        if(level == 1) {
            daysDuration = 50;
        } else if (level == 2) {
            daysDuration = 70;
        } else if (level == 3) {
            daysDuration = 90;
        }
    }
    public override void OnAddTrait(ITraitable addedTo) {
        base.OnAddTrait(addedTo);
        if(addedTo is Character) {
            Character character = addedTo as Character;
            if(character.marker.inVisionCharacters.Count >= 3) {
                ApplyAgoraphobiaEffect(character, true);
            }
        }
    }
    public override void OnSeePOI(IPointOfInterest targetPOI, Character character) {
        base.OnSeePOI(targetPOI, character);
        if (targetPOI is Character) {
            Character targetCharacter = targetPOI as Character;
            if(character.GetNormalTrait("Berserked") != null) {
                return;
            }
            if(character.stateComponent.currentState == null || character.stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT) {
                if (character.marker.inVisionCharacters.Count >= 3) {
                    ApplyAgoraphobiaEffect(character, true);
                }
            } else if (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                CombatState combatState = character.stateComponent.currentState as CombatState;
                if (combatState.isAttacking) {
                    if (character.marker.inVisionCharacters.Count >= 3) {
                        ApplyAgoraphobiaEffect(character, false);
                        Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, character);
                    }
                }
            }
        } 
    }
    #endregion

    private void ApplyAgoraphobiaEffect(Character character, bool processCombat) {
        character.marker.AddAvoidsInRange(character.marker.inVisionCharacters, processCombat);
        character.AdjustHappiness(-50);
        character.AdjustTiredness(-150);
    }
}
