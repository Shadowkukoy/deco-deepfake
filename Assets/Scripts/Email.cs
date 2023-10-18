using System;
using UnityEngine;

[Serializable]
public class Email
{
    public int index;
    public string emailTitle;
    public string emailSender;
    public string sendDate;
    public string sendTime;
    public bool highPriorityFlag;
    public bool completedFlag;
    public bool attachmentFlag;
    public string emailMessage;
    public string tagName;
    public string tagColor;
    public int emailSignOff;
    public string videoId;
    public string imageDir;
    public string correctRequired;
    public string incorrectRequired;
}