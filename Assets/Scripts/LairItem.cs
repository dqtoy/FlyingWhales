using UnityEngine;
using System.Collections;

public class LairItem : MonoBehaviour {

	private Lair _lair;

    [SerializeField] private UILabel _hpLbl;
    [SerializeField] private UILabel _cityLbl;
	[SerializeField] private UIProgressBar _hpProgBar;


    #region getters/setters
	public Lair lair {
		get { return this._lair; }
    }
    #endregion

	public void SetLair(Lair _lair) {
		this._lair = _lair;
		_hpLbl.text = this._lair.hp.ToString();
		_cityLbl.text = this._lair.name;
		float hpValue = (float)this._lair.hp / (float)this._lair.maxHP;
		if(hpValue > 1f){
			hpValue = 1f;
		}
        _hpProgBar.value = hpValue;
    }
}
