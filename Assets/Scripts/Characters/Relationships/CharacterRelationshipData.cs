using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRelationshipData {

    public Character targetCharacter { get; private set; }

    public List<RelationshipTrait> rels { get; private set; }
    public int lastEncounter { get; private set; } //counts up
    public float encounterMultiplier { get; private set; }
    public bool isCharacterMissing { get; private set; }
    public bool isCharacterLocated { get; private set; }
    public LocationStructure knownStructure { get; private set; }
    public Trait trouble { get; private set; } //Set this to trait for now, but this could change in future iterations

    public CharacterRelationshipData(Character targetCharacter) {
        this.targetCharacter = targetCharacter;
        rels = new List<RelationshipTrait>();
        lastEncounter = 0;
        encounterMultiplier = 0f;
        isCharacterMissing = false;
        isCharacterLocated = true;
        knownStructure = targetCharacter.homeStructure;
        trouble = null;
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
}
