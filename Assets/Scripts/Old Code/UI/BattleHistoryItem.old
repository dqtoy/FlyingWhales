using UnityEngine;
using System.Collections;

public class BattleHistoryItem : MonoBehaviour {

    private Battle battle;
    private WarHistoryItem parentWarItem;

    [SerializeField] private UILabel battleNameLbl;
    [SerializeField] private UILabel battleSummaryLbl;

    public void SetBattle(Battle battle, WarHistoryItem parentWarItem) {
        this.battle = battle;
        this.parentWarItem = parentWarItem;
        battleNameLbl.text = "Battle";
        ConstructBattleSummary();
        battleSummaryLbl.gameObject.SetActive(false);
    }

    private void ConstructBattleSummary() {
        battleSummaryLbl.text = string.Empty;
        for (int i = 0; i < battle.battleLogs.Count; i++) {
            battleSummaryLbl.text += battle.battleLogs[i];
            if (i + 1 < battle.battleLogs.Count) {
                battleSummaryLbl.text += "\n";
            }
        }
    }

    public void ToggleBattleSummary() {
        UIManager.Instance.ToggleObject(battleSummaryLbl.gameObject);
        parentWarItem.RepositionBattlesTable();
    }
}
