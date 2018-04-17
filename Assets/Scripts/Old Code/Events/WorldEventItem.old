using UnityEngine;
using System.Collections;

public class WorldEventItem : MonoBehaviour {

	public GameEvent gameEvent;
	public SpriteRenderer icon;

	public void SetGameEvent(GameEvent gameEvent){
		this.gameEvent = gameEvent;
		this.SetIcon(UIManager.Instance.GetSpriteForEvent(this.gameEvent.eventType));
	}

	private void SetIcon(Sprite sprite){
		icon.sprite = sprite;
	}

	public void DestroyObject(){
		Destroy (this.gameObject);
	}
}
