using UnityEngine;
using System.Collections;

public class LairItem : MonoBehaviour {

	private Lair _lair;

    [SerializeField] private UILabel _cityLbl;


    #region getters/setters
	public Lair lair {
		get { return this._lair; }
    }
    #endregion

	public void SetLair(Lair _lair) {
		this._lair = _lair;
		_cityLbl.text = this._lair.name;
    }
}
