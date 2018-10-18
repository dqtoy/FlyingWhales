using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour {
    public static InteractionUI Instance;

    public HorizontalScrollSnap scrollSnap;
    public Transform scrollSnapContentTransform;
    public GameObject interactionPrefab;
    public GameObject togglePrefab;
    public GameObject interactionHolder;
    public ToggleGroup toggleGroup;

    public Sprite toggleActiveUnselected;
    public Sprite toggleActiveSelected;
    public Sprite toggleInactiveUnselected;
    public Sprite toggleInactiveSelected;


    private IInteractable _interactable;
    private List<InteractionItem> _allInteractionItems;

    private void Awake() {
        Instance = this;
        _allInteractionItems = new List<InteractionItem>();
    }
    private void Start() {
        Messenger.AddListener<IInteractable, Interaction>(Signals.ADDED_INTERACTION, OnReceiveAddInteractionSignal);
        Messenger.AddListener<IInteractable, Interaction>(Signals.REMOVED_INTERACTION, OnReceiveRemoveInteractionSignal);
    }
    public void RunExample() {
        for (int i = 0; i < 10; i++) {
            AddInteraction(null);
        }
    }
    public void OpenInteractionUI(IInteractable interactable) {
        if (interactable.currentInteractions != null && interactable.currentInteractions.Count > 0) {
            _interactable = interactable;
            StartCoroutine(UpdateInteractions(interactable.currentInteractions));
            interactionHolder.SetActive(true);
        }
    }
    public void HideInteractionUI() {
        _interactable = null;
        interactionHolder.SetActive(false);
    }
    public IEnumerator UpdateInteractions(List<Interaction> interactions) {
        _allInteractionItems.Clear();
        scrollSnapContentTransform.DestroyChildren();
        toggleGroup.transform.DestroyChildren();

        yield return null;
        //RunExample();
        if (interactions != null) {
            for (int i = 0; i < interactions.Count; i++) {
                AddInteraction(interactions[i]);
            }
        }
    }
    public void OnReceiveAddInteractionSignal(IInteractable interactable, Interaction interaction) {
        if (interactionHolder.activeSelf && _interactable == interactable) {
            AddInteraction(interaction);
        }
    }
    public void OnReceiveRemoveInteractionSignal(IInteractable interactable, Interaction interaction) {
        if (interactionHolder.activeSelf && _interactable == interactable) {
            RemoveInteraction(interaction);
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
        toggle.group = toggleGroup;

        interactionItem.SetToggle(toggle);

        scrollSnap.UpdateLayout();
    }
    public void RemoveInteraction(Interaction interaction) {
        for (int i = 0; i < _allInteractionItems.Count; i++) {
            InteractionItem interactionItem = _allInteractionItems[i];
            if(interactionItem.interaction == interaction) {
                _allInteractionItems.RemoveAt(i);
                GameObject removedChild = null;
                scrollSnap.RemoveChild(i, out removedChild);
                if (removedChild != null) {
                    GameObject.Destroy(removedChild);
                }
                scrollSnap.UpdateLayout();
                break;
            }
        }
    }
    public int GetIndexOfInteraction(InteractionItem item) {
        return _allInteractionItems.IndexOf(item);
    }
    public void OnScrollSnapToggle(bool state) {
        if (state) {
            //When the scroll bar toggle is clicked
        }
    }
}
