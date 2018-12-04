
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterItemsMenu : MonoBehaviour {

    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ScrollRect characterItemsScrollView;

    private List<DraggableCharacterItem> items;

    public delegate void OnCharacterItemDraggedBack(Character character);
    private OnCharacterItemDraggedBack onCharacterItemDraggedBack;

    public void Initialize() {
        items = new List<DraggableCharacterItem>();
        Messenger.AddListener<Character>(Signals.CHARACTER_CREATED, AddCharacter);
        Messenger.AddListener<Character>(Signals.CHARACTER_REMOVED, RemoveCharacter);
    }

    public void Show() {
        this.gameObject.SetActive(true);
    }
    public void Hide() {
        this.gameObject.SetActive(false);
    }

    private void AddCharacter(Character character) {
        GameObject itemGO = GameObject.Instantiate(characterItemPrefab, characterItemsScrollView.content.transform);
        itemGO.GetComponent<Draggable>().parentWhileDragging = this.transform;
        DraggableCharacterItem item = itemGO.GetComponent<DraggableCharacterItem>();
        item.SetCharacter(character);
        items.Add(item);
    }
    private void RemoveCharacter(Character character) {
        DraggableCharacterItem currItem = GetCharacterItem(character);
        GameObject.Destroy(currItem.gameObject);
        items.Remove(currItem);
    }

    public DraggableCharacterItem GetCharacterItem(Character character) {
        for (int i = 0; i < items.Count; i++) {
            DraggableCharacterItem currItem = items[i];
            if (currItem.character.id == character.id) {
                return currItem;
            }
        }
        return null;
    }

    public void ReturnItems() {
        for (int i = 0; i < items.Count; i++) {
            DraggableCharacterItem currItem = items[i];
            currItem.transform.SetParent(characterItemsScrollView.content.transform);
            currItem.transform.SetSiblingIndex(i);
        }
    }

    public void SetActionOnItemDraggedBack(OnCharacterItemDraggedBack action) {
        onCharacterItemDraggedBack = action;
    }

    public void OnItemDraggedBack(Transform transform) {
        DraggableCharacterItem item = transform.GetComponent<DraggableCharacterItem>();
        if (item != null) {
            if (onCharacterItemDraggedBack != null) {
                onCharacterItemDraggedBack(item.character);
            }
        }
    }

    public void OnlyShowCharacterItems(List<Character> characters) {
        for (int i = 0; i < items.Count; i++) {
            DraggableCharacterItem currItem = items[i];
            currItem.gameObject.SetActive(characters.Contains(currItem.character));
        }
    }
    public void HideCharacterItems(List<Character> characters) {
        for (int i = 0; i < items.Count; i++) {
            DraggableCharacterItem currItem = items[i];
            currItem.gameObject.SetActive(!characters.Contains(currItem.character));
        }
    }

}
