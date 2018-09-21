using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Relationship {

    private ECS.Character _sourceCharacter;
    private ECS.Character _targetCharacter;
    //private ECS.Character _character2;

    //private int _value;
    //private int _baseValue; //This is the value affected by traits, race, etc.

    private List<CHARACTER_RELATIONSHIP> _relationshipStatuses;
    //private List<CharacterRelationship> _relationshipStatus;

    #region getters/setters
    //public int totalValue {
    //    get { return _value + _baseValue; }
    //}
    public List<CHARACTER_RELATIONSHIP> relationshipStatuses {
        get { return _relationshipStatuses; }
    }
    public ECS.Character targetCharacter {
		get { return _targetCharacter; }
	}
    public ECS.Character sourceCharacter {
        get { return _sourceCharacter; }
    }
    #endregion

    public Relationship(ECS.Character sourceCharacter, ECS.Character targetCharacter) {
        _sourceCharacter = sourceCharacter;
        _targetCharacter = targetCharacter;
        //_character2 = character2;
        _relationshipStatuses = new List<CHARACTER_RELATIONSHIP>();
        Messenger.AddListener<ECS.Character, GENDER>(Signals.GENDER_CHANGED, OnCharacterChangedGender);
        //UpdateBaseValue();
    }

    //  private void UpdateBaseValue() {
    //      _baseValue = 0;
    //      if(_character1.id == _character2.id) {
    //          _baseValue += 10; //Same Tribe: +10
    //      }

    //if(_character1.faction != null && _character2.faction != null){
    //	FactionRelationship rel = FactionManager.Instance.GetRelationshipBetween(_character1.faction, _character2.faction);
    //	if (rel != null && rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
    //		_baseValue -= 25;//Opposing Tribes: -25
    //	}
    //}


    //      if (_character1.raceSetting.race == _character2.raceSetting.race) {
    //          _baseValue += 5; //Same Race: +5
    //      } else {
    //          if (_character1.HasTrait(TRAIT.RACIST) || 
    //              _character2.HasTrait(TRAIT.RACIST)) {
    //              if(_character1.raceSetting.race != _character2.raceSetting.race) {
    //                  _baseValue -= 25; //Different Race: -25 (if racist)
    //              }
    //          }
    //      }

    //      if (_character1.HasTrait(TRAIT.RUTHLESS)) {
    //          if (_character2.HasTrait(TRAIT.BENEVOLENT)) {
    //              _baseValue -= 20; //Ruthless (-20 Opinion vs Benevolent)
    //          }
    //      } else if (_character2.HasTrait(TRAIT.RUTHLESS)) {
    //          if (_character1.HasTrait(TRAIT.BENEVOLENT)) {
    //              _baseValue -= 20; //Ruthless (-20 Opinion vs Benevolent)
    //          }
    //      }

    //      if (_character1.HasTrait(TRAIT.CHARISMATIC) || 
    //          _character2.HasTrait(TRAIT.CHARISMATIC)) {
    //          _baseValue += 25; //Charismatic: +15
    //      }

    //      if (_character1.HasTrait(TRAIT.REPULSIVE) ||
    //          _character2.HasTrait(TRAIT.REPULSIVE)) {
    //          _baseValue -= 15; //Repulsive: -15
    //      }

    //      if (_character1.HasTrait(TRAIT.SMART)) {
    //          if (_character2.HasTrait(TRAIT.SMART)) {
    //              _baseValue += 10; //Both Smart: +10
    //          } else if (_character2.HasTrait(TRAIT.DUMB)) {
    //              _baseValue -= 10; //Smart vs Dumb: -10
    //          }
    //      } else if (_character2.HasTrait(TRAIT.SMART)) {
    //          if (_character1.HasTrait(TRAIT.SMART)) {
    //              _baseValue += 10; //Both Smart: +10
    //          } else if (_character1.HasTrait(TRAIT.DUMB)) {
    //              _baseValue -= 10; //Smart vs Dumb: -10
    //          }
    //      }

    //      if (_character1.HasTrait(TRAIT.EFFICIENT)) {
    //          if (_character2.HasTrait(TRAIT.EFFICIENT)) {
    //              _baseValue += 10; //Both Efficient: +10
    //          } else if (_character2.HasTrait(TRAIT.INEPT)) {
    //              _baseValue -= 10; //Efficient vs Inept: -10
    //          }
    //      } else if (_character2.HasTrait(TRAIT.EFFICIENT)) {
    //          if (_character1.HasTrait(TRAIT.EFFICIENT)) {
    //              _baseValue += 10; //Both Efficient: +10
    //          } else if (_character1.HasTrait(TRAIT.INEPT)) {
    //              _baseValue -= 10; //Efficient vs Inept: -10
    //          }
    //      }

    //      if (_character1.HasTrait(TRAIT.PACIFIST) && 
    //          _character2.HasTrait(TRAIT.PACIFIST)) {
    //          _baseValue += 10; //Both Pacifist: +10
    //      } else {
    //          if (_character1.HasTrait(TRAIT.PACIFIST) || 
    //              _character2.HasTrait(TRAIT.PACIFIST)) {
    //              _baseValue += 10;//Pacifist (this character): +10
    //          }
    //          if (_character1.HasTrait(TRAIT.HOSTILE) ||
    //              _character2.HasTrait(TRAIT.HOSTILE)) {
    //              _baseValue -= 10;//Hostile (this character): -10
    //          }
    //      }

    //      if (_character1.HasTrait(TRAIT.DIPLOMATIC)) {
    //          if (_character2.HasTrait(TRAIT.HOSTILE)) {
    //              _baseValue -= 20;//Diplomatic (-20 Opinion vs Hostile) (exclusive from Hostile and Opportunist)
    //          }
    //      } else if (_character2.HasTrait(TRAIT.DIPLOMATIC)) {
    //          if (_character1.HasTrait(TRAIT.HOSTILE)) {
    //              _baseValue -= 20;//Diplomatic (-20 Opinion vs Hostile) (exclusive from Hostile and Opportunist)
    //          }
    //      }

    //      if (_character1.HasTrait(TRAIT.HONEST)) {
    //          if (_character2.HasTrait(TRAIT.SCHEMING) || _character2.HasTrait(TRAIT.DECEITFUL)) {
    //              _baseValue -= 20; //Honest (-20 Opinion vs Deceitful and Scheming) (exclusive from Deceitful and Scheming)
    //          }
    //      } else if (_character2.HasTrait(TRAIT.HONEST)) {
    //          if (_character1.HasTrait(TRAIT.SCHEMING) || _character1.HasTrait(TRAIT.DECEITFUL)) {
    //              _baseValue -= 20; //Honest (-20 Opinion vs Deceitful and Scheming) (exclusive from Deceitful and Scheming)
    //          }
    //      }

    //      _baseValue = Mathf.Max(0, _baseValue);
    //  }

    //public void AdjustValue(int adjustment) {
    //    _value += adjustment;
    //    _value = Mathf.Max(0, _value);
    //}

    public void OnCharacterChangedGender(ECS.Character characterWithNewGender, GENDER newGender) {
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
