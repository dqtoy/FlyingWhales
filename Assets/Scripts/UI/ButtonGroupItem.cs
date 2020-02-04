using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
            if(clickedSprite == null) {
                this.GetComponent<UI2DSprite>().color = Color.gray;
            } else {
                this.GetComponent<UI2DSprite>().sprite2D = clickedSprite;
            }
            buttonGroup.ClickedItem(this);
        } else {
            this.GetComponent<UI2DSprite>().color = Color.white;
            this.GetComponent<UI2DSprite> ().sprite2D = normalSprite;
		}
	}

	void OnHover(bool isOver){
		if (isOver) {
            if (hoveredSprite == null) {
                this.GetComponent<UI2DSprite>().color = Color.yellow;
            } else {
                this.GetComponent<UI2DSprite>().sprite2D = hoveredSprite;
            }
		} else {
			if (isClicked) {
                if (clickedSprite == null) {
                    this.GetComponent<UI2DSprite>().color = Color.gray;
                } else {
                    this.GetComponent<UI2DSprite>().sprite2D = clickedSprite;
                }
			} else {
                this.GetComponent<UI2DSprite>().color = Color.white;
                this.GetComponent<UI2DSprite> ().sprite2D = normalSprite;
			}
		}
	}

    public void OnPointerEnterUGUI(){
        if (hoveredSprite == null) {
            this.GetComponent<UI2DSprite>().color = Color.yellow;
        }
        else {
            this.GetComponent<UI2DSprite>().sprite2D = hoveredSprite;
        }
	}

    public void OnPointerExitUGUI() {
        if (isClicked) {
            if (clickedSprite == null) {
                this.GetComponent<UI2DSprite>().color = Color.gray;
            }
            else {
                this.GetComponent<UI2DSprite>().sprite2D = clickedSprite;
            }
        }
        else {
            this.GetComponent<UI2DSprite>().color = Color.white;
            this.GetComponent<UI2DSprite>().sprite2D = normalSprite;
        }
    }


    public void SetAsClicked(){
        SetClickState(true);
  //      this.isClicked = true;
		//this.GetComponent<UI2DSprite> ().sprite2D = clickedSprite;
		//buttonGroup.ClickedItem (this);
	}
}
