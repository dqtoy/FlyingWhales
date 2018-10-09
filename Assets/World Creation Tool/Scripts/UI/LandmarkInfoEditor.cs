using ECS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using worldcreator;

public class LandmarkInfoEditor : MonoBehaviour {

    private BaseLandmark landmark;

    [SerializeField] private InputField landmarkName;
    [SerializeField] private Text landmarkType;

    [Header("Defenders")]
    [SerializeField] private GameObject characterSlotPrefab;
    [SerializeField] private ScrollRect defendersScrollView;

    private CharacterSlot[] defenderSlots;

    public void Initialize() {
        LoadDefenderSlots();
    }

    private void LoadDefenderSlots() {
        defenderSlots = new CharacterSlot[LandmarkManager.MAX_DEFENDERS];
        for (int i = 0; i < LandmarkManager.MAX_DEFENDERS; i++) {
            GameObject currSlot = GameObject.Instantiate(characterSlotPrefab, defendersScrollView.content.transform);
            defenderSlots[i] = currSlot.GetComponent<CharacterSlot>();
            DropZone slotDropZone = currSlot.GetComponent<DropZone>();
            slotDropZone.onItemDropped.RemoveAllListeners();
            slotDropZone.onItemDropped.AddListener(OnDefenderDropped);
        }
    }

    public void ShowLandmarkInfo(BaseLandmark landmark) {
        WorldCreatorUI.Instance.characterItemsMenu.ReturnItems();
        WorldCreatorUI.Instance.characterItemsMenu.SetActionOnItemDraggedBack(OnCharacterItemDraggedBack);
        this.gameObject.SetActive(true);
        this.landmark = landmark;
        landmarkName.text = landmark.landmarkName;
        landmarkType.text = landmark.specificLandmarkType.ToString();
        LoadDefenders();
        WorldCreatorUI.Instance.characterItemsMenu.Show();
    }

    private void LoadDefenders() {
        for (int i = 0; i < this.landmark.defenders.Length; i++) {
            Party currDefender = this.landmark.defenders[i];
            if (currDefender != null) {
                DraggableCharacterItem characterItem = WorldCreatorUI.Instance.characterItemsMenu.GetCharacterItem(currDefender.owner as Character);
                if (characterItem != null) {
                    CharacterSlot defenderSlot = defenderSlots[i];
                    characterItem.transform.SetParent(defenderSlot.transform);
                    characterItem.transform.localPosition = new Vector3(300f, 45f, 0f);
                    characterItem.GetComponent<Draggable>().parentToReturnTo = defenderSlot.transform;
                }
            }
        }
    }

    private void OnCharacterItemDraggedBack(Character character) {
        this.landmark.RemoveDefender(character.party);
        Debug.Log("Removed " + character.party.name + " to " + this.landmark.landmarkName + "'s defenders");
    }

    private void OnDefenderDropped(Transform characterItem) {
        DraggableCharacterItem item = characterItem.gameObject.GetComponent<DraggableCharacterItem>();
        if (item != null) {
            this.landmark.AddDefender(item.character.party);
            Debug.Log("Added " + item.character.party.name + " to " + this.landmark.landmarkName + "'s defenders");
        }
    }

    public void CloseMenu() {
        WorldCreatorUI.Instance.characterItemsMenu.SetActionOnItemDraggedBack(null);
        this.gameObject.SetActive(false);
        WorldCreatorUI.Instance.characterItemsMenu.ReturnItems();
        WorldCreatorUI.Instance.characterItemsMenu.Hide();
    }

    #region Change Handlers
    public void SetName(string newName) {
        landmark.SetName(newName);
    }
    #endregion
}
