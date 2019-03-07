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

    [Header("Intel")]
    [SerializeField] private GameObject intelGO;
    [SerializeField] private IntelItem[] intelItems;

    private Character targetCharacter;
    private Character actor;

    public void Open(Character targetCharacter, Character actor) {
        UIManager.Instance.SetCoverState(true);
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);
        this.gameObject.SetActive(true);

        this.targetCharacter = targetCharacter;
        this.actor = actor;

        Utilities.DestroyChildren(dialogScrollView.content);

        GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem item = targetDialog.GetComponent<DialogItem>();
        item.SetData(targetCharacter, "What do you want from me?");

        GameObject actorDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem actorItem = actorDialog.GetComponent<DialogItem>();
        actorItem.SetData(actor, "I am here to share information with you.", DialogItem.Position.Right);

        UpdateIntel();
    }

    private void UpdateIntel() {
        intelGO.SetActive(true);
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            Intel intel = PlayerManager.Instance.player.allIntel.ElementAtOrDefault(i);
            if (intel == null) {
                currItem.gameObject.SetActive(false);
            } else {
                currItem.SetIntel(intel);
                currItem.SetClickAction(ReactToIntel);
                currItem.gameObject.SetActive(true);
            }
        }
    }
    private void HideIntel() {
        intelGO.SetActive(false);
    }
    public void Close() {
        UIManager.Instance.SetCoverState(false);
        UIManager.Instance.SetSpeedTogglesState(true);
        this.gameObject.SetActive(false);
    }

    private void ReactToIntel(Intel intel) {
        closeBtn.interactable = false;
        HideIntel();

        //GameObject actorDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        //DialogItem actorItem = actorDialog.GetComponent<DialogItem>();
        //actorItem.SetData(actor, "You might want to know that, " + Utilities.LogReplacer(intel.intelLog), DialogItem.Position.Right);

        ShareIntel share = PlayerManager.Instance.player.roleSlots[JOB.DIPLOMAT].GetAction(typeof(ShareIntel)) as ShareIntel;
        share.BaseActivate(targetCharacter);
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
            UIManager.Instance.ScrollRectSnapTo(dialogScrollView, item.characterDialogParent);
            //dialogScrollView.verticalNormalizedPosition = 0f;
            yield return new WaitForSeconds(0.5f);
        }
        closeBtn.interactable = true;
        yield return null;
        ShareIntel share = PlayerManager.Instance.player.roleSlots[JOB.DIPLOMAT].GetAction(typeof(ShareIntel)) as ShareIntel;
        share.DeactivateAction();
    }
}
