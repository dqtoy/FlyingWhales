using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SecretItem : MonoBehaviour {

    private Secret secret;

    public void SetSecret(Secret secret) {
        this.secret = secret;
    }

    public void ShowSecretInfo() {
        UIManager.Instance.ShowSmallInfo(secret.displayText);
    }
    public void HideSecretInfo() {
        UIManager.Instance.HideSmallInfo();
    }
}
