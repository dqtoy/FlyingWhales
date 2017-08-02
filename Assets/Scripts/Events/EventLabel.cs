using UnityEngine;
using System.Collections;

public class EventLabel : MonoBehaviour {

	[SerializeField] private EventLogItem eventLogItem;

	void OnClick(){
		UILabel lbl = GetComponent<UILabel>();
		string url = lbl.GetUrlAtPosition(UICamera.lastWorldPosition);

		if (!string.IsNullOrEmpty (url)) {
			int indexToUse = int.Parse (url);
			LogFiller lf = eventLogItem.thisLog.fillers[indexToUse];
			if (lf.obj != null) {
				if (lf.obj is City) {
                    //UIManager.Instance.ShowCityInfo ((City)lf.obj);
                    City currCity = (City)lf.obj;
                    if(currCity.kingdom.id == UIManager.Instance.currentlyShowingKingdom.id) {
                        CameraMove.Instance.CenterCameraOn(currCity.hexTile.gameObject);
                    }
                    //UIManager.Instance.SetKingdomAsSelected(((City)lf.obj).kingdom);
				} else if (lf.obj is Citizen) {
					UIManager.Instance.ShowCitizenInfo ((Citizen)lf.obj);
				} else if (lf.obj is Kingdom) {
                    Kingdom currKingdom = (Kingdom)lf.obj;
                    if(currKingdom.id == UIManager.Instance.currentlyShowingKingdom.id) {
                        CameraMove.Instance.CenterCameraOn(currKingdom.capitalCity.hexTile.gameObject);
                    }
                    //UIManager.Instance.SetKingdomAsSelected ();
				} else if (lf.obj is GameEvent) {
					UIManager.Instance.ShowEventLogs (lf.obj);
				}
			}
		}
	}
}
