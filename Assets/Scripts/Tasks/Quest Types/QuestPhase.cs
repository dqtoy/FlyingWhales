using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestPhase {
    private Quest _sourceQuest;
    private List<CharacterTask> _importantTasks; //tasks in this phase that NEEDS to be completed before proceeding to the next phase
    private List<CharacterTask> _unimportantTasks; //tasks in this phase that are just for helping the quest doer to achieve his/her goal (i.e. tasks that are not required to be done.)

    private string _phaseName;

    #region getters/setters
    public string phaseName {
        get { return GetPhaseName(); }
    }
    public List<CharacterTask> importantTasks {
        get { return GetImportantTasks(); }
    }
    public List<CharacterTask> untimportantTasks {
        get { return GetUnimportantTasks(); }
    }
    #endregion

    public QuestPhase(Quest sourceQuest, string phaseName = "") {
        _sourceQuest = sourceQuest;
        _phaseName = phaseName;
        _importantTasks = new List<CharacterTask>();
        _unimportantTasks = new List<CharacterTask>();
    }

    protected string GetPhaseName() {
        if (!string.IsNullOrEmpty(_phaseName)) {
            return _phaseName;
        }
        return "Phase " + _sourceQuest.phases.IndexOf(this).ToString();
    }
    public void AddTask(CharacterTask task, bool isImportant = true) {
        if (isImportant) {
            if (!_importantTasks.Contains(task)) {
                _importantTasks.Add(task);
            }
        } else {
            if (!_unimportantTasks.Contains(task)) {
                _unimportantTasks.Add(task);
            }
        }
    }
    private List<CharacterTask> GetImportantTasks() {
        List<CharacterTask> importantTasks = new List<CharacterTask>();
        for (int i = 0; i < _importantTasks.Count; i++) {
            CharacterTask currTaskToCopy = _importantTasks[i];
            importantTasks.Add(currTaskToCopy.CloneTask());
        }
        return importantTasks;
    }
    private List<CharacterTask> GetUnimportantTasks() {
        List<CharacterTask> unimportantTasks = new List<CharacterTask>();
        for (int i = 0; i < _unimportantTasks.Count; i++) {
            CharacterTask currTaskToCopy = _unimportantTasks[i];
            unimportantTasks.Add(currTaskToCopy.CloneTask());
        }
        return unimportantTasks;
    }
}
