using UnityEngine;
using System.Collections;

public class EventLogItem : MonoBehaviour {

	[SerializeField] private UILabel _dateLbl;
	[SerializeField] private UILabel _logLbl;
//	[SerializeField] private UIAnchor _anchor;
//	[SerializeField] private GameObject _anchorPoint;
	[SerializeField] private GameObject _bgGO;

	private Log _thisLog;

	#region getters/setters
	public Log thisLog{
		get{ return this._thisLog; }
	}
	#endregion

	/*
	 * Assign log item
	 * */
	internal void SetLog(Log _thisLog){
		this._thisLog = _thisLog;
		this._dateLbl.text = LocalizationManager.Instance.GetLocalizedValue("General", "Months", _thisLog.month.ToString()) + " " + _thisLog.day.ToString() + ", " + _thisLog.year.ToString();
		if(_thisLog.fillers.Count > 0){
			this._logLbl.text = Utilities.LogReplacer(_thisLog);
		}else{
			this._logLbl.text = LocalizationManager.Instance.GetLocalizedValue (_thisLog.category, _thisLog.file, _thisLog.key);
		}
		UI2DSprite[] bgSprites = this._bgGO.GetComponentsInChildren<UI2DSprite> ();
		for (int i = 0; i < bgSprites.Length; i++) {
			bgSprites [i].UpdateAnchors ();
		}
	}

	internal void DisableBG(){
		UI2DSprite[] bgSprites = this._bgGO.GetComponentsInChildren<UI2DSprite> ();
		for (int i = 0; i < bgSprites.Length; i++) {
			bgSprites [i].alpha = (0f / 255f);
		}
	}

    internal void EnableBG() {
        UI2DSprite[] bgSprites = this._bgGO.GetComponentsInChildren<UI2DSprite>();
        for (int i = 0; i < bgSprites.Length; i++) {
            bgSprites[i].alpha = (255f / 255f);
        }
    }
}
