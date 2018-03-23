using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StorylineItem : MonoBehaviour {

    private StorylineData storyline;

    [SerializeField] private UILabel storylineDescriptionLbl;
    [SerializeField] private UIEventTrigger expandCollapseBtn;
    [SerializeField] private TweenRotation expandCollapseTween;
    [SerializeField] private GameObject contentGO;
    [SerializeField] private UITable elementsTable;

    [SerializeField] private UILabel questsLbl;

    [SerializeField] private GameObject elementPrefab;

    private void Awake() {
        EventDelegate.Set(expandCollapseBtn.onClick, PlayExpandCollapseAnimation);
        EventDelegate.Add(expandCollapseTween.onFinished, ToggleContent);
    }

    public void SetStoryline(StorylineData storyline) {
        this.storyline = storyline;
        storylineDescriptionLbl.text = Utilities.LogReplacer(storyline.storylineDescription);
        LoadElements();
        LoadQuests();
    }

    private void PlayExpandCollapseAnimation() {
        expandCollapseTween.enabled = true;
        expandCollapseTween.PlayForward();
    }

    private void ToggleContent() {
        expandCollapseTween.ResetToBeginning();
        contentGO.SetActive(!contentGO.activeSelf);
    }

    private void LoadElements() {
        foreach (KeyValuePair<object, List<Log>> kvp in storyline.relevantItems) {
            object currElement = kvp.Key;
            List<Log> currElementLogs = kvp.Value;
            GameObject elementGO = UIManager.Instance.InstantiateUIObject(elementPrefab.name, elementsTable.transform);
            elementGO.transform.localScale = Vector3.one;
            StorylineElement element = elementGO.GetComponent<StorylineElement>();
            element.SetElement(currElement, currElementLogs);
        }
    }

    private void LoadQuests() {
        questsLbl.text = string.Empty;
        for (int i = 0; i < storyline.relevantQuests.Count; i++) {
            Quest currQuest = storyline.relevantQuests[i];
            questsLbl.text += currQuest.questName + "\n";
        }
    }
}
