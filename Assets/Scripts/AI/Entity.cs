using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Entity {

    private ENTITY_TYPE _entityType;
    private int _totalHP;
    private int _currentHP;
    private int _attackRatio;
    private int _fleeRatio;
    private int _randomRatio;
    private float _fov;
    private float _baseSpeed;
    private List<ENTITY_TYPE> _strongAgainst;
    private List<ENTITY_TYPE> _weakAgainst;
    private AIBehaviour _attackBehaviour;
    private AIBehaviour _fleeBehaviour;
    private AIBehaviour _randomBehaviour;
    private EntityAI _entityGO;
    private Color _entityColor;
    private bool _isDead;

    #region getters/setetrs
    internal ENTITY_TYPE entityType {
        get { return _entityType; }
    }
    internal int totalHP {
        get { return _totalHP; }
    }
    internal int currentHP {
        get { return _currentHP; }
    }
    internal float percentageOfHPRemaining {
        get { return ((float)currentHP / (float)totalHP); }
    }
    internal int attackDamage {
        get { return Mathf.FloorToInt((float)currentHP * 0.25f); }
    }
    internal int attackRatio {
        get { return _attackRatio; }
    }
    internal int fleeRatio {
        get { return _fleeRatio; }
    }
    internal int randomRatio {
        get { return _randomRatio; }
    }
    internal float fov {
        get { return _fov; }
    }
    internal float speed {
        get { return _baseSpeed; }
    }
    internal List<ENTITY_TYPE> strongAgainst {
        get { return _strongAgainst; }
    }
    internal List<ENTITY_TYPE> weakAgainst {
        get { return _weakAgainst; }
    }
    internal AIBehaviour attackBehaviour {
        get { return _attackBehaviour; }
    }
    internal AIBehaviour fleeBehaviour {
        get { return _fleeBehaviour; }
    }
    internal AIBehaviour randomBehaviour {
        get { return _randomBehaviour; }
    }
    internal EntityAI entityGO {
        get { return _entityGO; }
    }
    internal Color entityColor {
        get { return _entityColor; }
    }
    internal bool isDead {
        get { return _isDead; }
    }
    #endregion

    public Entity(ENTITY_TYPE entityType) {
        _entityType = entityType;
        _isDead = false;
    }

    internal void SetEntityObj(EntityAI obj) {
        _entityGO = obj;
    }
    protected void SetStartingHP(int totalHP, int startingHP) {
        _totalHP = totalHP;
        _currentHP = startingHP;
    }
    internal void AdjustHP(int amount) {
        _currentHP += amount;
        if(_currentHP <= 0) {
            //Mark entity as Dead
            _isDead = true;
        }
    }

    internal void KillEntity() {
        GameObject.Destroy(_entityGO.transform.parent.gameObject);
        if (Messenger.eventTable.ContainsKey("EntityDied")) {
            Messenger.Broadcast<Entity>("EntityDied", this);
        }
    }

    /*
     * <summary>
     * Set AJR Ratio
     * 0 index - attackRatio
     * 1 index - fleeRatio
     * 2 index - randomRatio
     * </summary>
     * */
    protected void SetRatios(int[] ratios) {
        _attackRatio = ratios[0];
        _fleeRatio = ratios[1];
        _randomRatio = ratios[2];
    }

    protected void SetFOV(float fov) {
        _fov = fov;
    }

    protected void SetBaseSpeed(float baseSpeed) {
        _baseSpeed = baseSpeed;
    }

    protected void SetStrengths(List<ENTITY_TYPE> strongAgainst) {
        _strongAgainst = strongAgainst;
    }
    protected void SetWeaknesses(List<ENTITY_TYPE> weakAgainst) {
        _weakAgainst = weakAgainst;
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

    protected void SetEntityColor(Color color) {
        _entityColor = color;
    }

    internal virtual AIBehaviour DetermineAction(List<Entity> hostilesInRange, List<Entity> targetsInRange, List<Entity> alliesInRange, bool isPerformingAction) {
        int totalSurroundingInitiative = GetTotalSurroundingInitiative(targetsInRange);
        int totalSurroundingThreat = GetTotalSurroundingThreat(hostilesInRange);
        int totalInitiativeFromAllies = alliesInRange.Count * 2;

        int totalAttackRatio = attackRatio + totalSurroundingInitiative + totalInitiativeFromAllies;
        int totalFleeRatio = fleeRatio + totalSurroundingThreat;

        if (isPerformingAction && entityGO.currentAction != ACTION_TYPE.RANDOM) {
            return null;
        }

        if (targetsInRange.Count > 0 && hostilesInRange.Count > 0) {
            if(totalAttackRatio > totalFleeRatio) {
                return _attackBehaviour;
            } else if( totalFleeRatio > totalAttackRatio) {
                return _fleeBehaviour;
            } else {
                //totalFleeRatio and totalAttackRatio are equal
                if(Random.Range(0, 2) == 0) {
                    return _attackBehaviour;
                } else {
                    return _fleeBehaviour;
                }
            }
        } else if(targetsInRange.Count > 0 && hostilesInRange.Count <= 0) {
            return _attackBehaviour;
        } else if (hostilesInRange.Count > 0 && targetsInRange.Count <= 0) {
            return _fleeBehaviour;
        } else {
            if (!isPerformingAction) {
                return _randomBehaviour;
            }
        }
        return null;
    }

    protected int GetTotalSurroundingThreat(List<Entity> threats) {
        int totalThreat = 0;
        for (int i = 0; i < threats.Count; i++) {
            totalThreat += GetThreatOfEntity(threats[i]);
        }
        return totalThreat;
    }

    protected int GetTotalSurroundingInitiative(List<Entity> targets) {
        int totalInitiative = 0;
        for (int i = 0; i < targets.Count; i++) {
            totalInitiative += GetThreatOfEntity(targets[i]);
        }
        return totalInitiative;
    }

    /*
     * <summary>
     * Returns the threat value of another entity relative to this one
     * 3 - hp and attack of other entity is higher or equal to your attack and hp
     * 2 - attack of other entity is higher than your hp
     * 1 - hp of entity is higher than your attack
     * 0 - your hp and attack is higher than the attack and hp of the other entity
     * </summary>
     * */
    internal int GetThreatOfEntity(Entity otherEntity) {
        int threat = 0;
        if(otherEntity.currentHP >= this.currentHP && otherEntity.attackDamage >= this.attackDamage) {
            threat = 3;
        } else if(otherEntity.attackDamage > this.currentHP) {
            threat = 2;
        }else if(otherEntity.currentHP > this.attackDamage) {
            threat = 1;
        }
        return threat;
    }

    /*
     * <summary>
     * Returns the initiative gained from another entity
     * 3 - if you have higher hp and attack damage than the other entity
     * 2 - if your attack is higher than the other entity's hp
     * 1 - if your hp is higher than the other entity's attack
     * 0 - if you have lower or equal hp and attack damge than the other entity
     * </summary>
     * */
    internal int GetInitiativeFromEntity(Entity otherEntity) {
        int initiative = 0;
        if(this.currentHP > otherEntity.currentHP && this.attackDamage > otherEntity.attackDamage) {
            initiative = 3;
        } else if (this.attackDamage > otherEntity.currentHP) {
            initiative = 2;
        } else if (this.currentHP > otherEntity.attackDamage) {
            initiative = 1;
        }
        return initiative;
    }

}
