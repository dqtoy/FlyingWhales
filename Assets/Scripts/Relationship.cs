using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Relationship {

    private ECS.Character _character1;
    private ECS.Character _character2;

    private int _value;
    private int _baseValue; //This is the value affected by traits, race, etc.

	private List<CharacterRelationship> _relationshipStatus;

    #region getters/setters
    public int totalValue {
        get { return _value + _baseValue; }
    }
	public List<CharacterRelationship> relationshipStatus {
		get { return _relationshipStatus; }
	}
	public ECS.Character character1 {
		get { return _character1; }
	}
	public ECS.Character character2 {
		get { return _character2; }
	}
    #endregion

    public Relationship(ECS.Character character1, ECS.Character character2) {
        _character1 = character1;
        _character2 = character2;
		_relationshipStatus = new List<CharacterRelationship> ();

        UpdateBaseValue();
    }

    private void UpdateBaseValue() {
        _baseValue = 0;
        if(_character1.id == _character2.id) {
            _baseValue += 10; //Same Tribe: +10
        }
        
		if(_character1.faction != null && _character2.faction != null){
			FactionRelationship rel = FactionManager.Instance.GetRelationshipBetween(_character1.faction, _character2.faction);
			if (rel != null && rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
				_baseValue -= 25;//Opposing Tribes: -25
			}
		}


        if (_character1.raceSetting.race == _character2.raceSetting.race) {
            _baseValue += 5; //Same Race: +5
        } else {
            if (_character1.HasTrait(TRAIT.RACIST) || 
                _character2.HasTrait(TRAIT.RACIST)) {
                if(_character1.raceSetting.race != _character2.raceSetting.race) {
                    _baseValue -= 25; //Different Race: -25 (if racist)
                }
            }
        }

        if (_character1.HasTrait(TRAIT.RUTHLESS)) {
            if (_character2.HasTrait(TRAIT.BENEVOLENT)) {
                _baseValue -= 20; //Ruthless (-20 Opinion vs Benevolent)
            }
        } else if (_character2.HasTrait(TRAIT.RUTHLESS)) {
            if (_character1.HasTrait(TRAIT.BENEVOLENT)) {
                _baseValue -= 20; //Ruthless (-20 Opinion vs Benevolent)
            }
        }

        if (_character1.HasTrait(TRAIT.CHARISMATIC) || 
            _character2.HasTrait(TRAIT.CHARISMATIC)) {
            _baseValue += 25; //Charismatic: +15
        }

        if (_character1.HasTrait(TRAIT.REPULSIVE) ||
            _character2.HasTrait(TRAIT.REPULSIVE)) {
            _baseValue -= 15; //Repulsive: -15
        }

        if (_character1.HasTrait(TRAIT.SMART)) {
            if (_character2.HasTrait(TRAIT.SMART)) {
                _baseValue += 10; //Both Smart: +10
            } else if (_character2.HasTrait(TRAIT.DUMB)) {
                _baseValue -= 10; //Smart vs Dumb: -10
            }
        } else if (_character2.HasTrait(TRAIT.SMART)) {
            if (_character1.HasTrait(TRAIT.SMART)) {
                _baseValue += 10; //Both Smart: +10
            } else if (_character1.HasTrait(TRAIT.DUMB)) {
                _baseValue -= 10; //Smart vs Dumb: -10
            }
        }

        if (_character1.HasTrait(TRAIT.EFFICIENT)) {
            if (_character2.HasTrait(TRAIT.EFFICIENT)) {
                _baseValue += 10; //Both Efficient: +10
            } else if (_character2.HasTrait(TRAIT.INEPT)) {
                _baseValue -= 10; //Efficient vs Inept: -10
            }
        } else if (_character2.HasTrait(TRAIT.EFFICIENT)) {
            if (_character1.HasTrait(TRAIT.EFFICIENT)) {
                _baseValue += 10; //Both Efficient: +10
            } else if (_character1.HasTrait(TRAIT.INEPT)) {
                _baseValue -= 10; //Efficient vs Inept: -10
            }
        }

        if (_character1.HasTrait(TRAIT.PACIFIST) && 
            _character2.HasTrait(TRAIT.PACIFIST)) {
            _baseValue += 10; //Both Pacifist: +10
        } else {
            if (_character1.HasTrait(TRAIT.PACIFIST) || 
                _character2.HasTrait(TRAIT.PACIFIST)) {
                _baseValue += 10;//Pacifist (this character): +10
            }
            if (_character1.HasTrait(TRAIT.HOSTILE) ||
                _character2.HasTrait(TRAIT.HOSTILE)) {
                _baseValue -= 10;//Hostile (this character): -10
            }
        }

        if (_character1.HasTrait(TRAIT.DIPLOMATIC)) {
            if (_character2.HasTrait(TRAIT.HOSTILE)) {
                _baseValue -= 20;//Diplomatic (-20 Opinion vs Hostile) (exclusive from Hostile and Opportunist)
            }
        } else if (_character2.HasTrait(TRAIT.DIPLOMATIC)) {
            if (_character1.HasTrait(TRAIT.HOSTILE)) {
                _baseValue -= 20;//Diplomatic (-20 Opinion vs Hostile) (exclusive from Hostile and Opportunist)
            }
        }

        if (_character1.HasTrait(TRAIT.HONEST)) {
            if (_character2.HasTrait(TRAIT.SCHEMING) || _character2.HasTrait(TRAIT.DECEITFUL)) {
                _baseValue -= 20; //Honest (-20 Opinion vs Deceitful and Scheming) (exclusive from Deceitful and Scheming)
            }
        } else if (_character2.HasTrait(TRAIT.HONEST)) {
            if (_character1.HasTrait(TRAIT.SCHEMING) || _character1.HasTrait(TRAIT.DECEITFUL)) {
                _baseValue -= 20; //Honest (-20 Opinion vs Deceitful and Scheming) (exclusive from Deceitful and Scheming)
            }
        }

        _baseValue = Mathf.Max(0, _baseValue);
    }

    public void AdjustValue(int adjustment) {
        _value += adjustment;
        _value = Mathf.Max(0, _value);
    }

	public void AddRelationshipStatus(CHARACTER_RELATIONSHIP char1Relationship, CHARACTER_RELATIONSHIP char2Relationship){
		if(GetExactRelationship(char1Relationship, char2Relationship) == null){
			CharacterRelationship charRel = new CharacterRelationship (char1Relationship, char2Relationship);
			_relationshipStatus.Add (charRel);
		}
	}
	public void RemoveRelationshipStatus(CHARACTER_RELATIONSHIP char1Relationship, CHARACTER_RELATIONSHIP char2Relationship){
		CharacterRelationship rel = GetExactRelationship (char1Relationship, char2Relationship);
		if(rel != null){
			_relationshipStatus.Remove (rel);
		}
	}
	public bool HasCategory(CHARACTER_RELATIONSHIP_CATEGORY category){
		for (int i = 0; i < _relationshipStatus.Count; i++) {
			if(Utilities.charRelationshipCategory[_relationshipStatus[i].character1Relationship] == category){
				return true;
			}
		}
		return false;
	}

	public bool HasStatus(CHARACTER_RELATIONSHIP status){
		for (int i = 0; i < _relationshipStatus.Count; i++) {
			if(_relationshipStatus[i].character1Relationship == status || _relationshipStatus[i].character2Relationship == status){
				return true;
			}
		}
		return false;
	}
	public CharacterRelationship GetExactRelationship(CHARACTER_RELATIONSHIP char1Relationship, CHARACTER_RELATIONSHIP char2Relationship){
		for (int i = 0; i < _relationshipStatus.Count; i++) {
			if(_relationshipStatus[i].character1Relationship == char1Relationship && _relationshipStatus[i].character2Relationship == char2Relationship){
				return _relationshipStatus[i];
			}
		}
		return null;
	}
}
