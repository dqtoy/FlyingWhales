/*
 This is the base class for quests.
 - Quests are created by either the internal manager or military manager.
 - Once a quest is created, it has an x amount of days before it expires, expire meaning that no more characters can accept it and it is discarded.
 - Each quest can have quest filters, that filter the characters that can accept it.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ECS;

public class Quest : CharacterTask{

    protected delegate void OnQuestAccepted();
    protected OnQuestAccepted onQuestAccepted;

    protected int _id;
    protected QUEST_TYPE _questType;
    //protected int _daysBeforeDeadline;
    protected bool _isExpired;
    protected bool _isAccepted;
    protected bool _isWaiting;
    protected Party _assignedParty;
    protected List<QuestFilter> _questFilters;
    protected TaskAction _currentAction;
	protected int _activeDuration;
    
    protected Queue<TaskAction> _questLine;
    protected Faction _targetFaction; //This is only supposed to have a value when this quest is harmful, put the faction that will be harmed by this quest
    protected Settlement _postedAt; //Where this quest was posted.

    #region getters/setters
    public int id {
        get { return _id; }
    }
	public string urlName{
		get { return "[url=" + this._id.ToString() + "_quest]" + _questType.ToString() + "[/url]"; }
	}
    public TaskCreator createdBy {
        get { return _createdBy; }
    }
    public QUEST_TYPE questType {
        get { return _questType; }
    }
    public bool isExpired {
        get { return _isExpired; }
    }
    public bool isAccepted {
        get { return _isAccepted; }
    }
    public bool isWaiting {
        get { return _isWaiting; }
    }
    public Party assignedParty {
        get { return _assignedParty; }
    }
    public TaskAction currentAction {
        get { return _currentAction; }
    }
	public int activeDuration {
		get { return _activeDuration; }
	}
    public Faction targetFaction {
        get { return _targetFaction; }
    }
    public Settlement postedAt {
        get { return _postedAt; }
    }
    #endregion
    /*
     Create a new quest object.
     NOTE: Set daysBeforeDeadline to -1 if quest cannot expire.
         */
    public Quest(TaskCreator createdBy, QUEST_TYPE questType): base (createdBy, TASK_TYPE.QUEST) {
        _id = Utilities.SetID(this);
        _questType = questType;
        //_daysBeforeDeadline = daysBeforeDeadline;
		_activeDuration = 0;
        _questFilters = new List<QuestFilter>();
        _taskLogs = new List<string>();
        //if(daysBeforeDeadline != -1) {
        //    ScheduleDeadline();
        //}
        //_createdBy.AddNewQuest(this);
        if (!(createdBy is ECS.Character)) {
            FactionManager.Instance.AddQuest(this);
        }
    }

    #region virtuals
    /*
     This is called when a quest is posted on the board.
     This will perform the actions needed by the quest, once it is posted,
     such as reserve citizens, setup data, etc.
         */
    public virtual void OnQuestPosted() {}
    /*
     Accept this quest.
     Quests can only be accepted by characters that can be party leaders.
         */
    protected virtual void AcceptQuest(ECS.Character partyLeader) {
        Debug.Log(partyLeader.name + " accepts quest " + questType.ToString() + " on " + Utilities.GetDateString(GameManager.Instance.Today()));
        _isAccepted = true;
        AddNewLog(partyLeader.name + " accepted this quest.");
        if (partyLeader.party != null) {
            //if character already has a party, assign that party to this quest
            AssignPartyToQuest(partyLeader.party);
        } else {
            //Character that accepts this quest must now create a party
            CreateNewPartyForQuest(partyLeader);
        }
        //UnScheduleDeadline();
        SchedulePartyExpiration();
        if (onTaskInfoChanged != null) {
            onTaskInfoChanged();
        }
        if(onQuestAccepted != null) {
            onQuestAccepted();
        }
    }
    protected virtual void EndQuest(TASK_STATUS result) {
        if (!_isDone) {
            _taskStatus = result;
			if(_currentAction != null){
				_currentAction.onTaskActionDone = null;
			}
            switch (result) {
                case TASK_STATUS.SUCCESS:
                    QuestSuccess();
                    break;
                case TASK_STATUS.FAIL:
                    QuestFail();
                    break;
                case TASK_STATUS.CANCEL:
                    QuestCancel();
                    break;
                default:
                    break;
            }
            CheckForInternationalIncident();
        }
    }
    protected virtual void QuestSuccess() {
		_isDone = true;
		_createdBy.RemoveQuest(this);
		if (_postedAt != null) {
			_postedAt.RemoveQuestFromBoard(this);//Remove quest from quest board
		}
		if (_currentAction != null) {
			_currentAction.ActionDone (TASK_ACTION_RESULT.SUCCESS);
		}
        //RetaskParty(_assignedParty.OnReachNonHostileSettlementAfterQuest);
        GiveRewards();
        _assignedParty.OnQuestEnd();
    }
    protected virtual void QuestFail() {
        _isAccepted = false;
        _createdBy.RemoveQuest(this);
        if (_postedAt != null) {
            _postedAt.RemoveQuestFromBoard(this);//Remove quest from quest board
        }
        if (_currentAction != null) {
			_currentAction.ActionDone (TASK_ACTION_RESULT.FAIL);
		}
        _assignedParty.OnQuestEnd();
        //RetaskParty(_assignedParty.OnReachNonHostileSettlementAfterQuest);
    }
    protected virtual void QuestCancel() {
		_isAccepted = false;
		if (_currentAction != null) {
			_currentAction.ActionDone (TASK_ACTION_RESULT.CANCEL);
		}
        //RetaskParty(_assignedParty.partyLeader.OnReachNonHostileSettlementAfterQuest);
        _assignedParty.OnQuestEnd();
        ResetQuestValues ();
    }
	//Some variables in a specific quest must be reset so if other party will get the quest it will not have any values
	protected virtual void ResetQuestValues(){
        _isAccepted = false;
        _isWaiting = false;
        _isExpired = false;
        _assignedParty = null;
        _currentAction = null;
        _questLine.Clear();
    }
    /*
     Construct the list of quest actions that the party will perform.
         */
    protected virtual void ConstructQuestLine() { _questLine = new Queue<TaskAction>(); }
	internal virtual void Result(bool isSuccess){}
    internal virtual HexTile GetQuestTargetLocation() {
        return null;
    }
    internal virtual void GiveRewards() {
        if(_assignedParty != null) {
            QuestTypeSetup qts = FactionManager.Instance.GetQuestTypeSetup(this.questType);
            if(qts != null) {
                QuestReward questReward = qts.questRewards;
                //Give rewards to the characters
                for (int i = 0; i < _assignedParty.partyMembers.Count; i++) {
                    ECS.Character currMember = _assignedParty.partyMembers[i];
                    if (_assignedParty.IsCharacterLeaderOfParty(currMember)) {
                        //CurrMember is Party Leader
                        currMember.AdjustGold(questReward.leaderGoldReward);
                        currMember.AdjustGold(questReward.leaderPrestigeReward);
                    } else {
                        //CurrMember is a member
                        currMember.AdjustGold(questReward.membersGoldReward);
                        currMember.AdjustGold(questReward.membersPrestigeReward);
                    }
                }
            }
        }
    }
    public virtual bool CanAcceptQuest(ECS.Character character) {
        if (_isAccepted) {
            return false;
        }
        for (int i = 0; i < _questFilters.Count; i++) {
            QuestFilter currFilter = _questFilters[i];
            if (!currFilter.MeetsRequirements(character)) {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region overrides
    public override void PerformTask(ECS.Character character) {
        base.PerformTask(character);
        AcceptQuest(character);
    }
    public override void EndTask(TASK_STATUS taskResult) {
        EndQuest(taskResult);
    }
    public override void TaskSuccess() {
        QuestSuccess();
    }
    public override void TaskCancel() {
        QuestCancel();
    }
    public override void TaskFail() {
        QuestFail();
    }
    #endregion

    #region Deadline
    public void SchedulePartyExpiration() {
        GameDate deadline = GameManager.Instance.Today();
        deadline.AddDays(3);
        SchedulingManager.Instance.AddEntry(deadline, () => QuestExpired());
    }
    private void QuestExpired() {
        Debug.Log(this.questType.ToString() + " has expired on " + Utilities.GetDateString(GameManager.Instance.Today()));
        _isExpired = true;
        //Quest has reached the expiry date
        if (_isAccepted) {
            if (!_isWaiting) {//Check first if all the party members have arrived
                //Quest has already been accepted, and all registered party members have arrived
                StartQuestLine();
            }
        } else {
            //Quest has not been accepted, remove quest from quest log
            _createdBy.RemoveQuest(this);
        }
    }
    protected void ScheduleQuestEnd(int days, TASK_STATUS result) {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(days);
        SchedulingManager.Instance.AddEntry(dueDate, () => EndQuest(result));
    }
    protected void ScheduleQuestAction(int days, Action action) {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(days);
        SchedulingManager.Instance.AddEntry(dueDate, () => action());
    }
    #endregion

    #region Quest Line
    public void StartQuestLine() {
        if(_assignedParty != null) {
            _assignedParty.SetOpenStatus(false); //set party to not accept party members
        }
        Debug.Log("Start " + this.questType.ToString() + " Quest!");
        ConstructQuestLine();
        PerformNextQuestAction();
        if (onTaskInfoChanged != null) {
            onTaskInfoChanged();
        }
    }
    internal void PerformNextQuestAction() {
        _currentAction = _questLine.Dequeue();
        if(_assignedParty == null) {
            if(_createdBy is ECS.Character) {
                _currentAction.DoAction((ECS.Character)_createdBy);
            }
        } else {
            _currentAction.DoAction(_assignedParty.partyLeader);
        }
        
    }
    internal void RepeatCurrentAction() {
        if(_currentAction != null) {
            _currentAction.DoAction(_assignedParty.partyLeader);
        }
    }
    /*
     Turn in this quest, This will end this quest and give the rewards to
     the characters if any.
         */
    public void TurnInQuest(TASK_STATUS taskResult) {
        EndTask(taskResult);
    }
    #endregion

    #region Party
    /*
     Create a new party for this quest.
     The created party will automatically be assigned to this quest.
         */
    internal Party CreateNewPartyForQuest(ECS.Character partyLeader) {
        Party newParty = new Party(partyLeader);
        //newParty.onPartyFull = OnPartyFull;
        AssignPartyToQuest(newParty);
        return newParty;
    }
    /*
     Assign a party to this quest.
         */
    internal void AssignPartyToQuest(Party party) {
        _assignedParty = party;
        _assignedParty.SetCurrentTask(this);
        AddNewLog("Party " + party.name + " is now assigned to this quest.");
        if (_assignedParty.partyLeader.avatar == null) {
            _assignedParty.partyLeader.CreateNewAvatar();//Characters that have accepted a Quest should have icon already even if they are still forming party in the city
            _assignedParty.SetAvatar(_assignedParty.partyLeader.avatar);
        }
        //onQuestEnd += _assignedParty.OnQuestEnd;
        if (_assignedParty.isFull) {
            //Party is already full, check party members
            CheckPartyMembers();
        } else {
            _assignedParty.SetOpenStatus(true); //Set party as open to members
            //_assignedParty.onPartyFull = OnPartyFull;
            _assignedParty.InviteCharactersOnLocation(CHARACTER_ROLE.ADVENTURER, _assignedParty.specificLocation);
        }
    }
    /*
     This will check which characters will choose to leave
     the party. 
         */
	protected void RetaskParty(Action action) {
        //Make party go to nearest non hostile settlement after a quest
		_assignedParty.GoToNearestNonHostileSettlement(() => action());
    }
    /*
     Make the assigned party go back to the settlement that
     gave the quest.
         */
    internal void GoBackToQuestGiver(TASK_STATUS taskResult) {
        _assignedParty.GoBackToQuestGiver(taskResult);
    }
    internal void CheckPartyMembers() {
        if (_assignedParty.isFull) { //if the assigned party is full
            if (_assignedParty.AreAllPartyMembersPresent()) { //check if all the party members are present
                //if they are all present, start the quest
                SetWaitingStatus(false);
                StartQuestLine();
            } else {
                SetWaitingStatus(true);
            }
        } else { //otherwise, if the party is not yet full
            if (_isExpired) { //check if the quest has expired
                //if the quest has expired
                //check if all the registered members are present
                if (_assignedParty.AreAllPartyMembersPresent()) {
                    //if they are present, start the quest.
                    SetWaitingStatus(false);
                    StartQuestLine();
                } else {
                    //if not, wait for them
                    SetWaitingStatus(true);
                }
            } else {
                //if has not expired, and the party is not yet full, wait for participants until expiration
                if (_assignedParty.AreAllPartyMembersPresent()) {
                    SetWaitingStatus(false);
                }
            }
        }
    }
    internal void SetWaitingStatus(bool isWaiting) {
        _isWaiting = isWaiting;
    }
    #endregion

    #region International Incidents
    protected void CheckForInternationalIncident() {
        //Check first if this quest is targetting any faction, and if it is harmful
        if(_targetFaction != null && FactionManager.Instance.IsQuestHarmful(this.questType)) {
            Faction currLocationOwner = _assignedParty.currLocation.region.owner;
            if (currLocationOwner != null && currLocationOwner.id != _assignedParty.partyLeader.faction.id) { //the party is at a region not owned by his/her faction
                if (currLocationOwner.id == _targetFaction.id) {//the party is at a region owned by his/her target faction
                    if (_assignedParty.partyLeader.faction != null) {
                        FactionManager.Instance.InternationalIncidentOccured(_targetFaction, _assignedParty.partyLeader.faction, INTERNATIONAL_INCIDENT_TYPE.HARMFUL_QUEST, this);
                    }
                }
            }
        }
    }
    #endregion

    #region Utilities
    public void SetSettlement(Settlement postedAt) {
        _postedAt = postedAt;
    }
    #endregion
}
