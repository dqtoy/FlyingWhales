using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPongCanvasGroupAlpha : MonoBehaviour {

    public CanvasGroup target;
    public float speed = 1f;

    private float _t = 0f;

    void Update() {
        _t += Time.deltaTime * speed;
        target.alpha = Mathf.PingPong(_t, 1f);
    }

}
