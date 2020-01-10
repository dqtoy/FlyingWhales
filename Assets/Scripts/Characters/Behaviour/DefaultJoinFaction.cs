using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class DefaultJoinFaction : CharacterBehaviourComponent {
    public DefaultJoinFaction() {
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.DO_NOT_SKIP_PROCESSING, BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY };
    }

    public override bool TryDoBehaviour(Character character, ref string log) {
        if(UnityEngine.Random.Range(0, 100) < 15) {
            if (character.isFriendlyFactionless) {
                log += "\n-" + character.name + " is factionless, 15% chance to join faction";
                List<Faction> viableFactions = new List<Faction>();
                if (character.currentRegion != null) {
                    Settlement potentialSettlement = character.currentSettlement;
                    log += "\n-" + character.name + " is factionless and in a settlement: " + potentialSettlement.name + ", will try to join a faction...";
                    Faction potentialFaction = potentialSettlement.owner;
                    if (!potentialFaction.isPlayerFaction
                        && !potentialSettlement.owner.IsCharacterBannedFromJoining(character) 
                        && potentialFaction.ideologyComponent.DoesCharacterFitCurrentIdeologies(character)) {
                        if (!viableFactions.Contains(potentialFaction)) {
                            viableFactions.Add(potentialFaction);
                        }
                    }
                } 
                // else if (character.currentRegion != null) {
                //     log += "\n-" + character.name + " is factionless and in a non settlement region: " + character.currentRegion.name + ", will try to join a faction...";
                //     Region potentialRegion = character.currentRegion;
                //     //TODO:
                //     // if (potentialRegion.owner != null && !potentialRegion.owner.isPlayerFaction
                //     //     && !potentialRegion.owner.IsCharacterBannedFromJoining(character) && potentialRegion.owner.ideologyComponent.DoesCharacterFitCurrentIdeologies(character)) {
                //     //     if (viableFactions == null) { viableFactions = new List<Faction>(); }
                //     //     if (!viableFactions.Contains(potentialRegion.owner)) {
                //     //         viableFactions.Add(potentialRegion.owner);
                //     //     }
                //     // }
                //     //TODO:
                //     // for (int i = 0; i < character.currentRegion.connections.Count; i++) {
                //     //     potentialRegion = character.currentRegion.connections[i].region;
                //     //     if (potentialRegion.owner != null && !potentialRegion.owner.isPlayerFaction
                //     //         && !potentialRegion.owner.IsCharacterBannedFromJoining(character) && potentialRegion.owner.ideologyComponent.DoesCharacterFitCurrentIdeologies(character)) {
                //     //         if (viableFactions == null) { viableFactions = new List<Faction>(); }
                //     //         if (!viableFactions.Contains(potentialRegion.owner)) {
                //     //             viableFactions.Add(potentialRegion.owner);
                //     //         }
                //     //     }
                //     // }
                // }
                if (viableFactions.Count > 0) {
                    Faction chosenFaction = viableFactions[UnityEngine.Random.Range(0, viableFactions.Count)];
                    character.ChangeFactionTo(chosenFaction);
                    //chosenFaction.JoinFaction(character);
                    log += "\n-Chosen faction to join: " + chosenFaction.name;
                } else {
                    log += "\n-No available faction that the character fits the ideology";
                }
            }
            return true;
        } else if (UnityEngine.Random.Range(0, 100) < 10) {
            if (character.isFriendlyFactionless) {
                log += "\n-" + character.name + " is factionless, 10% chance to create faction";
                if (character.traitContainer.GetNormalTrait<Trait>("Inspiring", "Ambitious") != null) {
                    log += "\n-" + character.name + " is Ambitious or Inspiring, creating new faction...";
                    Faction newFaction = FactionManager.Instance.CreateNewFaction();
                    character.ChangeFactionTo(newFaction);
                    newFaction.SetLeader(character);

                    Log createFactionLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "character_create_faction");
                    createFactionLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    createFactionLog.AddToFillers(newFaction, newFaction.name, LOG_IDENTIFIER.FACTION_1);
                    createFactionLog.AddToFillers(character.currentRegion, character.currentRegion.name, LOG_IDENTIFIER.LANDMARK_1);
                    character.RegisterLogAndShowNotifToThisCharacterOnly(createFactionLog, onlyClickedCharacter: false);
                }
            }
            return true;
        }

        return false;
    }
}
