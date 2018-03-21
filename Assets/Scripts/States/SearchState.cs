using UnityEngine;
using System.Collections;
using System;

public class SearchState : State {

    private object searchingFor;
    private Action _afterFindingAction;
    private Action _searchAction;

    public SearchState(CharacterTask parentTask, object searchingFor) : base(parentTask, STATE.SEARCH) {
        this.searchingFor = searchingFor;
        if (searchingFor is string) {
            if ((searchingFor as string).Equals("Heirloom Necklace")) {
				_searchAction = () => SearchForItemInCharacter();
            } else if ((searchingFor as string).Equals("Book of Inimical Incantations")) {
                _searchAction = () => SearchForItemInLandmark();
            } else if ((searchingFor as string).Equals("Neuroctus")) {
                _searchAction = () => SearchForItemInLandmark();
            } else if ((searchingFor as string).Equals("Herbalist")) {
				_searchAction = () => SearchForATag();
				if(_parentTask.parentQuest != null){
					if(_parentTask.parentQuest is PsytoxinCure){
						_afterFindingAction = () => CurePsytoxin();
					}
				}
            }
        }
    }

    #region overrides
    public override bool PerformStateAction() {
        if (!base.PerformStateAction()) { return false; }
        if (_searchAction != null) {
            _searchAction();
        } else {
            throw new Exception("NO SEARCH ACTION!");
        }
        return true;
    }
    #endregion

    #region Search for Item in Character
    private void SearchForItemInCharacter() {
		bool hasBeenFound = false;
		string itemName = (string)searchingFor;
        for (int i = 0; i < _targetLandmark.charactersAtLocation.Count; i++) {
			ECS.Character currCharacter = _targetLandmark.charactersAtLocation[i].mainCharacter;
			if (currCharacter.HasItem(itemName)) {
				hasBeenFound = true;
                //Each day while he is in Search State, if the character with the Heirloom Necklace is in the location then he would successfully perform the action and end the Search State.
                if (_afterFindingAction != null) {
                    _afterFindingAction();
                }
                parentTask.EndTask(TASK_STATUS.SUCCESS);
                break;
                //_assignedCharacter.questData.AdvanceToNextPhase();
            }
        }
		if(!hasBeenFound){
			CheckTraces (itemName, "item");
		}
    }
    #endregion

	#region Search for a Tag
	private void SearchForATag() {
		bool hasBeenFound = false;
		string tagName = (string)searchingFor;
		for (int i = 0; i < _targetLandmark.charactersAtLocation.Count; i++) {
			ECS.Character currCharacter = _targetLandmark.charactersAtLocation[i].mainCharacter;
			if (currCharacter.HasTag(tagName, true)) {
				hasBeenFound = true;
				if (_afterFindingAction != null) {
					_afterFindingAction();
				}
				parentTask.EndTask(TASK_STATUS.SUCCESS);
				break;
			}
		}
		if(!hasBeenFound){
			CheckTraces (tagName, "tag");
		}
	}
	#endregion

    #region Search for Landmark Items
    private void SearchForItemInLandmark() {
		string itemName = (string)searchingFor;
		for (int i = 0; i < _targetLandmark.itemsInLandmark.Count; i++) {
			ECS.Item item = _targetLandmark.itemsInLandmark[i];
			if (item.itemName == itemName) {
				int chance = UnityEngine.Random.Range(0, 100);
				if (chance < item.collectChance) {
					//_assignedCharacter.AddHistory ("Found a " + (string)_searchingFor + "!");
					//_targetLandmark.AddHistory (_assignedCharacter.name +  " found a " + (string)_searchingFor + "!");
					_assignedCharacter.PickupItem(item);
					_targetLandmark.RemoveItemInLandmark(item);
					if (_afterFindingAction != null) {
						_afterFindingAction();
					}
					parentTask.EndTask(TASK_STATUS.SUCCESS);
					break;
				}
			}
		}
    }
    #endregion

    #region Psytoxin Herbalist
    private void CurePsytoxin() {
        ECS.Item meteorite = _assignedCharacter.GetItemInInventory("Meteorite");
        ECS.Item neuroctus = _assignedCharacter.GetItemInInventory("Neuroctus");
        if (meteorite != null && neuroctus != null) {
            _assignedCharacter.ThrowItem(meteorite, false);
            _assignedCharacter.ThrowItem(neuroctus, false);
            string psytoxinLevel = string.Empty;
            if (!_assignedCharacter.RemoveCharacterTag(CHARACTER_TAG.MILD_PSYTOXIN)) {
                if (!_assignedCharacter.RemoveCharacterTag(CHARACTER_TAG.MODERATE_PSYTOXIN)) {
                    _assignedCharacter.RemoveCharacterTag(CHARACTER_TAG.SEVERE_PSYTOXIN);
                    psytoxinLevel = "Severe";
                } else {
                    psytoxinLevel = "Moderate";
                }
            } else {
                psytoxinLevel = "Mild";
            }
            Log cureLog = new Log(GameManager.Instance.Today(), "CharacterTags", "Psytoxin", "cured");
            cureLog.AddToFillers(null, psytoxinLevel, LOG_IDENTIFIER.OTHER);
        }
    }
    #endregion

	#region Traces
	private void CheckTraces(string strSearchingFor, string identifier, bool includeParty = false){
		if (identifier == "tag") {
			for (int i = 0; i < _targetLandmark.characterTraces.Count; i++) {
				ECS.Character currCharacter = _targetLandmark.characterTraces [i];
				if (currCharacter.HasTag (strSearchingFor, includeParty)) {
					_assignedCharacter.AddTraceInfo (currCharacter, strSearchingFor);
					break;
				}
			}
		} else if (identifier == "item") {
			for (int i = 0; i < _targetLandmark.characterTraces.Count; i++) {
				ECS.Character currCharacter = _targetLandmark.characterTraces [i];
				if (currCharacter.HasItem (strSearchingFor)) {
					_assignedCharacter.AddTraceInfo (currCharacter, strSearchingFor);
					break;
				}
			}
		}
	}
	#endregion
}
