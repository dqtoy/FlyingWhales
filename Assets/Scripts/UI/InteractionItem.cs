using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System.Linq;
using System;
using UnityEngine.Events;
using ECS;

public class InteractionItem : MonoBehaviour {
    private Interaction _interaction;
    private ActionOption _currentSelectedActionOption;
    //private Toggle _toggle;

    //public CharacterPortrait portrait;
    public TextMeshProUGUI descriptionText;
    public EventLabel descriptionEventLbl;
    public ActionOptionButton[] actionOptionButtons;
    public Button confirmBtn;
    public RectTransform confirmBtnRect;
    public GameObject assignmentGO;

    [SerializeField] private SlotItem slotItem;
    [SerializeField] private SlotItem defaultAssignedSlotItem;
    [SerializeField] private GameObject interactionAssignedSlotPrefab;
    [SerializeField] private ScrollRect interactionAssignedScrollView;
    [SerializeField] private TextMeshProUGUI descriptionAssignment;
    [SerializeField] private Vector3 confirmBtnPosNoSlot;
    [SerializeField] private Vector3 confirmBtnPosWithSlot;
    //[SerializeField] private SlotItem[] slotItems;

    //private List<CustomDropZone> neededObjectSlots;

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

    private bool _isToggled;

    private int currentNeededObjectIndex;
    //private object lastAssignedObject;

    #region getters/setters
    public Interaction interaction {
        get { return _interaction; }
    }
    #endregion

    public void Initialize() {
        slotItem.SetSlotIndex(0);
        slotItem.itemDroppedCallback = new ItemDroppedCallback();
        slotItem.itemDroppedCallback.AddListener(OnItemDroppedInSlot);
        //for (int i = 0; i < slotItems.Length; i++) {
        //    SlotItem currItem = slotItems[i];
        //    currItem.SetSlotIndex(i);
        //    currItem.itemDroppedCallback = new ItemDroppedCallback();
        //    currItem.itemDroppedCallback.AddListener(OnItemDroppedInSlot);
        //}
    }

    private void Start() {
        InitializeActionButtons();
        Messenger.AddListener<Interaction>(Signals.UPDATED_INTERACTION_STATE, OnUpdatedInteractionState);
        Messenger.AddListener<Interaction>(Signals.CHANGED_ACTIVATED_STATE, OnChangedActivatedState);
    }
    private void OnDestroy() {
        Messenger.RemoveListener<Interaction>(Signals.UPDATED_INTERACTION_STATE, OnUpdatedInteractionState);
        Messenger.RemoveListener<Interaction>(Signals.CHANGED_ACTIVATED_STATE, OnChangedActivatedState);
        //GameObject.Destroy(_toggle.gameObject);
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
            //_interaction.SetInteractionItem(this);
            defaultAssignedSlotItem.PlaceObject(_interaction.explorerMinion);
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
        //confirmBtn.gameObject.SetActive(false);
        assignmentGO.SetActive(false);
        slotItem.ClearSlot();

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
        if (_interaction.isActivated && !_interaction.currentState.isEnd) {
            //this is to load any objects already assigned to the option
            //all needed slots will always be occupied since the interaction has already been activated
            //meaning that all needed objects were already assigned beforehand.
            //assignmentGO.SetActive(true);
            //confirmBtn.interactable = false;
            //confirmBtn.gameObject.SetActive(true);
            //descriptionAssignment.gameObject.SetActive(false);
            ActionOption actionOption = _interaction.currentState.chosenOption;
            if (actionOption != null) {
                if(actionOption.assignedObjects.Count > 0) {
                    for (int i = 0; i < actionOption.assignedObjects.Count; i++) {
                        object assignedObject = actionOption.assignedObjects[i];
                        AddAssignedObject(assignedObject);
                        //SlotItem currItem = slotItems.ElementAtOrDefault(i);
                        //if (currItem == null) {
                        //    Debug.LogWarning("Not enough slots for an option!");
                        //    break; //no more slots
                        //} else {
                        //    currItem.gameObject.SetActive(true);
                        //    currItem.PlaceObject(assignedObject);
                        //}
                    }
                    slotItem.ClearSlot();
                }
                ShowConfirmButtonOnly(false);
                //if (actionOption.neededObjects == null || actionOption.neededObjects.Count == 0) {
                //    //no needed slots
                //    confirmBtnRect.anchoredPosition = confirmBtnPosNoSlot;
                //} else {
                //    //needs slots
                //    confirmBtnRect.anchoredPosition = confirmBtnPosWithSlot;
                //}
                //if (actionOption.assignedMinion != null) {
                //    portrait.GeneratePortrait(actionOption.assignedMinion.icharacter.portraitSettings, 95, true);
                //} else {
                //    portrait.GeneratePortrait(null, 95, true);
                //}
            }
        } else {
            if (_interaction.currentState.isEnd) {
                //assignmentGO.SetActive(true);
                ShowConfirmButtonOnly(confirmBtn.interactable);
            }
            //portrait.GeneratePortrait(null, 95, true);
        }
    }
    public void SetDescription(string text, Log log) {
        descriptionText.text = text;
        descriptionEventLbl.SetLog(log);
    }
    private void ChangedActivatedState() {
        ChangeStateAllButtons(!_interaction.isActivated);
        //if (_interaction.isActivated) {
        //    _toggle.targetGraphic.GetComponent<Image>().sprite = InteractionUI.Instance.toggleInactiveUnselected;
        //    _toggle.graphic.GetComponent<Image>().sprite = InteractionUI.Instance.toggleInactiveSelected;
        //} else {
        //    _toggle.targetGraphic.GetComponent<Image>().sprite = InteractionUI.Instance.toggleActiveUnselected;
        //    _toggle.graphic.GetComponent<Image>().sprite = InteractionUI.Instance.toggleActiveSelected;
        //}
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
                //ShowConfirmButtonOnly(true);
                //no needed slots
                //assignmentGO.SetActive(true);
                //ShowConfirmButtonOnly(true);
                //confirmBtnRect.anchoredPosition = confirmBtnPosNoSlot;
                //slotItem.gameObject.SetActive(false);
                ////for (int i = 0; i < slotItems.Length; i++) {
                ////    slotItems[i].gameObject.SetActive(false);
                ////}
                //confirmBtn.interactable = true;
            } else {
                //needs slots
                //assignmentGO.SetActive(true);
                ShowDescriptionAssignment();
                //confirmBtnRect.anchoredPosition = confirmBtnPosWithSlot;
                //slotItem.gameObject.SetActive(true);
                //slotItems[0].gameObject.SetActive(true);
                //for (int i = 1; i < slotItems.Length; i++) {
                //    slotItems[i].gameObject.SetActive(false);
                //}
                LoadSlotData();
                //confirmBtn.interactable = false;
                UpdateSideMenu();
            }
        }
    }
    private void LoadSlotData() {
        slotItem.ClearSlot();
        System.Type neededType = _currentSelectedActionOption.neededObjects[currentNeededObjectIndex];
        slotItem.SetNeededType(neededType);
        if (neededType == typeof(FactionIntel)) {
            descriptionAssignment.text = "Requires a Faction Intel to be dragged from the list.";
        } else if (neededType == typeof(LocationIntel)) {
            descriptionAssignment.text = "Requires a Location Intel to be dragged from the list.";
        } else if (neededType == typeof(CharacterIntel)) {
            descriptionAssignment.text = "Requires a Character Intel to be dragged from the list.";
        } else if (neededType == typeof(Minion)) {
            descriptionAssignment.text = "Requires a Demon Minion to be dragged from the list.";
        } else if (neededType == typeof(IUnit)) {
            descriptionAssignment.text = "Requires a Minion/Character to be dragged from the list.";
        } else if (neededType == typeof(ICharacter)) {
            descriptionAssignment.text = "Requires a Demon Minion/Character to be dragged from the list.";
        }
        //for (int i = 0; i < slotItems.Length; i++) {
        //    SlotItem currItem = slotItems[i];
        //    System.Type neededType = _currentSelectedActionOption.neededObjects.ElementAtOrDefault(i);
        //    currItem.ClearSlot();
        //    currItem.SetNeededType(neededType);
        //}
    }

    public void OnClickConfirm() {
        if (_currentSelectedActionOption == null || _currentSelectedActionOption.neededObjects == null || _currentSelectedActionOption.neededObjects.Count <= 0) {
            if (!_interaction.currentState.isEnd) {
                _currentSelectedActionOption.ActivateOption(_interaction.interactable);
            } else {
                _interaction.interactable.tileLocation.areaOfTile.areaInvestigation.ExploreArea();
                _interaction.currentState.EndResult();
            }
        } else {
            _currentSelectedActionOption.AddAssignedObject(slotItem.placedObject);
            //check if needed objects are done
            if (_currentSelectedActionOption.neededObjects.Count > currentNeededObjectIndex + 1) {
                //there are still needed objects
                //SlotItem currentSlotItem = slotItems[currentNeededObjectIndex];
                //SlotItem nextSlotItem = slotItems[currentNeededObjectIndex + 1];
                //nextSlotItem.gameObject.SetActive(true);
                currentNeededObjectIndex++;
                LoadSlotData();
                UpdateSideMenu();
            } else {
                //no more needed objects
                if (!_interaction.currentState.isEnd) {
                    _currentSelectedActionOption.ActivateOption(_interaction.interactable);
                } else {
                    _interaction.interactable.tileLocation.areaOfTile.areaInvestigation.ExploreArea();
                    _interaction.currentState.EndResult();
                }
            }
        }
    }
    private void ChangeStateAllButtons(bool state) {
        confirmBtn.interactable = state;
        if (!state) {
            for (int i = 0; i < actionOptionButtons.Length; i++) {
                actionOptionButtons[i].toggle.interactable = state;
            }
        }
    }
    //public void OnMinionDrop(Transform transform) {
    //    if (_interaction.isActivated) {
    //        return;
    //    }
    //    MinionItem minionItem = transform.GetComponent<MinionItem>();
    //    _currentSelectedActionOption.assignedMinion = minionItem.minion;
    //    portrait.GeneratePortrait(minionItem.portrait.portraitSettings, 95, true);
    //    if (_currentSelectedActionOption.assignedMinion != null) {
    //        confirmBtn.interactable = true;
    //    }
    //}

    private void OnItemDroppedInSlot(object droppedObject, int slotIndex) {
        ShowConfirmButtonWithSlot(true);
        //lastAssignedObject = droppedObject;
    }
    //public void SetToggle(Toggle toggle) {
    //    _toggle = toggle;
    //    _toggle.onValueChanged.AddListener(OnToggle);
    //}
    //private void OnToggle(bool state) {
    //    if(_isToggled != state) {
    //        _isToggled = state;
    //        _toggle.targetGraphic.enabled = !state;
    //        _toggle.interactable = !state;
    //        if (state) {
    //            GoToThisPage();
    //        }
    //    } 
    //}
    //private void GoToThisPage() {
    //    int index = InteractionUI.Instance.GetIndexOfInteraction(this);
    //    if (InteractionUI.Instance.scrollSnap.CurrentPage != index) {
    //        InteractionUI.Instance.scrollSnap.ChangePage(index);
    //    }
    //}

    public void ClearNeededObjectSlots() {
        slotItem.ClearSlot();
        //for (int i = 0; i < slotItems.Length; i++) {
        //    SlotItem currItem = slotItems[i];
        //    currItem.ClearSlot();
        //    currItem.gameObject.SetActive(false);
        //}
    }

    private void UpdateSideMenu() {
        System.Type neededObject = _currentSelectedActionOption.neededObjects[currentNeededObjectIndex];
        if (neededObject == typeof(FactionIntel)) {
            UIManager.Instance.ShowFactionIntelMenu();
        } else if (neededObject == typeof(CharacterIntel)) {
            UIManager.Instance.ShowCharacterIntelMenu();
        } else if (neededObject == typeof(Minion)) {
            UIManager.Instance.ShowMinionsMenu();
        } else if (neededObject == typeof(LocationIntel)) {
            UIManager.Instance.ShowLocationIntelMenu();
        }
    }
    public void AddAssignedObject(object obj) {
        GameObject go = GameObject.Instantiate(interactionAssignedSlotPrefab, interactionAssignedScrollView.content);
        SlotItem slot = go.GetComponent<SlotItem>();
        slot.PlaceObject(obj);
    }
    public void ClearAssignedObjects() {
        Transform[] objects = Utilities.GetComponentsInDirectChildren<Transform>(interactionAssignedScrollView.content.gameObject);
        for (int i = 1; i < objects.Length; i++) {
            GameObject.Destroy(objects[i].gameObject);
        }
        //for (int i = 1; i < interactionAssignedScrollView.content.childCount; i++) {
        //    GameObject.Destroy(interactionAssignedScrollView.content.GetChild(i));
        //    i--;
        //}
    }
    private void ShowConfirmButtonOnly(bool isInteractable) {
        assignmentGO.SetActive(true);
        confirmBtn.gameObject.SetActive(true);
        confirmBtn.interactable = isInteractable;
        confirmBtnRect.anchoredPosition = confirmBtnPosNoSlot;
        slotItem.gameObject.SetActive(false);
        descriptionAssignment.gameObject.SetActive(false);
    }
    private void ShowConfirmButtonWithSlot(bool isInteractable) {
        assignmentGO.SetActive(true);
        confirmBtn.gameObject.SetActive(true);
        confirmBtn.interactable = isInteractable;
        confirmBtnRect.anchoredPosition = confirmBtnPosWithSlot;
        slotItem.gameObject.SetActive(true);
        descriptionAssignment.gameObject.SetActive(false);
    }
    private void ShowDescriptionAssignment() {
        assignmentGO.SetActive(true);
        confirmBtn.gameObject.SetActive(false);
        descriptionAssignment.gameObject.SetActive(true);
        slotItem.gameObject.SetActive(true);
    }

    public void ShowObjectInfo(object obj) {
        if (obj is BaseLandmark) {
            ShowLandmarkInfo(obj as BaseLandmark);
        } else if (obj is Area) {
            ShowAreaInfo(obj as Area);
        } else if (obj is ICharacter) {
            ShowCharacterInfo(obj as ICharacter);
        } else if (obj is Minion) {
            ShowCharacterInfo((obj as Minion).icharacter);
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
            locationTypeLbl.text = Utilities.GetNormalizedSingularRace(landmark.owner.race) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(landmark.specificLandmarkType.ToString());
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

    private void ShowCharacterInfo(ICharacter character) {
        if (characterInfoGO.activeSelf) {
            return;
        }

        characterNameLbl.text = character.name;
        characterLvlLbl.text = "Lvl." + character.level.ToString() + " " + character.characterClass.className;
        characterPortrait.GeneratePortrait(character);
        characterFactionEmblem.SetFaction(character.faction);
        characterInfoGO.SetActive(true);
    }
}
