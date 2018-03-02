using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerActionsUI : MonoBehaviour {
	public GameObject characterTaskButtonGO;
	public GameObject specificCharacterTaskButtonGO;

	public GameObject specificTargetsGO;
	public UIGrid buttonsGrid;
	public UI2DSprite stretchableBG;

	public UIGrid specificButtonsGrid;
	public UI2DSprite specificStretchableBG;

	private ILocation location;
	private List<CharacterTaskButton> taskButtons = new List<CharacterTaskButton>();
	private List<SpecificCharacterTaskButton> specificTaskButtons = new List<SpecificCharacterTaskButton>();

//	private UIWidget buttonsGridWidget;
	private int defaultWidgetHeight;
	private int characterTaskButtonHeight;
	private int numOfShowingButtons;
	private int specificNumOfShowingButtons;

	void Awake(){
//		buttonsGridWidget = buttonsGrid.GetComponent<UIWidget>();
		characterTaskButtonHeight = characterTaskButtonGO.GetComponent<UI2DSprite>().height;
		defaultWidgetHeight = buttonsGrid.GetComponent<UIWidget>().height;
	}
	public void ShowPlayerActionsUI(ILocation location){
		this.location = location;
		specificTargetsGO.SetActive (false);
		OnShow ();
	}
	public void HidePlayerActionsUI(){
		this.gameObject.SetActive (false);
	}

	private void OnShow(){
		ECS.Character character = UIManager.Instance.characterInfoUI.activeCharacter;
		if(character == null){
			return;
		}
		numOfShowingButtons = 0;
		List<CharacterTask> tasksAvailable = character.GetAllPossibleTasks (this.location);
		if(tasksAvailable.Count > 0){
			if(tasksAvailable.Count > taskButtons.Count){
				for (int i = 0; i < tasksAvailable.Count; i++) {
					if(i < taskButtons.Count){
						taskButtons [i].SetTask (tasksAvailable [i]);
						taskButtons [i].SetLocation (this.location);
						taskButtons [i].gameObject.SetActive (true);
						numOfShowingButtons++;
					}else{
						CreateButton (tasksAvailable [i]);
						numOfShowingButtons++;
					}
				}
			}else{
				for (int i = 0; i < taskButtons.Count; i++) {
					if(i < tasksAvailable.Count){
						taskButtons [i].SetTask (tasksAvailable [i]);
						taskButtons [i].SetLocation (this.location);
						taskButtons [i].gameObject.SetActive (true);
						numOfShowingButtons++;
					}else{
						taskButtons [i].gameObject.SetActive (false);
					}
				}
			}
			this.gameObject.SetActive (true);
			buttonsGrid.Reposition ();
			UpdateGrid();
		}
	}

	public void ShowSpecificTargets(CharacterTask task){
		specificNumOfShowingButtons = 0;
		if(task.specificTargetClassification == "character"){
			List<ECS.Character> characters = CharacterTargets (task);
			if(characters.Count > 0){
				if(characters.Count > specificTaskButtons.Count){
					for (int i = 0; i < characters.Count; i++) {
						if(i < specificTaskButtons.Count){
							specificTaskButtons [i].SetTask (task);
							specificTaskButtons [i].SetTarget (characters [i]);
							specificTaskButtons [i].gameObject.SetActive (true);
							specificNumOfShowingButtons++;
						}else{
							CreateSpecificButton (task, characters [i]);
							specificNumOfShowingButtons++;
						}
					}
				}else{
					for (int i = 0; i < specificTaskButtons.Count; i++) {
						if(i < characters.Count){
							specificTaskButtons [i].SetTask (task);
							specificTaskButtons [i].SetTarget (characters [i]);
							specificTaskButtons [i].gameObject.SetActive (true);
							specificNumOfShowingButtons++;
						}else{
							specificTaskButtons [i].gameObject.SetActive (false);
						}
					}
				}
				specificTargetsGO.SetActive (true);
				specificButtonsGrid.Reposition ();
				UpdateSpecificGrid();
			}
		}else if(task.specificTargetClassification == "item"){
			//TODO: Item Targets
		}
	}

	private List<ECS.Character> CharacterTargets(CharacterTask task){
		List<ECS.Character> characters = new List<ECS.Character> ();
		BaseLandmark landmark = (BaseLandmark)this.location;
		for (int i = 0; i < landmark.charactersAtLocation.Count; i++) {
			ECS.Character character = landmark.charactersAtLocation [i].mainCharacter;
			if(task.CanMeetRequirements(character)){
				characters.Add (character);
			}
		}
		return characters;
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

	private void CreateSpecificButton(CharacterTask task, object target){
		GameObject specificCharacterTaskButton = (GameObject)GameObject.Instantiate (specificCharacterTaskButtonGO, specificButtonsGrid.transform);
		specificCharacterTaskButton.transform.localScale = Vector3.one;
		specificCharacterTaskButton.transform.localPosition = Vector3.zero;

		SpecificCharacterTaskButton taskButton = specificCharacterTaskButton.GetComponent<SpecificCharacterTaskButton> ();
		taskButton.SetTask (task);
		taskButton.SetTarget (target);
		specificTaskButtons.Add (taskButton);
	}

	[ContextMenu("Update Grid")]
	public void UpdateGrid(){
		if(numOfShowingButtons > 0){
			stretchableBG.height = numOfShowingButtons * characterTaskButtonHeight;
		}else{
			stretchableBG.height = defaultWidgetHeight;
		}
	}

	private void UpdateSpecificGrid(){
		if(specificNumOfShowingButtons > 0){
			specificStretchableBG.height = specificNumOfShowingButtons * characterTaskButtonHeight;
		}else{
			specificStretchableBG.height = defaultWidgetHeight;
		}
	}
}
