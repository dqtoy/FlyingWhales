using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;


public class CombatLogsUI : UIMenu {

    [SerializeField] private TextMeshProUGUI combatLogsLbl;
	[SerializeField] private TextMeshProUGUI sideALbl;
	[SerializeField] private TextMeshProUGUI sideBLbl;

	private Combat _currentlyShowingCombat;

	public Combat currentlyShowingCombat{
		get { return _currentlyShowingCombat; }
	}
    public void ShowCombatLogs(Combat combat) {
		_currentlyShowingCombat = combat;
        isShowing = true;
        this.gameObject.SetActive(true);
        SideACharacters();
        SideBCharacters();
        CombatLogs();
    }

	public void HideCombatLogs() {
        isShowing = false;
        this.gameObject.SetActive(false);
    }
	private void CombatLogs() {
        string text = string.Empty;
		text += $"Winning Side: {_currentlyShowingCombat.winningSide}";
        text += $"\nLosing Side: {_currentlyShowingCombat.losingSide}";
        text += "\nDead Characters:";
        for (int i = 0; i < _currentlyShowingCombat.charactersSideA.Count; i++) {
            if (_currentlyShowingCombat.charactersSideA[i].isDead) {
                text += $"\n{_currentlyShowingCombat.charactersSideA[i].name}";
            }
        }
        for (int i = 0; i < _currentlyShowingCombat.charactersSideB.Count; i++) {
            if (_currentlyShowingCombat.charactersSideB[i].isDead) {
                text += $"\n{_currentlyShowingCombat.charactersSideB[i].name}";
            }
        }
        combatLogsLbl.text = text;
    }
	private void SideACharacters(){
		string text = string.Empty;
        if(_currentlyShowingCombat.charactersSideA != null) {
            if(_currentlyShowingCombat.charactersSideA.Count > 0) {
                text += _currentlyShowingCombat.charactersSideA[0].name;
                for (int i = 1; i < _currentlyShowingCombat.charactersSideA.Count; i++) {
                    text += $"\n{_currentlyShowingCombat.charactersSideA[i].name}";
                }
            }
        }
		sideALbl.text = text;
	}
	private void SideBCharacters(){
		string text = string.Empty;
        if (_currentlyShowingCombat.charactersSideB != null) {
            if (_currentlyShowingCombat.charactersSideB.Count > 0) {
                text += _currentlyShowingCombat.charactersSideB[0].name;
                for (int i = 1; i < _currentlyShowingCombat.charactersSideB.Count; i++) {
                    text += $"\n{_currentlyShowingCombat.charactersSideB[i].name}";
                }
            }
        }
        sideBLbl.text = text;
	}
}
