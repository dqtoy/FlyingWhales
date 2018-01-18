using UnityEngine;
using System.Collections;

public class Relationship {

    private ECS.Character _character1;
    private ECS.Character _character2;

    private int _value;
    private int _baseValue; //This is the value affected by traits, race, etc.

    #region getters/setters
    public int totalValue {
        get { return _value + _baseValue; }
    }
    #endregion

    public Relationship(ECS.Character character1, ECS.Character character2) {
        _character1 = character1;
        _character2 = character2;

        UpdateBaseValue();
    }

    private void UpdateBaseValue() {
        _baseValue = 0;
        if(_character1.id == _character2.id) {
            _baseValue += 10; //Same Tribe: +10
        }
        
        FactionRelationship rel = FactionManager.Instance.GetRelationshipBetween(_character1.faction, _character2.faction);
        if (rel != null && rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
            _baseValue -= 25;//Opposing Tribes: -25
        }

        if (_character1.raceSetting.race == _character2.raceSetting.race) {
            _baseValue += 5; //Same Race: +5
        } else {
            if (_character1.HasTrait(TRAIT.RACIST) || 
                _character2.HasTrait(TRAIT.RACIST)) {
                _baseValue -= 25; //Different Race: -25 (if racist)
            }
        }

        if(_character1.HasTrait(TRAIT.CHARISMATIC) || 
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
            } else if (_character2.HasTrait(TRAIT.INEFFICIENT)) {
                _baseValue -= 10; //Efficient vs Inept: -10
            }
        } else if (_character2.HasTrait(TRAIT.EFFICIENT)) {
            if (_character1.HasTrait(TRAIT.EFFICIENT)) {
                _baseValue += 10; //Both Efficient: +10
            } else if (_character1.HasTrait(TRAIT.INEFFICIENT)) {
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

        if (_character1.HasTrait(TRAIT.DIPLOMATIC) ||
                _character2.HasTrait(TRAIT.DIPLOMATIC)) {
            _baseValue += 10;//Diplomatic (this character): +10
        }

        if (_character1.HasTrait(TRAIT.HONEST)) {
            if (_character2.HasTrait(TRAIT.HONEST)) {
                _baseValue += 10; //Both Honest: +10
            } else if (_character2.HasTrait(TRAIT.SCHEMING)) {
                _baseValue -= 10; //Honest vs Scheming: -10
            }
        } else if (_character2.HasTrait(TRAIT.HONEST)) {
            if (_character1.HasTrait(TRAIT.HONEST)) {
                _baseValue += 10; //Both Honest: +10
            } else if (_character1.HasTrait(TRAIT.SCHEMING)) {
                _baseValue -= 10; //Honest vs Scheming: -10
            }
        }

        _baseValue = Mathf.Max(0, _baseValue);
    }

    public void AdjustValue(int adjustment) {
        _value += adjustment;
        _value = Mathf.Max(0, _value);
    }
}
