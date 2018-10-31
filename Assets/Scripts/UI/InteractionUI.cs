using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour {
    public static InteractionUI Instance;

    //public HorizontalScrollSnap scrollSnap;
    //public Transform scrollSnapContentTransform;
    public InteractionItem interactionItem;
    //public GameObject togglePrefab;
    public GameObject interactionHolder;
    //public ToggleGroup toggleGroup;

    //public Sprite toggleActiveUnselected;
    //public Sprite toggleActiveSelected;
    //public Sprite toggleInactiveUnselected;
    //public Sprite toggleInactiveSelected;


    private Interaction _interaction;
    //private List<InteractionItem> _allInteractionItems;

    #region getters/setters
    //public Interaction interaction {
    //    get { return _interaction; }
    //}
    #endregion
    private void Awake() {
        Instance = this;
        //_allInteractionItems = new List<InteractionItem>();
    }
    private void Start() {
        interactionItem.Initialize();
        //Messenger.AddListener<IInteractable, Interaction>(Signals.ADDED_INTERACTION, OnReceiveAddInteractionSignal);
        //Messenger.AddListener<IInteractable, Interaction>(Signals.REMOVED_INTERACTION, OnReceiveRemoveInteractionSignal);
    }
    public void RunExample() {
        for (int i = 0; i < 10; i++) {
            AddInteraction(null);
        }
    }
    public void OpenInteractionUI(Interaction interaction) {
        if (interaction != null) {
            _interaction = interaction;
            interactionItem.SetInteraction(_interaction);
            interactionHolder.SetActive(true);
            GameManager.Instance.SetPausedState(true);
            Messenger.Broadcast(Signals.INTERACTION_MENU_OPENED);
        }
    }
    public void HideInteractionUI() {
        if (interactionHolder.activeSelf) { //only if the menu is showing
            _interaction = null;
            interactionItem.SetInteraction(_interaction);
            interactionHolder.SetActive(false);
            GameManager.Instance.SetPausedState(false);
            Messenger.Broadcast(Signals.INTERACTION_MENU_CLOSED);
        }
    }
    //public void UpdateInteraction() {
    //    //RunExample();
    //    if (_interaction != null) {
    //    }
    //}
    public void OnReceiveAddInteractionSignal(IInteractable interactable, Interaction interaction) {
        //if (interactionHolder.activeSelf && _interactable == interactable) {
        //    AddInteraction(interaction);
        //}
    }
    public void OnReceiveRemoveInteractionSignal(IInteractable interactable, Interaction interaction) {
        //if (interactionHolder.activeSelf && _interactable == interactable) {
        //    RemoveInteraction(interaction);
        //}
    }
    public void AddInteraction(Interaction interaction) {
        //GameObject go = GameObject.Instantiate(interactionPrefab, scrollSnapContentTransform);
        //InteractionItem interactionItem = go.GetComponent<InteractionItem>();
        //interactionItem.Initialize();
        //_allInteractionItems.Add(interactionItem);

        //GameObject toggleGO = GameObject.Instantiate(togglePrefab, toggleGroup.transform);
        //Toggle toggle = toggleGO.GetComponent<Toggle>();
        //toggleGroup.RegisterToggle(toggle);
        //toggle.group = toggleGroup;

        //interactionItem.SetToggle(toggle);
        //interactionItem.SetInteraction(interaction);

        //StartCoroutine(scrollSnap.UpdateLayoutCoroutine());
    }
    public void RemoveInteraction(Interaction interaction) {
        //for (int i = 0; i < _allInteractionItems.Count; i++) {
        //    InteractionItem interactionItem = _allInteractionItems[i];
        //    if(interactionItem.interaction == interaction) {
        //        _allInteractionItems.RemoveAt(i);
        //        GameObject removedChild = null;
        //        scrollSnap.RemoveChild(i, out removedChild);
        //        if (removedChild != null) {
        //            GameObject.Destroy(removedChild);
        //        }
        //        StartCoroutine(scrollSnap.UpdateLayoutCoroutine());
        //        break;
        //    }
        //}
    }
    //public int GetIndexOfInteraction(InteractionItem item) {
        //return _allInteractionItems.IndexOf(item);
    //}
    public void OnScrollSnapToggle(bool state) {
        if (state) {
            //When the scroll bar toggle is clicked
        }
    }
}
