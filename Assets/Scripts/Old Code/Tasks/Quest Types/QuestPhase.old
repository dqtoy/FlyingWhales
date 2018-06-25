using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestPhase {
    private Quest _sourceQuest;
    private List<CharacterTask> _tasks; //tasks in this phase that NEEDS to be completed before proceeding to the next phase
    private List<QuestPhaseRequirement> _phaseRequirements;

    private string _phaseName;

    #region getters/setters
    public string phaseName {
        get { return GetPhaseName(); }
    }
    public List<CharacterTask> tasks {
        get { return _tasks; }
    }
    #endregion

    public QuestPhase(Quest sourceQuest, string phaseName = "") {
        _sourceQuest = sourceQuest;
        _phaseName = phaseName;
        _tasks = new List<CharacterTask>();
        _phaseRequirements = new List<QuestPhaseRequirement>();
    }

    protected string GetPhaseName() {
        if (!string.IsNullOrEmpty(_phaseName)) {
            return _phaseName;
        }
        return "Phase " + _sourceQuest.phases.IndexOf(this).ToString();
    }
    /*
     This is called when a character reaches this phase
         */
    public void OnPhaseActive(ECS.Character character) {
        //add all requirements to the characters quest data
        for (int i = 0; i < _phaseRequirements.Count; i++) {
            QuestPhaseRequirement currRequirement = _phaseRequirements[i];
            character.questData.AddPhaseRequirement(currRequirement.Clone() as QuestPhaseRequirement);
        }
    }
    public void AddTask(CharacterTask task) {
        if (!_tasks.Contains(task)) {
            _tasks.Add(task);
        }
    }
    public void AddPhaseRequirement(QuestPhaseRequirement requirement) {
        _phaseRequirements.Add(requirement);
    }
}
