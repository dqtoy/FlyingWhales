using UnityEngine;
using System.Collections;

public class HextileEventItem : MonoBehaviour {

	private GameEvent _gameEvent;

	[SerializeField] private UILabel _gameEventLbl;

	#region getters/setters
	public GameEvent gameEvent {
		get { return this._gameEvent; }
	}
	#endregion

	public void SetHextileEvent(GameEvent _gameEvent) {
		this._gameEvent = _gameEvent;
		this._gameEventLbl.text = this._gameEvent.name;
	}
}
