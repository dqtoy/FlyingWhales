using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using UnityEngine.Events;


public class InteractionItem : MonoBehaviour {
    private Interaction _interaction;
    private ActionOption _currentSelectedActionOption;

    public TextMeshProUGUI descriptionText;
    public EventLabel descriptionEventLbl;
    public CharacterPortrait characterInvolvedPortrait;
    public LocationPortrait characterInvolvedLocationPortrait;
    public Button closeButton;
    public ActionOptionButton[] actionOptionButtons;

    //public Button confirmBtn;
    //public RectTransform confirmBtnRect;
    //public GameObject assignmentGO;

    //[SerializeField] private SlotItem slotItem;
    //[SerializeField] private SlotItem defaultAssignedSlotItem;
    //[SerializeField] private GameObject interactionAssignedSlotPrefab;
    //[SerializeField] private ScrollRect interactionAssignedScrollView;
    //[SerializeField] private TextMeshProUGUI descriptionAssignment;
    //[SerializeField] private Vector3 confirmBtnPosNoSlot;
    //[SerializeField] private Vector3 confirmBtnPosWithSlot;
    //[SerializeField] private SlotItem[] slotItems;

    [Space(10)]
    [Header("Landmark Info")]
    [SerializeField] private GameObject landmarkInfoGO;
    [SerializeField] private TextMeshProUGUI locationNameLbl;
    [SerializeField] private TextMeshProUGUI locationTypeLbl;
    [SerializeField] private TextMeshProUGUI locationSuppliesLbl;
    [SerializeField] private FactionEmblem locationOwnerEmblem;

    [Space(10)]
    [Header("Character Info")]
    [SerializeField] private GameObject characterInfoGO;
    [SerializeField] private TextMeshProUGUI characterNameLbl;
    [SerializeField] private TextMeshProUGUI characterLvlLbl;
    [SerializeField] private CharacterPortrait characterPortrait;
    [SerializeField] private FactionEmblem characterFactionEmblem;

    [Space(10)]
    [Header("Token Choices")]
    [SerializeField] private GameObject tokenChoicesGO;
    [SerializeField] private ScrollRect tokenChoicesScrollView;
    [SerializeField] private GameObject tokenItemPrefab;

    private bool _isToggled;

    private int currentNeededObjectIndex;

    #region getters/setters
    public Interaction interaction {
        get { return _interaction; }
    }
    #endregion

    public void Initialize() {
        //slotItem.SetSlotIndex(0);
        //slotItem.SetItemDroppedCallback(OnItemDroppedInSlot);
    }

    private void Start() {
        InitializeActionButtons();
        Messenger.AddListener<Interaction>(Signals.UPDATED_INTERACTION_STATE, OnUpdatedInteractionState);
        Messenger.AddListener<Interaction>(Signals.CHANGED_ACTIVATED_STATE, OnChangedActivatedState);
    }
    private void OnDestroy() {
        Messenger.RemoveListener<Interaction>(Signals.UPDATED_INTERACTION_STATE, OnUpdatedInteractionState);
        Messenger.RemoveListener<Interaction>(Signals.CHANGED_ACTIVATED_STATE, OnChangedActivatedState);
    }
    private void OnUpdatedInteractionState(Interaction interaction) {
        if(_interaction != null && _interaction == interaction) {
            UpdateState();
        }
    }
    private void OnChangedActivatedState(Interaction interaction) {
        if (_interaction != null && _interaction == interaction) {
            ChangedActivatedState();
        }
    }
    public void SetInteraction(Interaction interaction) {
        _interaction = interaction;
        if(_interaction != null) {
            landmarkInfoGO.SetActive(false);
            characterInfoGO.SetActive(false);
            if (_interaction.characterInvolved != null) {
                characterInvolvedPortrait.GeneratePortrait(_interaction.characterInvolved);
                characterInvolvedLocationPortrait.SetLocation(_interaction.characterInvolved.specificLocation);
                characterInvolvedPortrait.gameObject.SetActive(true);
                characterInvolvedLocationPortrait.gameObject.SetActive(true);
            } else {
                characterInvolvedPortrait.gameObject.SetActive(false);
                characterInvolvedLocationPortrait.gameObject.SetActive(false);
            }
            UpdateState();
            ChangedActivatedState();
        }
    }
    private void InitializeActionButtons() {
        for (int i = 0; i < actionOptionButtons.Length; i++) {
            actionOptionButtons[i].Initialize();
        }
    }
    public void UpdateState() {
        SetDescription(_interaction.currentState.description, _interaction.currentState.descriptionLog);
        //assignmentGO.SetActive(false);
        //slotItem.ClearSlot();
        closeButton.gameObject.SetActive(_interaction.currentState.isEnd);
        for (int i = 0; i < actionOptionButtons.Length; i++) {
            actionOptionButtons[i].SetOption(_interaction.currentState.actionOptions[i]);
            if (_interaction.currentState.actionOptions[i] != null) {
                if(_interaction.currentState.chosenOption != null && _interaction.currentState.chosenOption == actionOptionButtons[i].actionOption) {
                    actionOptionButtons[i].toggle.isOn = true;
                } else {
                    actionOptionButtons[i].toggle.isOn = false;
                }
                actionOptionButtons[i].gameObject.SetActive(true);
            } else {
                actionOptionButtons[i].gameObject.SetActive(false);
            }
        }

        //if (_interaction.isActivated && !_interaction.currentState.isEnd) {
        //    //this is to load any objects already assigned to the option
        //    //all needed slots will always be occupied since the interaction has already been activated
        //    //meaning that all needed objects were already assigned beforehand.
        //    ActionOption actionOption = _interaction.currentState.chosenOption;
        //    if (actionOption != null) {
        //        if(actionOption.assignedObjects.Count > 0) {
        //            for (int i = 0; i < actionOption.assignedObjects.Count; i++) {
        //                object assignedObject = actionOption.assignedObjects[i];
        //                AddAssignedObject(assignedObject);
        //            }
        //            slotItem.ClearSlot();
        //        }
        //        ShowConfirmButtonOnly(false);
        //    }
        //} else {
        //    if (_interaction.currentState.isEnd) {
        //        ShowConfirmButtonOnly(confirmBtn.interactable);
        //    }
        //}
    }
    public void SetDescription(string text, Log log) {
        descriptionText.text = text;
        descriptionEventLbl.SetLog(log);
    }
    private void ChangedActivatedState() {
        ChangeStateAllButtons(!_interaction.isActivated);
    }
    public void SetCurrentSelectedActionOption(ActionOption actionOption) {
        if (_currentSelectedActionOption != null) {
            _currentSelectedActionOption.ClearAssignedObjects();
        }
        _currentSelectedActionOption = actionOption;
        currentNeededObjectIndex = 0; //reset index
        if (actionOption != null) {
            if (actionOption.neededObjects == null || actionOption.neededObjects.Count == 0) {
                _currentSelectedActionOption.ActivateOption(_interaction.interactable);
                HideChoicesMenu();
            } else {
                //needs slots
                //ShowDescriptionAssignment();
                LoadSlotData();
                UpdateChoicesMenu();
            }
        }
    }
    private void LoadSlotData() {
        //slotItem.ClearSlot();
        //System.Type neededType = _currentSelectedActionOption.neededObjects[currentNeededObjectIndex];
        //slotItem.SetNeededType(neededType);
        //if (neededType == typeof(FactionToken)) {
        //    descriptionAssignment.text = "Requires a Faction Intel to be dragged from the list.";
        //} else if (neededType == typeof(LocationToken)) {
        //    descriptionAssignment.text = "Requires a Location Intel to be dragged from the list.";
        //} else if (neededType == typeof(CharacterToken)) {
        //    descriptionAssignment.text = "Requires a Character Intel to be picked from the list.";
        //} else if (neededType == typeof(Minion)) {
        //    descriptionAssignment.text = "Requires a Demon Minion to be picked from the list.";
        //} else if (neededType == typeof(Character)) {
        //    descriptionAssignment.text = "Requires a Demon Minion/Character to be picked from the list.";
        //}
    }

    public void OnClickConfirm() {
        if (_currentSelectedActionOption == null || _currentSelectedActionOption.neededObjects == null || _currentSelectedActionOption.neededObjects.Count <= 0) {
            if (!_interaction.currentState.isEnd) {
                _currentSelectedActionOption.ActivateOption(_interaction.interactable);
            } else {
                _interaction.currentState.EndResult();
            }
        } else {
            //check if needed objects are done
            if (_currentSelectedActionOption.neededObjects.Count > currentNeededObjectIndex + 1) {
                //there are still needed objects
                currentNeededObjectIndex++;
                LoadSlotData();
                UpdateChoicesMenu();
            } else {
                ResetSideMenu();
                //no more needed objects
                if (!_interaction.currentState.isEnd) {
                    _currentSelectedActionOption.ActivateOption(_interaction.interactable);
                } else {
                    _interaction.currentState.EndResult();
                }
            }
        }
    }
    public void OnClickClose() {
        if (_interaction.currentState.isEnd) {
            _interaction.currentState.EndResult();
        }
    }
    private void ChangeStateAllButtons(bool state) {
        //confirmBtn.interactable = state;
        if (!state) {
            for (int i = 0; i < actionOptionButtons.Length; i++) {
                actionOptionButtons[i].toggle.interactable = state;
            }
        }
    }
    private void OnItemDroppedInSlot(object droppedObject, int slotIndex) {
        //ShowConfirmButtonWithSlot(true);
    }
    public void ClearNeededObjectSlots() {
        //slotItem.ClearSlot();
    }

    #region Choices Menu
    private void UpdateChoicesMenu() {
        System.Type neededObject = _currentSelectedActionOption.neededObjects[currentNeededObjectIndex];
        if (neededObject == typeof(CharacterToken)) {
            LoadCharacterChoices(_currentSelectedActionOption);
        } else if (neededObject == typeof(Minion)) {
            LoadMinionChoices(_currentSelectedActionOption);
        }
        //TODO: Add Loading for Locations and Factions
        tokenChoicesGO.SetActive(true);
    }
    public void HideChoicesMenu() {
        tokenChoicesGO.SetActive(false);
    }
    private void LoadCharacterChoices(ActionOption option) {
        List<Character> validCharacters = new List<Character>();
        if (option.neededObjectsChecker == null) {
            validCharacters.AddRange(CharacterManager.Instance.allCharacters); //means that all characters can be dragged
        } else {
            for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                Character currCharacter = CharacterManager.Instance.allCharacters[i];
                bool isCharacterValid = true;
                for (int j = 0; j < option.neededObjectsChecker.Count; j++) {
                    ActionOptionNeededObjectChecker currFilter = option.neededObjectsChecker[j];
                    if (!currFilter.IsMatch(currCharacter)) {
                        isCharacterValid = false;
                        break;
                    }
                }
                if (isCharacterValid) {
                    validCharacters.Add(currCharacter);
                }
            }
        }

        Utilities.DestroyChildren(tokenChoicesScrollView.content);
        for (int i = 0; i < validCharacters.Count; i++) {
            Character currCharacter = validCharacters[i];
            GameObject characterGO = UIManager.Instance.InstantiateUIObject(tokenItemPrefab.name, tokenChoicesScrollView.content);
            TokenItem item = characterGO.GetComponent<TokenItem>();
            item.SetClickAction(OnChooseToken);
            item.SetObject(currCharacter.characterToken);
        }
    }
    private void LoadMinionChoices(ActionOption option) {
        List<Minion> validMinions = new List<Minion>();
        if (option.neededObjectsChecker == null) {
            validMinions.AddRange(PlayerManager.Instance.player.minions.Where(x => x.isEnabled)); //means that all characters can be dragged
        } else {
            List<Minion> ableMinions = PlayerManager.Instance.player.minions.Where(x => x.isEnabled).ToList();
            for (int i = 0; i < ableMinions.Count; i++) {
                Minion currMinion = ableMinions[i];
                bool isMinionValid = true;
                for (int j = 0; j < option.neededObjectsChecker.Count; j++) {
                    ActionOptionNeededObjectChecker currFilter = option.neededObjectsChecker[j];
                    if (!currFilter.IsMatch(currMinion.character)) {
                        isMinionValid = false;
                        break;
                    }
                }
                if (isMinionValid) {
                    validMinions.Add(currMinion);
                }
            }
        }

        Utilities.DestroyChildren(tokenChoicesScrollView.content);
        for (int i = 0; i < validMinions.Count; i++) {
            Minion currMinion = validMinions[i];
            GameObject characterGO = UIManager.Instance.InstantiateUIObject(tokenItemPrefab.name, tokenChoicesScrollView.content);
            TokenItem item = characterGO.GetComponent<TokenItem>();
            item.SetClickAction(OnChooseToken);
            item.SetObject(currMinion);
        }
    }
    private void OnChooseToken(object obj) {
        HideChoicesMenu();
        _currentSelectedActionOption.AddAssignedObject(obj);
        OnClickConfirm();
    }
    #endregion


    private void UpdateSideMenu() {
        System.Type neededObject = _currentSelectedActionOption.neededObjects[currentNeededObjectIndex];
        if (neededObject == typeof(FactionToken)) {
            UIManager.Instance.ShowFactionTokenMenu();
        } else if (neededObject == typeof(CharacterToken)) {
            UIManager.Instance.ShowCharacterTokenMenu();
            CharacterIntelChecker();
        } else if (neededObject == typeof(Minion)) {
            UIManager.Instance.ShowMinionsMenu();
        } else if (neededObject == typeof(LocationToken)) {
            UIManager.Instance.ShowLocationTokenMenu();
        }
    }
    private void ResetSideMenu() {
        System.Type neededObject = _currentSelectedActionOption.neededObjects[currentNeededObjectIndex];
        if (neededObject == typeof(FactionToken)) {
            //TODO
        } else if (neededObject == typeof(CharacterToken)) {
            ResetCharacterIntels();
        } else if (neededObject == typeof(Minion)) {
            //TODO
        } else if (neededObject == typeof(LocationToken)) {
            //TODO
        }
    }
    public void AddAssignedObject(object obj) {
        //GameObject go = GameObject.Instantiate(interactionAssignedSlotPrefab, interactionAssignedScrollView.content);
        //SlotItem slot = go.GetComponent<SlotItem>();
        //slot.PlaceObject(obj);
    }
    public void ClearAssignedObjects() {
        //Transform[] objects = Utilities.GetComponentsInDirectChildren<Transform>(interactionAssignedScrollView.content.gameObject);
        //for (int i = 1; i < objects.Length; i++) {
        //    GameObject.Destroy(objects[i].gameObject);
        //}
    }
    //private void ShowConfirmButtonOnly(bool isInteractable) {
    //    assignmentGO.SetActive(true);
    //    confirmBtn.gameObject.SetActive(true);
    //    confirmBtn.interactable = isInteractable;
    //    confirmBtnRect.anchoredPosition = confirmBtnPosNoSlot;
    //    descriptionAssignment.gameObject.SetActive(false);
    //}
    //private void ShowConfirmButtonWithSlot(bool isInteractable) {
    //    assignmentGO.SetActive(true);
    //    confirmBtn.gameObject.SetActive(true);
    //    confirmBtn.interactable = isInteractable;
    //    confirmBtnRect.anchoredPosition = confirmBtnPosWithSlot;
    //    descriptionAssignment.gameObject.SetActive(false);
    //}
    //private void ShowDescriptionAssignment() {
    //    assignmentGO.SetActive(true);
    //    confirmBtn.gameObject.SetActive(false);
    //    descriptionAssignment.gameObject.SetActive(true);
    //}

    public void ShowObjectInfo(object obj) {
        if (obj is BaseLandmark) {
            ShowLandmarkInfo(obj as BaseLandmark);
        } else if (obj is Area) {
            ShowAreaInfo(obj as Area);
        } else if (obj is Character) {
            ShowCharacterInfo(obj as Character);
        } else if (obj is Minion) {
            ShowCharacterInfo((obj as Minion).character);
        }
    }
    public void HideObjectInfo() {
        landmarkInfoGO.SetActive(false);
        characterInfoGO.SetActive(false);
    }

    private void ShowLandmarkInfo(BaseLandmark landmark) {
        if (landmarkInfoGO.activeSelf) {
            return;
        }
        if (landmark.tileLocation.areaOfTile != null) {
            locationNameLbl.text = landmark.tileLocation.areaOfTile.name;
            locationSuppliesLbl.text = landmark.tileLocation.areaOfTile.suppliesInBank.ToString();
        } else {
            locationNameLbl.text = landmark.landmarkName;
            locationSuppliesLbl.text = "0";
        }

        if (landmark.owner != null) {
            locationTypeLbl.text = Utilities.GetNormalizedSingularRace(landmark.owner.raceType) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(landmark.specificLandmarkType.ToString());
            locationOwnerEmblem.gameObject.SetActive(true);
            locationOwnerEmblem.SetFaction(landmark.owner);
        } else {
            locationTypeLbl.text = Utilities.NormalizeStringUpperCaseFirstLetters(landmark.specificLandmarkType.ToString());
            locationOwnerEmblem.gameObject.SetActive(false);
        }
        landmarkInfoGO.SetActive(true);
    }
    private void ShowAreaInfo(Area area) {
        if (landmarkInfoGO.activeSelf) {
            return;
        }
        locationNameLbl.text = area.name;
        locationSuppliesLbl.text = area.suppliesInBank.ToString();

        locationTypeLbl.text = Utilities.NormalizeStringUpperCaseFirstLetters(area.areaType.ToString());

        if (area.owner != null) {
            locationOwnerEmblem.gameObject.SetActive(true);
            locationOwnerEmblem.SetFaction(area.owner);
        } else {
            locationOwnerEmblem.gameObject.SetActive(false);
        }
        landmarkInfoGO.SetActive(true);
    }

    private void ShowCharacterInfo(Character character) {
        if (characterInfoGO.activeSelf) {
            return;
        }

        characterNameLbl.text = character.name;
        characterLvlLbl.text = "Lvl." + character.level.ToString() + " " + character.characterClass.className;
        characterPortrait.GeneratePortrait(character);
        characterFactionEmblem.SetFaction(character.faction);
        characterInfoGO.SetActive(true);
    }

    #region Action Option Needed Object Checker
    private void ResetCharacterIntels() {
        for (int i = 0; i < PlayerUI.Instance.charactersIntelUI.activeCharacterEntries.Count; i++) {
            if (!PlayerUI.Instance.charactersIntelUI.activeCharacterEntries[i].isDraggable) {
                PlayerUI.Instance.charactersIntelUI.activeCharacterEntries[i].SetDraggable(true);
            }
        }
    }
    private void CharacterIntelChecker() {
        if (_currentSelectedActionOption.neededObjectsChecker != null) {
            ActionOptionNeededObjectChecker checker = _currentSelectedActionOption.neededObjectsChecker.ElementAtOrDefault(currentNeededObjectIndex);
            if (checker is ActionOptionTraitRequirement) {
                ActionOptionTraitRequirement requirement = checker as ActionOptionTraitRequirement;
                if (checker != null && requirement.requirements != null) {
                    if (requirement.categoryReq == TRAIT_REQUIREMENT.RACE) {
                        if (_interaction.characterInvolved != null) {
                            for (int i = 0; i < PlayerUI.Instance.charactersIntelUI.activeCharacterEntries.Count; i++) {
                                if (!PlayerUI.Instance.charactersIntelUI.activeCharacterEntries[i].isDraggable) {
                                    continue;
                                }
                                Character character = PlayerUI.Instance.charactersIntelUI.activeCharacterEntries[i].character;
                                if (_interaction.characterInvolved == character) {
                                    PlayerUI.Instance.charactersIntelUI.activeCharacterEntries[i].SetDraggable(false);
                                } else {
                                    PlayerUI.Instance.charactersIntelUI.activeCharacterEntries[i].SetDraggable(checker.IsMatch(character));
                                }
                            }
                        } else {
                            for (int i = 0; i < PlayerUI.Instance.charactersIntelUI.activeCharacterEntries.Count; i++) {
                                if (!PlayerUI.Instance.charactersIntelUI.activeCharacterEntries[i].isDraggable) {
                                    continue;
                                }
                                Character character = PlayerUI.Instance.charactersIntelUI.activeCharacterEntries[i].character;
                                PlayerUI.Instance.charactersIntelUI.activeCharacterEntries[i].SetDraggable(checker.IsMatch(character));
                            }
                        }
                    }
                }
            } else {
                for (int i = 0; i < PlayerUI.Instance.charactersIntelUI.activeCharacterEntries.Count; i++) {
                    if (!PlayerUI.Instance.charactersIntelUI.activeCharacterEntries[i].isDraggable) {
                        continue;
                    }
                    Character character = PlayerUI.Instance.charactersIntelUI.activeCharacterEntries[i].character;
                    PlayerUI.Instance.charactersIntelUI.activeCharacterEntries[i].SetDraggable(checker.IsMatch(character));
                }
            }
            
        }
    }
    #endregion
}
