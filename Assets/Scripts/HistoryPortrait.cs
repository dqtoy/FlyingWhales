using UnityEngine;
using System.Collections;

public class HistoryPortrait : MonoBehaviour {

//	public UI2DSprite kingdomColorGO;
	public UILabel dateLbl;
	public UI2DSprite iconSprite;

	public Sprite[] icons;
	public History history;

	public void SetHistory(History history){
		this.history = history;
//		this.iconSprite.sprite2D = GetSprite ();
//		this.iconSprite.MakePixelPerfect ();
		this.dateLbl.text = "[b]" + ((MONTH)history.month).ToString () + " " + history.week.ToString () + ", " + history.year.ToString () + "[/b]";

	}
	private Sprite GetSprite(){
		switch(this.history.identifier){
		case HISTORY_IDENTIFIER.NONE:
			return icons [0];
		}
		return null;
	}
	void OnHover(bool isOver){
		if (isOver) {
			UIManager.Instance.ShowSmallInfo ("[b]" + this.history.description + "[/b]", this.transform);
		} else {
			UIManager.Instance.HideSmallInfo ();
		}
	}

}
