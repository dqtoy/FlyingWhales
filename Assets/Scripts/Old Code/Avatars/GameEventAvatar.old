using UnityEngine;
using System.Collections;

public class GameEventAvatar : MonoBehaviour {

    [SerializeField] private SpriteRenderer eventSprite;
    internal GameEvent gameEvent;
    internal HexTile eventLocation;

    internal void Init(GameEvent gameEvent, HexTile eventLocation) {
        this.gameEvent = gameEvent;
        this.eventLocation = eventLocation;
        eventSprite.sprite = EventManager.Instance.GetEventAvatarSprite(gameEvent.eventType);
    }

    #region Monobehaviours
    private void OnMouseEnter() {
        if (!UIManager.Instance.IsMouseOnUI()) {
            UIManager.Instance.ShowSmallInfo(this.gameEvent.name);
        }
    }

    private void OnMouseExit() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion
}
