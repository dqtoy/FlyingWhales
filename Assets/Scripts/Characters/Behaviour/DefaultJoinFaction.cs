using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultJoinFaction : CharacterBehaviourComponent {
    public DefaultJoinFaction() {
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.DO_NOT_SKIP_PROCESSING, BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY };
    }

    public override bool TryDoBehaviour(Character character, ref string log) {
        if(UnityEngine.Random.Range(0, 100) < 15) {
            if (character.isFriendlyFactionless) {
                log += "\n-" + character.name + " is factionless, 15% chance to join faction";
                List<Faction> viableFactions = null;
                if(character.currentRegion != null) {
                    log += "\n-" + character.name + " is factionless and in a non settlement region: " + character.currentRegion.name + ", will try to join a faction...";
                    Region potentialRegion = character.currentRegion;
                    if(potentialRegion.owner != null && potentialRegion.owner != PlayerManager.Instance.player.playerFaction && potentialRegion.owner.ideologyComponent.DoesCharacterFitCurrentIdeology(character)) {
                        if(viableFactions == null) { viableFactions = new List<Faction>(); }
                        if (!viableFactions.Contains(potentialRegion.owner)) {
                            viableFactions.Add(potentialRegion.owner);
                        }
                    }
                    for (int i = 0; i < character.currentRegion.connections.Count; i++) {
                        potentialRegion = character.currentRegion.connections[i];
                        if (potentialRegion.owner != null && potentialRegion.owner != PlayerManager.Instance.player.playerFaction && potentialRegion.owner.ideologyComponent.DoesCharacterFitCurrentIdeology(character)) {
                            if (viableFactions == null) { viableFactions = new List<Faction>(); }
                            if (!viableFactions.Contains(potentialRegion.owner)) {
                                viableFactions.Add(potentialRegion.owner);
                            }
                        }
                    }
                } else if (character.specificLocation.areaMap != null) {
                    Region potentialRegion = character.specificLocation.region;
                    log += "\n-" + character.name + " is factionless and in a settlement region: " + potentialRegion.name + ", will try to join a faction...";
                    for (int i = 0; i < potentialRegion.factionsHere.Count; i++) {
                        Faction potentialFaction = potentialRegion.factionsHere[i];
                        if (potentialFaction != PlayerManager.Instance.player.playerFaction && potentialFaction.ideologyComponent.DoesCharacterFitCurrentIdeology(character)) {
                            if (viableFactions == null) { viableFactions = new List<Faction>(); }
                            if (!viableFactions.Contains(potentialFaction)) {
                                viableFactions.Add(potentialFaction);
                            }
                        }
                    }
                }
                if(viableFactions != null && viableFactions.Count > 0) {
                    Faction chosenFaction = viableFactions[UnityEngine.Random.Range(0, viableFactions.Count)];
                    chosenFaction.JoinFaction(character);
                    log += "\n-Chosen faction to join: " + chosenFaction.name;
                } else {
                    log += "\n-No available faction that the character fits the ideology";
                }

                character.currentRegion.owner.JoinFaction(character);
            }
            return true;
        }

        return false;
    }
}
