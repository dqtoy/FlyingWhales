using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coward : Trait {

    public Coward() {
        name = "Coward";
        description = "Cowards always flee from combat.";
        type = TRAIT_TYPE.FLAW;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        canBeTriggered = true;
        //effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void TriggerFlaw(Character character) {
        base.TriggerFlaw(character);
        //If outside and the character lives in a house, the character will flee and go back home.
        if (character.homeStructure is Dwelling && character.currentStructure != character.homeStructure) {
            if (character.currentAction != null) {
                character.StopCurrentAction(false);
            }
            if (character.stateComponent.currentState != null) {
                character.stateComponent.currentState.OnExitThisState();
            } else if (character.stateComponent.stateToDo != null) {
                character.stateComponent.SetStateToDo(null, false, false);
            }

            LocationGridTile tile = character.homeStructure.tiles[Random.Range(0, character.homeStructure.tiles.Count)];
            character.marker.GoTo(tile);
        }
    }
    #endregion
}
