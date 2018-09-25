using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SecretItem : MonoBehaviour, IPointerClickHandler {

    private Secret secret;

    public void SetSecret(Secret secret) {
        this.secret = secret;
    }

    public void ShowSecretInfo() {
        if (secret.isRevealed) {
            UIManager.Instance.ShowSmallInfo(secret.displayText);
        } else {
            PlayerAbility pa = PlayerManager.Instance.player.GetAbility("Reveal Secret");
            UIManager.Instance.ShowSmallInfo("Unlock secret? Cost is: " + pa.GetMagicCostString(UIManager.Instance.characterInfoUI.currentlyShowingCharacter));
        }
        
    }
    public void HideSecretInfo() {
        UIManager.Instance.HideSmallInfo();
    }

    public void OnPointerClick(PointerEventData eventData) {
        secret.RevealSecret();
    }
}
