using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HextileInfoUI : UIMenu {

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private UILabel hexTileInfoLbl;
    [SerializeField] private UIScrollView infoScrollView;

    internal HexTile currentlyShowingHexTile {
        get { return _data as HexTile; }
    }

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener("UpdateUI", UpdateHexTileInfo);
        tweenPos.AddOnFinished(() => UpdateHexTileInfo());
    }

    public override void OpenMenu() {
        base.OpenMenu();
        UpdateHexTileInfo();
    }

    public override void SetData(object data) {
        if (currentlyShowingHexTile != null) {
            currentlyShowingHexTile.clickHighlightGO.SetActive(false);
        }
        base.SetData(data); //replace this existing data
        currentlyShowingHexTile.clickHighlightGO.SetActive(true);
        if (isShowing) {
            UpdateHexTileInfo();
        }
    }

    public void UpdateHexTileInfo() {
        if(currentlyShowingHexTile == null) {
            return;
        }
        string text = string.Empty;
        text += "[b]Name:[/b] " + currentlyShowingHexTile.tileName;
		text += "\n[b]Biome:[/b] " + currentlyShowingHexTile.biomeType.ToString();
		text += "\n[b]Elevation:[/b] " + currentlyShowingHexTile.elevationType.ToString ();
        text += "\n[b]Material:[/b] " + currentlyShowingHexTile.materialOnTile.ToString();

        text += "\n[b]Landmark Name:[/b] ";
		if(currentlyShowingHexTile.landmarkOnTile != null){
			text += currentlyShowingHexTile.landmarkOnTile.landmarkName;
		}else{
			text += "NONE";
		}

		text += "\n[b]Base Landmark Type:[/b] ";
		if(currentlyShowingHexTile.landmarkOnTile != null){
			text += currentlyShowingHexTile.landmarkOnTile.urlName;
		}else{
			text += "NONE";
		}

		text += "\n[b]Road Type:[/b] " + currentlyShowingHexTile.roadType.ToString ();;

		text += "\n[b]Characters: [/b] ";
		if (currentlyShowingHexTile.charactersAtLocation.Count > 0) {
			for (int i = 0; i < currentlyShowingHexTile.charactersAtLocation.Count; i++) {
				if (currentlyShowingHexTile.charactersAtLocation[i] is ECS.Character) {
					ECS.Character currChar = (ECS.Character)currentlyShowingHexTile.charactersAtLocation [i];
					text += "\n" + currChar.urlName + " - " + (currChar.characterClass != null ? currChar.characterClass.className : "NONE") + "/" + (currChar.role != null ? currChar.role.roleType.ToString () : "NONE");
					if (currChar.currentTask != null) {
						if (currChar.currentTask.taskType == TASK_TYPE.QUEST) {
							OldQuest.Quest currQuest = (OldQuest.Quest)currChar.currentTask;
							text += " (" + currQuest.urlName + ")";
						} else {
							text += " (" + currChar.currentTask.taskType.ToString () + ")";
						}
					}
				} else if (currentlyShowingHexTile.charactersAtLocation[i] is Party) {
					Party currParty = (Party)currentlyShowingHexTile.charactersAtLocation [i];
					text += "\n" + currParty.urlName + " - " + (currParty.currentTask != null ? currParty.currentTask.ToString () : "NONE");
				}
			}
		} else {
			text += "NONE";
		}
        hexTileInfoLbl.text = text;
        infoScrollView.ResetPosition();
    }

//	public void OnClickCloseBtn(){
////		UIManager.Instance.playerActionsUI.HidePlayerActionsUI ();
//		HideMenu ();
//	}
}
