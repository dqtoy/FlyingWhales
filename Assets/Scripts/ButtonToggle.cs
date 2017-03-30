using UnityEngine;
using System.Collections;

public class ButtonToggle : MonoBehaviour {

	public Sprite normalSprite;
	public Sprite hoveredSprite;
	public Sprite clickedSprite;

	public bool isClicked = false;

	public void OnClick(){
		if (isClicked) {
			isClicked = false;
			this.GetComponent<UI2DSprite> ().sprite2D = normalSprite;
		} else {
			isClicked = true;
			this.GetComponent<UI2DSprite> ().sprite2D = clickedSprite;
		}
	}

	void OnHover(bool isOver){
		if (isOver) {
			this.GetComponent<UI2DSprite> ().sprite2D = hoveredSprite;
		} else {
			if (isClicked) {
				this.GetComponent<UI2DSprite> ().sprite2D = clickedSprite;
			} else {
				this.GetComponent<UI2DSprite> ().sprite2D = normalSprite;
			}
		}
	}
}
