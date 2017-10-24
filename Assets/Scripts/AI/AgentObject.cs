﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Panda;

[RequireComponent(typeof(PandaBehaviour))]
public class AgentObject : MonoBehaviour {

    private Agent _agent;
    private AIBehaviour _currentBehaviour;

    //Actions
    [SerializeField] private AgentAI _aiPath;
    [SerializeField] private Seeker _seeker;
    private ACTION_TYPE _currentAction;
    private bool isPerformingAction;
    private int layerMask;
    private List<Agent> _targetsInRange; //Agents in range that this agent wants to attack
    private List<Agent> _alliesInRange; //Agents in range that is the same type as this
    private List<Agent> _threatsInRange; //Agents in range that wants to attack this agent;
    //private List<Agent> _entitiesAttackingThis;
    //private List<Agent> _threatsInRange;

    //Visuals
    [SerializeField] private SpriteRenderer visualSprite;
    [SerializeField] private UIProgressBar hpProgBar;
    [SerializeField] private Transform agentInfoPanel;
    [SerializeField] private UILabel agentActionLbl;

    #region getters/setters
    internal Agent agent {
        get { return _agent; }
    }
    internal ACTION_TYPE currentAction {
        get { return _currentAction; }
    }
    internal List<Agent> targetsInRange {
        get { return _targetsInRange; }
    }
    internal List<Agent> alliesInRange {
        get { return _alliesInRange; }
    }
    internal List<Agent> threatsInRange {
        get { return _threatsInRange; }
    }
    #endregion

    public void Initialize(Agent agent, int[] validTags) {
        _agent = agent;
        _aiPath.speed = agent.movementSpeed;
        isPerformingAction = false;
        //name = entityType.ToString() + UnityEngine.Random.Range(0, 5).ToString();
        //visualSprite.gameObject.name = entityType.ToString();
        layerMask = 1 << LayerMask.NameToLayer("Agent");
        _targetsInRange = new List<Agent>();
        _threatsInRange = new List<Agent>();
        _alliesInRange = new List<Agent>();
        //_entitiesAttackingThis = new List<Entity>();
        //_threatsInRange = new List<Entity>();
        SetValidTags(validTags);
        SetSpriteColor(agent.agentColor);
        //Messenger.AddListener<Entity>("EntityDied", OnOtherEntityDied);
    }

    #region Visual Functions
    internal void SetSpriteColor(Color color) {
        visualSprite.color = color;
    }
    #endregion

    #region Tag Functions
    internal void SetValidTags(int[] validTags) {
        _seeker.traversableTags = 0;
        for (int i = 0; i < validTags.Length; i++) {
            AddValidTag(validTags[i]);
        }
    }
    internal void AddValidTag(int validTag) {
        _seeker.traversableTags |= 1 << validTag;
    }
    internal void RemoveFromValidTags(int tagToRemove) {
        _seeker.traversableTags &= ~1 << tagToRemove;
    }
    #endregion

    #region Target Functions
    internal void SetTarget(Transform target) {
        _aiPath.target = target;
    }
    internal void SetTarget(Vector3 target) {
        _seeker.StartPath(_aiPath.GetFeetPosition(), target);
    }
    #endregion

    #region Entity Functions
    internal void SetIsPerformingAction(bool state) {
        isPerformingAction = state;
    }
    internal void SetCurrentBehaviour(AIBehaviour currentBehaviour) {
        _currentBehaviour = currentBehaviour;
        _currentAction = currentBehaviour.actionType;
    }
    #endregion

    #region AI
    internal void OnTargetReached() {
        _currentBehaviour.OnActionDone();
    }
    #endregion

    #region Behaviour Tree
    [Task]
    public void DetermineAction() {
        Collider[] objectsInRange = Physics.OverlapSphere(this.transform.position, _agent.visibilityRange, layerMask);
        _targetsInRange.Clear();
        _threatsInRange.Clear();
        _alliesInRange.Clear();
        if (objectsInRange.Length > 1) {
            for (int i = 0; i < objectsInRange.Length; i++) {
                GameObject currGOInRange = objectsInRange[i].gameObject;
                if (currGOInRange != visualSprite.gameObject) {
                    Agent otherAgent = currGOInRange.transform.parent.parent.GetComponent<AgentObject>().agent;
                    if (this.agent.allyTypes.Contains(otherAgent.agentType)) {
                        //other agent is considered as ally
                        //TODO: add checking for if other agent is from another kingdom, if so, determine if that guard is considered to be an ally
                        if (!_alliesInRange.Contains(otherAgent)) {
                            _alliesInRange.Add(otherAgent);
                        }
                    } else {
                        //other agent is not an ally
                        //compute if it is a threat or a target, if threat value and initiative value is equal, randomize if the other agent should be considered a 
                        //threat or target.
                        int threatValueOfOtherAgent = this.agent.GetThreatOfAgent(otherAgent);
                        int initiativeValueOfOtherAgent = this.agent.GetInitiativeFromAgent(otherAgent);
                        if(threatValueOfOtherAgent > initiativeValueOfOtherAgent) {
                            //other agent is a threat
                            _threatsInRange.Add(otherAgent);
                        } else if(initiativeValueOfOtherAgent > threatValueOfOtherAgent) {
                            //other agent is a target
                            _targetsInRange.Add(otherAgent);
                        }
                    }
                    
                }
            }
        }

        ////Determine whether other entites that have attacked you or allies are to be considered as threats or targets
        //List<Entity> otherEntites = new List<Entity>();
        //otherEntites.AddRange(_entitiesAttackingThis);
        //otherEntites.AddRange(_threatsInRange);

        //for (int i = 0; i < otherEntites.Count; i++) {
        //    Entity otherEntity = otherEntites[i];
        //    int threatOfEntity = _entity.GetThreatOfEntity(otherEntity);
        //    int initiativeFromEntity = _entity.GetInitiativeFromEntity(otherEntity);
        //    int initiativeFromAllies = _alliesInRange.Count * 2;
        //    int totalInitiative = initiativeFromEntity + initiativeFromAllies;
        //    if (totalInitiative > threatOfEntity) {
        //        //other entity is to be considered a target
        //        if (!_targetsInRange.Contains(otherEntity)) {
        //            _targetsInRange.Add(otherEntity);
        //        }
        //    } else {
        //        //other entity is to be considered a threat
        //        if (!_threatsInRange.Contains(otherEntity)) {
        //            _threatsInRange.Add(otherEntity);
        //        }
        //    }
        //}

        AIBehaviour behaviourToPerform = _agent.DetermineAction(_threatsInRange, _targetsInRange, _alliesInRange, isPerformingAction);
        if (behaviourToPerform != null) {
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
    private void Update() {
        Vector3 pos = _aiPath.transform.localPosition;
        pos.y += 0.5f;
        pos.z = 0f;
        agentInfoPanel.transform.localPosition = pos;
        hpProgBar.value = (float)agent.currentHP / (float)agent.totalHP;
        if (currentAction == ACTION_TYPE.ATTACK) {
            agentActionLbl.text = "A";
        } else if (currentAction == ACTION_TYPE.FLEE) {
            agentActionLbl.text = "F";
        } else if (currentAction == ACTION_TYPE.RANDOM) {
            agentActionLbl.text = "W";
        }

    }
    #endregion
}
