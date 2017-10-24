using UnityEngine;
using System.Collections;
using Panda;
using Pathfinding;
using System.Collections.Generic;
using System;
using System.Linq;

[RequireComponent(typeof(PandaBehaviour))]
public class EntityAI : AIPath {

    private Entity _entity;

    private AIBehaviour _currentBehaviour;
    
    //Actions
    [SerializeField] private ACTION_TYPE _currentAction;
    [SerializeField] private bool isPerformingAction;
    private int layerMask;
    private List<Entity> _targetsInRange;
    private List<Entity> _hostilesInRange;
    private List<Entity> _alliesInRange;
    private List<Entity> _entitiesAttackingThis;
    private List<Entity> _threatsInRange;

    //Visuals
    [SerializeField] private SpriteRenderer visualSprite;
    [SerializeField] private UIProgressBar hpProgBar;
    [SerializeField] private Transform entityInfoPanel;
    [SerializeField] private UILabel entityActionLbl;

    #region getters/setters
    internal ENTITY_TYPE entityType {
        get { return _entity.entityType; }
    }
    internal ACTION_TYPE currentAction {
        get { return _currentAction; }
    }
    internal List<Entity> targetsInRange {
        get { return _targetsInRange; }
    }
    internal List<Entity> weaknessesInRange {
        get { return _hostilesInRange; }
    }
    internal List<Entity> entitiesAttackingThis {
        get { return _entitiesAttackingThis; }
    }
    internal Entity entity {
        get { return _entity; }
    }
    #endregion

    public void Initialize(Entity entity, Kingdom allowedInKingdom) {
        this._entity = entity;
        this.speed = entity.speed;
        isPerformingAction = false;
        name = entityType.ToString() + UnityEngine.Random.Range(0, 5).ToString();
        visualSprite.gameObject.name = entityType.ToString();
        layerMask = 1 << LayerMask.NameToLayer("Entity");
        _targetsInRange = new List<Entity>();
        _hostilesInRange = new List<Entity>();
        _alliesInRange = new List<Entity>();
        _entitiesAttackingThis = new List<Entity>();
        _threatsInRange = new List<Entity>();
        SetValidTags(new int[] { allowedInKingdom.kingdomTagIndex });
        SetSpriteColor(entity.entityColor);
        Messenger.AddListener<Entity>("EntityDied", OnOtherEntityDied);
    }

    #region Entity Visuals
    internal void SetSpriteColor(Color color) {
        visualSprite.color = color;
    }
    #endregion

    #region Target Functions
    internal void SetTarget(Transform target) {
        this.target = target;
    }
    internal void SetTarget(Vector3 target) {
        this.seeker.StartPath(this.GetFeetPosition(), target);
    }
    #endregion

    #region Tag Functions
    internal void SetValidTags(int[] validTags) {
        this.seeker.traversableTags = 0;
        for (int i = 0; i < validTags.Length; i++) {
            AddValidTag(validTags[i]);
        }
    }
    internal void AddValidTag(int validTag) {
        this.seeker.traversableTags |= 1 << validTag;
    }
    internal void RemoveFromValidTags(int tagToRemove) {
        this.seeker.traversableTags &= ~1 << tagToRemove;
    }
    #endregion

    #region Entity Functions
    internal void OnEntityAttacked(Entity attacker) {
        if (!_entitiesAttackingThis.Contains(attacker)) {
            _entitiesAttackingThis.Add(attacker);
            AlertAllies(attacker);
        }
    }
    internal void AlertAllies(Entity attackerOfAlly) {
        for (int i = 0; i < _alliesInRange.Count; i++) {
            _alliesInRange[i].entityGO.AddThreatInRange(attackerOfAlly);
        }
    }
    internal void AddThreatInRange(Entity threat) {
        if (!_threatsInRange.Contains(threat)) {
            _threatsInRange.Add(threat);
        }
    }
    internal void OnOtherEntityDied(Entity otherEntity) {
        if(_entity != otherEntity) {
            _entitiesAttackingThis.Remove(otherEntity);
            _threatsInRange.Remove(otherEntity);
        }
    }
    internal void SetIsPerformingAction(bool state) {
        isPerformingAction = state;
    }
    internal void SetCurrentBehaviour(AIBehaviour currentBehaviour) {
        _currentBehaviour = currentBehaviour;
        _currentAction = currentBehaviour.actionType;
    }
    #endregion

    #region Behaviour Tree Functions
    [Task]
    public void DetermineAction() {
        Collider[] objectsInRange = Physics.OverlapSphere(this.transform.position, _entity.fov, layerMask);
        _targetsInRange.Clear();
        _hostilesInRange.Clear();
        _alliesInRange.Clear();
        if (objectsInRange.Length > 1) {
            List<ENTITY_TYPE> entitiesInRange = new List<ENTITY_TYPE>();
            for (int i = 0; i < objectsInRange.Length; i++) {
                GameObject currGOInRange = objectsInRange[i].gameObject;
                if(currGOInRange != visualSprite.gameObject) {
                    entitiesInRange.Add((ENTITY_TYPE)Enum.Parse(typeof(ENTITY_TYPE), currGOInRange.name));
                    Entity currEntityInRange = currGOInRange.transform.parent.GetComponent<EntityAI>().entity;
                    if (_entity.strongAgainst.Contains(currEntityInRange.entityType)) {
                        _targetsInRange.Add(currEntityInRange);
                    }
                    if (_entity.weakAgainst.Contains(currEntityInRange.entityType)) {
                        _hostilesInRange.Add(currEntityInRange);
                    }
                    if(_entity.entityType == currEntityInRange.entityType) {
                        _alliesInRange.Add(currEntityInRange);
                    }
                }
            }
        }

        //Determine whether other entites that have attacked you or allies are to be considered as threats or targets
        List<Entity> otherEntites = new List<Entity>();
        otherEntites.AddRange(_entitiesAttackingThis);
        otherEntites.AddRange(_threatsInRange);

        for (int i = 0; i < otherEntites.Count; i++) {
            Entity otherEntity = otherEntites[i];
            int threatOfEntity = _entity.GetThreatOfEntity(otherEntity);
            int initiativeFromEntity = _entity.GetInitiativeFromEntity(otherEntity);
            int initiativeFromAllies = _alliesInRange.Count * 2;
            int totalInitiative = initiativeFromEntity + initiativeFromAllies;
            if (totalInitiative > threatOfEntity) {
                //other entity is to be considered a target
                if (!_targetsInRange.Contains(otherEntity)) {
                    _targetsInRange.Add(otherEntity);
                }
            } else {
                //other entity is to be considered a threat
                if (!_hostilesInRange.Contains(otherEntity)) {
                    _hostilesInRange.Add(otherEntity);
                }
            }
        }
        
        AIBehaviour behaviourToPerform = _entity.DetermineAction(_hostilesInRange, _targetsInRange, _alliesInRange, isPerformingAction);
        if(behaviourToPerform != null) {
            behaviourToPerform.DoAction();
            Task.current.Succeed();
        } else {
            Task.current.Fail();
        }
    }
    #endregion

    #region Monobehaviour Functions
    //protected override void OnDrawGizmos() {
    //    base.OnDrawGizmos();
    //    if (alwaysDrawGizmos) {
    //        Gizmos.color = Color.yellow;
    //        Gizmos.DrawWireSphere(Vector3.zero, fovOfEntity);
    //    }
    //}
    protected override void Update() {
        base.Update();
        Vector3 pos = this.transform.localPosition;
        pos.y += 0.5f;
        pos.z = 0f;
        entityInfoPanel.transform.localPosition = pos;
        hpProgBar.value = (float)_entity.currentHP / (float)_entity.totalHP;
        if(currentAction == ACTION_TYPE.ATTACK) {
            entityActionLbl.text = "A";
        } else if (currentAction == ACTION_TYPE.FLEE) {
            entityActionLbl.text = "F";
        }else if (currentAction == ACTION_TYPE.RANDOM) {
            entityActionLbl.text = "W";
        }

    }
    #endregion

    public override void OnPathComplete(Path _p) {
        if (_p.error) {
            return;
        }
        base.OnPathComplete(_p);
    }

    public override void OnTargetReached() {
        _currentBehaviour.OnActionDone();
    }
}
