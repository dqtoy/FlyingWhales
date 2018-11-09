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
        for (int i = 0; i < _progressAmount; i++) {
            progressMeter.positionCount ++;
            progressMeter.SetPosition(progressMeter.positionCount - 1, _positions[progressMeter.positionCount - 1]);
        }
        if(progressMeter.positionCount == _positions.Length) {
            return true;
        }
        return false;
    }
    private void TweenLineRenderer() {

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
