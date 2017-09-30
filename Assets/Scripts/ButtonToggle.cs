using UnityEngine;
using System.Collections;

public class ButtonToggle : MonoBehaviour {

	public Sprite normalSprite;
	public Sprite hoveredSprite;
	public Sprite clickedSprite;

	public bool isClicked = false;

	public void OnClick(){
		if (isClicked) {
			SetClickState(false);
		} else {
			SetClickState(true);
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

	public void SetClickState(bool isClicked){
		this.isClicked = isClicked;
		if (isClicked) {
			this.GetComponent<UI2DSprite> ().sprite2D = clickedSprite;
		} else {
			this.GetComponent<UI2DSprite> ().sprite2D = normalSprite;
		}
	}

    public void SetAsClicked() {
        SetClickState(true);
    }

    public void SetAsUnClicked() {
        SetClickState(false);
    }


}
