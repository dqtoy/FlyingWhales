using UnityEngine;
using System.Collections;

public class KingdomFlagItem : MonoBehaviour {

	public delegate void OnHoverOver();
	public OnHoverOver onHoverOver;

	public delegate void OnHoverExit ();
	public OnHoverExit onHoverExit;

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
			if (onHoverOver != null) {
				this.onHoverOver ();
			}
		} else {
			this.isHovering = false;
			UIManager.Instance.HideSmallInfo ();
			if (onHoverExit != null) {
				this.onHoverExit ();
			}
		}
	}

	void Update(){
		if (this.isHovering) {
			UIManager.Instance.ShowSmallInfo ("[b]" + this.kingdom.name + "[/b]", this.transform);
		}
	}
}
