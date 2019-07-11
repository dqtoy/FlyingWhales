using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterAvatarClick : MonoBehaviour {
    public CharacterAvatar characterAvatar;

    void OnMouseDown() {
        if (UIManager.Instance.IsMouseOnUI()) {
            return;
        }
        UIManager.Instance.ShowCharacterInfo(characterAvatar.party.mainCharacter);
    }
}
