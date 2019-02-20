using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaMapTravelLine : MonoBehaviour {

    [SerializeField] private LineRenderer line;
    [SerializeField] private SpriteRenderer lineSprite;
    [SerializeField] private SpriteRenderer lineSpriteFill;
    [SerializeField] private Transform startTransform;
    [SerializeField] private Transform targetTransform;

    private float targetXScale; //linespriteFill
    private int ticksLeft;


    public void DrawLine(LocationGridTile start, LocationGridTile end) {
        this.transform.localPosition = Vector3.zero;
        startTransform.localPosition = new Vector3(start.localPlace.x + 0.5f, start.localPlace.y + 0.5f, 0f);
        targetTransform.localPosition = new Vector3(end.localPlace.x + 0.5f, end.localPlace.y + 0.5f, 0f);
        line.positionCount = 2;
        line.SetPosition(0, startTransform.localPosition);
        line.SetPosition(1, targetTransform.localPosition);

        float angle = Mathf.Atan2(targetTransform.localPosition.y - startTransform.localPosition.y,
            targetTransform.localPosition.x - startTransform.localPosition.x) * Mathf.Rad2Deg;

        lineSprite.transform.eulerAngles = new Vector3(line.transform.rotation.x, line.transform.rotation.y, angle);
        lineSpriteFill.transform.eulerAngles = new Vector3(line.transform.rotation.x, line.transform.rotation.y, angle);

        float distance = Vector2.Distance(startTransform.localPosition, targetTransform.localPosition);
        //Debug.Log(distance);
        lineSprite.transform.localPosition = new Vector2(startTransform.localPosition.x, startTransform.localPosition.y);
        lineSpriteFill.transform.localPosition = new Vector2(startTransform.localPosition.x, startTransform.localPosition.y);

        lineSprite.transform.localScale = new Vector2(distance/7.04f, 0.03f);
        lineSpriteFill.transform.localScale = new Vector2(0f, 0.03f);

        targetXScale = lineSprite.transform.localScale.x;
        
        ticksLeft = InteractionManager.Character_Action_Delay;
        Messenger.AddListener(Signals.TICK_STARTED, FillProgress);

        //SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddDays(5), () => DestroyLine());
    }

    private void FillProgress() {
        ticksLeft--;
        if (ticksLeft == 0) {
            lineSpriteFill.transform.localScale = new Vector2(targetXScale, 0.03f);
            DestroyLine();
        } else {
            lineSpriteFill.transform.localScale = new Vector2(targetXScale/(float)ticksLeft, 0.03f);
        }
        
    }

    private void DestroyLine() {
        Messenger.RemoveListener(Signals.TICK_STARTED, FillProgress);
        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }
}
