using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupMessageBox : MonoBehaviour{

    [SerializeField] private Vector2 hiddenPos;
    [SerializeField] private Vector2 showingPos;

    [SerializeField] private EnvelopContentUnityUI envelopContent;
    [SerializeField] private TextMeshProUGUI messageLbl;
    [SerializeField] private TweenPosition tweenPosition;

    private void Awake() {
        Messenger.AddListener<string, bool>(Signals.SHOW_POPUP_MESSAGE, ShowMessage);
        Messenger.AddListener(Signals.HIDE_POPUP_MESSAGE, HideMessage);
    }

    private void ShowMessage(string message, bool autoHide) {
        messageLbl.text = message;
        envelopContent.Execute();
        tweenPosition.ResetToBeginning();
        tweenPosition.PlayForward();
    }

    private void HideMessage() {
        tweenPosition.PlayReverse();
    }
}
