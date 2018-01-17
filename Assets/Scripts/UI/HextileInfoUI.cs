using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HextileInfoUI : UIMenu {

    internal bool isShowing;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private UILabel hexTileInfoLbl;

    internal HexTile currentlyShowingHexTile;

    internal override void Initialize() {
        Messenger.AddListener("UpdateUI", UpdateHexTileInfo);
        tweenPos.AddOnFinished(() => UpdateHexTileInfo());
    }

    public void ShowHexTileInfo() {
        isShowing = true;
		this.gameObject.SetActive (true);
//        tweenPos.PlayForward();
    }
    public void HideHexTileInfo() {
        isShowing = false;
//        tweenPos.PlayReverse();
		this.gameObject.SetActive (false);
    }

	public void SetHexTileAsActive(HexTile hexTile) {
		if(currentlyShowingHexTile != null){
			currentlyShowingHexTile.clickHighlightGO.SetActive (false);
		}
        currentlyShowingHexTile = hexTile;
		hexTile.clickHighlightGO.SetActive (true);
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

		text += "\n[b]Landmark Name:[/b] ";
		if(currentlyShowingHexTile.landmarkOnTile != null){
			text += currentlyShowingHexTile.landmarkOnTile.landmarkName;
		}else{
			text += "NONE";
		}

		text += "\n[b]Base Landmark Type:[/b] ";
		if(currentlyShowingHexTile.landmarkOnTile != null){
			string baseLandmarkType = Utilities.GetBaseLandmarkType (currentlyShowingHexTile.landmarkOnTile.specificLandmarkType).ToString ();
			text += "[url=" + currentlyShowingHexTile.landmarkOnTile.id + "_landmark" + "]" + baseLandmarkType + "[/url]";
		}else{
			text += "NONE";
		}

		text += "\n[b]Road Type:[/b] " + currentlyShowingHexTile.roadType.ToString ();;

        hexTileInfoLbl.text = text;
    }
}
