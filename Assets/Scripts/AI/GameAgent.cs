using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GameAgent {

    [SerializeField] protected AGENT_CATEGORY _agentCategory;
    [SerializeField] protected AGENT_TYPE _agentType;

    [SerializeField] protected int _attackRatio;
    [SerializeField] protected int _fleeRatio;
    [SerializeField] protected int _randomRatio;

    protected int _totalHP;
    protected int _currentHP;
    protected float _attackRange;
    protected float _attackSpeed;
    protected int _attackValue;
    protected float _visibilityRange;
    [SerializeField] protected bool _isDead;

    protected MOVE_TYPE _movementType;
    protected float _movementSpeed;

    [SerializeField] protected AIBehaviour _attackBehaviour;
    [SerializeField] protected AIBehaviour _fleeBehaviour;
    [SerializeField] protected AIBehaviour _randomBehaviour;

    protected HashSet<AGENT_TYPE> _allyTypes;

    [SerializeField] protected AgentObject _agentObj;

    internal Color agentColor;

    #region getters/setters
    internal AGENT_CATEGORY agentCategory {
        get { return _agentCategory; }
    }
    internal AGENT_TYPE agentType {
        get { return _agentType; }
    }
    internal int totalHP {
        get { return _totalHP; }
    }
    internal int currentHP {
        get { return _currentHP; }
    }
    internal float attackRange {
        get { return _attackRange; }
    }
    internal float attackSpeed {
        get { return _attackSpeed; }
    }
    internal int attackValue {
        get { return _attackValue; }
    }
    internal float visibilityRange {
        get { return _visibilityRange; }
    }
    internal bool isDead {
        get { return _isDead; }
    }
    internal MOVE_TYPE movementType {
        get { return _movementType; }
    }
    internal float movementSpeed {
        get { return _movementSpeed; }
    }
    internal HashSet<AGENT_TYPE> allyTypes {
        get { return _allyTypes; }
    }
    internal AgentObject agentObj {
        get { return _agentObj; }
    }
    #endregion

    public GameAgent(AGENT_CATEGORY agentCategory, AGENT_TYPE agentType, MOVE_TYPE movementType) {
        _agentCategory = agentCategory;
        _agentType = agentType;
        _movementType = movementType;
    }

    internal void SetAgentObj(AgentObject agentObj) {
        _agentObj = agentObj;
    }

    #region HP
    internal void SetInitialHP(int currentHP, int totalHP) {
        _totalHP = totalHP;
        _currentHP = currentHP;
    }
    internal void AdjustHP(int amount) {
        _currentHP += amount;
        if (_currentHP <= 0) {
            //Mark entity as Dead
            _isDead = true;
        }
    }
    internal void KillAgent() {
        if(agentObj.gameObject == null) {
            throw new System.Exception(agentType.ToString() + " cannot be killed because it does not have a gameobject!");
        } else {
            if(_attackBehaviour != null) {
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
            ObjectPoolManager.Instance.DestroyObject(agentObj.gameObject);
            _isDead = true;
            _agentObj = null;
            Messenger.Broadcast<GameAgent>("OnAgentDied", this);
        }
    }
    #endregion

    #region Behaviour Functions
    internal void SetAttackBehaviour(AIBehaviour attackBehaviour) {
        _attackBehaviour = attackBehaviour;
    }
    internal void SetFleeBehaviour(AIBehaviour fleeBehaviour) {
        _fleeBehaviour = fleeBehaviour;
    }
    internal void SetRandomBehaviour(AIBehaviour randomBehaviour) {
        _randomBehaviour = randomBehaviour;
    }
    internal virtual AIBehaviour DetermineAction(List<GameAgent> threatsInRange, List<GameAgent> targetsInRange, List<GameAgent> alliesInRange, bool isPerformingAction) {
        int totalSurroundingInitiative = GetTotalSurroundingInitiative(targetsInRange);
        int totalSurroundingThreat = GetTotalSurroundingThreat(threatsInRange);
        int totalInitiativeFromAllies = GetTotalInitiativeFromAllies(alliesInRange);

        int totalAttackRatio = totalSurroundingInitiative + totalInitiativeFromAllies;
        int totalFleeRatio = totalSurroundingThreat;

        if (isPerformingAction && agentObj.currentAction != ACTION_TYPE.RANDOM) {
            //this agent is currently performing a flee or attack action, let it finish.
            return null;
        }

        if (targetsInRange.Count > 0 && threatsInRange.Count > 0) {
            if (totalAttackRatio > totalFleeRatio) {
                return _attackBehaviour;
            } else if (totalFleeRatio > totalAttackRatio) {
                return _fleeBehaviour;
            } else {
                //totalFleeRatio and totalAttackRatio are equal
                if (Random.Range(0, 2) == 0) {
                    return _attackBehaviour;
                } else {
                    return _fleeBehaviour;
                }
            }
        } else if (targetsInRange.Count > 0 && threatsInRange.Count <= 0) {
            return _attackBehaviour;
        } else if (threatsInRange.Count > 0 && targetsInRange.Count <= 0) {
            return _fleeBehaviour;
        } else {
            if (!isPerformingAction) {
                return _randomBehaviour;
            }
        }
        return null;
    }
    #endregion

    #region Threat/Initiative
    protected int GetTotalSurroundingThreat(List<GameAgent> threats) {
        int totalThreat = 0;
        for (int i = 0; i < threats.Count; i++) {
            totalThreat += GetThreatOfAgent(threats[i]);
        }
        return totalThreat;
    }
    protected int GetTotalSurroundingInitiative(List<GameAgent> targets) {
        int totalInitiative = 0;
        for (int i = 0; i < targets.Count; i++) {
            totalInitiative += GetInitiativeFromAgent(targets[i]);
        }
        return totalInitiative;
    }
    internal int GetTotalInitiativeFromAllies(List<GameAgent> allies) {
        int totalInitiative = 0;
        for (int i = 0; i < allies.Count; i++) {
            totalInitiative += GetInitiativeFromAlly(allies[i]);
        }
        return totalInitiative;
    }
    internal void SetAllyTypes(AGENT_TYPE[] allyTypes) {
        _allyTypes = new HashSet<AGENT_TYPE>(allyTypes);
    }
    /*
     * <summary>
     * Returns the threat value of another agent relative to this one
     * 3 - hp and attack of other agent is higher or equal to your attack and hp
     * 2 - attack of other agent is higher than your hp
     * 1 - hp of agent is higher than your attack
     * 0 - your hp and attack is higher than the attack and hp of the other agent
     * </summary>
     * */
    internal int GetThreatOfAgent(GameAgent otherAgent) {
        int threat = 0;
        if (otherAgent.currentHP >= this.currentHP && otherAgent.attackValue >= this.attackValue) {
            threat = 3;
        } else if (otherAgent.attackValue > this.currentHP) {
            threat = 2;
        } else if (otherAgent.currentHP > this.attackValue) {
            threat = 1;
        }
        return threat;
    }
    /*
     * <summary>
     * Returns the initiative gained from another agent
     * 3 - if you have higher hp and attack damage than the other agent
     * 2 - if your attack is higher than the other agent's hp
     * 1 - if your hp is higher than the other agent's attack
     * 0 - if you have lower or equal hp and attack damge than the other agent
     * </summary>
     * */
    internal int GetInitiativeFromAgent(GameAgent otherAgent) {
        int initiative = 0;
        if (this.currentHP > otherAgent.currentHP && this.attackValue > otherAgent.attackValue) {
            initiative = 3;
        } else if (this.attackValue > otherAgent.currentHP) {
            initiative = 2;
        } else if (this.currentHP > otherAgent.attackValue) {
            initiative = 1;
        }
        return initiative;
    }

    internal int GetInitiativeFromAlly(GameAgent ally) {
        int initiative = 0;
        if(ally.currentHP >= this.currentHP && ally.attackValue >= this.attackValue) {
            initiative = 3;
        } else if(ally.currentHP >= this.currentHP) {
            initiative = 2;
        } else if (ally.attackValue >= this.attackValue) {
            initiative = 1;
        }
        return initiative;
    }
    #endregion

}
