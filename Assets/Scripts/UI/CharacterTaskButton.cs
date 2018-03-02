using UnityEngine;
using System.Collections;

public class CharacterTaskButton : MonoBehaviour {

	public CharacterTask task;
	public UILabel btnLabel;
	private ILocation location;

	public void SetTask (CharacterTask task){
		this.task = task;
		ChangeButtonText ();
	}
	public void SetLocation(ILocation location){
		this.location = location;
	}

	private void ChangeButtonText(){
		btnLabel.text = Utilities.NormalizeString (task.taskType.ToString ());
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
		if (!task.needsSpecificTarget) {
			if (character.avatar != null && character.avatar.isMovingToHex) {
				character.avatar.SetQueuedAction (() => ClickAction ());
				UIManager.Instance.HidePlayerActions ();
				return;
			}
			if (character.currentTask != null) {
				character.currentTask.SetIsHalted (true);
			}
		}
		task.ResetTask ();
		task.SetLocation (this.location);
		if(!task.needsSpecificTarget){
			task.OnChooseTask (character);
			UIManager.Instance.HidePlayerActions ();
		}else{
			UIManager.Instance.playerActionsUI.ShowSpecificTargets (task);
		}
//		task.PerformTask ();
	}
}
