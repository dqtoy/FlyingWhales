using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PartyInfoUI : UIMenu {

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private UILabel partyInfoLbl;
    [SerializeField] private UIScrollView infoScrollView;

    internal Party currentlyShowingParty {
        get { return _data as Party; }
    }

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener("UpdateUI", UpdatePartyInfo);
        tweenPos.AddOnFinished(() => UpdatePartyInfo());
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
        text += "[b]Name:[/b] " + currentlyShowingParty.name;
		text += "\n[b]IsDisbanded:[/b] " + currentlyShowingParty.isDisbanded.ToString ();
		text += "\n[b]IsOpen:[/b] " + currentlyShowingParty.isOpen.ToString ();
		text += "\n[b]IsFull:[/b] " + currentlyShowingParty.isFull.ToString ();
		text += "\n[b]Party Leader:[/b] " + currentlyShowingParty.partyLeader.urlName;
		text += "\n[b]Members:[/b] ";
		if((currentlyShowingParty.partyMembers.Count - 1) > 0){
			for (int i = 0; i < currentlyShowingParty.partyMembers.Count; i++) {
				if(currentlyShowingParty.partyMembers[i].id != currentlyShowingParty.partyLeader.id){
					ECS.Character member = currentlyShowingParty.partyMembers [i];
					text += "\n" + member.urlName;
				}
			}
		} else {
			text += "NONE";
		}

		text += "\n[b]Prisoners:[/b] ";
		if(currentlyShowingParty.prisoners.Count > 0){
			for (int i = 0; i < currentlyShowingParty.prisoners.Count; i++) {
				ECS.Character prisoner = currentlyShowingParty.prisoners [i];
				text += "\n" + prisoner.prisonerName;
			}
		} else {
			text += "NONE";
		}

		text += "\n[b]Current Task:[/b] ";
		if (currentlyShowingParty.currentTask != null) {
			//if (currentlyShowingParty.currentTask.taskType == TASK_TYPE.QUEST) {
			//	OldQuest.Quest currQuest = (OldQuest.Quest)currentlyShowingParty.currentTask;
			//	text += " (" + currQuest.urlName + ")";
			//} else {
				text += " (" + currentlyShowingParty.currentTask.taskType.ToString () + ")";
			//}
		} else {
			text += "NONE";
		}
		text += "\n[b]Civilians:[/b] " + currentlyShowingParty.civilians.ToString ();
        foreach (KeyValuePair<RACE, int> kvp in currentlyShowingParty.civiliansByRace) {
            if (kvp.Value > 0) {
                text += "\n" + kvp.Key.ToString() + " - " + kvp.Value.ToString();
            }
        }

        //text += "\n[b]Materials:[/b] ";
        //if (currentlyShowingParty.materialInventory.Sum(x => x.Value) > 0) {
        //    text += "\n";
        //    foreach (KeyValuePair<MATERIAL, int> kvp in currentlyShowingParty.materialInventory) {
        //        if (kvp.Value > 0) {
        //            text += kvp.Key.ToString() + " - " + kvp.Value.ToString();
        //        }
        //    }
        //} else {
        //    text += "NONE";
        //}

        partyInfoLbl.text = text;
        infoScrollView.ResetPosition();
    }
	public void CenterCameraOnParty() {
		CameraMove.Instance.CenterCameraOn(currentlyShowingParty.currLocation.gameObject);
	}
//	public void OnClickCloseBtn(){
////		UIManager.Instance.playerActionsUI.HidePlayerActionsUI ();
//		HideMenu ();
//	}
}
