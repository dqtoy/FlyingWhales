using UnityEngine;
using System.Collections;
using System.Linq;

public class WarHistoryItem : MonoBehaviour {

    private Warfare war;

    [SerializeField] private UILabel warNameLbl;
    [SerializeField] private UITable battlesTable;
    
    public void SetWar(Warfare war) {
        this.war = war;
        warNameLbl.text = war.name;
        battlesTable.gameObject.SetActive(false);
        LoadBattles();
    }

    private void LoadBattles() {
        BattleHistoryItem[] presentItems = Utilities.GetComponentsInDirectChildren<BattleHistoryItem>(battlesTable.gameObject);
        int nextIndex = 0;
        for (int i = 0; i < presentItems.Length; i++) {
            BattleHistoryItem currItem = presentItems[i];
            Battle battleToShow = war.battles.ElementAtOrDefault(i);
            if(battleToShow == null) {
                currItem.gameObject.SetActive(false);
            } else {
                currItem.SetBattle(battleToShow, this);
                currItem.gameObject.SetActive(true);
            }
            nextIndex = i + 1;
        }

        for (int i = nextIndex; i < war.battles.Count; i++) {
            GameObject battleGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.battleHistoryPrefab.name, battlesTable.transform);
            battleGO.transform.localScale = Vector3.one;
            battleGO.GetComponent<BattleHistoryItem>().SetBattle(war.battles[i], this);
        }
        RepositionBattlesTable();
    }

    internal void RepositionBattlesTable() {
        //StartCoroutine(UIManager.Instance.RepositionTable(battlesTable));
        battlesTable.Reposition();
        battlesTable.repositionNow = true;
        //StartCoroutine(RepositionTable(battlesTable));
    }

    public void ToggleBattles() {
        UIManager.Instance.ToggleObject(battlesTable.gameObject);
        UIManager.Instance.RepositionWarHistoryTable();
    }
}
