using System.Collections;
using System.Collections.Generic;
using TMPro.SpriteAssetUtilities;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

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

    // Start is called before the first frame update
    void Start()
    {
        JArray emailsJArray = JArray.Parse(jsonFile.text);
        int i = 0;
        var emailListArea = transform.GetChild(0);
        emailBodyText = transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        foreach (var emailJObject in emailsJArray) {
            var email = JsonUtility.FromJson<Email>(emailJObject.ToString());
            emails.Add(email);

            if (DateTime.Parse(email.sendDate).Date > globalControl.dateTime.Date) continue;

            var emailItem = Instantiate(emailPrefab, emailListArea, false);
            emailItem.GetComponent<RectTransform>().anchoredPosition += (i * 30 * Vector2.down);
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
