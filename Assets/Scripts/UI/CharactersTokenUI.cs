using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;

public class CharactersTokenUI : UIMenu {

    [SerializeField] private GameObject characterEntryPrefab;
    [SerializeField] private ScrollRect charactersScrollRect;
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closePosition;
    [SerializeField] private Vector3 halfPosition;
    [SerializeField] private EasyTween tweener;
    [SerializeField] private AnimationCurve curve;

    private Dictionary<Character, CharacterTokenItem> characterEntries;
    private List<CharacterTokenItem> _activeCharacterEntries;

    #region getters/setters
    public List<CharacterTokenItem> activeCharacterEntries {
        get { return _activeCharacterEntries; }
    }
    #endregion
    internal override void Initialize() {
        base.Initialize();
        _activeCharacterEntries = new List<CharacterTokenItem>();
        characterEntries = new Dictionary<Character, CharacterTokenItem>();
        Messenger.AddListener<Character>(Signals.CHARACTER_CREATED, AddCharacterEntry);
        //Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, RemoveCharacterEntry);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, UpdateCharacterEntry);
        Messenger.AddListener<Character>(Signals.ROLE_CHANGED, UpdateCharacterEntry);
        Messenger.AddListener<Character>(Signals.FACTION_SET, UpdateCharacterEntry);
        //Messenger.AddListener(Signals.INTERACTION_MENU_OPENED, OnInteractionMenuOpened);
        //Messenger.AddListener(Signals.INTERACTION_MENU_CLOSED, OnInteractionMenuClosed);
        Messenger.AddListener<Token>(Signals.TOKEN_ADDED, OnTokenAdded);
    }

    public override void OpenMenu() {
        //base.OpenMenu();
        isShowing = true;
    }
    public override void CloseMenu() {
        //base.CloseMenu();
        isShowing = false;
    }

    private void AddCharacterEntry(Character character) {
        if(character.role != null && character.role.roleType == CHARACTER_ROLE.PLAYER) {
            return;
        }
        GameObject newEntryGO = UIManager.Instance.InstantiateUIObject(characterEntryPrefab.name, charactersScrollRect.content);
        newEntryGO.transform.localScale = Vector3.one;
        CharacterTokenItem newEntry = newEntryGO.GetComponent<CharacterTokenItem>();
        newEntry.SetCharacter(character.characterToken);
        newEntry.Initialize();
        newEntry.gameObject.SetActive(false);
        characterEntries.Add(character, newEntry);
    }
    private void RemoveCharacterEntry(Character character) {
        CharacterTokenItem characterEntry = GetCharacterEntry(character);
        if (characterEntry != null) {
            characterEntries.Remove(character);
            ObjectPoolManager.Instance.DestroyObject(characterEntry.gameObject);
            //UpdateListColors();
        }
        //if (this.isShowing) {
        //    StartCoroutine(ExecuteLayoutGroup());
        //}
        //sortingAction();
    }
    private CharacterTokenItem GetCharacterEntry(Character character) {
        if (characterEntries.ContainsKey(character)) {
            return characterEntries[character];
        }
        return null;
    }
    private void UpdateCharacterEntry(Character character) {
        CharacterTokenItem charEntry = GetCharacterEntry(character);
        if (charEntry != null) {
            charEntry.UpdateCharacterInfo();
        }
        //sortingAction();
    }

    private void OnInteractionMenuOpened() {
        if (this.isShowing) {
            //if the menu is showing update it's open position
            //only open halfway
            tweener.SetAnimationPosition(openPosition, halfPosition, curve, curve);
            tweener.ChangeSetState(false);
            tweener.TriggerOpenClose();
            tweener.SetAnimationPosition(closePosition, halfPosition, curve, curve);
        } else {
            //only open halfway
            tweener.SetAnimationPosition(closePosition, halfPosition, curve, curve);
        }
    }
    private void OnInteractionMenuClosed() {
        if (this.isShowing) {
            tweener.SetAnimationPosition(halfPosition, openPosition, curve, curve);
            tweener.ChangeSetState(false);
            tweener.TriggerOpenClose();
            tweener.SetAnimationPosition(closePosition, openPosition, curve, curve);
        } else {
            //reset positions to normal
            tweener.SetAnimationPosition(closePosition, openPosition, curve, curve);
        }
    }
    private void OnTokenAdded(Token token) {
        if (token is CharacterToken) {
            CharacterToken charToken = (token as CharacterToken);
            CharacterTokenItem item = GetCharacterEntry(charToken.character);
            if (item != null) {
                item.gameObject.SetActive(true);
                item.SetDraggable(true);
                _activeCharacterEntries.Add(item);
            }
        }
    }
}
