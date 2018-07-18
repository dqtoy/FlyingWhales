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
        Messenger.AddListener<ECS.Character>(Signals.CHARACTER_CREATED, OnCharacterCreated);
        Messenger.AddListener<ECS.Character>(Signals.CHARACTER_REMOVED, OnCharacterRemoved);
        items = new List<CharacterSquadEditorItem>();
        squadItems = new List<SquadEditorItem>();
    }

    public void OnCharacterCreated(ECS.Character newCharacter) {
        GameObject characterGO = GameObject.Instantiate(characterSquadEditorPrefab, charactersScrollView.content);
        CharacterSquadEditorItem characterItem = characterGO.GetComponent<CharacterSquadEditorItem>();
        characterItem.SetCharacter(newCharacter);
        characterItem.gameObject.GetComponent<Draggable>().parentWhileDragging = this.transform;
        items.Add(characterItem);
    }
    public void OnCharacterRemoved(ECS.Character removedCharacter) {
        CharacterSquadEditorItem characterItem = GetItem(removedCharacter);
        items.Remove(characterItem);
        GameObject.Destroy(characterItem.gameObject);
    }

    private CharacterSquadEditorItem GetItem(ECS.Character character) {
        for (int i = 0; i < items.Count; i++) {
            CharacterSquadEditorItem currItem = items[i];
            if (currItem.character.id == character.id) {
                return currItem;
            }
        }
        return null;
    }

    public void CreateNewSquad() {
        GameObject squadGO = GameObject.Instantiate(squadEditorPrefab, squadsScrollView.content);
        SquadEditorItem squadItem = squadGO.GetComponent<SquadEditorItem>();
        squadItem.SetSquad(new Squad());
        squadItems.Add(squadItem);
    }

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
