using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditSquadsMenu : MonoBehaviour {

    [SerializeField] private GameObject squadEditorPrefab;
    [SerializeField] private GameObject characterSquadEditorPrefab;
    [SerializeField] private ScrollRect squadsScrollView;
    [SerializeField] private ScrollRect charactersScrollView;

    private List<CharacterSquadEditorItem> items;
    private List<SquadEditorItem> squadItems;

    public void Initialize() {
        Messenger.AddListener<Character>(Signals.CHARACTER_CREATED, OnCharacterCreated);
        Messenger.AddListener<Character>(Signals.CHARACTER_REMOVED, OnCharacterRemoved);

        Messenger.AddListener<Squad>(Signals.SQUAD_CREATED, OnSquadCreated);
        Messenger.AddListener<Squad>(Signals.SQUAD_DELETED, OnSquadDeleted);

        Messenger.AddListener<Character, Squad>(Signals.SQUAD_MEMBER_REMOVED, OnCharacterRemovedFromSquad);
        Messenger.AddListener<Character, Squad>(Signals.SQUAD_MEMBER_ADDED, OnCharacterAddedToSquad);

        Messenger.AddListener<Character, Squad>(Signals.SQUAD_LEADER_SET, OnSquadLeaderSet);

        items = new List<CharacterSquadEditorItem>();
        squadItems = new List<SquadEditorItem>();
    }

    public void OnCharacterCreated(Character newCharacter) {
        GameObject characterGO = GameObject.Instantiate(characterSquadEditorPrefab, charactersScrollView.content);
        CharacterSquadEditorItem characterItem = characterGO.GetComponent<CharacterSquadEditorItem>();
        characterItem.SetCharacter(newCharacter);
        characterItem.gameObject.GetComponent<Draggable>().parentWhileDragging = this.transform;
        items.Add(characterItem);
    }
    public void OnCharacterRemoved(Character removedCharacter) {
        CharacterSquadEditorItem characterItem = GetCharacterSquadItem(removedCharacter);
        items.Remove(characterItem);
        GameObject.Destroy(characterItem.gameObject);
    }
    public void OnCharacterRemovedFromSquad(Character character, Squad squad) {
        CharacterSquadEditorItem item = GetCharacterSquadItem(character);
        if (item != null && item.transform.parent != charactersScrollView.content) {
            item.transform.SetParent(charactersScrollView.content);
        }
    }
    public void OnCharacterAddedToSquad(Character character, Squad squad) {
        if (squad.squadLeader.id != character.id) {
            CharacterSquadEditorItem item = GetCharacterSquadItem(character);
            SquadEditorItem squadItem = GetSquadItem(squad);
            if (item.transform.parent != squadItem.membersContainer) {
                item.transform.SetParent(squadItem.membersContainer);
            }
        }
    }
    public void OnSquadLeaderSet(Character character, Squad squad) {
        CharacterSquadEditorItem item = GetCharacterSquadItem(character);
        SquadEditorItem squadItem = GetSquadItem(squad);
        if (item.transform.parent != squadItem.leaderContainer) {
            item.transform.SetParent(squadItem.leaderContainer);
            item.transform.localPosition = Vector3.zero;
        }
    }

    private CharacterSquadEditorItem GetCharacterSquadItem(Character character) {
        for (int i = 0; i < items.Count; i++) {
            CharacterSquadEditorItem currItem = items[i];
            if (currItem.character.id == character.id) {
                return currItem;
            }
        }
        return null;
    }
    private SquadEditorItem GetSquadItem(Squad squad) {
        for (int i = 0; i < squadItems.Count; i++) {
            SquadEditorItem currItem = squadItems[i];
            if (currItem.squad.id == squad.id) {
                return currItem;
            }
        }
        return null;
    }

    private void OnSquadCreated(Squad squad) {
        GameObject squadGO = GameObject.Instantiate(squadEditorPrefab, squadsScrollView.content);
        SquadEditorItem squadItem = squadGO.GetComponent<SquadEditorItem>();
        squadItem.SetSquad(squad);
        squadItems.Add(squadItem);
    }
    private void OnSquadDeleted(Squad squad) {
        SquadEditorItem squadItem = GetSquadItem(squad);
        GameObject.Destroy(squadItem.gameObject);
    }

    //public void CreateNewSquad() {
    //    CharacterManager.Instance.CreateNewSquad();
    //}

    public void OnItemDraggedBack(Transform transform) {
        CharacterSquadEditorItem item = transform.GetComponent<CharacterSquadEditorItem>();
        if (item != null) {
            for (int i = 0; i < squadItems.Count; i++) {
                Squad currSquad = squadItems[i].squad;
                if (currSquad.squadLeader.id == item.character.id) {
                    currSquad.SetLeader(null);
                }
                if (currSquad.squadMembers.Contains(item.character)) {
                    currSquad.RemoveMember(item.character);
                }
            }
        }
    }

}
