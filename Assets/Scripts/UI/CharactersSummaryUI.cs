using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;

public class CharactersSummaryUI : UIMenu {

    private Action sortingAction;

    [SerializeField] private GameObject characterEntryPrefab;
    [SerializeField] private ScrollRect charactersScrollRect;
    //[SerializeField] private UIEventTrigger nameHeaderTrigger;
    //[SerializeField] private UIEventTrigger factionHeaderTrigger;
    //[SerializeField] private UIEventTrigger raceHeaderTrigger;
    //[SerializeField] private UIEventTrigger roleHeaderTrigger;

    private Dictionary<ECS.Character, CharacterSummaryEntry> characterEntries;

    //public override void ShowMenu() {
    //    base.ShowMenu();
    //    StartCoroutine(UIManager.Instance.RepositionGrid(characterEntriesGrid));
    //}

    internal override void Initialize() {
        base.Initialize();
        characterEntries = new Dictionary<ECS.Character, CharacterSummaryEntry>();
        Messenger.AddListener<ECS.Character>(Signals.CHARACTER_CREATED, AddCharacterEntry);
        //Messenger.AddListener<ECS.Character>(Signals.CHARACTER_DEATH, RemoveCharacterEntry);
        Messenger.AddListener<ECS.Character>(Signals.ROLE_CHANGED, UpdateCharacterEntry);
        Messenger.AddListener<ECS.Character>(Signals.FACTION_SET, UpdateCharacterEntry);
        sortingAction = () => OrderByName();
        //EventDelegate.Set(nameHeaderTrigger.onClick, () => SetSortingAction(OrderByName));
        //EventDelegate.Set(factionHeaderTrigger.onClick, () => SetSortingAction(OrderByFaction));
        //EventDelegate.Set(raceHeaderTrigger.onClick, () => SetSortingAction(OrderByRace));
        //EventDelegate.Set(roleHeaderTrigger.onClick, () => SetSortingAction(OrderByRole));
    }

    private void AddCharacterEntry(ECS.Character character) {
        //if (character.role.roleType != CHARACTER_ROLE.FOLLOWER) {
            GameObject newEntryGO = UIManager.Instance.InstantiateUIObject(characterEntryPrefab.name, charactersScrollRect.content);
            newEntryGO.transform.localScale = Vector3.one;
            CharacterSummaryEntry newEntry = newEntryGO.GetComponent<CharacterSummaryEntry>();
            newEntry.SetCharacter(character);
            characterEntries.Add(character, newEntry);
            //characterEntriesGrid.Reposition();
        //}
        sortingAction();
    }
    private void RemoveCharacterEntry(ECS.Character character) {
        CharacterSummaryEntry characterEntry = GetCharacterEntry(character);
        if (characterEntry != null) {
            characterEntries.Remove(character);
            ObjectPoolManager.Instance.DestroyObject(characterEntry.gameObject);
        }
        sortingAction();
    }

    private CharacterSummaryEntry GetCharacterEntry(ECS.Character character) {
        if (characterEntries.ContainsKey(character)) {
            return characterEntries[character];
        }
        return null;
    }

    private void UpdateCharacterEntry(ECS.Character character) {
        CharacterSummaryEntry charEntry = GetCharacterEntry(character);
        if (charEntry != null) {
            charEntry.UpdateCharacterInfo();
        }
        sortingAction();
    }

    #region Sorting
    private void SetSortingAction(Action action) {
        sortingAction = () => action();
        sortingAction();
    }
    public void OnClickOrderByName() {
        SetSortingAction(() => OrderByName());
    }
    public void OnClickOrderByFaction() {
        SetSortingAction(() => OrderByFaction());
    }
    public void OnClickOrderByRace() {
        SetSortingAction(() => OrderByRace());
    }
    public void OnClickOrderByRole() {
        SetSortingAction(() => OrderByRole());
    }
    private void OrderByName() {
        int index = 0;
        foreach (KeyValuePair<ECS.Character, CharacterSummaryEntry> kvp in characterEntries.OrderBy(x => x.Key.name)) {
            kvp.Value.transform.SetSiblingIndex(index);
            index++;
        }
        //for (int i = 0; i < characterEntries.Count; i++) {
        //    CharacterSummaryEntry currEntry = characterEntries[i];
        //    currEntry.OnOrderByName();
        //}
        //if (this.gameObject.activeInHierarchy) {
        //    StartCoroutine(UIManager.Instance.RepositionGrid(characterEntriesGrid));
        //}
    }
    private void OrderByFaction() {
        int index = 0;
        foreach (KeyValuePair<ECS.Character, CharacterSummaryEntry> kvp in characterEntries.OrderBy(x => x.Key.faction.name)) {
            kvp.Value.transform.SetSiblingIndex(index);
            index++;
        }
        //for (int i = 0; i < characterEntries.Count; i++) {
        //    CharacterSummaryEntry currEntry = characterEntries[i];
        //    currEntry.OnOrderByFaction();
        //}
        //if (this.gameObject.activeInHierarchy) {
        //    StartCoroutine(UIManager.Instance.RepositionGrid(characterEntriesGrid));
        //}
    }
    private void OrderByRace() {
        int index = 0;
        foreach (KeyValuePair<ECS.Character, CharacterSummaryEntry> kvp in characterEntries.OrderBy(x => x.Key.raceSetting.race.ToString())) {
            kvp.Value.transform.SetSiblingIndex(index);
            index++;
        }
        //for (int i = 0; i < characterEntries.Count; i++) {
        //    CharacterSummaryEntry currEntry = characterEntries[i];
        //    currEntry.OnOrderByRace();
        //}
        //if (this.gameObject.activeInHierarchy) {
        //    StartCoroutine(UIManager.Instance.RepositionGrid(characterEntriesGrid));
        //}
    }
    private void OrderByRole() {
        int index = 0;
        foreach (KeyValuePair<ECS.Character, CharacterSummaryEntry> kvp in characterEntries.OrderBy(x => x.Key.role.ToString())) {
            kvp.Value.transform.SetSiblingIndex(index);
            index++;
        }
        //for (int i = 0; i < characterEntries.Count; i++) {
        //    CharacterSummaryEntry currEntry = characterEntries[i];
        //    currEntry.OnOrderByRole();
        //}
        //if (this.gameObject.activeInHierarchy) {
        //    StartCoroutine(UIManager.Instance.RepositionGrid(characterEntriesGrid));
        //}
    }
    #endregion

}
