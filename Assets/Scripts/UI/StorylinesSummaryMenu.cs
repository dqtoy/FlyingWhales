using UnityEngine;
using System.Collections;

public class StorylinesSummaryMenu : UIMenu {

    [SerializeField] private UIScrollView storylinesScrollview;
    [SerializeField] private UITable storylinesTable;

    [SerializeField] private GameObject storylineItemPrefab;

    internal void PopulateStorylinesTable() {
        for (int i = 0; i < StorylineManager.Instance.activeStorylines.Count; i++) {
            StorylineData currStoryline = StorylineManager.Instance.activeStorylines[i];
            GameObject storylineItemGO = UIManager.Instance.InstantiateUIObject(storylineItemPrefab.name, storylinesTable.transform);
            storylineItemGO.transform.localScale = Vector3.one;
            StorylineItem storylineItem = storylineItemGO.GetComponent<StorylineItem>();
            storylineItem.SetStoryline(currStoryline);
            storylinesTable.Reposition();
        }
        
    }

}
