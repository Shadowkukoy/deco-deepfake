using System.Collections;
using System.Collections.Generic;
using TMPro.SpriteAssetUtilities;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using Unity.VisualScripting;
using System.Threading;

public class EmailManager : MonoBehaviour
{
    public GameObject emailPrefab;
    public UIManager uiManager;
    public GlobalControlScript globalControl;
    public Button emailAttachmentButton;
    public List<string> emailNames;
    public TextMeshProUGUI emailBodyText;
    public List<Email> emails;
    public GameObject calender;
    public GameObject calenderDayPrefab;
    public GameObject emailAttachmentViewer;
    public GameObject composedEmail;
    public GameObject videosNotCompleteNotification;
    private int unread;

    // Start is called before the first frame update
    void Start()
    {
        GenerateEmail();

        GenerateCalender();

        uiManager.AssignButtonListeners(emailAttachmentViewer);
        emailAttachmentViewer.SetActive(false);
        videosNotCompleteNotification.SetActive(false);
        composedEmail.SetActive(false);
    }

    public void RefreshEmail()
    {
        var emailListArea = transform.Find("Scroll View/Viewport/EmailsList");
        for (int i = 0; i < emailListArea.childCount; i++)
        {
            Destroy(emailListArea.GetChild(i).gameObject);
        }

        GenerateEmail();
    }

    private void GenerateEmail()
    {
        unread = 0;
        var emailListArea = transform.Find("Scroll View/Viewport/EmailsList");
        emailBodyText = transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        int i = 0;
        emails = new List<Email>(globalControl.emails);
        emails.OrderBy(x => x.emailMessage);
        emails.OrderBy(x => x.sendTime);
        emails.OrderBy(x => x.sendDate);
        emails.Reverse();
        float size = 0;
        foreach (var email in emails)
        {
            if (DateTime.Parse($"{email.sendTime} {email.sendDate}") > globalControl.dateTime) continue;
            var requirementsFailed = false;
            if (email.correctRequired != null)
            {
                foreach (var videoId in email.correctRequired.Split(','))
                {
                    var videoCorrect = globalControl.videosCorrect.FirstOrDefault(x => x.Key.videoId == videoId);
                    if (!videoCorrect.Equals(default(KeyValuePair<VideoInfo, bool>)) && !videoCorrect.Value)
                    {
                        requirementsFailed = true;
                    }
                }
                if (requirementsFailed) { continue; }
            }
            requirementsFailed = false;

            if (email.incorrectRequired != null)
            {
                foreach (var videoId in email.incorrectRequired.Split(','))
                {
                    var videoCorrect = globalControl.videosCorrect.FirstOrDefault(x => x.Key.videoId == videoId);
                    if (!videoCorrect.Equals(default(KeyValuePair<VideoInfo, bool>)) && videoCorrect.Value)
                    {
                        requirementsFailed = true;
                    }
                }
                if (requirementsFailed) { continue; }
            }

            var emailItem = Instantiate(emailPrefab, emailListArea, false);
            RectTransform emailItemRectTransform = emailItem.GetComponent<RectTransform>();
            size = emailItemRectTransform.sizeDelta.y;
            emailItemRectTransform.anchoredPosition += (i * size * Vector2.down);
            emailItem.name = "EmailItem";

            var emailListObject = emailItem.GetComponent<EmailListObject>();
            emailListObject.email = email;
            emailListObject.globalControl = globalControl;

            emailListObject.SetStuff();
            if (!emailListObject.read) unread++;

            i++;
        }
        uiManager.AssignButtonListeners(transform.GetChild(0).gameObject);
        emailBodyText.transform.parent.gameObject.SetActive(false);
        emailListArea.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, i * size);

        var notificationImage = uiManager.canvas.transform.Find("EmailButton/NotificationImage");
        var notificationText = notificationImage.Find("NotificationText").GetComponent<TextMeshProUGUI>();
        if (unread == 0)
        {
            notificationImage.gameObject.SetActive(false);
        }
        else
        {
            notificationImage.gameObject.SetActive(true);
            notificationText.text = unread.ToString();
        }
    }

    public void RefreshCalender()
    {
        var calenderArea = transform.Find("EmailSideBar/CalenderButton");
        for (int i = 0; i < calenderArea.childCount; i++)
        {
            Destroy(calenderArea.GetChild(i).gameObject);
        }

        GenerateCalender();
    }

    private void GenerateCalender()
    {
        DateTime calenderStartDate = new DateTime(2025, 4, 28);
        DateTime date = calenderStartDate;
        for (int i = 0; i < 7; i++)
        {
            var dayName = date.ToString("ddd").Substring(0,2);
            var calenderDayObject = Instantiate(calenderDayPrefab, calender.transform);
            RectTransform calenderDayObjetRectTransform = calenderDayObject.GetComponent<RectTransform>();

            calenderDayObjetRectTransform.anchoredPosition += i * calenderDayObjetRectTransform.sizeDelta.x * Vector2.right;

            var dayText = calenderDayObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            dayText.text = dayName;
            dayText.fontStyle = FontStyles.Bold;
            var dayImageObject = calenderDayObject.transform.GetChild(1).gameObject;
            dayImageObject.SetActive(false);
            date = date.AddDays(1);
        }

        date = calenderStartDate;
        for (int i = 0; i < 35; i++)
        {
            var dayName = date.Day.ToString();
            var calenderDayObject = Instantiate(calenderDayPrefab, calender.transform);
            RectTransform calenderDayObjetRectTransform = calenderDayObject.GetComponent<RectTransform>();

            calenderDayObjetRectTransform.anchoredPosition += (i % 7) * calenderDayObjetRectTransform.sizeDelta.x * Vector2.right;
            calenderDayObjetRectTransform.anchoredPosition += (1 + (i / 7)) * calenderDayObjetRectTransform.sizeDelta.y * Vector2.down;

            var dayText = calenderDayObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            dayText.text = dayName;

            if (date.Month != globalControl.dateTime.Month)
            {
                dayText.color = Color.grey;
            }
            if (date.Date != globalControl.dateTime.Date)
            {
                var dayImageObject = calenderDayObject.transform.GetChild(1).gameObject;
                dayImageObject.SetActive(false);
            }

            date = date.AddDays(1).Date.AddHours(GlobalControlScript.DayStartTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
