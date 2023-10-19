using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialFocusControl : MonoBehaviour
{
    public RectTransform tutorialFocusTextBorder;
    public TextMeshProUGUI tutorialFocusText;

    public IEnumerator SetTutorialText(Vector2 pos, string text)
    {
        tutorialFocusTextBorder.anchoredPosition = pos;
        tutorialFocusText.text = text;

        yield return new WaitUntil(() => Input.anyKeyDown);
        yield return null;
    }
}
