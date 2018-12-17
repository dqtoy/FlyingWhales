using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TokenItem : MonoBehaviour {

    private Token token;

    [SerializeField] private Image intelImage;
    [SerializeField] private GameObject lockedGO;

    public void SetToken(Token token) {
        this.token = token;
        UpdateVisuals();
        Messenger.AddListener<Token>(Signals.TOKEN_ADDED, OnTokenAddedToPlayer);
    }

    private void UpdateVisuals() {
        if (token.isObtainedByPlayer) {
            lockedGO.SetActive(false);
        } else {
            lockedGO.SetActive(true);
        }
    }
    public void ShowTokenInfo() {
        if (token.isObtainedByPlayer) {
            //UIManager.Instance.ShowSmallInfo(intel.description);
        }
    }
    public void HideTokenInfo() {
        UIManager.Instance.HideSmallInfo();
    }

    private void OnTokenAddedToPlayer(Token token) {
        UpdateVisuals();
    }

    public void Reset() {
        Messenger.RemoveListener<Token>(Signals.TOKEN_ADDED, OnTokenAddedToPlayer);
    }
}
