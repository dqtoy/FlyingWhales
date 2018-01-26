using UnityEngine;
using System.Collections;

public class CombatLogsUI : UIMenu {

    internal bool isShowing = false;

    [SerializeField] private UILabel combatLogsLbl;
    [SerializeField] private UIScrollView logsScrollView;

	private ECS.CombatPrototype currentlyShowingCombat;

	public void ShowCombatLogs(ECS.CombatPrototype combat) {
		currentlyShowingCombat = combat;
        isShowing = true;
        this.gameObject.SetActive(true);
        logsScrollView.ResetPosition();
    }

	public void HideCombatLogs() {
        isShowing = false;
        this.gameObject.SetActive(false);
    }

	public void UpdateCombatLogs() {
        string text = string.Empty;
        for (int i = 0; i < currentlyShowingCombat.resultsLog.Count; i++) {
			string currLog = currentlyShowingCombat.resultsLog[i];
            text +=  "- " + currLog + "\n";
        }
		combatLogsLbl.text = text;
        logsScrollView.UpdatePosition();
    }
}
