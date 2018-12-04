using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Relationship {

    private Character _sourceCharacter;
    private Character _targetCharacter;
    //private Character _character2;

    //private int _value;
    //private int _baseValue; //This is the value affected by traits, race, etc.

    private List<CHARACTER_RELATIONSHIP> _relationshipStatuses;

    #region getters/setters
    //public int totalValue {
    //    get { return _value + _baseValue; }
    //}
    public List<CHARACTER_RELATIONSHIP> relationshipStatuses {
        get { return _relationshipStatuses; }
    }
    public Character targetCharacter {
		get { return _targetCharacter; }
	}
    public Character sourceCharacter {
        get { return _sourceCharacter; }
    }
    #endregion

    public Relationship(Character sourceCharacter, Character targetCharacter) : this(){
        _sourceCharacter = sourceCharacter;
        _targetCharacter = targetCharacter;
        
        Messenger.AddListener<Character, GENDER>(Signals.GENDER_CHANGED, OnCharacterChangedGender);
    }
    public Relationship() {
        _relationshipStatuses = new List<CHARACTER_RELATIONSHIP>();
    }

    public Relationship CreateCopy() {
        Relationship copy = new Relationship();
        copy._sourceCharacter = sourceCharacter;
        copy._targetCharacter = targetCharacter;
        copy._relationshipStatuses = new List<CHARACTER_RELATIONSHIP>(relationshipStatuses);
        return copy;
    }

    public void OnCharacterChangedGender(Character characterWithNewGender, GENDER newGender) {
        if (characterWithNewGender.id == _sourceCharacter.id || characterWithNewGender.id == _targetCharacter.id) {
            RevalidateRelationshipStatuses();
        }
    }

    private void RevalidateRelationshipStatuses() {
        List<CHARACTER_RELATIONSHIP> possibleStats = Utilities.GetPossibleRelationshipsBasedOnGender(_sourceCharacter.gender, _targetCharacter.gender);
        List<CHARACTER_RELATIONSHIP> invalidStats = new List<CHARACTER_RELATIONSHIP>();
        for (int i = 0; i < _relationshipStatuses.Count; i++) {
            CHARACTER_RELATIONSHIP currStat = _relationshipStatuses[i];
            if (!possibleStats.Contains(currStat)) {
                invalidStats.Add(currStat);
            }
        }
        for (int i = 0; i < invalidStats.Count; i++) {
            CHARACTER_RELATIONSHIP currStat = invalidStats[i];
            _relationshipStatuses.Remove(currStat);
        }
    }
    
    public void AddRelationshipStatus(CHARACTER_RELATIONSHIP relStat) {
        if (!_relationshipStatuses.Contains(relStat)) {
            _relationshipStatuses.Add(relStat);
#if !WORLD_CREATION_TOOL
            if(relStat == CHARACTER_RELATIONSHIP.STALKER) {
                Stalker stalker = targetCharacter.AddAttribute(ATTRIBUTE.STALKER) as Stalker;
                if(stalker != null) {
                    stalker.SetStalkee(sourceCharacter);
                }
            }
#endif
        }
    }
    public void AddRelationshipStatus(List<CHARACTER_RELATIONSHIP> relStat) {
        for (int i = 0; i < relStat.Count; i++) {
            AddRelationshipStatus(relStat[i]);
        }
    }
    public void RemoveRelationshipStatus(CHARACTER_RELATIONSHIP relStat) {
        _relationshipStatuses.Remove(relStat);
        if (relStat == CHARACTER_RELATIONSHIP.STALKER) {
            targetCharacter.RemoveAttribute(ATTRIBUTE.STALKER);
        }
    }
    //public bool HasCategory(CHARACTER_RELATIONSHIP_CATEGORY category){
    //	for (int i = 0; i < _relationshipStatus.Count; i++) {
    //		if(Utilities.charRelationshipCategory[_relationshipStatus[i].character1Relationship] == category){
    //			return true;
    //		}
    //	}
    //	return false;
    //}

    public bool HasStatus(CHARACTER_RELATIONSHIP status) {
        return _relationshipStatuses.Contains(status);
        //for (int i = 0; i < _relationshipStatus.Count; i++) {
        //    if (_relationshipStatus[i].character1Relationship == status || _relationshipStatus[i].character2Relationship == status) {
        //        return true;
        //    }
        //}
        //return false;
    }
    public bool HasAnyStatus(CHARACTER_RELATIONSHIP[] status) {
        for (int i = 0; i < status.Length; i++) {
            CHARACTER_RELATIONSHIP rel = status[i];
            if (_relationshipStatuses.Contains(rel)) {
                return true;
            }
        }
        return false;
        
    }
    //public CharacterRelationship GetExactRelationship(CHARACTER_RELATIONSHIP char1Relationship, CHARACTER_RELATIONSHIP char2Relationship){
    //	for (int i = 0; i < _relationshipStatus.Count; i++) {
    //		if(_relationshipStatus[i].character1Relationship == char1Relationship && _relationshipStatus[i].character2Relationship == char2Relationship){
    //			return _relationshipStatus[i];
    //		}
    //	}
    //	return null;
    //}

    public bool IsNegative() {
        if (HasStatus(CHARACTER_RELATIONSHIP.ENEMY) || HasStatus(CHARACTER_RELATIONSHIP.RIVAL)) {
            return true;
        }
        return false;
    }
}
