using UnityEngine;
using System.Collections;

public class KingdomFlagItem : MonoBehaviour {

	internal Kingdom kingdom;

	public UI2DSprite kingdomColorSprite;

	internal void SetKingdom(Kingdom kingdom){
		this.kingdom = kingdom;
		kingdomColorSprite.color = kingdom.kingdomColor;
	}

	void OnClick(){
		UIManager.Instance.SetKingdomAsActive (this.kingdom);
	}
}
