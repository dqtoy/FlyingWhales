using UnityEngine;
using System.Collections;

public class HistoryPortrait : MonoBehaviour {

//	public UI2DSprite kingdomColorGO;
	public UI2DSprite historyBG;
	public UILabel dateLbl;
	public UI2DSprite iconSprite;

	public Sprite[] icons;
	public History history;

	private bool isHovering = false;

	public void SetHistory(History history){
		this.history = history;
//		this.iconSprite.sprite2D = GetSprite ();
//		this.iconSprite.MakePixelPerfect ();
		if (history.identifier == HISTORY_IDENTIFIER.KING_RELATIONS) {
			if (history.isPositive) {
				historyBG.color = Color.green;
			} else {
				historyBG.color = Color.red;
			}
		}

		this.dateLbl.text = "[b]" + ((MONTH)history.month).ToString () + " " + history.days.ToString () + ", " + history.year.ToString () + "[/b]";

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
			this.isHovering = true;
			UIManager.Instance.ShowSmallInfo ("[b]" + this.history.description + "[/b]");
		} else {
			this.isHovering = false;
			UIManager.Instance.HideSmallInfo ();
		}
	}
	void Update(){
		if (this.isHovering) {
			UIManager.Instance.ShowSmallInfo ("[b]" + this.history.description + "[/b]");
		}
	}
}
