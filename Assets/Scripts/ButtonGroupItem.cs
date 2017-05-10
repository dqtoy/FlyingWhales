using UnityEngine;
using System.Collections;

public class ButtonGroupItem : MonoBehaviour {

	private ButtonGroup buttonGroup;

	public Sprite normalSprite;
	public Sprite hoveredSprite;
	public Sprite clickedSprite;
	public bool isClicked = false;

	void Awake(){
		buttonGroup = this.transform.parent.GetComponent<ButtonGroup>();
	}

	public void SetClickState(bool isClicked){
		this.isClicked = isClicked;
		if (isClicked) {
			this.GetComponent<UI2DSprite> ().sprite2D = clickedSprite;
		} else {
			this.GetComponent<UI2DSprite> ().sprite2D = normalSprite;
		}
	}

	void OnClick(){
		buttonGroup.ClickedItem (this);
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


	public void SetAsClicked(){
		this.isClicked = true;
		this.GetComponent<UI2DSprite> ().sprite2D = clickedSprite;
	}
}
