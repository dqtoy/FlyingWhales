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
            } else if (logItem.GetComponent<LogHistoryItem>() != null) {
                lf = logItem.GetComponent<LogHistoryItem>().thisLog.fillers[indexToUse];
            }

            if (lf.obj != null) {
                if (lf.obj is ECS.Character) {
                    UIManager.Instance.ShowCharacterInfo(lf.obj as ECS.Character);
                } else if (lf.obj is Party) {
                    UIManager.Instance.ShowCharacterInfo((lf.obj as Party).partyLeader);
                } else if (lf.obj is BaseLandmark) {
                    UIManager.Instance.ShowLandmarkInfo(lf.obj as BaseLandmark);
                } else if (lf.obj is ECS.CombatPrototype) {
                    UIManager.Instance.ShowCombatLog(lf.obj as ECS.CombatPrototype);
                }
            }
		}
	}
}
