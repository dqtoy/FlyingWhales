using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;

public class CharactersSummaryUI : UIMenu {

    //private Action sortingAction;

    [SerializeField] private GameObject characterEntryPrefab;
    [SerializeField] private ScrollRect charactersScrollRect;
    [SerializeField] private Color evenColor;
    [SerializeField] private Color oddColor;

    private Dictionary<ECS.Character, CharacterSummaryEntry> characterEntries;


    internal override void Initialize() {
        base.Initialize();
        characterEntries = new Dictionary<ECS.Character, CharacterSummaryEntry>();
        Messenger.AddListener<ECS.Character>(Signals.CHARACTER_CREATED, AddCharacterEntry);
        //Messenger.AddListener<ECS.Character>(Signals.CHARACTER_DEATH, RemoveCharacterEntry);
        Messenger.AddListener<ECS.Character>(Signals.ROLE_CHANGED, UpdateCharacterEntry);
        Messenger.AddListener<ECS.Character>(Signals.FACTION_SET, UpdateCharacterEntry);
        //sortingAction = () => OrderByName();
    }

    private void AddCharacterEntry(ECS.Character character) {
        GameObject newEntryGO = UIManager.Instance.InstantiateUIObject(characterEntryPrefab.name, charactersScrollRect.content);
        newEntryGO.transform.localScale = Vector3.one;
        CharacterSummaryEntry newEntry = newEntryGO.GetComponent<CharacterSummaryEntry>();
        newEntry.SetCharacter(character);
        newEntry.Initialize();
        characterEntries.Add(character, newEntry);
        int index = characterEntries.Count - 1;
        if (index % 2 == 0) {
            newEntry.SetBGColor(evenColor);
        } else {
            newEntry.SetBGColor(oddColor);
        }
        //sortingAction();
    }
    private void RemoveCharacterEntry(ECS.Character character) {
        CharacterSummaryEntry characterEntry = GetCharacterEntry(character);
        if (characterEntry != null) {
            characterEntries.Remove(character);
            ObjectPoolManager.Instance.DestroyObject(characterEntry.gameObject);
            UpdateListColors();
        }
        //sortingAction();
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
        //sortingAction();
    }
    private void UpdateListColors() {
        for (int i = 0; i < characterEntries.Values.Count; i++) {
            CharacterSummaryEntry entry = characterEntries.Values.ElementAt(i);
            if (i % 2 == 0) {
                entry.SetBGColor(evenColor);
            } else {
                entry.SetBGColor(oddColor);
            }
        }
    }

    #region Sorting
    //private void SetSortingAction(Action action) {
    //    sortingAction = () => action();
    //    sortingAction();
    //}
    //public void OnClickOrderByName() {
    //    SetSortingAction(() => OrderByName());
    //}
    //public void OnClickOrderByFaction() {
    //    SetSortingAction(() => OrderByFaction());
    //}
    //public void OnClickOrderByRace() {
    //    SetSortingAction(() => OrderByRace());
    //}
    //public void OnClickOrderByRole() {
    //    SetSortingAction(() => OrderByRole());
    //}
    private void OrderByName() {
        int index = 0;
        foreach (KeyValuePair<ECS.Character, CharacterSummaryEntry> kvp in characterEntries.OrderBy(x => x.Key.name)) {
            kvp.Value.transform.SetSiblingIndex(index);
            index++;
        }
    }
    private void OrderByFaction() {
        int index = 0;
        foreach (KeyValuePair<ECS.Character, CharacterSummaryEntry> kvp in characterEntries.OrderBy(x => x.Key.faction.name)) {
            kvp.Value.transform.SetSiblingIndex(index);
            index++;
        }
    }
    private void OrderByRace() {
        int index = 0;
        foreach (KeyValuePair<ECS.Character, CharacterSummaryEntry> kvp in characterEntries.OrderBy(x => x.Key.raceSetting.race.ToString())) {
            kvp.Value.transform.SetSiblingIndex(index);
            index++;
        }
    }
    private void OrderByRole() {
        int index = 0;
        foreach (KeyValuePair<ECS.Character, CharacterSummaryEntry> kvp in characterEntries.OrderBy(x => x.Key.role.ToString())) {
            kvp.Value.transform.SetSiblingIndex(index);
            index++;
        }
    }
    #endregion

}
