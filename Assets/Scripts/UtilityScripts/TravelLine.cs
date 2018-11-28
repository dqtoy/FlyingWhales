using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TravelLine : MonoBehaviour {

    public GameObject holder;
    public RectTransform rectTransform;

    public Slider progressMeter;
    public Image fillImg, iconImg;

    private TravelLineParent _travelLineParent;
    private int _currentTick;
    private bool _isDone;

    #region getters/setters
    public bool isDone {
        get { return _isDone; }
    }
    public TravelLineParent travelLineParent {
        get { return _travelLineParent; }
    }
    #endregion

    public void Initialize() {
        _currentTick = 0;
    }
    public void SetColor(Color color) {
        fillImg.color = color;
        iconImg.color = color;
    }

    public void SetLineParent(TravelLineParent lineParent) {
        _travelLineParent = lineParent;
    }

    public void SetActiveMeter(bool state) {
        holder.SetActive(state);
        _travelLineParent.SetActiveBG(state);
    }

    public void AddProgress() {
        iTween.ValueTo(gameObject, iTween.Hash("from", (float) _currentTick, "to", (float) (_currentTick + 1), "time", GameManager.Instance.progressionSpeed, "onupdate", "TraverseLine"));
        _currentTick++;
        if(_currentTick >= _travelLineParent.numOfTicks) {
            _isDone = true;
        }
    }
    public void ReduceProgress() {
        iTween.ValueTo(gameObject, iTween.Hash("from", (float) _currentTick, "to", (float) (_currentTick - 1), "time", GameManager.Instance.progressionSpeed, "onupdate", "TraverseLine"));
        _currentTick--;
        if (_currentTick <= 0) {
            _isDone = true;
        }
    }
    private void TraverseLine(float val) {
        progressMeter.value = val / _travelLineParent.numOfTicks;
    }
}
