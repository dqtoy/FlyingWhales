using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinionPicker : MonoBehaviour {


    [SerializeField] private ScrollRect minonsScrollView;
    [SerializeField] private GameObject minionItemPrefab;
    [SerializeField] private ToggleGroup toggleGroup;

    public System.Action<Minion> onClickMinionItemAction { get; private set; }
    private System.Func<Minion, bool> shouldItemBeActiveChecker;

    public void ShowMinionPicker(List<Minion> minionsToShow, System.Func<Minion, bool> shouldItemBeActiveChecker, System.Action<Minion> onClickMinionItemAction) {
        this.onClickMinionItemAction = onClickMinionItemAction;
        this.shouldItemBeActiveChecker = shouldItemBeActiveChecker;
        Utilities.DestroyChildren(minonsScrollView.content);
        for (int i = 0; i < minionsToShow.Count; i++) {
            Minion currMinion = minionsToShow[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(minionItemPrefab.name, Vector3.zero, Quaternion.identity, minonsScrollView.content);
            CharacterItem item = go.GetComponent<CharacterItem>();
            item.SetCharacter(currMinion.character);
            item.SetCoverState(!shouldItemBeActiveChecker.Invoke(currMinion), true);
            item.ResetToggle();
            item.SetAsToggle(toggleGroup);
            item.AddOnToggleAction(() => onClickMinionItemAction.Invoke(currMinion), true);
        }
    }
}
