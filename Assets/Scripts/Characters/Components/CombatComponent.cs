using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class CombatComponent {
	public Character owner { get; private set; }
    public bool willProcessCombat { get; private set; }

	public CombatComponent(Character owner) {
		this.owner = owner;
	}

    #region Fight or Flight
    public void FightOrFlight(IPointOfInterest target, bool isLethal = true) {
        string debugLog = "FIGHT or FLIGHT response of " + owner.name + " against " + target.nameWithID;
        if(target is Character) {
            debugLog += "\n-Target is character";
            Character targetCharacter = target as Character;
            if (owner.traitContainer.GetNormalTrait<Trait>("Coward") != null) {
                debugLog += "\n-Character is coward";
                debugLog += "\n-FLIGHT";
                owner.logComponent.PrintLogIfActive(debugLog);
                Flight(target);
            } else {
                debugLog += "\n-Character is not coward";
                if (owner.traitContainer.GetNormalTrait<Trait>("Combatant") == null) {
                    debugLog += "\n-Character is not combatant, 20% to Fight";
                    int chance = UnityEngine.Random.Range(0, 100);
                    debugLog += "\n-Roll: " + chance;
                    if (chance < 20) {
                        debugLog += "\n-FIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
                        Fight(target, isLethal);
                    } else {
                        debugLog += "\n-FLIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
                        Flight(target);
                    }
                } else {
                    debugLog += "\n-Character is combatant";
                    if (owner.currentHP > targetCharacter.currentHP) {
                        debugLog += "\n-Character hp is higher than target";
                        debugLog += "\n-FIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
                        Fight(target, isLethal);
                    } else {
                        debugLog += "\n-Character hp is lower or equal than target";
                        int fightChance = 25;
                        for (int i = 0; i < owner.marker.inVisionCharacters.Count; i++) {
                            if (owner.marker.inVisionCharacters[i].marker.hostilesInRange.Contains(target)) {
                                debugLog += "\n-Character has another character in vision who has the same target";
                                fightChance = 75;
                                break;
                            }
                        }
                        debugLog += "\n-Fight chance: " + fightChance;
                        int roll = UnityEngine.Random.Range(0, 100);
                        debugLog += "\n-Roll: " + roll;
                        if (roll < fightChance) {
                            debugLog += "\n-FIGHT";
                            owner.logComponent.PrintLogIfActive(debugLog);
                            Fight(target, isLethal);
                        } else {
                            debugLog += "\n-FLIGHT";
                            owner.logComponent.PrintLogIfActive(debugLog);
                            Flight(target);
                        }
                    }
                }
            }
        } else {
            debugLog += "\n-Target is object";
            if (owner.traitContainer.GetNormalTrait<Trait>("Coward") != null) {
                debugLog += "\n-Character is coward";
                debugLog += "\n-FLIGHT";
                owner.logComponent.PrintLogIfActive(debugLog);
                Flight(target);
            } else {
                debugLog += "\n-Character is not coward";
                debugLog += "\n-FIGHT";
                owner.logComponent.PrintLogIfActive(debugLog);
                Fight(target, isLethal);
            }
        }
    }
    public void Fight(IPointOfInterest target, bool isLethal = true) {
        string debugLog = "Triggered FIGHT response for " + owner.name + " against " + target.nameWithID;
        if (!owner.marker.hostilesInRange.Contains(target)) {
            owner.marker.hostilesInRange.Add(target);
            owner.marker.avoidInRange.Remove(target);
            owner.marker.SetWillProcessCombat(true);
            if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                owner.marker.lethalCharacters.Add(target as Character, isLethal);
            }
            debugLog += "\n" + target.name + " was added to " + owner.name + "'s hostile range!";
        }
        owner.logComponent.PrintLogIfActive(debugLog);
    }
    public void Flight(IPointOfInterest target) {
        string debugLog = "Triggered FLIGHT response for " + owner.name + " against " + target.nameWithID;
        if (owner.marker.hostilesInRange.Remove(target)) {
            if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                owner.marker.lethalCharacters.Remove(target as Character);
            }
        }
        if(owner.homeStructure != null && owner.homeStructure != owner.currentStructure) {
            debugLog += "\n" + owner.name + " has a home and not in his/her home";
            if(UnityEngine.Random.Range(0, 100) < 20) {
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, owner);
                debugLog += "\n" + owner.name + " triggered Cowering interrupt";
            }
            debugLog += "\n" + owner.name + " will flee to home";
            owner.jobComponent.TriggerFleeHome();
        } else {
            if (UnityEngine.Random.Range(0, 2) == 0) {
                debugLog += "\n" + owner.name + " triggered Cowering interrupt";
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, owner);
            }
            if (!owner.marker.avoidInRange.Contains(target)) {
                if (owner.marker.inVisionPOIs.Contains(target)) {
                    owner.marker.avoidInRange.Add(target);
                    owner.marker.SetWillProcessCombat(true);
                    debugLog += "\n" + target.name + " was added to " + owner.name + "'s avoid range!";
                }
            }
        }
        owner.logComponent.PrintLogIfActive(debugLog);
    }
    #endregion
}
