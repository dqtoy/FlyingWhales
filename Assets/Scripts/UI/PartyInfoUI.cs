using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class PartyInfoUI : UIMenu {

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private TextMeshProUGUI partyGeneralInfoLbl;
    [SerializeField] private ScrollRect infoScrollView;
    [SerializeField] private GameObject content;

    internal NewParty currentlyShowingParty {
        get { return _data as NewParty; }
    }

    //internal override void Initialize() {
    //    base.Initialize();
    //    //Messenger.AddListener(Signals.UPDATE_UI, UpdatePartyInfo);
    //}

    public override void OpenMenu() {
        if (!isShowing) {
            ResetPartyCharacters();
            UpdatePartyInfo();
        }
        base.OpenMenu();
    }
    public override void SetData(object data) {
        base.SetData(data);
        if (isShowing) {
            ResetPartyCharacters();
            UpdatePartyInfo();
        }
    }

    public void UpdatePartyInfo() {
        if(currentlyShowingParty == null) {
            return;
        }
		if(!currentlyShowingParty.isDead){
            UpdateGeneralInfo();
            UpdatePartyCharacters();
        }
        
        //infoScrollView.ResetPosition();
    }
    private void UpdateGeneralInfo() {
        string text = string.Empty;
        text += "<b>Name:</b> " + currentlyShowingParty.name;
        text += "\n<b>Party Power:</b> " + currentlyShowingParty.computedPower;
        partyGeneralInfoLbl.text = text;
    }
    private void UpdatePartyCharacters() {
        for (int i = 0; i < currentlyShowingParty.icharacters.Count; i++) {
            ICharacter member = currentlyShowingParty.icharacters[i];
            if(member.characterPortrait.transform.parent != content.transform) {
                member.characterPortrait.transform.SetParent(content.transform);
            }
            member.characterPortrait.SetImageSize(IMAGE_SIZE.X36, false);
            member.characterPortrait.gameObject.SetActive(true);
            member.characterPortrait.ToggleNameLabel(true);
        }
    }
    private void ResetPartyCharacters() {
        foreach (Transform child in content.transform) {
            child.gameObject.SetActive(false);
            //child.parent = UIManager.Instance.characterPortraitsParent;
        }
    }
}
