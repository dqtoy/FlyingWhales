using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionTimer : MonoBehaviour {
    public delegate void OnStopTimer();

    public Image progressImage;
    public GameObject noInteractionForegroundGO;
    public GameObject interactionForegroundGO;
    public OnStopTimer onStopTimer;

    private int _timer;
    private int _currentTimerTick;
    private bool _isStopped;
    private bool _isPaused;

    public void SetTimer(int ticks) {
        _timer = ticks;
        if(_timer == -1) {
            progressImage.gameObject.SetActive(false);
        } else {
            progressImage.gameObject.SetActive(true);
        }
    }
	public void StartTimer() {
        ResetTimer();
        if (_timer == -1) { return; }
        Messenger.AddListener(Signals.HOUR_STARTED, RunTimer);
    }
    public void StopTimer() {
        if (!_isStopped) {
            _isStopped = true;
            if(onStopTimer != null) {
                onStopTimer();
                onStopTimer = null;
            }
            if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
                Messenger.RemoveListener(Signals.HOUR_STARTED, RunTimer);
            }
        }
    }
    private void RunTimer() {
        if (_isPaused) { return; }
        iTween.ValueTo(gameObject, iTween.Hash("from", (float) _currentTimerTick, "to", (float)(_currentTimerTick - 1), "time", GameManager.Instance.progressionSpeed, "onupdate", "ChangeProgressFillAmount"));
        _currentTimerTick--;
        if (_currentTimerTick <= 0) {
            StopTimer();
        }
    }
    private void ChangeProgressFillAmount(float value) {
        progressImage.fillAmount = value / (float) _timer;
    }
    public void SetPauseState(bool state) {
        _isPaused = state;
    }
    public void ResetTimer() {
        progressImage.fillAmount = 1f;
        _currentTimerTick = _timer;
        _isStopped = false;
    }
    public void ShowInteractionForeground() {
        interactionForegroundGO.SetActive(true);
        noInteractionForegroundGO.SetActive(false);
    }
    public void ShowNoInteractionForeground() {
        interactionForegroundGO.SetActive(false);
        noInteractionForegroundGO.SetActive(true);
    }
}
