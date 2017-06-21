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
                    CameraMove.Instance.CenterCameraOn(((City)lf.obj).hexTile.gameObject);
                    UIManager.Instance.SetKingdomAsSelected(((City)lf.obj).kingdom);
				} else if (lf.obj is Citizen) {
					UIManager.Instance.ShowCitizenInfo ((Citizen)lf.obj);
				} else if (lf.obj is Kingdom) {
					UIManager.Instance.SetKingdomAsSelected ((Kingdom)lf.obj);
				} else if (lf.obj is GameEvent) {
					UIManager.Instance.ShowEventLogs (lf.obj);
				}
			}
		}
	}
}
