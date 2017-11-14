using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WorldHistoryUI : MonoBehaviour {

    [Header("Prefabs")]
    [SerializeField] private GameObject kingdomEmblemPrefab;
    [SerializeField] private GameObject worldHistoryItem;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private UIGrid kingdomEmblemsGrid;
    [SerializeField] private GameObject allSelectedGO;
    [SerializeField] private UIScrollView worldHistoryScrollView;
    [SerializeField] private UITable worldHistoryTable;

    private bool isShowing = false;

    //Emblems
    private HashSet<Kingdom> allKingdomsInMenu;
    private List<KingdomEmblem> inactiveEmblems;
    private Dictionary<Kingdom, KingdomEmblem> activeEmblems;

    //Logs
    private Dictionary<Log, WorldHistoryItem> logHistory;
    private List<WorldHistoryItem> allLogItems;
    private const int LOG_HISTORY_LIMIT = 269;

    //Selected Kingdoms
    [SerializeField] private List<Kingdom> selectedKingdoms;

    //private void Awake() {
    //    Messenger.AddListener("InitializeUI", Initialize);
    //}

    internal void Initialize() {
        allKingdomsInMenu = new HashSet<Kingdom>();
        inactiveEmblems = new List<KingdomEmblem>();
        activeEmblems = new Dictionary<Kingdom, KingdomEmblem>();
        logHistory = new Dictionary<Log, WorldHistoryItem>();
        allLogItems = new List<WorldHistoryItem>();
        selectedKingdoms = new List<Kingdom>();
        LoadKingdomEmblems();
        LoadInitialLogItems();
        SelectAll(true);
        Messenger.AddListener<Kingdom>("OnNewKingdomCreated", CreateEmblemForKingdom);
        Messenger.AddListener<Kingdom>("OnKingdomDied", RemoveEmblemOfKingdom);
        Messenger.AddListener<Log>("AddLogToHistory", AddLogToWorldHistory);
    }

    public void ToggleWorldHistoryUI() {
        if (isShowing) {
            HideWorldHistory();
        } else {
            ShowWorldHistory();
        }
    }

    private void ShowWorldHistory() {
        isShowing = true;
        tweenPos.PlayForward();
    }

    private void HideWorldHistory() {
        isShowing = false;
        tweenPos.PlayReverse();
    }

    #region Emblem Selection
    /*
     * This will instantiate the emblems of the initial
     * kingdoms. This is called when the UI is initialized.
     * */
    private void LoadKingdomEmblems() {
        for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
            Kingdom currKingdom = KingdomManager.Instance.allKingdoms[i];
            CreateEmblemForKingdom(currKingdom);
        }
    }
    private void ToggleKingdomEmblemSelection(KingdomEmblem clickedEmblem) {
        clickedEmblem.ToggleEmblemSelectedState();
        if (clickedEmblem.isSelected) {
            SelectKingdom(clickedEmblem.kingdom);
        } else {
            DeselectKingdom(clickedEmblem.kingdom);
        }
    }
    private void CreateEmblemForKingdom(Kingdom kingdom) {
        if (!allKingdomsInMenu.Contains(kingdom)) {
            KingdomEmblem currEmblem = null;
            if(inactiveEmblems.Count > 0) {
                currEmblem = inactiveEmblems[0];
            } else {
                GameObject kingdomEmblemGO = UIManager.Instance.InstantiateUIObject(kingdomEmblemPrefab.name, this.transform);
                kingdomEmblemGO.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                currEmblem = kingdomEmblemGO.GetComponent<KingdomEmblem>();
            }
            
            currEmblem.SetKingdom(kingdom);
            currEmblem.onEmblemClicked += ToggleKingdomEmblemSelection;

            //Add emblem to grid
            kingdomEmblemsGrid.AddChild(currEmblem.transform);
            kingdomEmblemsGrid.Reposition();

            //Maintain lists
            allKingdomsInMenu.Add(kingdom);
            activeEmblems.Add(kingdom, currEmblem);
            inactiveEmblems.Remove(currEmblem);
        }
    }
    private void RemoveEmblemOfKingdom(Kingdom kingdom) {
        KingdomEmblem emblemOfKingdom = activeEmblems[kingdom];
        emblemOfKingdom.gameObject.SetActive(false);
        allKingdomsInMenu.Remove(kingdom);
        activeEmblems.Remove(kingdom);
        inactiveEmblems.Add(emblemOfKingdom);
    }
    public void ToggleAllSelected() {
        if (allSelectedGO.activeSelf) {
            //select none
            SelectAll(false);
        } else {
            //select all
            SelectAll(true);
        }
    }
    private void SelectAll(bool isSelectAll) {
        for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
            Kingdom currKingdom = KingdomManager.Instance.allKingdoms[i];
            KingdomEmblem currEmblem = activeEmblems[currKingdom];
            currEmblem.SetEmblemSelectedState(isSelectAll);
            if (currEmblem.isSelected) {
                SelectKingdom(currKingdom, false);
            } else {
                DeselectKingdom(currKingdom, false);
            }
        }
        UpdateWorldHistory();
    }
    private void SelectKingdom(Kingdom selectedKingdom, bool updateLogs = true) {
        if (!selectedKingdoms.Contains(selectedKingdom)) {
            selectedKingdoms.Add(selectedKingdom);
        }
        if (AreAllKingdomsSelected()) {
            allSelectedGO.SetActive(true);
        }
        if (updateLogs) {
            UpdateWorldHistory();
        }
    }
    private void DeselectKingdom(Kingdom selectedKingdom, bool updateLogs = true) {
        selectedKingdoms.Remove(selectedKingdom);
        allSelectedGO.SetActive(false);
        if (updateLogs) {
            UpdateWorldHistory();
        }
    }
    private bool AreAllKingdomsSelected() {
        return selectedKingdoms.Count == KingdomManager.Instance.allKingdoms.Count;
    }
    #endregion

    #region World History
    private void LoadInitialLogItems() {
        worldHistoryScrollView.ResetPosition();
        for (int i = 0; i < LOG_HISTORY_LIMIT; i++) {
            GameObject logGO = UIManager.Instance.InstantiateUIObject(worldHistoryItem.name, worldHistoryTable.transform);
            logGO.name = i.ToString();
            logGO.transform.localScale = Vector3.one;
            allLogItems.Add(logGO.GetComponent<WorldHistoryItem>());
            worldHistoryTable.Reposition();
            worldHistoryScrollView.ResetPosition();
        }
        worldHistoryScrollView.UpdateScrollbars();
        allLogItems.Reverse();

        for (int i = 0; i < allLogItems.Count; i++) {
            allLogItems[i].gameObject.SetActive(false);
        }
        worldHistoryScrollView.UpdateScrollbars();
    }
    internal void AddLogToWorldHistory(Log log) {
        if (logHistory.Count + 1 > LOG_HISTORY_LIMIT) {
            int numOfExcessLogs = (logHistory.Count + 1) - LOG_HISTORY_LIMIT;
            for (int i = 0; i < numOfExcessLogs; i++) {
                RemoveLogFromWorldHistory(logHistory.Keys.First());
            }
        }

        WorldHistoryItem logItemToUse = null;
        for (int i = 0; i < allLogItems.Count; i++) {
            WorldHistoryItem currItem = allLogItems[i];
            if (!currItem.gameObject.activeSelf && !logHistory.Values.Contains(currItem)) {
                logItemToUse = currItem;
                break;
            }
        }

        if(logItemToUse == null) {
            throw new System.Exception("No log item to use!");
        }

        logItemToUse.SetLog(log);
        logHistory.Add(log, logItemToUse);
        if(DoesLogIncludeKingdom(log, selectedKingdoms)) {
            logItemToUse.gameObject.SetActive(true);
            StartCoroutine(RepositionTable(worldHistoryTable));
            StartCoroutine(RepositionScrollView(worldHistoryScrollView, true));
        }
        
    }
    private void RemoveLogFromWorldHistory(Log log) {
        WorldHistoryItem itemOfLog = logHistory[log];
        logHistory.Remove(log);
        itemOfLog.gameObject.SetActive(false);
    }
    private void UpdateWorldHistory() {
        foreach (KeyValuePair<Log, WorldHistoryItem> kvp in logHistory) {
            Log currLog = kvp.Key;
            WorldHistoryItem currItem = kvp.Value;
            if(DoesLogIncludeKingdom(currLog, selectedKingdoms)) {
                currItem.gameObject.SetActive(true);
            } else {
                currItem.gameObject.SetActive(false);
            }
        }
        StartCoroutine(RepositionTable(worldHistoryTable));
        StartCoroutine(RepositionScrollView(worldHistoryScrollView));
    }
    private bool DoesLogIncludeKingdom(Log log, List<Kingdom> includedKingdoms) {
        if (log.fillers.Count > 0) {
            for (int i = 0; i < log.fillers.Count; i++) {
                LogFiller filler = log.fillers[i];
                if (filler.obj is Kingdom) {
                    if (includedKingdoms.Contains((Kingdom)filler.obj)) {
                        return true;
                    }
                }
            }
        }
        if (log.allInvolved != null && log.allInvolved.Length > 0) {
            for (int i = 0; i < log.allInvolved.Length; i++) {
                object obj = log.allInvolved[i];
                if (obj is Kingdom) {
                    if (includedKingdoms.Contains((Kingdom)obj)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion

    public IEnumerator RepositionTable(UITable thisTable) {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        thisTable.Reposition();
    }
    public IEnumerator RepositionScrollView(UIScrollView thisScrollView, bool keepScrollPosition = false) {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (keepScrollPosition) {
            thisScrollView.UpdatePosition();
        } else {
            thisScrollView.ResetPosition();
            thisScrollView.Scroll(0f);
        }
        yield return new WaitForEndOfFrame();
        thisScrollView.UpdateScrollbars();
    }

}
