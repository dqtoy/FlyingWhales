using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class LogHistoryItem : LogItem {

    [SerializeField] private TextMeshProUGUI logLbl;
    [SerializeField] private TextMeshProUGUI dateLbl;
    [SerializeField] private Image logBG;
    [SerializeField] private EnvelopContentUnityUI envelopContent;

    //private bool _isInspected;

    public override void SetLog(Log log) {
        base.SetLog(log);
        this.name = log.id.ToString();
        dateLbl.text = "Day " + new GameDate((int) log.month, log.day, log.year, log.hour).GetDayAndTicksString();
        if (_log.fillers.Count > 0) {
            this.logLbl.text = Utilities.LogReplacer(_log);
        } else {
            this.logLbl.text = LocalizationManager.Instance.GetLocalizedValue(_log.category, _log.file, _log.key);
        }
        //if (log.isInspected || GameManager.Instance.inspectAll) {
        //    dateLbl.text = "Day " + new GameDate((int) log.month, log.day, log.year, log.hour).GetDayAndTicksString();
        //    if (_log.fillers.Count > 0) {
        //        this.logLbl.text = Utilities.LogReplacer(_log);
        //    } else {
        //        this.logLbl.text = LocalizationManager.Instance.GetLocalizedValue(_log.category, _log.file, _log.key);
        //    }
        //} else {
        //    int numOfChars = LocalizationManager.Instance.GetLocalizedValue(_log.category, _log.file, _log.key).Length;
        //    dateLbl.text = "???";
        //    this.logLbl.text = "???";
        //    for (int i = 0; i < numOfChars; i++) {
        //        this.logLbl.text += " ";
        //    }
        //}

        //if (!this.gameObject.activeSelf) {
        //    throw new System.Exception("Log Item is not active!");
        //}
        EnvelopContentExecute();
    }
    public void EnvelopContentExecute() {
        envelopContent.Execute();
    }

    public void SetLogColor(Color color) {
        //logBG.color = color;
    }

    public void ShowLogDebugInfo() {
        if (log.fromInteraction != null) {
            string text = log.fromInteraction.ToString() + " Intel Data: ";
            text += "\n<b>Interaction:</b> " + log.fromInteraction.intel.connectedInteraction.ToString();
            text += "\n<b>Actor:</b> " + log.fromInteraction.intel.actor.name;
            text += "\n<b>Target:</b> " + log.fromInteraction.intel.target?.ToString() ?? "None";
            text += "\n<b>Categories:</b> ";
            if (log.fromInteraction.intel.categories == null) {
                text += "None";
            } else {
                for (int i = 0; i < log.fromInteraction.intel.categories.Length; i++) {
                    text += "|" + log.fromInteraction.intel.categories[i].ToString() + "|";
                }
            }
            text += "\n<b>Alignment:</b> " + log.fromInteraction.intel.alignment.ToString();
            text += "\n<b>isCompleted?:</b> " + log.fromInteraction.intel.isCompleted.ToString();
            UIManager.Instance.ShowSmallInfo(text);
        }
        
    }
    public void HideLogDebugInfo() {
        UIManager.Instance.HideSmallInfo();
    }
}
