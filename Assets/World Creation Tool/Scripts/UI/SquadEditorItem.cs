using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquadEditorItem : MonoBehaviour {

    public Squad squad;

    [SerializeField] private InputField squadNameField;
    public RectTransform leaderContainer;
    public RectTransform membersContainer;

    public void SetSquad(Squad squad) {
        this.squad = squad;
        squadNameField.text = squad.name;
    }

    public void OnLeaderDropped(Transform transform) {
        CharacterSquadEditorItem charItem = transform.GetComponent<CharacterSquadEditorItem>();
        if (charItem != null) {
            squad.SetLeader(charItem.character);
        }
    }

    public void OnMemberDropped(Transform transform) {
        CharacterSquadEditorItem charItem = transform.GetComponent<CharacterSquadEditorItem>();
        if (charItem != null) {
            squad.AddMember(charItem.character);
        }
    }

    public void OnSquadNameChange(string newName) {
        squad.SetName(newName);
    }

    public void DeleteSquad() {
        CharacterManager.Instance.DeleteSquad(squad);
    }
}
