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

    [SerializeField] protected int _maxHP;
    [SerializeField] protected float _currentHP;
    protected float _attackRange;
    protected float _attackSpeed;
    protected int _attackValue;
    protected float _visibilityRange;
    [SerializeField] protected bool _isDead;
    protected bool _isInCombat;
    protected GameDate _lastDamagedOn;

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
    internal int maxHP {
        get { return _maxHP; }
    }
    internal int currentHP {
        get { return Mathf.FloorToInt(_currentHP); }
    }
    internal float attackRange {
        get { return _attackRange; }
    }
    internal float attackSpeed {
        get { return _attackSpeed; }
    }
    internal int attackValue {
        get { return _attackValue + currentHP; }
    }
    internal float visibilityRange {
        get { return _visibilityRange; }
    }
    internal bool isDead {
        get { return _isDead; }
    }
    internal bool isInCombat {
        get { return _isInCombat; }
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
        _lastDamagedOn = new GameDate(0, 0, 0);
        if(_agentCategory == AGENT_CATEGORY.LIVING) {
            Messenger.AddListener("OnDayEnd", RegainHP);
        }else if(_agentCategory == AGENT_CATEGORY.STRUCTURE) {
            Messenger.AddListener("OnMonthEnd", RegainHP);
        }
        
    }

    internal void SetAgentObj(AgentObject agentObj) {
        _agentObj = agentObj;
    }

    #region HP
    internal void SetNewMaxHP(int newMaxHP) {
        _maxHP = newMaxHP;
        _currentHP = newMaxHP;
    }
    protected void RegainHP() {
        //Living Agents regenerate 1% of their max HP each day while out of combat.
        //Structures regenerate 5 % of ther max HP at the start of each month while out of combat.

        //Check if agent is out of combat (has not recieved damage within the last 3 days)
        GameDate checker = new GameDate();
        checker.SetDate(_lastDamagedOn);
        checker.AddDays(3);
        if(checker.year == 0 || checker.year != GameManager.Instance.year || checker.month != GameManager.Instance.month || checker.day < GameManager.Instance.days) {
            SetCombatState(false); //agent is no longer in combat

            float regeneratedHealth = 0f;
            if(_agentCategory == AGENT_CATEGORY.LIVING) {
                regeneratedHealth = ((float)maxHP * 0.01f);
            } else if(_agentCategory == AGENT_CATEGORY.STRUCTURE) {
                regeneratedHealth = ((float)maxHP * 0.05f);
            }
            AdjustHP(regeneratedHealth);
        }
    }
    internal void SetInitialHP(int currentHP, int totalHP) {
        _maxHP = totalHP;
        _currentHP = currentHP;
    }
    internal void AdjustHP(int amount) {
        _currentHP += amount;
        _currentHP = Mathf.Clamp(_currentHP, 0f, _maxHP);
        if (_currentHP <= 0) {
            //Mark entity as Dead
            _isDead = true;
        }
    }
    private void AdjustHP(float amount) {
        _currentHP += amount;
        _currentHP = Mathf.Clamp(_currentHP, 0f, _maxHP);
        if (_currentHP <= 0) {
            //Mark entity as Dead
            _isDead = true;
        }
    }
    internal virtual void KillAgent() {
        if(agentObj == null) {
            return;
        }
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
            Messenger.RemoveListener("OnDayEnd", RegainHP);
            _isDead = true;

            BroadcastDeath();
            ObjectPoolManager.Instance.DestroyObject(agentObj.gameObject);
            _agentObj = null;
        }
    }
    internal void BroadcastDeath() {
        Messenger.Broadcast<GameAgent>("OnAgentDied", this);
    }
    #endregion

    #region Behaviour Functions
    internal void SetCombatState(bool combatState) {
        _isInCombat = combatState;
        if (_isInCombat) {
            //You are out of combat if you have not received any damage within the last 3 days.
            //Set day the damage was taken
            _lastDamagedOn.SetDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        }
    }
    internal void SetAttackBehaviour(AIBehaviour attackBehaviour) {
        _attackBehaviour = attackBehaviour;
    }
    internal void SetFleeBehaviour(AIBehaviour fleeBehaviour) {
        _fleeBehaviour = fleeBehaviour;
    }
    internal void SetRandomBehaviour(AIBehaviour randomBehaviour) {
        _randomBehaviour = randomBehaviour;
    }
    internal void OnAttacked(GameAgent attackedBy) {
        //Determine whether this agent will attack back or flee
        _agentObj.OnAttacked(attackedBy); //Alert allies in range
        _agentObj.AddAgentAsNearbyAttacker(attackedBy); //Add attacker as nearby attacker for this agent
        //Let the fixed update determine the next action given the threats, targets and allies in range.
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
