using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinionPicker : MonoBehaviour {


    [SerializeField] private ScrollRect minonsScrollView;
    [SerializeField] private GameObject minionItemPrefab;
    [SerializeField] private ToggleGroup toggleGroup;

    public System.Action<Character, bool> onClickMinionItemAction { get; private set; }
    private System.Func<Minion, bool> shouldItemBeActiveChecker;

    public void ShowMinionPicker(List<Minion> minionsToShow, System.Func<Minion, bool> shouldItemBeActiveChecker, System.Action<Character, bool> onClickMinionItemAction) {
        this.onClickMinionItemAction = onClickMinionItemAction;
        this.shouldItemBeActiveChecker = shouldItemBeActiveChecker;
        Utilities.DestroyChildren(minonsScrollView.content);
        List<MinionCharacterItem> inactiveItems = new List<MinionCharacterItem>();
        for (int i = 0; i < minionsToShow.Count; i++) {
            Minion currMinion = minionsToShow[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(minionItemPrefab.name, Vector3.zero, Quaternion.identity, minonsScrollView.content);
            MinionCharacterItem item = go.GetComponent<MinionCharacterItem>();
            item.SetObject(currMinion.character);
            bool shouldItemBeClickable = shouldItemBeActiveChecker.Invoke(currMinion);
            if (!shouldItemBeClickable) {
                inactiveItems.Add(item);
            }
            item.SetInteractableState(shouldItemBeClickable);
            item.SetAsToggle();
            item.ClearAllOnToggleActions();
            item.AddOnToggleAction(onClickMinionItemAction.Invoke);
            item.SetToggleGroup(toggleGroup);
        }

        for (int i = 0; i < inactiveItems.Count; i++) {
            inactiveItems[i].transform.SetAsLastSibling();
        }
    }
}
