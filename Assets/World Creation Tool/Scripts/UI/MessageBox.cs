using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour {
    [SerializeField] private Text windowTitleLbl;
    [SerializeField] private GameObject messageGO;
    [SerializeField] private Text messageLbl;
    [SerializeField] private GameObject yesNoGO;
    [SerializeField] private GameObject okGO;
    [SerializeField] private Button yesBtn;
    [SerializeField] private Button noBtn;
    [SerializeField] private Button okBtn;

    [Space(10)]
    [Header("Input Message Box")]
    [SerializeField] private GameObject inputGO;
    [SerializeField] private Text inputInstructionLbl;
    [SerializeField] private InputField inputField;

    public void ShowMessageBox(MESSAGE_BOX mode, string windowTitle, string windowMessage, UnityAction onClickYes = null, UnityAction onClickNo = null, UnityAction onClickOK = null) {
        windowTitleLbl.text = windowTitle;
        messageLbl.text = windowMessage;
        inputGO.SetActive(false);
        messageGO.SetActive(true);
        if (mode == MESSAGE_BOX.YES_NO) {
            yesNoGO.SetActive(true);
            okGO.SetActive(false);
        } else if (mode == MESSAGE_BOX.OK) {
            yesNoGO.SetActive(false);
            okGO.SetActive(true);
        }
        yesBtn.onClick.RemoveAllListeners();
        noBtn.onClick.RemoveAllListeners();
        okBtn.onClick.RemoveAllListeners();
        if (onClickYes != null) {
            yesBtn.onClick.AddListener(onClickYes);
        }
        yesBtn.onClick.AddListener(HideMessageBox);
        if (onClickNo != null) {
            noBtn.onClick.AddListener(onClickNo);
        }
        noBtn.onClick.AddListener(HideMessageBox);
        if (onClickOK != null) {
            okBtn.onClick.AddListener(onClickOK);
        }
        okBtn.onClick.AddListener(HideMessageBox);
        this.gameObject.SetActive(true);
    }

    public void ShowInputMessageBox(string windowTitle, string windowMessage, UnityAction<string> onClickOk) {
        windowTitleLbl.text = windowTitle;
        inputInstructionLbl.text = windowMessage;

        inputGO.SetActive(true);
        messageGO.SetActive(false);
        yesNoGO.SetActive(false);
        okGO.SetActive(true);

        yesBtn.onClick.RemoveAllListeners();
        noBtn.onClick.RemoveAllListeners();
        okBtn.onClick.RemoveAllListeners();

        if (onClickOk != null) {
            okBtn.onClick.AddListener(() => onClickOk(inputField.text));
        }
        //okBtn.onClick.AddListener(HideMessageBox);
        this.gameObject.SetActive(true);
    }

    public void HideMessageBox() {
        this.gameObject.SetActive(false);
    }
}

public enum MESSAGE_BOX {
    YES_NO,
    OK,
}
