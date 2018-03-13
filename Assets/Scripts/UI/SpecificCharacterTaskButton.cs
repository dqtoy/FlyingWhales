using UnityEngine;
using System.Collections;

public class SpecificCharacterTaskButton : MonoBehaviour {
	public CharacterTask task;
	public object target;
	public UILabel btnLabel;

	public void SetTask (CharacterTask task){
		this.task = task;
	}
	public void SetTarget (object target){
		this.target = target;
		ChangeButtonText ();
	}

	private void ChangeButtonText(){
		string text = string.Empty;
		if(this.target is ECS.Character){
			text = ((ECS.Character)this.target).name;
		}else if(this.target is ECS.Item){
			text = ((ECS.Item)this.target).itemName;
		} else if (this.target is Quest) {
            text = ((Quest)this.target).questName;
        }
        btnLabel.text = text;
	}

	void OnClick(){
		if(UICamera.currentTouchID == -1){
			ClickAction ();
		}
	}

	private void ClickAction(){
		ECS.Character character = UIManager.Instance.characterInfoUI.activeCharacter;
		if(character == null){
			UIManager.Instance.HidePlayerActions ();
			return;
		}
		if(character.avatar != null && character.avatar.isMovingToHex){
			character.avatar.SetQueuedAction (() => ClickAction ());
			UIManager.Instance.HidePlayerActions ();
			return;
		}
		if(character.currentTask != null){
			character.currentTask.SetIsHalted (true);
		}
		task.SetSpecificTarget (target);
		task.OnChooseTask (character);
		UIManager.Instance.HidePlayerActions ();
//		task.PerformTask ();
	}
}
