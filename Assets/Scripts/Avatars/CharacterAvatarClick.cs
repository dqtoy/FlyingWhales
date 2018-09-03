using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class CharacterAvatarClick : MonoBehaviour {
    public CharacterAvatar characterAvatar;

    void OnMouseDown() {
        if (UIManager.Instance.IsMouseOnUI()) {
            return;
        }
        if (characterAvatar.party.icharacters.Count > 1) {
            UIManager.Instance.ShowPartyInfo(characterAvatar.party);
        } else {
            UIManager.Instance.ShowCharacterInfo(characterAvatar.party.mainCharacter as Character);
        }
    }
}
