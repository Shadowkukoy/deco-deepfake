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

public class EmailManager : MonoBehaviour
{
    public GameObject emailPrefab;
    public UIManager uiManager;
    public GlobalControlScript globalControl;
    public TextAsset jsonFile;
    public Button emailAttachmentButton;
    public List<string> emailNames;
    public TextMeshProUGUI emailBodyText;
    public List<Email> emails;
    public GameObject calender;
    public GameObject calenderDayPrefab;
    public GameObject emailAttachmentViewer;
    // Start is called before the first frame update
    void Start()
    {
        GenerateEmail();

        GenerateCalender();

        uiManager.AssignButtonListeners(emailAttachmentViewer);
        emailAttachmentViewer.SetActive(false);
    }

    private void GenerateEmail()
    {
        JArray emailsJArray = JArray.Parse(jsonFile.text);
        int i = 0;
        var emailListArea = transform.GetChild(0);
        emailBodyText = transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        foreach (var emailJObject in emailsJArray)
        {
            var email = JsonUtility.FromJson<Email>(emailJObject.ToString());
            emails.Add(email);

            if (DateTime.Parse(email.sendDate).Date > globalControl.dateTime.Date) continue;

            var emailItem = Instantiate(emailPrefab, emailListArea, false);
            RectTransform emailItemRectTransform = emailItem.GetComponent<RectTransform>();
            emailItemRectTransform.anchoredPosition += (i * emailItemRectTransform.sizeDelta.y * Vector2.down);
            emailItem.name = "EmailItem";

            var emailListObject = emailItem.GetComponent<EmailListObject>();
            emailListObject.email = email;
            emailListObject.globalControl = globalControl;

            emailListObject.SetStuff();

            i++;
        }
        uiManager.AssignButtonListeners(transform.GetChild(0).gameObject);
        emailBodyText.transform.parent.gameObject.SetActive(false);
    }

    private void GenerateCalender()
    {
        DateTime calenderStartDate = new DateTime(2023, 8, 28);
        DateTime date = calenderStartDate;
        for (int i = 0; i < 7; i++)
        {
            var dayName = date.DayOfWeek.HumanName().Substring(0,2);
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

            if (date.Month != 9)
            {
                dayText.color = Color.grey;
            }
            if (date.Date != globalControl.dateTime.Date)
            {
                var dayImageObject = calenderDayObject.transform.GetChild(1).gameObject;
                dayImageObject.SetActive(false);
            }

            date = date.AddDays(1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
