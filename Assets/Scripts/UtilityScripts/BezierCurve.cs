using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour {
    public LineRenderer bgLineRenderer;
    public LineRenderer progressMeter;

    private int _progressAmount;
    private Vector3[] _positions;

    private void Start() {
        progressMeter.positionCount = 0;
    }

    public void SetPositions(Vector3[] positions) {
        _positions = positions;
        bgLineRenderer.positionCount = _positions.Length;
        bgLineRenderer.SetPositions(_positions);
    }
    public void SetProgressAmount(int amount) {
        _progressAmount = amount;
    }

    //Returns true if progress is complete
    public bool AddProgress() {
        if(progressMeter.positionCount >= _positions.Length) {
            return true;
        }
        //if(progressMeter.positionCount == 0) {
        //    progressMeter.positionCount++;
        //    progressMeter.SetPosition(0, _positions[0]);
        //    _progressAmount -= 1;
        //}
        Vector3 prevPos = _positions[0];
        for (int i = 0; i < _progressAmount; i++) {
            if(progressMeter.positionCount > 0) {
                prevPos = _positions[progressMeter.positionCount - 1];
            }
            progressMeter.positionCount++;
            float time = 0f;
            StartCoroutine(TweenPath(prevPos, time));
            //progressMeter.SetPosition(progressMeter.positionCount - 1, _positions[progressMeter.positionCount - 1]);
        }

        if (progressMeter.positionCount == _positions.Length) {
            return true;
        }
        return false;
    }
    private IEnumerator TweenPath(Vector3 prevPos, float time) {
        float secs = GameManager.Instance.progressionSpeed / (float)_progressAmount;
        //float t = 0f;
        Vector3 orig2 = _positions[progressMeter.positionCount - 1];
        progressMeter.SetPosition(progressMeter.positionCount - 1, prevPos);
        Vector3 newpos;
        for (; time < secs; time += Time.deltaTime) {
            newpos = Vector3.Lerp(prevPos, orig2, time / secs);
            progressMeter.SetPosition(progressMeter.positionCount - 1, newpos);
            yield return null;
        }
        progressMeter.SetPosition(progressMeter.positionCount - 1, orig2);
    }
    private void TraverseLineRenderer(Vector3 vector3) {
        progressMeter.SetPosition(progressMeter.positionCount - 1, vector3);
    }
    //Returns true when progress reaches 0
    public bool ReduceProgress() {
        progressMeter.positionCount -= _progressAmount;
        if(progressMeter.positionCount <= 0) {
            return true;
        }
        return false;
        //for (int i = 0; i < _progressAmount; i++) {
        //    progressMeter.positionCount--;
        //    if (progressMeter.positionCount <= 0) {
        //        break;
        //    }
        //}
    }
}
