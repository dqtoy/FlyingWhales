using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventPopup : PooledObject {
    private const int Lifetime = 5;
    private Log log;
    private int ticksAlive;
    private LocationGridTile location;
    private Canvas canvas;

    [Header("Log")]
    [SerializeField] private GameObject logGO;
    [SerializeField] private TextMeshProUGUI logText;

    public void Initialize(Log log, LocationGridTile location, Canvas canvas) {
        this.log = log;
        this.location = location;
        this.canvas = canvas;
        logText.SetText(Utilities.LogReplacer(log));
        logGO.SetActive(false);
        StartCountdown();
    }

    private void StartCountdown() {
        ticksAlive = 0;
        Messenger.AddListener(Signals.TICK_STARTED, CheckForExpiry);
    }
    private void CheckForExpiry() {
        if (ticksAlive == Lifetime) {
            DestroyPopup();
        }
        ticksAlive++;
    }
    private void DestroyPopup() {
        Messenger.RemoveListener(Signals.TICK_STARTED, CheckForExpiry);
        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }

    private void ToggleLog() {
        logGO.SetActive(!logGO.activeSelf);
    }

    private void UpdatePosition() {
        Vector3 worldPos = location.parentMap.CellToWorld(location.localPlace);
        (this.transform as RectTransform).OverlayPosition(worldPos, canvas.worldCamera);
    }

    private void LateUpdate() {
        UpdatePosition();
    }

    #region Mouse Events
    public void OnClick() {
        ToggleLog();
        if (logGO.activeSelf) {
            UIManager.Instance.Pause();
        } else {
            UIManager.Instance.Unpause();
        }
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        ticksAlive = 0;
        log = null;
    }
    #endregion
}
