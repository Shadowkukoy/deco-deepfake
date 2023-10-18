using System;
using System.Collections.Generic;
using System.Linq;
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

    internal bool Completed(GlobalControlScript globalControl)
    {
        var videoCorrect = globalControl.videosCorrect.FirstOrDefault(x => x.Key.videoId == videoId);
        return !videoCorrect.Equals(default(KeyValuePair<VideoInfo, bool>));
    }

    internal bool SentOnDate(DateTime date)
    {
        return date.Date == DateTime.Parse(sendDate).Date;
    }
}