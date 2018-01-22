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

public class Quest {

    protected delegate void OnQuestAccepted();
    protected OnQuestAccepted onQuestAccepted;

    protected delegate void OnQuestEnd(QUEST_RESULT result);
    protected OnQuestEnd onQuestEnd;

    protected QuestCreator _createdBy;
    protected QUEST_TYPE _questType;
    protected int _daysBeforeDeadline;
    protected bool _isExpired;
    protected bool _isAccepted;
    protected bool _isDone;
    protected bool _isWaiting;
    protected Party _assignedParty;
    protected List<QuestFilter> _questFilters;
    protected QuestAction _currentAction;
    protected QUEST_RESULT _questResult;
	protected int _activeDuration;

    protected Queue<QuestAction> _questLine;

    private GameDate _deadline;
    private Action _deadlineAction;

    #region getters/setters
    public QUEST_TYPE questType {
        get { return _questType; }
    }
    public bool isExpired {
        get { return _isExpired; }
    }
    public bool isAccepted {
        get { return _isAccepted; }
    }
	public bool isDone {
		get { return _isDone; }
	}
    public bool isWaiting {
        get { return _isWaiting; }
    }
    public Party assignedParty {
        get { return _assignedParty; }
    }
    public QuestAction currentAction {
        get { return _currentAction; }
    }
    public QUEST_RESULT questResult {
        get { return _questResult; }
    }
	public int activeDuration {
		get { return _activeDuration; }
	}
    #endregion
    /*
     Create a new quest object.
     NOTE: Set daysBeforeDeadline to -1 if quest cannot expire.
         */
    public Quest(QuestCreator createdBy, int daysBeforeDeadline, QUEST_TYPE questType) {
        _createdBy = createdBy;
        _questType = questType;
        _daysBeforeDeadline = daysBeforeDeadline;
		_activeDuration = 0;
        _questFilters = new List<QuestFilter>();
        //if(daysBeforeDeadline != -1) {
        //    ScheduleDeadline();
        //}
        //_createdBy.AddNewQuest(this);
    }

    #region virtuals
    /*
     Accept this quest.
     Quests can only be accepted by characters that can be party leaders.
         */
    public virtual void AcceptQuest(ECS.Character partyLeader) {
        Debug.Log(partyLeader.name + " accepts quest " + questType.ToString() + " on " + Utilities.GetDateString(GameManager.Instance.Today()));
        _isAccepted = true;

        if (partyLeader.party != null) {
            //if character already has a party, assign that party to this quest
            AssignPartyToQuest(partyLeader.party);
        } else {
            //Character that accepts this quest must now create a party
            CreateNewPartyForQuest(partyLeader);
        }
        //UnScheduleDeadline();
        SchedulePartyExpiration();
        if(onQuestAccepted != null) {
            onQuestAccepted();
        }
    }
    /*
     Add a new character as a party member of this quest.
         */
    public virtual void JoinQuest(ECS.Character member) {
        _assignedParty.AddPartyMember(member);
        member.SetCurrentQuest(this);
    }
    /*
     This is the action done, when the party assigned to this quest is full.
     Full meaning a number of characters have registered to join this quest,
     but they might not be with the party leader yet, if they are not, wait for them to arrive,
     otherwise, start the quest immediately.
         */
    protected virtual void OnPartyFull(Party party) {
        //When the party is full, check if all the characters have arrived at the party leaders location
        //if not wait for them to arrive before starting the quest.
        CheckPartyMembers();
    }
    internal virtual void EndQuest(QUEST_RESULT result) {
        if (!_isDone) {
			_isDone = true;
			_questResult = result;
            if(onQuestEnd != null) {
                onQuestEnd(result);
            }
			if(_currentAction != null){
				_currentAction.onQuestActionDone = null;
			}
			_createdBy.RemoveQuest(this);
//			if(_assignedParty != null){
//				for (int i = 0; i < _assignedParty.partyMembers.Count; i++) {
//					_assignedParty.partyMembers [i].SetCurrentQuest (null);
//				}
//			}

            switch (result) {
                case QUEST_RESULT.SUCCESS:
                    QuestSuccess();
                    break;
                case QUEST_RESULT.FAIL:
                    QuestFail();
                    break;
                case QUEST_RESULT.CANCEL:
                    QuestCancel();
                    break;
                default:
                    break;
            }
        }
    }
    internal virtual void QuestSuccess() {
		if (_currentAction != null) {
			_currentAction.ActionDone (QUEST_ACTION_RESULT.SUCCESS);
		}
		RetaskParty (_assignedParty.partyLeader.OnReachNonHostileSettlementAfterQuest);
    }
    internal virtual void QuestFail() {
		if (_currentAction != null) {
			_currentAction.ActionDone (QUEST_ACTION_RESULT.FAIL);
		}
		RetaskParty(_assignedParty.partyLeader.OnReachNonHostileSettlementAfterQuest);
    }
    internal virtual void QuestCancel() {
        _questResult = QUEST_RESULT.CANCEL;
		_isAccepted = false;
		if (_currentAction != null) {
			_currentAction.ActionDone (QUEST_ACTION_RESULT.CANCEL);
		}
		RetaskParty(_assignedParty.partyLeader.OnReachNonHostileSettlementAfterQuest);
		ResetQuestValues ();
    }
	//Some variables in a specific quest must be reset so if other party will get the quest it will not have any values
	protected virtual void ResetQuestValues(){}
    /*
     Construct the list of quest actions that the party will perform.
         */
    protected virtual void ConstructQuestLine() { _questLine = new Queue<QuestAction>(); }
    #endregion

    public virtual bool CanAcceptQuest(ECS.Character character) {
		if(_isAccepted){
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

    #region Deadline
    public void ScheduleDeadline() {
        if (_daysBeforeDeadline != -1) {
            GameDate deadline = GameManager.Instance.Today();
            deadline.AddDays(_daysBeforeDeadline);
            _deadline = deadline;
            _deadlineAction = QuestExpired;
            SchedulingManager.Instance.AddEntry(deadline, () => _deadlineAction());
        }
    }
    public void UnScheduleDeadline() {
        if(_deadlineAction != null) {
            SchedulingManager.Instance.RemoveSpecificEntry(_deadline.month, _deadline.day, _deadline.year, _deadlineAction);
        }
    }
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
    protected void ScheduleQuestEnd(int days, QUEST_RESULT result) {
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
    #endregion

    #region Party
    /*
     Create a new party for this quest.
     The created party will automatically be assigned to this quest.
         */
    internal Party CreateNewPartyForQuest(ECS.Character partyLeader) {
        Party newParty = new Party(partyLeader);
        newParty.onPartyFull = OnPartyFull;
        AssignPartyToQuest(newParty);
        return newParty;
    }
    /*
     Assign a party to this quest.
         */
    internal void AssignPartyToQuest(Party party) {
        _assignedParty = party;
        _assignedParty.SetCurrentQuest(this);
        if (_assignedParty.partyLeader.avatar == null) {
            _assignedParty.partyLeader.CreateNewAvatar();//Characters that have accepted a Quest should have icon already even if they are still forming party in the city
            _assignedParty.SetAvatar(_assignedParty.partyLeader.avatar);
        }
        onQuestEnd += _assignedParty.OnQuestEnd;
        if (_assignedParty.isFull) {
            //Party is already full, check party members
            CheckPartyMembers();
        } else {
            _assignedParty.SetOpenStatus(true); //Set party as open to members
            _assignedParty.onPartyFull = OnPartyFull;
            _assignedParty.InviteCharactersOnTile(CHARACTER_ROLE.ADVENTURER, _assignedParty.partyLeader.currLocation);
        }
    }
    /*
     This will check which characters will choose to leave
     the party. 
         */
	protected void RetaskParty(Action action) {
        //Make party go to nearest non hostile settlement after a quest
        //_assignedParty.SetCurrentQuest(null);
        _assignedParty.onPartyFull = null;
		_assignedParty.partyLeader.GoToNearestNonHostileSettlement(() => action());
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
}
