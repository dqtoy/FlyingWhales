using UnityEngine;
using System.Collections;

public class StorylinesSummaryMenu : UIMenu {

    [SerializeField] private UIScrollView storylinesScrollview;
    [SerializeField] private UITable storylinesTable;
    [SerializeField] private GameObject storylineInfoGO;
    [SerializeField] private UILabel storylineInfoLbl;

    [SerializeField] private GameObject storylineItemPrefab;

	public UITable storyTable{
		get { return storylinesTable; }
	}

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

    public void RepositionTable() {
        StartCoroutine(UIManager.Instance.RepositionTable(storylinesTable));
    }

    public void ShowElementInfo(string info) {
        storylineInfoGO.SetActive(true);
        storylineInfoLbl.text = info;
        var v3 = Input.mousePosition;
        v3.z = 10.0f;
        //v3 = UIManager.Instance.uiCamera.GetComponent<Camera>().ScreenToWorldPoint(v3);
        storylineInfoGO.transform.position = new Vector3(storylineInfoGO.transform.position.x, v3.y, v3.z);
    }

    public void HideElementInfo() {
        storylineInfoGO.SetActive(false);
    }

}
