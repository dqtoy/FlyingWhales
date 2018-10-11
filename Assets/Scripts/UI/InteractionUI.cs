using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour {
    public HorizontalScrollSnap scrollSnap;
    public Transform scrollSnapContentTransform;
    public GameObject interactionPrefab;
    public GameObject togglePrefab;
    public ToggleGroup toggleGroup;

    private List<InteractionItem> _allInteractionItems;

    private void Awake() {
        _allInteractionItems = new List<InteractionItem>();
    }
    private void Start() {
        RunExample();
    }
    public void RunExample() {
        for (int i = 0; i < 10; i++) {
            AddInteraction(null);
        }
    }
    public IEnumerator UpdateInteractions(List<Interaction> interactions) {
        _allInteractionItems.Clear();
        scrollSnapContentTransform.DestroyChildren();
        toggleGroup.transform.DestroyChildren();

        yield return null;
        for (int i = 0; i < interactions.Count; i++) {
            AddInteraction(interactions[i]);
        }
    }
    public void AddInteraction(Interaction interaction) {
        GameObject go = GameObject.Instantiate(interactionPrefab, scrollSnapContentTransform);
        InteractionItem interactionItem = go.GetComponent<InteractionItem>();
        interactionItem.SetInteraction(interaction);
        _allInteractionItems.Add(interactionItem);

        GameObject toggleGO = GameObject.Instantiate(togglePrefab, toggleGroup.transform);
        Toggle toggle = toggleGO.GetComponent<Toggle>();
        toggleGroup.RegisterToggle(toggle);

        scrollSnap.UpdateLayout();
    }
    public void OnScrollSnapToggle(bool state) {
        if (state) {
            
        }
    }
}
