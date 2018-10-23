using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;

public class CharactersIntelUI : UIMenu {

    [SerializeField] private GameObject characterEntryPrefab;
    [SerializeField] private ScrollRect charactersScrollRect;
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closePosition;
    [SerializeField] private Vector3 halfPosition;
    [SerializeField] private EasyTween tweener;
    [SerializeField] private AnimationCurve curve;

    private Dictionary<ECS.Character, CharacterIntelItem> characterEntries;


    internal override void Initialize() {
        base.Initialize();
        characterEntries = new Dictionary<ECS.Character, CharacterIntelItem>();
        Messenger.AddListener<ECS.Character>(Signals.CHARACTER_CREATED, AddCharacterEntry);
        //Messenger.AddListener<ECS.Character>(Signals.CHARACTER_DEATH, RemoveCharacterEntry);
        Messenger.AddListener<ECS.Character>(Signals.CHARACTER_DEATH, UpdateCharacterEntry);
        Messenger.AddListener<ECS.Character>(Signals.ROLE_CHANGED, UpdateCharacterEntry);
        Messenger.AddListener<ECS.Character>(Signals.FACTION_SET, UpdateCharacterEntry);
        Messenger.AddListener(Signals.INTERACTION_MENU_OPENED, OnInteractionMenuOpened);
        Messenger.AddListener(Signals.INTERACTION_MENU_CLOSED, OnInteractionMenuClosed);
        Messenger.AddListener<Intel>(Signals.INTEL_ADDED, OnIntelAdded);
    }

    public override void OpenMenu() {
        //base.OpenMenu();
        isShowing = true;
    }
    public override void CloseMenu() {
        //base.CloseMenu();
        isShowing = false;
        UIManager.Instance.OnCloseCharacterSummary();
    }

    private void AddCharacterEntry(ECS.Character character) {
        if(character.role.roleType == CHARACTER_ROLE.PLAYER) {
            return;
        }
        GameObject newEntryGO = UIManager.Instance.InstantiateUIObject(characterEntryPrefab.name, charactersScrollRect.content);
        newEntryGO.transform.localScale = Vector3.one;
        CharacterIntelItem newEntry = newEntryGO.GetComponent<CharacterIntelItem>();
        newEntry.SetCharacter(character.characterIntel);
        newEntry.Initialize();
        newEntry.gameObject.SetActive(false);
        characterEntries.Add(character, newEntry);
    }
    private void RemoveCharacterEntry(ECS.Character character) {
        CharacterIntelItem characterEntry = GetCharacterEntry(character);
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
    private CharacterIntelItem GetCharacterEntry(ECS.Character character) {
        if (characterEntries.ContainsKey(character)) {
            return characterEntries[character];
        }
        return null;
    }
    private void UpdateCharacterEntry(ECS.Character character) {
        CharacterIntelItem charEntry = GetCharacterEntry(character);
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
    private void OnIntelAdded(Intel intel) {
        if (intel is CharacterIntel) {
            CharacterIntel charIntel = (intel as CharacterIntel);
            if (charIntel.character is ECS.Character) {
                CharacterIntelItem item = GetCharacterEntry(charIntel.character as ECS.Character);
                if (item != null) {
                    item.gameObject.SetActive(true);
                }
            }
        }
    }
}
