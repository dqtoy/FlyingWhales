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
			if (lf.obj is City) {
				UIManager.Instance.ShowCityInfo((City)lf.obj);
			} else if (lf.obj is Citizen) {
				UIManager.Instance.ShowCitizenInfo((Citizen)lf.obj);
			} else if (lf.obj is Kingdom) {
				UIManager.Instance.SetKingdomAsActive((Kingdom)lf.obj);
			}
		}
	}
}
