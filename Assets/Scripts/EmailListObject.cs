using System;
using System.Collections;
using System.Collections.Generic;
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

    internal void SetStuff()
    {
        headerText.text = email.emailTitle;
        senderText.text = email.emailSender;
        tagText.text = email.tagName;
        dateText.text = globalControl.dateTime.Date == DateTime.Parse(email.sendDate).Date ? email.sendTime: email.sendDate;

        if (!email.attachmentFlag)
        {
            attachmentImage.gameObject.SetActive(false);
        }

        if (email.tagName == null)
        {
            tagBox.gameObject.SetActive(false);
            RectTransform headerTextRectTransform = headerText.gameObject.GetComponent<RectTransform>();
            headerTextRectTransform.anchoredPosition += Vector2.left * tagBox.GetComponent<RectTransform>().sizeDelta.x;

}
        else
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
