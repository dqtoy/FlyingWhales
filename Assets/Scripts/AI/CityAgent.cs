using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CityAgent : GameAgent {

    private StructureObject _structureObj;

    #region getters/setters
    internal StructureObject structureObj {
        get { return _structureObj; }
    }
    #endregion

    public CityAgent(StructureObject structureObj) : base(AGENT_CATEGORY.STRUCTURE, AGENT_TYPE.CITY, MOVE_TYPE.NONE) {
        _structureObj = structureObj;
        _attackRange = 2f;
        _attackSpeed = 1f;
        _attackValue = 20;
        _visibilityRange = 2f;
        _movementSpeed = 0f;
        agentColor = Color.white;
        SetAllyTypes(new AGENT_TYPE[] { AGENT_TYPE.GUARD, AGENT_TYPE.CITY });
        SetInitialHP(125, 125);
    }

    #region overrides
    internal override AIBehaviour DetermineAction(List<GameAgent> threatsInRange, List<GameAgent> targetsInRange, List<GameAgent> alliesInRange, bool isPerformingAction) {
        if (isPerformingAction) {
            if(agentObj.currentAction == ACTION_TYPE.ATTACK) {
                GameAgent currTarget = ((AttackHostiles)agentObj.currentBehaviour).target;
                if(!threatsInRange.Contains(currTarget) && !targetsInRange.Contains(currTarget)) {
                    return _attackBehaviour;
                }
            }
        } else {
            if(threatsInRange.Count > 0 || targetsInRange.Count > 0) {
                return _attackBehaviour;
            }
        }
        return null;
    }
    internal override void KillAgent() {
        if (agentObj.gameObject == null) {
            throw new System.Exception(agentType.ToString() + " cannot be killed because it does not have a gameobject!");
        } else {
            if (_attackBehaviour != null) {
                _attackBehaviour.CancelAction();
                _attackBehaviour = null;
            }
            if (_fleeBehaviour != null) {
                _fleeBehaviour.CancelAction();
                _fleeBehaviour = null;
            }
            if (_randomBehaviour != null) {
                _randomBehaviour.CancelAction();
                _randomBehaviour = null;
            }
            
            Messenger.RemoveListener("OnMonthEnd", RegainHP);
            _isDead = true;
            BroadcastDeath();
            StructureObject structureToDestroy = ((CityAgent)this).structureObj;
            City cityOfStructure = structureToDestroy.hexTile.ownedByCity;
            cityOfStructure.RemoveTileFromCity(structureToDestroy.hexTile);
            if (structureToDestroy.hexTile.isHabitable) {
                //city is now dead
                cityOfStructure.KillCity();
            } else if (cityOfStructure.ownedTiles.Count <= 0) {
                //city is now dead
                cityOfStructure.KillCity();
            }
            _agentObj = null;
        }
    }
    #endregion
}
