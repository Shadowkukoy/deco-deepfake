using System;
using UnityEngine;

[Serializable]
public class Meeting
{
    public string videoDir;
    public int advanceTimeHours;
    public string date;
    public string delays;
    public string subTitleText;
    public int correctRequiredCount;
    public int incorrectRequiredCount;
}