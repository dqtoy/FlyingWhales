using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvelopContentUnityUI : MonoBehaviour {

    [SerializeField] private RectTransform otherTransform;
    [SerializeField] private bool followWidth;
    [SerializeField] private bool followHeight;
    [SerializeField] private Vector2 padding;

    [ContextMenu("Execute")]
    public void Execute() {
        if (this.gameObject.activeInHierarchy) {
            StartCoroutine(Envelop());
        }
        //UIManager.Instance.EnvelopContentCoroutineStarter(this.transform as RectTransform, otherTransform, followWidth, followHeight, padding);
    }

    private IEnumerator Envelop() {
        yield return null;
        RectTransform thisTransform = this.transform as RectTransform;
        Vector2 newSize = thisTransform.sizeDelta;
        if (followWidth) {
            newSize.x = otherTransform.sizeDelta.x;
            newSize.x += padding.x;
        }
        if (followHeight) {
            newSize.y = otherTransform.sizeDelta.y;
            newSize.y += padding.y;
        }
        (this.transform as RectTransform).sizeDelta = newSize;
    }
}
