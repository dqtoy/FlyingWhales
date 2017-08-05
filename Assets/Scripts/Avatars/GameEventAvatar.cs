using UnityEngine;
using System.Collections;

public class GameEventAvatar : MonoBehaviour {

    internal GameEvent gameEvent;

    internal void Init(GameEvent gameEvent) {
        this.gameEvent = gameEvent;
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
