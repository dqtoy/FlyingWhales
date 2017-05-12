using UnityEngine;
using System.Collections;

public class KingdomFlagItem : MonoBehaviour {

	internal Kingdom kingdom;

	public UI2DSprite kingdomColorSprite;
	private bool isHovering = false;

	internal void SetKingdom(Kingdom kingdom){
		this.kingdom = kingdom;
		kingdomColorSprite.color = kingdom.kingdomColor;
	}

	void OnClick(){
		UIManager.Instance.SetKingdomAsActive (this.kingdom);
	}

	void OnHover(bool isOver){
		if (isOver) {
			this.isHovering = true;
			UIManager.Instance.ShowSmallInfo ("[b]" + this.kingdom.name + "[/b]", this.transform);
		} else {
			this.isHovering = false;
			UIManager.Instance.HideSmallInfo ();
		}
	}

	void Update(){
		if (this.isHovering) {
			UIManager.Instance.ShowSmallInfo ("[b]" + this.kingdom.name + "[/b]", this.transform);
		}
	}
}
