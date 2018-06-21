using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace worldcreator {
    public class SaveItem : MonoBehaviour {
        [SerializeField] private Text saveNameLbl;
        [SerializeField] private Button overwriteBtn;
        [SerializeField] private Button loadBtn;

        public string saveName;

        public void SetSave(string saveName) {
            if (!saveName.Contains(Utilities.worldConfigFileExt)) {
                saveName += Utilities.worldConfigFileExt;
            }
            this.saveName = saveName;
            saveNameLbl.text = saveName;
        }

        public void OnClickDelete() {
            WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.YES_NO, "Delete save", "Delete save file " + saveName + "?", () => DeleteSaveFile());
        }

        private void DeleteSaveFile() {
            string path = Application.persistentDataPath + "/Saves/" + saveName;
            File.Delete(path);
            GameObject.Destroy(this.gameObject);
        }

        public void Overwrite() {
            WorldCreatorUI.Instance.ShowSaveConfirmation(saveName);
        }

        public void OnClickLoad() {
            WorldCreatorUI.Instance.ShowLoadConfirmation(saveName);
        }

        public void UpdateFormat(bool isForLoading) {
            if (isForLoading) {
                loadBtn.gameObject.SetActive(true);
                overwriteBtn.gameObject.SetActive(false);
            } else {
                loadBtn.gameObject.SetActive(false);
                overwriteBtn.gameObject.SetActive(true);
            }
        }
    }
}

