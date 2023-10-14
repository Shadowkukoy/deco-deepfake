using System.Collections;
using System.Collections.Generic;
using TMPro.SpriteAssetUtilities;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class EmailManager : MonoBehaviour
{
    public GameObject emailPrefab;
    public UIManager uiManager;
    public GlobalControlScript globalControl;
    public TextAsset jsonFile;
    public List<string> emailNames;
    // Start is called before the first frame update
    void Start()
    {
        JArray emails = JArray.Parse(jsonFile.text);
        int i = 0;
        foreach (var emailJObject in emails) {
            var email = JsonUtility.FromJson<Email>(emailJObject.ToString());

            var emailItem = Instantiate(emailPrefab, transform.GetChild(0), false);
            emailItem.GetComponent<RectTransform>().anchoredPosition += (i * 30 * Vector2.down);

            var emailListObject = emailItem.GetComponent<EmailListObject>();
            emailListObject.email = email;
            emailListObject.globalControl = globalControl;

            emailListObject.SetStuff();

            i++;
        }
        uiManager.AssignButtonListeners(transform.GetChild(0).gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
