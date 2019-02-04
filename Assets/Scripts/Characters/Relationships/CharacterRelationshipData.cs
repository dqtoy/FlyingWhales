using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRelationshipData {

    public Character owner { get; private set; }
    public Character targetCharacter { get; private set; }

    public List<RelationshipTrait> rels { get; private set; }
    public int lastEncounter { get; private set; } //counts up
    public float encounterMultiplier { get; private set; }
    public bool isCharacterMissing { get; private set; }
    public bool isCharacterLocated { get; private set; }
    public LocationStructure knownStructure { get; private set; }
    public Trait trouble { get; private set; } //Set this to trait for now, but this could change in future iterations

    public CharacterRelationshipData(Character owner, Character targetCharacter) {
        this.owner = owner;
        this.targetCharacter = targetCharacter;
        rels = new List<RelationshipTrait>();
        lastEncounter = 0;
        encounterMultiplier = 0f;
        isCharacterMissing = false;
        isCharacterLocated = true;
        SetKnownStructure(targetCharacter.homeStructure);
        trouble = null;
        //AddListeners();
    }

    public void AddListeners() {
        Messenger.AddListener(Signals.DAY_STARTED, LastEncounterTick);
        Messenger.AddListener<Interaction>(Signals.INTERACTION_INITIALIZED, OnInteractionInitialized);
        Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }
    public void RemoveListeners() {
        Messenger.RemoveListener(Signals.DAY_STARTED, LastEncounterTick);
        Messenger.RemoveListener<Interaction>(Signals.INTERACTION_INITIALIZED, OnInteractionInitialized);
        Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }

    private void OnInteractionInitialized(Interaction interaction) {
        if (interaction.characterInvolved.id == owner.id 
            && interaction.targetCharacter != null 
            && interaction.targetCharacter.id == targetCharacter.id) {
            ResetLastEncounter();
        }
    }
    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        if (owner.id == character.id && structure == knownStructure) {
            if (owner.forcedInteraction != null 
                && owner.forcedInteraction.targetCharacter != null 
                && owner.forcedInteraction.targetCharacter.id == targetCharacter.id) {
                if (targetCharacter.currentStructure != structure) {
                    SetIsCharacterLocated(false);
                    /*
                    - character located turns to False whenever the character visits the specific **known character location structure** 
                    with the intent to meet up with the target and not encounter the target there    
                     */
                }
            }
        }
    }

    #region Relationships
    public void AddRelationship(RelationshipTrait newRel) {
        if (!rels.Contains(newRel)) {
            rels.Add(newRel);
        }
    }
    public void RemoveRelationship(RelationshipTrait newRel) {
        rels.Remove(newRel);
    }
    public bool HasRelationshipTrait(RELATIONSHIP_TRAIT relType) {
        for (int i = 0; i < rels.Count; i++) {
            if(rels[i].relType == relType) {
                return true;
            }
        }
        return false;
    }
    public RelationshipTrait GetRelationshipTrait(RELATIONSHIP_TRAIT relType) {
        for (int i = 0; i < rels.Count; i++) {
            if (rels[i].relType == relType) {
                return rels[i];
            }
        }
        return null;
    }
    public int GetTotalRelationshipWeight() {
        int weight = 0;
        for (int i = 0; i < rels.Count; i++) {
            weight += GetRelationshipWeight(rels[i].relType);
        }
        return weight;
    }
    public int GetRelationshipWeight(RELATIONSHIP_TRAIT relType) {
        if(relType == RELATIONSHIP_TRAIT.FRIEND) {
            return 15;
        } else if (relType == RELATIONSHIP_TRAIT.RELATIVE) {
            return 10;
        } else if (relType == RELATIONSHIP_TRAIT.LOVER) {
            return 15;
        } else if (relType == RELATIONSHIP_TRAIT.PARAMOUR) {
            return 20;
        } else if (relType == RELATIONSHIP_TRAIT.MASTER) {
            return 5;
        } else if (relType == RELATIONSHIP_TRAIT.ENEMY) {
            return 10;
        } else if (relType == RELATIONSHIP_TRAIT.SAVE_TARGET) {
            return 15;
        }
        return 0;
    }
    #endregion

    #region Encounter Multiplier
    public void AdjustEncounterMultiplier(float adjustment) {
        encounterMultiplier += adjustment;
    }
    public void ResetEncounterMultiplier() {
        encounterMultiplier = 0;
    }
    #endregion

    #region Last Encounter
    private void LastEncounterTick() {
        /*
         - countdown from last encounter
          - ticks every day until the characters share the same non-Wilderness or non-Dungeon structure
          - in Wilderness or Dungeon, will reset this if either this character or target becomes part of the same event (either being the actor, target or reacting to it)
         */
        if (owner.currentStructure.structureType != STRUCTURE_TYPE.DUNGEON 
            && owner.currentStructure.structureType != STRUCTURE_TYPE.WILDERNESS) {
            if (owner.currentStructure == targetCharacter.currentStructure) {
                ResetLastEncounter();
            } else {
                lastEncounter++;
            }
        } else {
            //character is at a dungeon or wilderness structure
            lastEncounter++;
        }
        CheckForCharacterMissing();
    }
    public void ResetLastEncounter() {
        lastEncounter = 0;
        ResetCharacterMissing();
    }
    #endregion

    #region Character Missing
    private void CheckForCharacterMissing() {
        /*
         - character missing (this character has not seen the target in quite a while)
          - this turns to True when **countdown from last encounter** is 150 for relationships from the same faction and 300 for those outside
          - this turns back to False when **countdown from last encounter** has been reset
         */
        if (lastEncounter >= 150 && targetCharacter.faction.id == owner.faction.id) {
            SetIsCharacterMissing(true);
        } else if (lastEncounter >= 300 && targetCharacter.faction.id != owner.faction.id) {
            SetIsCharacterMissing(true);
        }
    }
    private void ResetCharacterMissing() {
        SetIsCharacterMissing(false);
    }
    private void SetIsCharacterMissing(bool state) {
        if (isCharacterMissing == state) {
            return; //ignore change
        }
        isCharacterMissing = state;
        if (!isCharacterMissing) {
            SetKnownStructure(targetCharacter.homeStructure);
        }
    }
    #endregion

    #region Character Located
    public void SetIsCharacterLocated(bool state) {
        isCharacterLocated = state;
    }
    #endregion

    #region Known Structure
    public void SetKnownStructure(LocationStructure structure) {
        knownStructure = structure;
    }
    #endregion
}
