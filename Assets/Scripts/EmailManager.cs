using System.Collections;
using System.Collections.Generic;
using TMPro.SpriteAssetUtilities;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class EmailManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        JArray emails = JArray.Parse(Resources.Load<TextAsset>("inbox").text);
        foreach (var email in emails) { 

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
