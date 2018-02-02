using UnityEngine;
using System.Collections;

public class CombatLogsUI : UIMenu {

    internal bool isShowing = false;

    [SerializeField] private UILabel combatLogsLbl;
	[SerializeField] private UILabel sideALbl;
	[SerializeField] private UILabel sideBLbl;

    [SerializeField] private UIScrollView logsScrollView;
	[SerializeField] private UIScrollView sideAScrollView;
	[SerializeField] private UIScrollView sideBScrollView;

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
	public void UpdateCombatLogs(){
		SideACharacters ();
		SideBCharacters ();
		CombatLogs ();
	}
	private void CombatLogs() {
        string text = string.Empty;
		text += "[000000]";
        for (int i = 0; i < currentlyShowingCombat.resultsLog.Count; i++) {
			string currLog = currentlyShowingCombat.resultsLog[i];
            text +=  "- " + currLog + "\n";
        }
		text += "[-]";
		combatLogsLbl.text = text;
        logsScrollView.UpdatePosition();
    }
	private void SideACharacters(){
		string text = string.Empty;
		for (int i = 0; i < currentlyShowingCombat.characterSideACopy.Length; i++) {
			ECS.Character character = currentlyShowingCombat.characterSideACopy [i];
			string currLog = character.name + "(" + (character.characterClass != null ? character.characterClass.className : "NONE") + "-" + (character.role != null ? character.role.roleType.ToString () : "NONE") + ")";
			text += currLog + "\n";
		}
		sideALbl.text = text;
//		sideAScrollView.UpdatePosition();
	}
	private void SideBCharacters(){
		string text = string.Empty;
		for (int i = 0; i < currentlyShowingCombat.characterSideBCopy.Length; i++) {
			ECS.Character character = currentlyShowingCombat.characterSideBCopy [i];
			string currLog = character.name + "(" + (character.characterClass != null ? character.characterClass.className : "NONE") + "-" + (character.role != null ? character.role.roleType.ToString () : "NONE") + ")";
			text += currLog + "\n";
		}
		sideBLbl.text = text;
//		sideBScrollView.UpdatePosition();
	}
}
