using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestData {

    private Quest _activeQuest;
    private int _currentPhase;
    private ECS.Character _owner;

    private List<CharacterTask> _tasks;
    private List<QuestPhaseRequirement> _advancementRequirements;

    #region getters/setters
    public Quest activeQuest {
        get { return _activeQuest; }
    }
    public List<CharacterTask> tasks {
        get { return _tasks; }
    }
	public int currentPhase {
		get { return _currentPhase; }
	}
    #endregion

    public QuestData(ECS.Character owner) {
        _owner = owner;
        _advancementRequirements = new List<QuestPhaseRequirement>();
    }

    public void SetActiveQuest(Quest quest) {
        _activeQuest = quest;
        if (quest == null) {
            DeactivateAllPhaseRequirements();
            //quest is being set to null, it means the quest is no longer available or the character chose to leave the quest
            //if (_owner.currentAction != null && tasks.Contains(_owner.currentAction)) {
            //    _owner.currentAction.EndTask(TASK_STATUS.CANCEL); //cancel the characters current task if it comes from the list of tasks granted by his/her active quest
            //}
        }
        SetQuestPhase(0);
    }
    public void SetQuestPhase(int phase) {
        _currentPhase = phase;
        if (_activeQuest != null) {
            QuestPhase questPhase = GetQuestPhase();
            _tasks = new List<CharacterTask>(questPhase.tasks);
            DeactivateAllPhaseRequirements();
            _advancementRequirements.Clear();
            questPhase.OnPhaseActive(_owner);
        } else {
            _tasks.Clear();
        }
    }
    public void AdvanceToNextPhase() {
        if (_activeQuest.phases.Count > _currentPhase + 1) {
            Debug.Log("Setting " + _owner.name + "'s quest phase to " + (_currentPhase + 1).ToString());
            //there is still a next phase, advance to the next phase
            SetQuestPhase(_currentPhase + 1);
        } else {
            Debug.Log("All phases have been finished! Quest: " + activeQuest.questName + " has ended!");
            //there are no more phases, end the quest
            EndQuest(TASK_STATUS.SUCCESS);
        }
        UIManager.Instance.UpdateQuestsSummary();
    }
    public QuestPhase GetQuestPhase() {
        return _activeQuest.phases[_currentPhase];
    }
    public void EndQuest(TASK_STATUS taskStatus) {
        _activeQuest.EndQuest(taskStatus, _owner);
    }
    public void AddQuestTasksToWeightedDictionary(WeightedDictionary<CharacterTask> actionWeights) {
        for (int i = 0; i < _tasks.Count; i++) {
            CharacterTask currTask = _tasks[i];
            if (!currTask.isDone && !currTask.forPlayerOnly && currTask.AreConditionsMet(_owner)) {
                actionWeights.AddElement(currTask, currTask.GetSelectionWeight(_owner));
            }
        }
    }
    public void AddPhaseRequirement(QuestPhaseRequirement requirement) {
        _advancementRequirements.Add(requirement);
        requirement.ActivateRequirement(_owner);
    }
    public void CheckPhaseAdvancement() {
        for (int i = 0; i < _advancementRequirements.Count; i++) {
            QuestPhaseRequirement currRequirement = _advancementRequirements[i];
            if (!currRequirement.isRequirementMet) {
                return; //there is a requirement that has not yet been met
            }
        }
        Debug.Log("All requirements for " + _owner.name + "'s quest (" + activeQuest.questName + "). Quest phase: " + GetQuestPhase().phaseName);
        AdvanceToNextPhase(); //all requirements have been met, advance to next phase
    }
    private void DeactivateAllPhaseRequirements() {
        for (int i = 0; i < _advancementRequirements.Count; i++) {
            QuestPhaseRequirement currRequirement = _advancementRequirements[i];
            currRequirement.DeactivateRequirement();
        }
    }
}
