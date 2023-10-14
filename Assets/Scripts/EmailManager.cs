using System.Collections;
using System.Collections.Generic;
using TMPro.SpriteAssetUtilities;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class EmailManager : MonoBehaviour
{
    public GameObject emailPrefab;
    public UIManager uiManager;
    // Start is called before the first frame update
    void Start()
    {
        JArray emails = JArray.Parse(Resources.Load<TextAsset>("inbox").text);
        foreach (var emailJObject in emails) {
            var email = JsonUtility.FromJson<Email>(emailJObject.ToString());

            var emailItem = Instantiate(emailPrefab, Vector3.zero, Quaternion.identity, transform.GetChild(0));
            emailItem.transform.GetChild(0).GetComponent<Text>().text = email.EmailTitle;
        }
        uiManager.AssignButtonListeners(transform.GetChild(0).gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
