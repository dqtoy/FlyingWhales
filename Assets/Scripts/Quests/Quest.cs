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

    protected QuestCreator _createdBy;
    protected QUEST_TYPE _questType;
    protected int _daysBeforeDeadline;
    protected bool _isExpired;
    protected bool _isAccepted;
    protected bool _isDone;
    protected Party _assignedParty;
    protected int _maxPartyMembers;
    protected List<QuestFilter> _questFilters;
    protected QuestAction _currentAction;
    protected QUEST_RESULT _questResult;

    protected Queue<QuestAction> _questLine;

    #region getters/setters
    public QUEST_TYPE questType {
        get { return _questType; }
    }
    public bool isAccepted {
        get { return _isAccepted; }
    }
    #endregion
    /*
     Create a new quest object.
     NOTE: Set daysBeforeDeadline to -1 if quest cannot expire.
         */
    public Quest(QuestCreator createdBy, int daysBeforeDeadline, int maxPartyMembers, QUEST_TYPE questType) {
        _createdBy = createdBy;
        _questType = questType;
        _daysBeforeDeadline = daysBeforeDeadline;
        _maxPartyMembers = maxPartyMembers;
        if(daysBeforeDeadline != -1) {
            ScheduleDeadline();
        }
        _createdBy.AddNewQuest(this);
    }

    #region virtuals
    /*
     Accept this quest.
     Quests can only be accepted by characters that can be party leaders.
         */
    public virtual void AcceptQuest(ECS.Character partyLeader) {
        _isAccepted = true;
        //ECS.Character that accepts this quest must now create a party
        Party newParty = new Party(partyLeader, _maxPartyMembers);
        newParty.onPartyFull += OnPartyFull;
        _assignedParty = newParty;

        partyLeader.SetCurrentQuest(this);

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
    protected virtual void OnPartyFull(Party party) {
        //When the party is full, proceed with the next step
        StartQuestLine();
    }
    protected virtual void EndQuest(QUEST_RESULT result) {
        if (!_isDone) {
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
    protected virtual void QuestSuccess() {
        _isDone = true;
        _questResult = QUEST_RESULT.SUCCESS;
        _createdBy.RemoveQuest(this);
        RetaskParty();
    }
    protected virtual void QuestFail() {
        _isDone = true;
        _questResult = QUEST_RESULT.FAIL;
        _createdBy.RemoveQuest(this);
        RetaskParty();
    }
    protected virtual void QuestCancel() {
        _isDone = true;
        _questResult = QUEST_RESULT.CANCEL;
        _createdBy.RemoveQuest(this);
        RetaskParty();
    }
    /*
     Construct the list of quest actions that the party will perform.
         */
    protected virtual void ConstructQuestLine() { _questLine = new Queue<QuestAction>(); }
    #endregion

    public bool CanAcceptQuest(ECS.Character character) {
        for (int i = 0; i < _questFilters.Count; i++) {
            QuestFilter currFilter = _questFilters[i];
            if (!currFilter.MeetsRequirements(character)) {
                return false;
            }
        }
        return true;
    }

    #region Deadline
    private void ScheduleDeadline() {
        GameDate deadline = GameManager.Instance.Today();
        deadline.AddDays(_daysBeforeDeadline);
        SchedulingManager.Instance.AddEntry(deadline, () => QuestExpired());
    }
    private void QuestExpired() {
        //Quest has reached the expiry date
        if (_isAccepted) {
            //Quest has already been accepted, procceed with the next step regardless of party members
            StartQuestLine();
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
    #endregion

    #region Quest Line
    public void StartQuestLine() {
        ConstructQuestLine();
        PerformNextQuestAction();
    }
    internal void PerformNextQuestAction() {
        _currentAction = _questLine.Dequeue();
        _currentAction.DoAction(_assignedParty.partyLeader);
    }
    internal void RepeatCurrentAction() {
        if(_currentAction != null) {
            _currentAction.DoAction(_assignedParty.partyLeader);
        }
    }
    #endregion

    private void RetaskParty() {
        for (int i = 0; i < _assignedParty.partyMembers.Count; i++) {
            ECS.Character currMember = _assignedParty.partyMembers[i];
            currMember.SetCurrentQuest(null);
            currMember.DetermineAction(); //Retask all party members
        }
    }
}
