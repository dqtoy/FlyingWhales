using UnityEngine;
using System.Collections;

public class EventItem : MonoBehaviour {

	public delegate void OnClickEvent(GameEvent gameEvent);
	public OnClickEvent onClickEvent;

	public GameEvent gameEvent;
	public UI2DSprite eventIcon;



	public void SetEvent(GameEvent gameEvent){
		this.gameEvent = gameEvent;
	}

	public void SetSpriteIcon(Sprite sprite){
		eventIcon.sprite2D = sprite;
		eventIcon.MakePixelPerfect();
	}

	void OnHover(bool isOver){
//		if (isOver) {
//			UIManager.Instance.ShowSmallInfo(this.gameEvent.eventType.ToString() + "\n")
//		}
	}

	void OnClick(){
		if (onClickEvent != null) {
			onClickEvent(this.gameEvent);
		}
	}
}
