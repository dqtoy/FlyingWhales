using UnityEngine;
using System.Collections;

public class CombatLogsUI : UIMenu {

    [SerializeField] private UILabel combatLogsLbl;
	[SerializeField] private UILabel sideALbl;
	[SerializeField] private UILabel sideBLbl;

    [SerializeField] private UIScrollView logsScrollView;
	[SerializeField] private UIScrollView sideAScrollView;
	[SerializeField] private UIScrollView sideBScrollView;

	private ECS.Combat _currentlyShowingCombat;

	public ECS.Combat currentlyShowingCombat{
		get { return _currentlyShowingCombat; }
	}

	public void ShowCombatLogs(ECS.Combat combat) {
		_currentlyShowingCombat = combat;
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
		for (int i = 0; i < _currentlyShowingCombat.resultsLog.Count; i++) {
			string currLog = _currentlyShowingCombat.resultsLog[i];
            text +=  "- " + currLog + "\n";
        }
		text += "[-]";
		combatLogsLbl.text = text;
        logsScrollView.UpdatePosition();
    }
	private void SideACharacters(){
		string text = string.Empty;
		for (int i = 0; i < _currentlyShowingCombat.characterSideACopy.Count; i++) {
			if(i > 0){
				text += "\n";
			}
			ECS.Character character = _currentlyShowingCombat.characterSideACopy [i];
			string currLog = character.urlName + "(" + (character.characterClass != null ? character.characterClass.className : "NONE") + "-" + (character.role != null ? character.role.roleType.ToString () : "NONE") + ")";
			text += currLog;
		}
		sideALbl.text = text;
		sideAScrollView.ResetPosition();
	}
	private void SideBCharacters(){
		string text = string.Empty;
		for (int i = 0; i < _currentlyShowingCombat.characterSideBCopy.Count; i++) {
			if(i > 0){
				text += "\n";
			}
			ECS.Character character = _currentlyShowingCombat.characterSideBCopy [i];
			string currLog = character.urlName + "(" + (character.characterClass != null ? character.characterClass.className : "NONE") + "-" + (character.role != null ? character.role.roleType.ToString () : "NONE") + ")";
			text += currLog;
		}
		sideBLbl.text = text;
		sideBScrollView.ResetPosition();
	}
}
