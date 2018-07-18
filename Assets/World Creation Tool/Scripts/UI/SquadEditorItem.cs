using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquadEditorItem : MonoBehaviour {

    public Squad squad;

    [SerializeField] private InputField squadNameField;
    [SerializeField] private RectTransform leaderContainer;
    [SerializeField] private RectTransform membersContainer;

    public void SetSquad(Squad squad) {
        this.squad = squad;
        squadNameField.text = squad.name;
    }

    public void OnLeaderDropped(Transform transfrom) {
        CharacterSquadEditorItem charItem = transform.GetComponent<CharacterSquadEditorItem>();
        if (charItem != null) {
            squad.SetLeader(charItem.character);
        }
    }

    public void OnMemberDropped(Transform transfrom) {
        CharacterSquadEditorItem charItem = transform.GetComponent<CharacterSquadEditorItem>();
        if (charItem != null) {
            squad.AddMember(charItem.character);
        }
    }
}
