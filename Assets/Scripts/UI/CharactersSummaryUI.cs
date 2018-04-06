using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CharactersSummaryUI : UIMenu {

    private Action sortingAction;

    [SerializeField] private GameObject characterEntryPrefab;
    [SerializeField] private UIGrid characterEntriesGrid;
    [SerializeField] private UIEventTrigger nameHeaderTrigger;
    [SerializeField] private UIEventTrigger factionHeaderTrigger;
    [SerializeField] private UIEventTrigger raceHeaderTrigger;
    [SerializeField] private UIEventTrigger roleHeaderTrigger;

    private List<CharacterSummaryEntry> characterEntries;

    public override void ShowMenu() {
        base.ShowMenu();
        StartCoroutine(UIManager.Instance.RepositionGrid(characterEntriesGrid));
    }

    internal override void Initialize() {
        base.Initialize();
        characterEntries = new List<CharacterSummaryEntry>();
        Messenger.AddListener<ECS.Character>(Signals.CHARACTER_CREATED, AddCharacterEntry);
        Messenger.AddListener<ECS.Character>(Signals.CHARACTER_DEATH, RemoveCharacterEntry);
        Messenger.AddListener<ECS.Character>(Signals.ROLE_CHANGED, UpdateCharacterEntry);
        sortingAction = () => OrderByName();
        EventDelegate.Set(nameHeaderTrigger.onClick, () => SetSortingAction(OrderByName));
        EventDelegate.Set(factionHeaderTrigger.onClick, () => SetSortingAction(OrderByFaction));
        EventDelegate.Set(raceHeaderTrigger.onClick, () => SetSortingAction(OrderByRace));
        EventDelegate.Set(roleHeaderTrigger.onClick, () => SetSortingAction(OrderByRole));
    }

    private void AddCharacterEntry(ECS.Character character) {
        if (character.role.roleType != CHARACTER_ROLE.FOLLOWER) {
            GameObject newEntryGO = UIManager.Instance.InstantiateUIObject(characterEntryPrefab.name, characterEntriesGrid.transform);
            newEntryGO.transform.localScale = Vector3.one;
            CharacterSummaryEntry newEntry = newEntryGO.GetComponent<CharacterSummaryEntry>();
            newEntry.SetCharacter(character);
            characterEntries.Add(newEntry);
            characterEntriesGrid.Reposition();
        }
        sortingAction();
    }
    private void RemoveCharacterEntry(ECS.Character character) {
        CharacterSummaryEntry characterEntry = GetCharacterEntry(character);
        if (characterEntry != null) {
            characterEntries.Remove(characterEntry);
        }
        ObjectPoolManager.Instance.DestroyObject(characterEntry.gameObject);
        UIManager.Instance.RepositionGrid(characterEntriesGrid);
    }

    private CharacterSummaryEntry GetCharacterEntry(ECS.Character character) {
        for (int i = 0; i < characterEntries.Count; i++) {
            CharacterSummaryEntry currEntry = characterEntries[i];
            if (currEntry.character.id == character.id) {
                return currEntry;
            }
        }
        return null;
    }

    private void UpdateCharacterEntry(ECS.Character character) {
        CharacterSummaryEntry charEntry = GetCharacterEntry(character);
        if (charEntry != null) {
            charEntry.UpdateCharacterInfo();
        }
    }

    #region Sorting
    private void SetSortingAction(Action action) {
        sortingAction = () => action();
        sortingAction();
    }
    private void OrderByName() {
        for (int i = 0; i < characterEntries.Count; i++) {
            CharacterSummaryEntry currEntry = characterEntries[i];
            currEntry.OnOrderByName();
        }
        if (this.gameObject.activeInHierarchy) {
            StartCoroutine(UIManager.Instance.RepositionGrid(characterEntriesGrid));
        }
    }
    private void OrderByFaction() {
        for (int i = 0; i < characterEntries.Count; i++) {
            CharacterSummaryEntry currEntry = characterEntries[i];
            currEntry.OnOrderByFaction();
        }
        if (this.gameObject.activeInHierarchy) {
            StartCoroutine(UIManager.Instance.RepositionGrid(characterEntriesGrid));
        }
    }
    private void OrderByRace() {
        for (int i = 0; i < characterEntries.Count; i++) {
            CharacterSummaryEntry currEntry = characterEntries[i];
            currEntry.OnOrderByRace();
        }
        if (this.gameObject.activeInHierarchy) {
            StartCoroutine(UIManager.Instance.RepositionGrid(characterEntriesGrid));
        }
    }
    private void OrderByRole() {
        for (int i = 0; i < characterEntries.Count; i++) {
            CharacterSummaryEntry currEntry = characterEntries[i];
            currEntry.OnOrderByRole();
        }
        if (this.gameObject.activeInHierarchy) {
            StartCoroutine(UIManager.Instance.RepositionGrid(characterEntriesGrid));
        }
    }
    #endregion

}
