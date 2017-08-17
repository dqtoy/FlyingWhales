using UnityEngine;
using System.Collections;

public class EventLabel : MonoBehaviour {

	[SerializeField] private GameObject logItem;

	void OnClick(){
		UILabel lbl = GetComponent<UILabel>();
		string url = lbl.GetUrlAtPosition(UICamera.lastWorldPosition);

		if (!string.IsNullOrEmpty (url)) {
			int indexToUse = int.Parse (url);
            LogFiller lf = new LogFiller();
            if(logItem.GetComponent<EventLogItem>() != null) {
                lf = logItem.GetComponent<EventLogItem>().thisLog.fillers[indexToUse];
            }else if(logItem.GetComponent<NotificationItem>() != null) {
                lf = logItem.GetComponent<NotificationItem>().thisLog.fillers[indexToUse];
            }

			if (lf.obj != null) {
				if (lf.obj is City) {
                    //UIManager.Instance.ShowCityInfo ((City)lf.obj);
                    City currCity = (City)lf.obj;
                    if(currCity.hexTile.currFogOfWarState != FOG_OF_WAR_STATE.HIDDEN) {
                        CameraMove.Instance.CenterCameraOn(currCity.hexTile.gameObject);
                    }
                    //UIManager.Instance.SetKingdomAsSelected(((City)lf.obj).kingdom);
				} else if (lf.obj is Citizen) {
					UIManager.Instance.ShowCitizenInfo ((Citizen)lf.obj);
				} else if (lf.obj is Kingdom) {
                    Kingdom currKingdom = (Kingdom)lf.obj;
                    for (int i = 0; i < currKingdom.cities.Count; i++) {
                        City currCity = currKingdom.cities[i];
                        if(currCity.hexTile.currFogOfWarState != FOG_OF_WAR_STATE.HIDDEN) {
                            CameraMove.Instance.CenterCameraOn(currKingdom.capitalCity.hexTile.gameObject);
                            break;
                        }
                    }
                    //UIManager.Instance.SetKingdomAsSelected ();
				} else if (lf.obj is GameEvent) {
					UIManager.Instance.ShowEventLogs (lf.obj);
				}
			}
		}
	}
}
