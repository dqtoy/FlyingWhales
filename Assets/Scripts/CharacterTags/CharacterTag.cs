using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterTag {
	protected ECS.Character _character;
	protected string _tagName;
	protected CHARACTER_TAG _tagType;
	protected List<CharacterTask> _tagTasks;
	protected StatsModifierPercentage _statsModifierPercentage;
	protected bool _isRemoved;

	#region getters/setters
	public string tagName {
		get { return _tagName; }
	}
	public CHARACTER_TAG tagType {
		get { return _tagType; }
	}
	public ECS.Character character{
		get { return _character; }
	}
	public List<CharacterTask> tagTasks {
		get { return _tagTasks; }
	}
	public StatsModifierPercentage statsModifierPercentage {
		get { return _statsModifierPercentage; }
	}
	public bool isRemoved {
		get { return _isRemoved; }
	}
	#endregion

	public CharacterTag(ECS.Character character, CHARACTER_TAG tagType){
		_character = character;
		_tagType = tagType;
		_tagName = Utilities.NormalizeStringUpperCaseFirstLetters (_tagType.ToString ());
		_tagTasks = new List<CharacterTask> ();
		_statsModifierPercentage = new StatsModifierPercentage ();
		_isRemoved = false;
	}

	#region Virtuals
	public virtual void Initialize(){}
	public virtual void AddTaskWeightsFromTags(WeightedDictionary<CharacterTask> tasks) {
		for (int i = 0; i < _tagTasks.Count; i++) {
			CharacterTask currTask = _tagTasks[i];
			if(currTask.forPlayerOnly || !currTask.AreConditionsMet(_character)){
				continue;
			}
			tasks.AddElement (currTask, currTask.GetTaskWeight(_character));
		}
	}
    /*
     What should happen when a tag is removed
         */
    public virtual void OnRemoveTag() {
		_isRemoved = true;
	}
	#endregion
}
