using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShareIntelMenu : MonoBehaviour {

    [Header("Main")]
    [SerializeField] private ScrollRect dialogScrollView;
    [SerializeField] private GameObject dialogItemPrefab;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TextMeshProUGUI instructionLbl;
    [SerializeField] private TextMeshProUGUI endOfConversationLbl;

    [Header("Intel")]
    [SerializeField] private GameObject intelGO;
    [SerializeField] private IntelItem[] intelItems;

    private Character targetCharacter;
    private Character actor;

    private bool wasPausedOnOpen;
    public void Open(Character targetCharacter) {
        //UIManager.Instance.SetCoverState(true);
        //UIManager.Instance.Pause();
        //UIManager.Instance.SetSpeedTogglesState(false);
        this.gameObject.SetActive(true);

        wasPausedOnOpen = GameManager.Instance.isPaused;
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);

        Messenger.Broadcast(Signals.ON_OPEN_SHARE_INTEL);

        this.targetCharacter = targetCharacter;
        this.actor = PlayerManager.Instance.player.minions.FirstOrDefault()?.character ?? null;
        instructionLbl.text = "Share Intel with " + targetCharacter.name;
        endOfConversationLbl.transform.SetParent(this.transform);
        endOfConversationLbl.gameObject.SetActive(false);

        Utilities.DestroyChildren(dialogScrollView.content);

        GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem item = targetDialog.GetComponent<DialogItem>();
        item.SetData(targetCharacter, "What do you want from me?");

        GameObject actorDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem actorItem = actorDialog.GetComponent<DialogItem>();
        actorItem.SetData(actor, "I am here to share information with you.", DialogItem.Position.Right);

        UpdateIntel(PlayerManager.Instance.player.allIntel);
    }
    public void Open(Character targetCharacter, Character actor, Intel intelToShare) {
        //UIManager.Instance.SetCoverState(true);
        //UIManager.Instance.Pause();
        //UIManager.Instance.SetSpeedTogglesState(false);
        this.gameObject.SetActive(true);

        wasPausedOnOpen = GameManager.Instance.isPaused;
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);

        Messenger.Broadcast(Signals.ON_OPEN_SHARE_INTEL);

        this.targetCharacter = targetCharacter;
        this.actor = actor;
        instructionLbl.text = "Share Intel with " + targetCharacter.name;
        endOfConversationLbl.transform.SetParent(this.transform);
        endOfConversationLbl.gameObject.SetActive(false);

        Utilities.DestroyChildren(dialogScrollView.content);

        GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem item = targetDialog.GetComponent<DialogItem>();
        item.SetData(targetCharacter, "What do you want from me?");

        GameObject actorDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem actorItem = actorDialog.GetComponent<DialogItem>();
        actorItem.SetData(actor, "I am here to share information with you.", DialogItem.Position.Right);

        DirectlyShowIntelReaction(intelToShare);
    }
    private void DirectlyShowIntelReaction(Intel intel) {
        HideIntel();
        ReactToIntel(intel);
    }

    private void UpdateIntel(List<Intel> intelToShow) {
        intelGO.SetActive(true);
        SetIntelButtonsInteractable(true);
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            Intel intel = intelToShow.ElementAtOrDefault(i);
            currItem.SetIntel(intel);
            if (intel != null) {
                currItem.SetClickAction(ReactToIntel);
            }
        }
    }
    private void HideIntel() {
        intelGO.SetActive(false);
    }
    private void SetIntelButtonsInteractable(bool state) {
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            currItem.GetComponent<Button>().interactable = state;
        }
    }
    public void Close() {
        //UIManager.Instance.SetCoverState(false);
        //UIManager.Instance.SetSpeedTogglesState(true);
        this.gameObject.SetActive(false);
        UIManager.Instance.SetSpeedTogglesState(true);
        GameManager.Instance.SetPausedState(wasPausedOnOpen);
        Messenger.Broadcast(Signals.ON_CLOSE_SHARE_INTEL);
    }

    private void ReactToIntel(Intel intel) {
        closeBtn.interactable = false;
        //HideIntel();
        //UpdateIntel(new List<Intel>() { intel });
        //intelItems[0].SetClickedState(true);
        //SetIntelButtonsInteractable(false);

        GameObject actorDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem actorItem = actorDialog.GetComponent<DialogItem>();
        actorItem.SetData(actor, Utilities.LogReplacer(intel.intelLog), DialogItem.Position.Right);

        //ShareIntel share = PlayerManager.Instance.player.shareIntelAbility;
        //share.BaseActivate(targetCharacter);
        List<string> reactions = targetCharacter.ShareIntel(intel);
        StartCoroutine(ShowReactions(reactions));
    }
    string[] randomNothings = new string[] {
        "I really don't care",
        "And I should care, why?",
        "Should this mean something to me?",
        "Stop wasting my time!",
        "And the point of sharing this with me is...?"
    };
    private IEnumerator ShowReactions(List<string> reactions) {
        Debug.Log("Showing Reactions of " + targetCharacter.name + ": " + reactions.Count.ToString());
        if (reactions.Count == 0) {
            //character had no reaction
            reactions.Add(randomNothings[Random.Range(0, randomNothings.Length)]);
        }
        for (int i = 0; i < reactions.Count; i++) {
            GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
            DialogItem item = targetDialog.GetComponent<DialogItem>();
            item.SetData(targetCharacter, reactions[i]);
            if (i + 1 == reactions.Count) {
                endOfConversationLbl.transform.SetParent(dialogScrollView.content);
                endOfConversationLbl.gameObject.SetActive(true);
                //UIManager.Instance.ScrollRectSnapTo(dialogScrollView, endOfConversationLbl.transform as RectTransform);
            } else {
                //UIManager.Instance.ScrollRectSnapTo(dialogScrollView, item.characterDialogParent);
            }
            
            //yield return new WaitForSeconds(0.5f);
            //dialogScrollView.verticalNormalizedPosition = 0f;

        }
        closeBtn.interactable = true;
        dialogScrollView.verticalNormalizedPosition = 0f;
        yield return null;
        

        //ShareIntel share = PlayerManager.Instance.player.shareIntelAbility;
        //share.DeactivateAction();
    }
}
