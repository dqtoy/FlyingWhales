using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerActionsUI : MonoBehaviour {
	public GameObject characterTaskButtonGO;
	public UIGrid buttonsGrid;

	private ILocation location;
	private List<CharacterTaskButton> taskButtons = new List<CharacterTaskButton>();

	public void ShowPlayerActionsUI(ILocation location){
		this.location = location;
		OnShow ();
	}
	public void HidePlayerActionsUI(){
		this.gameObject.SetActive (false);
	}

	private void OnShow(){
		ECS.Character character = UIManager.Instance.characterInfoUI.currentlyShowingCharacter;
		if(character == null){
			return;
		}
		List<CharacterTask> tasksAvailable = character.GetAllPossibleTasks (this.location);
		if(tasksAvailable.Count > taskButtons.Count){
			for (int i = 0; i < tasksAvailable.Count; i++) {
				if(i < taskButtons.Count){
					taskButtons [i].SetTask (tasksAvailable [i]);
					taskButtons [i].SetLocation (this.location);
					taskButtons [i].gameObject.SetActive (true);
				}else{
					CreateButton (tasksAvailable [i]);
				}
			}
		}else{
			for (int i = 0; i < taskButtons.Count; i++) {
				if(i < tasksAvailable.Count){
					taskButtons [i].SetTask (tasksAvailable [i]);
					taskButtons [i].SetLocation (this.location);
					taskButtons [i].gameObject.SetActive (true);
				}else{
					taskButtons [i].gameObject.SetActive (false);
				}
			}
		}
		this.gameObject.SetActive (true);
		buttonsGrid.Reposition ();
	}
	private void CreateButton(CharacterTask task){
		GameObject characterTaskButton = (GameObject)GameObject.Instantiate (characterTaskButtonGO, buttonsGrid.transform);
		characterTaskButton.transform.localScale = Vector3.one;
		characterTaskButton.transform.localPosition = Vector3.zero;

		CharacterTaskButton taskButton = characterTaskButton.GetComponent<CharacterTaskButton> ();
		taskButton.SetTask (task);
		taskButton.SetLocation (this.location);
		taskButtons.Add (taskButton);
	}
}
