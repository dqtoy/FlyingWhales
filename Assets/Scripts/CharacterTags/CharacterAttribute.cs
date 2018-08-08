using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterAttribute {
	protected ECS.Character _character;
	protected string _name;
	protected ATTRIBUTE _attribute;
	//protected List<CharacterTask> _tagTasks;
	protected StatsModifierPercentage _statsModifierPercentage;
	protected bool _isRemoved;

	#region getters/setters
	public string name {
		get { return _name; }
	}
	public ATTRIBUTE attribute {
		get { return _attribute; }
	}
	public ECS.Character character{
		get { return _character; }
	}
	//public List<CharacterTask> tagTasks {
	//	get { return _tagTasks; }
	//}
	public StatsModifierPercentage statsModifierPercentage {
		get { return _statsModifierPercentage; }
	}
	public bool isRemoved {
		get { return _isRemoved; }
	}
	#endregion

	public CharacterAttribute(ECS.Character character, ATTRIBUTE attribute) {
		_character = character;
        _attribute = attribute;
        _name = Utilities.NormalizeStringUpperCaseFirstLetters (_attribute.ToString ());
		//_tagTasks = new List<CharacterTask> ();
		_statsModifierPercentage = new StatsModifierPercentage ();
		_isRemoved = false;
	}

	#region Virtuals
	public virtual void Initialize(){}
	//public virtual void AddTaskWeightsFromTags(WeightedDictionary<CharacterTask> tasks) {
	//	for (int i = 0; i < _tagTasks.Count; i++) {
	//		CharacterTask currTask = _tagTasks[i];
	//		if(currTask.forPlayerOnly || !currTask.AreConditionsMet(_character)){
	//			continue;
	//		}
	//		tasks.AddElement (currTask, currTask.GetSelectionWeight(_character));
	//	}
	//}
    /*
     What should happen when a tag is removed
         */
    public virtual void OnRemoveTag() {
		_isRemoved = true;
	}
    public virtual void PerformDailyAction() {}
	#endregion

    //public CharacterTask GetTask(TASK_TYPE taskType) {
    //    for (int i = 0; i < _tagTasks.Count; i++) {
    //        CharacterTask currTask = _tagTasks[i];
    //        if (currTask.taskType == taskType) {
    //            return currTask;
    //        }
    //    }
    //    return null;
    //}
}
