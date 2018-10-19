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
        List<Character> charactersToShow = new List<Character>();
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character currCharacter = CharacterManager.Instance.allCharacters[i];
            if (currCharacter.isDefender) {
                //if the character is defending a landamark
                if (currCharacter.IsDefending(landmark)) { //check if it is defending the provided landmark
                    charactersToShow.Add(currCharacter); //if it is, show the character
                }
            } else {
                //show characters that are not yet defenders
                charactersToShow.Add(currCharacter);
            }
        }
        WorldCreatorUI.Instance.characterItemsMenu.Show();
        WorldCreatorUI.Instance.characterItemsMenu.OnlyShowCharacterItems(charactersToShow);
    }

    private void LoadDefenders() {
        if (this.landmark.defenders != null) {
            for (int i = 0; i < this.landmark.defenders.icharacters.Count; i++) {
                ICharacter currDefender = this.landmark.defenders.icharacters[i];
                if (currDefender != null) {
                    DraggableCharacterItem characterItem = WorldCreatorUI.Instance.characterItemsMenu.GetCharacterItem(currDefender as Character);
                    if (characterItem != null) {
                        CharacterSlot defenderSlot = defenderSlots[i];
                        characterItem.transform.SetParent(defenderSlot.transform);
                        //(characterItem.transform as RectTransform).anchorMin = new Vector2(0f, 1f);
                        //(characterItem.transform as RectTransform).anchorMax = new Vector2(0f, 1f);
                        //characterItem.GetComponent<Draggable>().parentToReturnTo = defenderSlot.transform;
                        characterItem.transform.localPosition = Vector3.zero;
                    }
                }
            }
        }
        
    }

    private void OnCharacterItemDraggedBack(Character character) {
        this.landmark.RemoveDefender(character);
        Debug.Log("Removed " + character.party.name + " to " + this.landmark.landmarkName + "'s defenders");
    }

    private void OnDefenderDropped(Transform characterItem) {
        DraggableCharacterItem item = characterItem.gameObject.GetComponent<DraggableCharacterItem>();
        if (item != null) {
            this.landmark.AddDefender(item.character);
            Debug.Log("Added " + item.character.name + " to " + this.landmark.landmarkName + "'s defenders");
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
