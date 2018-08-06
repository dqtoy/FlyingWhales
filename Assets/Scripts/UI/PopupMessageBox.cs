using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupMessageBox : UIMenu{

    public static PopupMessageBox Instance = null;

    [SerializeField] private Vector2 hiddenPos;
    [SerializeField] private Vector2 showingPos;

    [SerializeField] private EnvelopContentUnityUI envelopContent;
    [SerializeField] private TextMeshProUGUI messageLbl;
    [SerializeField] private TweenPosition tweenPosition;

    [SerializeField] private Button yesBtn;
    [SerializeField] private Button noBtn;

    private void Awake() {
        Instance = this;
    }

    internal override void Initialize() {
        Messenger.AddListener<string, MESSAGE_BOX_MODE, bool>(Signals.SHOW_POPUP_MESSAGE, ShowMessage);
        Messenger.AddListener(Signals.HIDE_POPUP_MESSAGE, HideMessage);
    }

    private void ShowMessage(string message, MESSAGE_BOX_MODE mode, bool autoHide) {
        messageLbl.text = message;
        envelopContent.Execute();

        if (mode == MESSAGE_BOX_MODE.MESSAGE_ONLY) {
            yesBtn.gameObject.SetActive(false);
            noBtn.gameObject.SetActive(false);
        } else if (mode == MESSAGE_BOX_MODE.YES_NO) {
            yesBtn.gameObject.SetActive(true);
            noBtn.gameObject.SetActive(true);
            SetNoAction(() => HideMessage());
        }

        tweenPosition.from = hiddenPos;
        tweenPosition.to = showingPos;
        tweenPosition.ResetToBeginning();
        tweenPosition.PlayForward();
        if (autoHide) {
            StartCoroutine(AutoHideAfterSeconds());
        }
    }

    private IEnumerator AutoHideAfterSeconds() {
        yield return new WaitForSeconds(2f);
        HideMessage();
    }

    public void SetYesAction(UnityAction yesAction) {
        yesBtn.onClick.RemoveAllListeners();
        AddYesAction(yesAction);
    }
    public void AddYesAction(UnityAction yesAction) {
        yesBtn.onClick.AddListener(yesAction);
    }
    public void SetNoAction(UnityAction noAction) {
        noBtn.onClick.RemoveAllListeners();
        AddNoAction(noAction);
    }
    public void AddNoAction(UnityAction noAction) {
        noBtn.onClick.AddListener(noAction);
    }

    private void HideMessage() {
        tweenPosition.from = showingPos;
        tweenPosition.to = hiddenPos;
        tweenPosition.ResetToBeginning();
        tweenPosition.PlayForward();
    }
}
