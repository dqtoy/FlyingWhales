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
                _searchAction = () => SearchForHeirloomNecklace();
            } else if ((searchingFor as string).Equals("Book of Inimical Incantations")) {
                _searchAction = () => SearchForItemInLandmark();
            } else if ((searchingFor as string).Equals("Neuroctus")) {
                _searchAction = () => SearchForItemInLandmark();
            } else if ((searchingFor as string).Equals("Psytoxin Herbalist")) {
                string[] splitted = ((string)searchingFor).Split(' ');
                _searchAction = () => SearchForATag(splitted[1]);
                _afterFindingAction = () => CurePsytoxin();
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

    #region Find Lost Heir
    private void SearchForHeirloomNecklace() {
        for (int i = 0; i < parentTask.targetLocation.charactersAtLocation.Count; i++) {
            ECS.Character currCharacter = parentTask.targetLocation.charactersAtLocation[i].mainCharacter;
            if (currCharacter.HasItem(searchingFor as string)) {
                //Each day while he is in Search State, if the character with the Heirloom Necklace is in the location then he would successfully perform the action and end the Search State.
                if (_afterFindingAction != null) {
                    _afterFindingAction();
                }
                parentTask.EndTask(TASK_STATUS.SUCCESS);
                break;
                //_assignedCharacter.questData.AdvanceToNextPhase();
            }
        }
    }
    #endregion

    #region Search for Landmark Items
    private void SearchForItemInLandmark() {
        if (parentTask.targetLocation is BaseLandmark) {
            BaseLandmark _targetLandmark = (BaseLandmark)parentTask.targetLocation;
            for (int i = 0; i < _targetLandmark.itemsInLandmark.Count; i++) {
                ECS.Item item = _targetLandmark.itemsInLandmark[i];
                if (item.itemName == (string)searchingFor) {
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
    }
    #endregion

    #region Search for a Tag
    private void SearchForATag(string tag) {
        if (parentTask.targetLocation is BaseLandmark) {
            BaseLandmark _targetLandmark = (BaseLandmark)parentTask.targetLocation;
            for (int i = 0; i < _targetLandmark.charactersAtLocation.Count; i++) {
                ECS.Character currCharacter = _targetLandmark.charactersAtLocation[i].mainCharacter;
                if (currCharacter.HasTag(tag, true)) {
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
}
