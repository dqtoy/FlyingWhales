using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterAvatarClick : MonoBehaviour {
    public CharacterAvatar characterAvatar;

    void OnMouseDown() {
        if (UIManager.Instance.IsMouseOnUI()) {
            return;
        }
        if (characterAvatar.party.characters.Count > 1) {
            UIManager.Instance.ShowPartyInfo(characterAvatar.party);
        } else {
            UIManager.Instance.ShowCharacterInfo(characterAvatar.party.mainCharacter);
        }
    }
}
