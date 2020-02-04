using UnityEngine;
using System.Collections;
using TMPro;

public class TextTyper : MonoBehaviour {

    //[Range(0, 100)]
    public float revealSpeed = 0.05f;
    private TMP_Text m_textMeshPro;


    void Awake() {
        // Get Reference to TextMeshPro Component
        m_textMeshPro = GetComponent<TMP_Text>();
        m_textMeshPro.enableWordWrapping = true;
    }

    public void Execute() {
        StartCoroutine(Type());
    }


    private IEnumerator Type() {

        // Force and update of the mesh to get valid information.
        m_textMeshPro.ForceMeshUpdate();


        int totalVisibleCharacters = m_textMeshPro.textInfo.characterCount; // Get # of Visible Character in text object
        int counter = 0;
        int visibleCount = 0;

        while (counter != totalVisibleCharacters + 1) {
            visibleCount = counter % (totalVisibleCharacters + 1);

            m_textMeshPro.maxVisibleCharacters = visibleCount; // How many characters should TextMeshPro display?

            counter += 1;

            yield return new WaitForSeconds(revealSpeed);
        }

    }

}