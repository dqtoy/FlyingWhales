using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurveParent : MonoBehaviour {
    public LineRenderer bgLineRenderer;
    public Color[] _childColors;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private int _usedColorIndex;

    private List<BezierCurve> _children;

    #region getters/setters
    public Vector3 startPos {
        get { return _startPos; }
    }
    public Vector3 endPos {
        get { return _endPos; }
    }
    #endregion

    public void SetStartAndEndPositions(Vector3 startPos, Vector3 endPos) {
        _children = new List<BezierCurve>();
        _startPos = startPos;
        _endPos = endPos;
    }
    public void AddChild(BezierCurve curve) {
        if(_children.Count <= 0) {
            bgLineRenderer.positionCount = curve.positions.Length;
            bgLineRenderer.SetPositions(curve.positions);
        }
        curve.progressMeter.sortingOrder = _children.Count + 2;
        _children.Add(curve);
        if(_usedColorIndex >= _childColors.Length) {
            _usedColorIndex = 0;
        }
        curve.progressMeter.startColor = _childColors[_usedColorIndex];
        curve.progressMeter.endColor = _childColors[_usedColorIndex];
        _usedColorIndex += 1;
        
        curve.SetCurveParent(this);
        curve.transform.parent = transform;
    }
    public void RemoveChild(BezierCurve curve) {
        _children.Remove(curve);
        curve.SetCurveParent(null);
        SetActiveBG(false);
        if(_children.Count <= 0) {
            BezierCurveManager.Instance.RemoveCurveParent(this);
        }
    }
    public void SetActiveBG(bool state) {
        if (state) {
            bgLineRenderer.gameObject.SetActive(true);
        } else {
            for (int i = 0; i < _children.Count; i++) {
                if (_children[i].holder.activeSelf) {
                    bgLineRenderer.gameObject.SetActive(true);
                    return;
                }
            }
            bgLineRenderer.gameObject.SetActive(false);
        }
    }
}
