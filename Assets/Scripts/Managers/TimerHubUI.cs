﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerHubUI : MonoBehaviour {
    public static TimerHubUI Instance = null;

    public ScrollRect timerHubScrollRect;
    public GameObject timerHubItemPrefab;

    public List<TimerHubItem> timerHubItems { get; private set; }

    #region Monobehaviour
    void Awake() {
        Instance = this;
    }
    #endregion

    public void Initialize() {
        timerHubItems = new List<TimerHubItem>();

        Messenger.AddListener(Signals.TICK_STARTED, PerTick);
        //Messenger.AddListener<string, int, System.Action>(Signals.SHOW_TIMER_HUB_ITEM, AddItem);
    }

    public void AddItem(string description, int durationInTicks, System.Action onClickAction) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(timerHubItemPrefab.name, Vector3.zero, Quaternion.identity, timerHubScrollRect.content);
        TimerHubItem timerHubItem = go.GetComponent<TimerHubItem>();
        timerHubItem.Initialize(description, durationInTicks, onClickAction);

        //Add and sort list
        bool hasBeenInserted = false;
        for (int i = 0; i < timerHubItems.Count; i++) {
            if (durationInTicks < timerHubItems[i].durationInTicks) {
                timerHubItems.Insert(i, timerHubItem);
                go.transform.SetSiblingIndex(i);
                hasBeenInserted = true;
                break;
            }
        }
        if (!hasBeenInserted) {
            timerHubItems.Add(timerHubItem);
        }
    }

    private void PerTick() {
        if (!GameManager.Instance.gameHasStarted) {
            return;
        }
        for (int i = 0; i < timerHubItems.Count; i++) {
            if (timerHubItems[i].PerTick()) {
                //If true, it means timer is done
                ObjectPoolManager.Instance.DestroyObject(timerHubItems[i].gameObject);
                timerHubItems.RemoveAt(i);
                i--;
            }
        }
    }
}
