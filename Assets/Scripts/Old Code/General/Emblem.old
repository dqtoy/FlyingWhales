using UnityEngine;
using System.Collections;

public class Emblem : MonoBehaviour {
	public SpriteRenderer emblemImage;
	public SpriteRenderer emblemBG;
	public SpriteRenderer emblemOutline;

	public void SetEmblem(Kingdom kingdom){
		this.emblemBG.sprite = kingdom.emblemBG;
		this.emblemOutline.sprite = kingdom.emblemBG;
		this.emblemImage.sprite = kingdom.emblem;

		Color kingdomColor = kingdom.kingdomColor;
		kingdomColor.a = 255f / 255f;
		this.emblemBG.color = kingdomColor;
	}
}
