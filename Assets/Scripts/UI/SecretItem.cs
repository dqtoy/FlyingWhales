using ECS;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SecretItem : MonoBehaviour, IPointerClickHandler {

    private Secret secret;
    private Character owner;

    [SerializeField] private Image secretImage; 

    public void Initialize() {
        //Messenger.AddListener<Character>(Signals.CHARACTER_INSPECTED, OnCharacterInspected);
    }

    public void SetSecret(Secret secret, Character owner) {
        this.secret = secret;
        this.owner = owner;
        UpdateVisuals();
    }

    //private void OnCharacterInspected(Character character) {
    //    if (character.id == owner.id) {
    //        UpdateVisuals();
    //    }
    //}

    private void UpdateVisuals() {
        if (this.secret.isRevealed) {
            secretImage.color = Color.white;
        } else {
            secretImage.color = Color.gray;
        }
    }

    public void ShowSecretInfo() {
        if (secret.isRevealed) {
            UIManager.Instance.ShowSmallInfo(secret.displayText + "\n" + secret.description);
        } else {
            PlayerAbility pa = PlayerManager.Instance.player.GetAbility("Reveal Secret");
            UIManager.Instance.ShowSmallInfo("Unlock secret? Cost is: " + pa.GetMagicCostString(UIManager.Instance.characterInfoUI.currentlyShowingCharacter));
        }
        
    }
    public void HideSecretInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    public void OnPointerClick(PointerEventData eventData) {
        if (secret.isRevealed) {
            return; //secret is already revealed
        }
        if (!owner.isBeingInspected) {
            return;
        }
        RevealSecret rs = PlayerManager.Instance.player.GetAbility("Reveal Secret") as RevealSecret;
        if (!rs.CanBeActivated(owner)) {
            Messenger.Broadcast<string, MESSAGE_BOX_MODE, bool>(Signals.SHOW_POPUP_MESSAGE, "You do not have enough magic!", MESSAGE_BOX_MODE.MESSAGE_ONLY, true);
            return;
        }
        rs.Activate(owner, secret);
    }
}
