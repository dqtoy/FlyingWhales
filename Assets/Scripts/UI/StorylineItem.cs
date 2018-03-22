using UnityEngine;
using System.Collections;

public class StorylineItem : MonoBehaviour {

    private StorylineData storyline;

    [SerializeField] private TweenRotation expandCollapseTween;
    [SerializeField] private GameObject contentGO;
    [SerializeField] private UITable elementsTable;

    public void SetStoryline(StorylineData storyline) {
        this.storyline = storyline;
    }

    public void ToggleContent() {
        expandCollapseTween.ResetToBeginning();
        contentGO.SetActive(!contentGO.activeSelf);
    }
}
