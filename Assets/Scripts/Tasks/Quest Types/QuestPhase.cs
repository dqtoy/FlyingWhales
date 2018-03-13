using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestPhase {
    private Quest _sourceQuest;
    private List<CharacterTask> _tasks;
    private string _phaseName;

    #region getters/setters
    public string phaseName {
        get { return GetPhaseName(); }
    }
    public List<CharacterTask> tasks {
        get { return GetTasks(); }
    }
    #endregion

    public QuestPhase(Quest sourceQuest, string phaseName = "") {
        _sourceQuest = sourceQuest;
        _phaseName = phaseName;
        _tasks = new List<CharacterTask>();
    }

    protected string GetPhaseName() {
        if (!string.IsNullOrEmpty(_phaseName)) {
            return _phaseName;
        }
        return "Phase " + _sourceQuest.phases.IndexOf(this).ToString();
    }
    public void AddTask(CharacterTask task) {
        if (!_tasks.Contains(task)) {
            _tasks.Add(task);
        }
    }
    private List<CharacterTask> GetTasks() {
        List<CharacterTask> tasks = new List<CharacterTask>();
        for (int i = 0; i < _tasks.Count; i++) {
            CharacterTask currTaskToCopy = _tasks[i];
            tasks.Add(currTaskToCopy.CloneTask());
        }
        return tasks;
    }
}
