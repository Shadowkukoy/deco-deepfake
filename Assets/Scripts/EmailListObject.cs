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
        dateText.text = globalControl.dateTime.Day == DateTime.Parse(email.sendDate).Day ? email.sendTime: email.sendDate;

        Debug.Log(globalControl.dateTime.ToString());
        Debug.Log(DateTime.Parse(email.sendDate).ToString());

        if (!email.attachmentFlag)
        {
            attachmentImage.gameObject.SetActive(false);
        }

        if (email.tagName == null)
        {
            tagBox.gameObject.SetActive(false);
            headerText.gameObject.GetComponent<RectTransform>().anchoredPosition += Vector2.left * 70 ;

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
