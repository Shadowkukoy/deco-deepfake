using System;
using UnityEngine;

[Serializable]
public class Email
{
    public int Index { get; set; }
    public string EmailTitle { get; set; }
    public string EmailSender { get; set; }
    public string SendDate { get; set; }
    public bool HighPriorityFlag { get; set; }
    public bool CompletedFlag { get; set; }
    public bool AttachmentFlag { get; set; }
    public string EmailMessage { get; set; }
    public int EmailSignOff { get; set; }
    public string VideoThumbnail { get; set; }
    public string VideoDir { get; set; }
}