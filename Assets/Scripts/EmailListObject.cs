using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailListObject : MonoBehaviour
{
    public TextMeshProUGUI senderText;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI tagText;
    public TextMeshProUGUI dateText;
    public Image attachmentImage;
    public Image tagBox;
    public Email email;
    public GlobalControlScript globalControl;
    public bool read;

    internal void SetStuff()
    {
        headerText.text = email.emailTitle;
        senderText.text = email.emailSender;
        tagText.text = email.tagName;
        dateText.text = email.SentOnDate(globalControl.dateTime) ? email.sendTime: email.sendDate;

        if (!email.attachmentFlag)
        {
            attachmentImage.gameObject.SetActive(false);
        }

        if (globalControl.read[email])
        {
            GetComponent<Image>().color = Color.white * 0.8f;
        }

        if (email.tagName == null)
        {
            tagBox.gameObject.SetActive(false);
            RectTransform headerTextRectTransform = headerText.gameObject.GetComponent<RectTransform>();
            headerTextRectTransform.anchoredPosition += Vector2.left * tagBox.GetComponent<RectTransform>().sizeDelta.x;

}
        else
        {
            if (email.videoId != null)
            {
                if (email.Completed(globalControl))
                {
                    tagBox.color = Color.green;

                    tagText.text = "Completed";
                }
                else
                {
                    SetTagColor();
                }
            }
            else
            {
                SetTagColor();
            }
        }
    }

    private void SetTagColor()
    {
        Color newCol;
        if (ColorUtility.TryParseHtmlString(email.tagColor, out newCol))
        {
            tagBox.color = newCol;
        }
        else
        {
            Debug.LogWarning($"Invalid color for email tag with id {email.index}");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
