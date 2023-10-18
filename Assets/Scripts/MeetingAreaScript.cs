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
using Deepfakes.Typography.TypeWriter;

public class MeetingAreaScript : MonoBehaviour
{
    public GameObject confirmMeetingEndDialog;
    public GameObject meetingDialogBox;
    public Text meetingChatText;
    public TypeWriter meetingChatTypeWriter;

    public void Start()
    {
        transform.SetAsLastSibling();
    }
}
