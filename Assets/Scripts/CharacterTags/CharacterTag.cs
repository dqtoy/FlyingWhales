using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterTag {
	protected ECS.Character _character;
	protected string _tagName;
	protected CHARACTER_TAG _tagType;
	protected List<CharacterTask> _tagTasks;

	#region getters/setters
	public CHARACTER_TAG tagType {
		get { return _tagType; }
	}
	public ECS.Character character{
		get { return _character; }
	}
	public List<CharacterTask> tagTasks {
		get { return _tagTasks; }
	}
	#endregion

	public CharacterTag(ECS.Character character){
		_character = character;
	}
}
