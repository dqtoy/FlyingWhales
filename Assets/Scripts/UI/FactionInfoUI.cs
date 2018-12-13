using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class FactionInfoUI : UIMenu {

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI factionNameLbl;
    [SerializeField] private TextMeshProUGUI factionDescriptionLbl;
    [SerializeField] private CharacterTokenItem leaderEntry;
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private ScrollRect propertiesScrollView;
    [SerializeField] private GameObject characterEntryPrefab;
    [SerializeField] private GameObject propertyPrefab;
    [SerializeField] private FactionEmblem emblem;
    [SerializeField] private Color evenColor;
    [SerializeField] private Color oddColor;

    internal Faction currentlyShowingFaction {
        get { return _data as Faction; }
    }

    internal override void Initialize() {
        base.Initialize();
        //Messenger.AddListener("UpdateUI", UpdateFactionInfo);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        leaderEntry.Initialize();
    }

    public override void OpenMenu() {
        base.OpenMenu();
        UpdateFactionInfo();
        UpdateFactionLeader();
        UpdateFactionCharacters();
        UpdateFactionAreas();
    }

    public override void SetData(object data) {
        base.SetData(data);
        if (isShowing) {
            UpdateFactionInfo();
        }
    }

    public void UpdateFactionInfo() {
        if(currentlyShowingFaction == null) {
            return;
        }
        factionNameLbl.text = currentlyShowingFaction.name;
        factionDescriptionLbl.text = currentlyShowingFaction.description;
        emblem.SetFaction(currentlyShowingFaction);
    }

    private void UpdateFactionLeader() {
        if (currentlyShowingFaction.leader != null && currentlyShowingFaction.leader is Character) {
            leaderEntry.gameObject.SetActive(true);
            leaderEntry.Initialize();
            leaderEntry.SetCharacter((currentlyShowingFaction.leader as Character).characterToken);
        } else {
            leaderEntry.gameObject.SetActive(false);
            leaderEntry.Reset();
        }
    }

    public void UpdateFactionCharacters() {
        Utilities.DestroyChildren(charactersScrollView.content);
        List<Character> characters = new List<Character>(currentlyShowingFaction.characters);
        if (currentlyShowingFaction.leader != null) {
            characters.Remove(currentlyShowingFaction.leader as Character);
        }
        for (int i = 0; i < characters.Count; i++) {
            Character currCharacter = characters[i];
            GameObject characterEntryGO = UIManager.Instance.InstantiateUIObject(characterEntryPrefab.name, charactersScrollView.content);
            CharacterTokenItem characterEntry = characterEntryGO.GetComponent<CharacterTokenItem>();
            characterEntry.SetCharacter(currCharacter.characterToken);
            characterEntry.Initialize();
            //if (Utilities.IsEven(i)) {
            //    characterEntry.SetBGColor(evenColor);
            //} else {
            //    characterEntry.SetBGColor(oddColor);
            //}
        }
    }
    public void UpdateFactionAreas() {
        Utilities.DestroyChildren(propertiesScrollView.content);
        for (int i = 0; i < currentlyShowingFaction.ownedAreas.Count; i++) {
            Area currArea = currentlyShowingFaction.ownedAreas[i];
            GameObject propertyGO = UIManager.Instance.InstantiateUIObject(propertyPrefab.name, propertiesScrollView.content);
            FactionPropertyItem property = propertyGO.GetComponent<FactionPropertyItem>();
            property.SetArea(currArea);
        }
    }
    private CharacterTokenItem GetCharacterSummary(Character character) {
        CharacterTokenItem[] entries = Utilities.GetComponentsInDirectChildren<CharacterTokenItem>(charactersScrollView.content.gameObject);
        for (int i = 0; i < entries.Length; i++) {
            CharacterTokenItem currEntry = entries[i];
            if (currEntry.character.id == character.id) {
                return currEntry;
            }
        }
        return null;
    }
    //	public void OnClickCloseBtn(){
    ////		UIManager.Instance.playerActionsUI.HidePlayerActionsUI ();
    //		HideMenu ();
    //	}

    #region Handlers
    private void OnCharacterDied(Character characterThatDied) {
        if (isShowing) {
            if (leaderEntry.gameObject.activeSelf) {
                if (leaderEntry.character.id == characterThatDied.id) {
                    leaderEntry.gameObject.SetActive(false);
                    leaderEntry.Reset();
                    return;
                }
            }
            CharacterTokenItem entry = GetCharacterSummary(characterThatDied);
            if (entry != null) {
                ObjectPoolManager.Instance.DestroyObject(entry.gameObject);
            }
        }
    }
    #endregion
}
