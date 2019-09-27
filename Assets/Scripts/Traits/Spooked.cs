﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spooked : Trait {
    public List<Character> terrifyingCharacters { get; private set; }

    public Spooked() {
        name = "Spooked";
        description = "This character is too scared and may refuse to sleep.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEUTRAL;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = GameManager.Instance.GetTicksBasedOnMinutes(30);
        //effects = new List<TraitEffect>();
        terrifyingCharacters = new List<Character>();
    }

    #region Overrides
    public override void OnAddTrait(ITraitable sourcePOI) {
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
                if (character.marker.inVisionCharacters.Count > 0) {
                    for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
                        AddTerrifyingCharacter(character.marker.inVisionCharacters[i]);
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
    public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
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
    public override void OnSeePOI(IPointOfInterest targetPOI, Character character) {
        base.OnSeePOI(targetPOI, character);
        if (targetPOI is Character) {
            Character targetCharacter = targetPOI as Character;
            if (character.marker.AddAvoidInRange(targetCharacter)) {
                AddTerrifyingCharacter(targetCharacter);
            }
        }
    }
    #endregion

    public void AddTerrifyingCharacter(Character character) {
        terrifyingCharacters.Add(character);
    }
    public void ClearTerrifyingCharacters() {
        terrifyingCharacters.Clear();
    }
}

public class SaveDataSpooked : SaveDataTrait {
    public List<int> terrifyingCharacterIDs;

    public override void Save(Trait trait) {
        base.Save(trait);
        Spooked derivedTrait = trait as Spooked;
        for (int i = 0; i < derivedTrait.terrifyingCharacters.Count; i++) {
            terrifyingCharacterIDs.Add(derivedTrait.terrifyingCharacters[i].id);
        }
    }

    public override Trait Load(ref Character responsibleCharacter) {
        Trait trait = base.Load(ref responsibleCharacter);
        Spooked derivedTrait = trait as Spooked;
        for (int i = 0; i < terrifyingCharacterIDs.Count; i++) {
            derivedTrait.AddTerrifyingCharacter(CharacterManager.Instance.GetCharacterByID(terrifyingCharacterIDs[i]));
        }
        return trait;
    }
}
