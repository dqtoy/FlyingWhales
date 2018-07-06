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
    [SerializeField] private TextMeshProUGUI partyInfoLbl;
    [SerializeField] private ScrollRect infoScrollView;

    internal NewParty currentlyShowingParty {
        get { return _data as NewParty; }
    }

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener(Signals.UPDATE_UI, UpdatePartyInfo);
    }

    public override void OpenMenu() {
        base.OpenMenu();
        UpdatePartyInfo();
    }
    public override void SetData(object data) {
        base.SetData(data);
        if (isShowing) {
            UpdatePartyInfo();
        }
    }

    public void UpdatePartyInfo() {
        if(currentlyShowingParty == null) {
            return;
        }
        string text = string.Empty;
		if(!currentlyShowingParty.isDead){
            text += "<b>Name:</b> " + currentlyShowingParty.name;
            text += "\n<b>Characters:</b>";
            for (int i = 0; i < currentlyShowingParty.icharacters.Count; i++) {
                ICharacter member = currentlyShowingParty.icharacters[i];
                text += "\n" + member.urlName;
            }
		} else {
			text += "Dead Party";
		}
        partyInfoLbl.text = text;
        //infoScrollView.ResetPosition();
    }
}
