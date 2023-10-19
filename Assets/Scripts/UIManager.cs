using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using Deepfakes.Typography.TypeWriter;
using System;
using System.Linq;
using System.IO;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Unity.VisualScripting;
using TMPro;
using UnityEditor;
using NUnit.Framework;
using System.ComponentModel;

public class UIManager
{
    private const int ToggleSliderTransitionFrames = 8;
    private const int PopInOutTransitionFrames = 8;
    private const int ToggleSliderPadding = 4;
    public readonly Color ToggleBackgroundOnColor = Color.white;

    //Code that assigns the method 'OnButtonPress' to the pressing of any children gameobjects.
    //This should be run on canvas whenever a new scene is loaded, and on any new instantiated UI element which contains buttons

    public VideoPlayer videoPlayer;
    public RawImage videoRawImage;
    private float videoZoom = 1;
    public Slider zoomSlider;
    public Canvas canvas;
    public Canvas videoCanvas;
    public Canvas backgroundCanvas;
    public Camera postProcessCam;
    public Camera noPostCam;
    public Slider videoScrubber;
    private bool settingValue;
    public bool metaState = false;
    public bool aboutUsState = false;
    public bool optionsState = false;
    public static bool soundOn = true;
    public int popup = 0;
    public Image metadataImage;
    public Image audioVisualImage;
    public Vector3 prevMousePosition;
    internal GlobalControlScript globalControl;
    public GameObject aboutUsPage;
    public GameObject optionsPage;
    public GameObject disclaimer;
    public bool disclaimerAgreed = false;
    public GameObject incomingCall;
    public bool managerCall = false;
    public AudioClip normalClick = (AudioClip) Resources.Load("Click-normal");
    public AudioClip errorClick = (AudioClip)Resources.Load("Click-error");
    public AudioClip settingsClick = (AudioClip)Resources.Load("Click-settings");
    public AudioClip windowsBootSound = (AudioClip)Resources.Load("Windows_sound");
    public AudioClip openingMusic = (AudioClip)Resources.Load("opening1");
    public AudioClip ringtone = (AudioClip)Resources.Load("Ringtone");
    public Sprite playImage = Resources.Load<Sprite>("playimage");
    public Sprite pauseImage = Resources.Load<Sprite>("pauseimage");
    internal TypeWriter aboutUsTypeWriter;
    internal GameObject emailsPage;
    public bool emailsPageShowing;
    public GameObject yesNoVideoArea;
    internal TypeWriter deepFakeSceneTypeWriter;
    public EmailManager emailManager;
    private Email selectedEmail;
    public int timesRejected = 0;
    internal MeetingAreaScript meetingArea;
    public Meeting meeting;
    private string currentMeetingButtonName;
    internal VideoPlayer facemeshVideoPlayer;
    internal Image blackImage;
    internal UnityEngine.Transform black;
    private readonly DateTime TwistDate = new DateTime(2025, 5, 15);

    public void AssignButtonListeners(GameObject elements)
    {
        foreach (Button button in elements.GetComponentsInChildren<Button>())
        {
            switch (button.name)
            {
                case "EmailItem":
                    button.onClick.AddListener(delegate { OnButtonPress(button, SceneManager.GetActiveScene().name + "." + button.name, button.gameObject.GetComponent<EmailListObject>().email.index); });
                    break;
                default:
                    button.onClick.AddListener(delegate { OnButtonPress(button, SceneManager.GetActiveScene().name + "." + button.name, 0); });
                    break;
            }
        }
    }

    public void AssignSliderListeners(GameObject elements)
    {
        foreach (Slider slider in elements.GetComponentsInChildren<Slider>())
        {
            switch (slider.name)
            {
                default:
                    slider.onValueChanged.AddListener(delegate { OnSliderValueChanged(slider, SceneManager.GetActiveScene().name + "." + slider.name, 0);  });
                    break;
            }
        }
    }

    public void AssignToggleListeners(GameObject elements)
    {
        foreach (Toggle toggle in elements.GetComponentsInChildren<Toggle>())
        {
            switch (toggle.name)
            {
                default:
                    toggle.onValueChanged.AddListener(delegate { OnToggleValueChanged(toggle, SceneManager.GetActiveScene().name + "." + toggle.name, 0); });
                    break;
            }
        }
       
    }

    private void OnButtonPress(Button button, string buttonIdentifier, int id)
    {
        //Code that should be run when a button is pressed!
        //button: the name of the scene and name of the button GameObject in the format Scene.ButtonName
        //id: a number which can optionally be assigned to be bruh passed through when the button is pressed (could be useful if multiple buttons have the same name).
        switch (buttonIdentifier)
        {
            case "HomePageScene.SendButton":
                var completed = true;
                var emailsToday = globalControl.emails.Where(x => x.SentOnDate(globalControl.dateTime));
                foreach (var email in emailsToday)
                {
                    if (email.videoId == null) continue;
                    if (!email.Completed(globalControl))
                    {
                        completed = false;
                        break;
                    }
                }

                if (completed)
                {
                    globalControl.NextDay();
                }
                else
                {
                    globalControl.StartCoroutine(PopIn(emailManager.videosNotCompleteNotification));
                }

                break;
            case "HomePageScene.CloseVideosNotCompleteNotificationButton":
                globalControl.StartCoroutine(PopOut(emailManager.videosNotCompleteNotification));
                break;

            case "DeepFakeScene.YesButton":
                PlaySound(normalClick);
                //stuff that happens when yes button is pressed
                if (globalControl.currentVideoInfo.deepfaked)
                {
                    // successfully identified deepfake
                    globalControl.videosCorrect[globalControl.currentVideoInfo] = true;
                }
                else
                {
                    // thought a real video was deepfaked
                    globalControl.videosCorrect[globalControl.currentVideoInfo] = false;
                }
                if (globalControl.dateTime.Date == TwistDate)
                {
                    int completedToday = GetEmailVideosCompletedToday();

                    if (completedToday >= 2)
                    {
                        globalControl.dateTime = globalControl.dateTime.AddHours(6);
                    }
                    Debug.Log(completedToday);
                }
                SceneManager.LoadScene("HomePageScene");
                break;
            case "DeepFakeScene.NoButton":
                PlaySound(normalClick);
                //stuff that happens when no button is pressed
                if (globalControl.currentVideoInfo.deepfaked)
                {
                    // incorrectly thought a deepfaked video was real
                    globalControl.videosCorrect[globalControl.currentVideoInfo] = false;
                }
                else
                {
                    // correctly identified a real video as real
                    globalControl.videosCorrect[globalControl.currentVideoInfo] = true;
                }
                if (globalControl.dateTime.Date == TwistDate)
                {
                    int completedToday = GetEmailVideosCompletedToday();

                    if (completedToday >= 2)
                    {
                        globalControl.dateTime = globalControl.dateTime.AddHours(6);
                    }
                    Debug.Log(completedToday);
                }
                SceneManager.LoadScene("HomePageScene");
                break;
            case "DeepFakeScene.MetadataButton":
                PlaySound(normalClick);
                // stuff happens when metadata button is pressed
                //Debug.Log($"{button} test3");
                if (!metaState)
                {
                    globalControl.StartCoroutine(PopIn(metadataImage.gameObject));
                    metaState = true;
                    metadataImage.transform.SetAsLastSibling();
                }
                else
                {
                    globalControl.StartCoroutine(PopOut(metadataImage.gameObject));
                    metaState = false;
                }
                break;
            case "DeepFakeScene.BackButton":
                PlaySound(normalClick);
                SceneManager.LoadScene("HomePageScene");
                break;
            case "MainMenuScene.PlayButton":
                PlaySound(normalClick);
                SceneManager.LoadScene("HomePageScene");
                break;
            case "HomePageScene.InfoButton":
            case "MainMenuScene.AboutButton":
                PlaySound(normalClick);
                if (!aboutUsState)
                {
                    globalControl.StartCoroutine(PopIn(aboutUsPage));
                    aboutUsTypeWriter.LoadNextText(aboutUsTypeWriter.gameObject);
                    aboutUsState = true;
                    aboutUsPage.transform.SetAsLastSibling();
                }
                else
                {
                    globalControl.StartCoroutine(PopOut(aboutUsPage));
                    aboutUsTypeWriter.StopTypeWriter();
                    aboutUsState = false;
                }
                break;
            case "MainMenuScene.DisclaimerAgreeButton":
                PlaySound(normalClick);
                GameObject disclaimer = GameObject.Find("Disclaimer");
                disclaimerAgreed = true;
                globalControl.StartCoroutine(PopOut(disclaimer));
                break;
            case "HomePageScene.HomeButton":
                PlaySound(normalClick);
                SceneManager.LoadScene("MainMenuScene");
                popup = 0;
                break;
            case "MainMenuScene.AboutExitButton":
            case "HomePageScene.AboutExitButton":
                PlaySound(normalClick);
                globalControl.StartCoroutine(PopOut(aboutUsPage));
                aboutUsTypeWriter.StopTypeWriter();
                aboutUsState = false;
                break;
            case "MainMenuScene.SettingsButton":
            case "HomePageScene.SettingsButton":
                PlaySound(settingsClick);
                if (!optionsState)
                {
                    globalControl.StartCoroutine(PopIn(optionsPage));
                    optionsState = true;
                    optionsPage.transform.SetAsLastSibling();
                }
                else
                {
                    globalControl.StartCoroutine(PopOut(optionsPage));
                    optionsState = false;
                }
                break;
            case "MainMenuScene.OptionsExitButton":
            case "HomePageScene.OptionsExitButton":
                PlaySound(normalClick);
                globalControl.StartCoroutine(PopOut(optionsPage));
                optionsState = false;
                break;
            case "HomePageScene.AcceptCallButton":
                PlaySound(normalClick);
                managerCall = true;
                globalControl.StartCoroutine(PopOut(incomingCall));

                globalControl.StartCoroutine(PopIn(meetingArea.gameObject));
                var meetingVideoPlayer = meetingArea.transform.Find("Video").GetComponent<VideoPlayer>();
                var emailsYesterday = globalControl.emails.Where(x => x.SentOnDate(globalControl.dateTime.AddDays(-1)));
                var videosYesterday = new List<VideoInfo>();
                foreach (var email in emailsYesterday)
                {
                    if (email.videoId == null) continue;
                    videosYesterday.Add(globalControl.videoInfos.FirstOrDefault(x => x.videoId == email.videoId));
                }
                var incorrect = 0;
                var correct = 0;
                foreach (var videoInfo in videosYesterday)
                {
                    if (globalControl.videosCorrect[videoInfo])
                    {
                        correct++;
                    }
                    else
                    {
                        incorrect++;
                    }
                }
                meeting = globalControl.meetings.FirstOrDefault(x => DateTime.Parse(x.date).Date == globalControl.dateTime.Date && x.incorrectRequiredCount <= incorrect && x.correctRequiredCount <= correct);
                meetingVideoPlayer.clip = Resources.Load<VideoClip>(meeting.videoDir);
                meetingVideoPlayer.isLooping = true;
                meetingVideoPlayer.loopPointReached += MeetingVideoPlayer_loopPointReached;

                UnityEngine.Transform chatText = meetingArea.transform.Find("ChatArea/Mask/ChatText");
                var meetingText = chatText.GetComponent<TextMeshProUGUI>(); 
                var meetingTypeWriter = chatText.GetComponent<TypeWriter>();

                meetingText.text = meeting.subTitleText;
                meetingTypeWriter.automatic = true;
                meetingTypeWriter.sound = false;
                meetingTypeWriter.LoadNextText(meetingTypeWriter.gameObject);

                break;
            case "HomePageScene.RejectCallButton":
                globalControl.StartCoroutine(PopOut(incomingCall));
                globalControl.Invoke("ShowManagerCall", 3);
                PlaySound(errorClick);
                timesRejected++;
                break;
            case "HomePageScene.EmailButton":
                PlaySound(normalClick);
                if (emailsPageShowing)
                {
                    globalControl.StartCoroutine(PopOut(emailsPage, false));
                }
                else
                {
                    globalControl.ShowEmailsPage();
                }
                emailsPageShowing = !emailsPageShowing;
                break;
            case "HomePageScene.CalendarButton":
                PlaySound(normalClick);
                break;
            case "MainMenuScene.WatchGameIntroButton":
            case "HomePageScene.WatchGameIntroButton":
                PlaySound(normalClick);
                break;
            case "MainMenuScene.OptionsSoundButton":
                PlaySound(normalClick);
                soundOn = !soundOn;
                GameObject openingMusicObject = GameObject.Find("OpeningMusicAudioSource");
                AudioSource openingMusicAudioSource = openingMusicObject.GetComponent<AudioSource>();
                if (!soundOn)
                {
                    // sound is now off, turn off opening music
                    openingMusicAudioSource.Stop();
                }
                else
                {
                    // sound is now on, turn on opening music
                    openingMusicAudioSource.clip = openingMusic;
                    openingMusicAudioSource.Play();
                }
                break;
            case "HomePageScene.OptionsSoundButton":
                PlaySound(normalClick);
                soundOn = !soundOn;
                if (!managerCall)
                {
                    // user hasn't accepted the incoming call yet
                    GameObject incomingCallObj = GameObject.Find("IncomingCall");
                    if (incomingCallObj != null)
                    {
                        AudioSource managerCallAudio = incomingCallObj.GetComponent<AudioSource>();
                        if (soundOn)
                        {
                            managerCallAudio.Play();
                        }
                        else
                        {
                            managerCallAudio.Stop();
                        }
                    }
                }
                break;
            case "MainMenuScene.OptionsQuickTextButton":
            case "HomePageScene.OptionsQuickTextButton":
                PlaySound(normalClick);
                break;
            case "HomePageScene.VPNButton":
                PlaySound(normalClick);
                break;
            case "DeepFakeScene.PlayPauseButton":
                PlaySound(normalClick);                
                PausePlayVideo();
                break;
            case "DeepFakeScene.StepForwardButton":
                PlaySound(normalClick);
                videoPlayer.frame = videoPlayer.frame + 1;
                break;
            case "DeepFakeScene.JumpForwardButton":
                PlaySound(normalClick);
                videoPlayer.time = videoPlayer.time + 3;
                break;
            case "DeepFakeScene.StepBackwardButton":
                PlaySound(normalClick);
                videoPlayer.frame = videoPlayer.frame - 1;
                break;
            case "DeepFakeScene.JumpBackwardButton":
                PlaySound(normalClick);
                videoPlayer.time = videoPlayer.time - 3;
                break;
            case "HomePageScene.EmailItem":
            case "DeepFakeScene.EmailItem":
                PlaySound(normalClick);
                ViewEmailContents(id);
                break;
            case "HomePageScene.EmailBodyExitButton":
            case "DeepFakeScene.EmailBodyExitButton":
                PlaySound(normalClick);
                emailManager.RefreshEmail();
                globalControl.StartCoroutine(PopOut(emailManager.emailBodyText.transform.parent.gameObject));
                break;
            case "HomePageScene.EmailExitButton":
            case "DeepFakeScene.EmailExitButton":
                PlaySound(normalClick);
                ExitEmailsPage();
                break;
            case "HomePageScene.EmailAttachmentButton":
            case "DeepFakeScene.EmailAttachmentButton":
                PlaySound(normalClick);
                if (selectedEmail.videoId != null)
                {
                    globalControl.currentVideoInfo = globalControl.videoInfos.FirstOrDefault(x => x.videoId == selectedEmail.videoId);
                    SceneManager.LoadScene("DeepFakeScene");
                }
                else
                {
                    globalControl.StartCoroutine(PopIn(emailManager.emailAttachmentViewer));
                    emailManager.emailAttachmentViewer.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(selectedEmail.imageDir);
                }
                break;
            case "HomePageScene.EmailsAttachmentViewerExitButton":
            case "DeepFakeScene.EmailsAttachmentViewerExitButton":
                globalControl.StartCoroutine(PopOut(emailManager.emailAttachmentViewer));
                break;
            case "HomePageScene.MeetingMuteButton":
                PlaySound(normalClick);
                globalControl.StartCoroutine(DisplayMeetingDialogBox("ASIO requires all participants keep their mics active for security reasons", "MeetingMuteButton"));
                break;
            case "HomePageScene.MeetingVideoButton":
                PlaySound(normalClick);
                globalControl.StartCoroutine(DisplayMeetingDialogBox("ASIO requires all participants keep their video active for security reasons", "MeetingVideoButton"));
                break;
            case "HomePageScene.MeetingSecurityButton":
                PlaySound(normalClick);
                globalControl.StartCoroutine(DisplayMeetingDialogBox("Data transmission encrypted by 128bit key (TLSv1.3)", "MeetingSecurityButton"));
                break;
            case "HomePageScene.MeetingParticipantsButton":
                PlaySound(normalClick);
                globalControl.StartCoroutine(DisplayMeetingDialogBox("2 Participants:\nYou,\nBen Mcrae (host)", "MeetingParticipantsButton"));
                break;
            case "HomePageScene.MeetingChatButton":
                PlaySound(normalClick);
                break;
            case "HomePageScene.MeetingShareScreenButton":
                PlaySound(normalClick);
                globalControl.StartCoroutine(DisplayMeetingDialogBox("ASIO has disabled screen sharing for security and privacy reasons", "MeetingShareScreenButton"));
                break;
            case "HomePageScene.MeetingRecordButton":
                PlaySound(normalClick);
                globalControl.StartCoroutine(DisplayMeetingDialogBox("ASIO has disabled screen recording for security and privacy reasons", "MeetingRecordButton"));
                break;
            case "HomePageScene.MeetingEndButton":
                PlaySound(normalClick);
                if (meetingArea.meetingDialogBox.gameObject.activeInHierarchy)
                {
                    globalControl.StartCoroutine(PopOut(meetingArea.meetingDialogBox));
                }
                if (meetingArea.confirmMeetingEndDialog.gameObject.activeInHierarchy)
                {
                    globalControl.StartCoroutine(PopOut(meetingArea.confirmMeetingEndDialog));
                }
                else
                {
                    globalControl.StartCoroutine(PopIn(meetingArea.confirmMeetingEndDialog));
                }
                break;
            case "HomePageScene.ConfirmMeetingEndButton":
                PlaySound(normalClick);
                globalControl.EndMeeting();
                break;
            case "HomePageScene.BackMeetingEndButton":
                PlaySound(normalClick);
                globalControl.StartCoroutine(PopOut(meetingArea.confirmMeetingEndDialog));
                break;
            case "HomePageScene.AcknowledgeMeetingDialogButton":
                PlaySound(normalClick);
                globalControl.StartCoroutine(PopOut(meetingArea.meetingDialogBox));
                currentMeetingButtonName = null;
                break;
            default:
                //unknown button pressed
                Debug.LogWarning($"Unknown button with name: {buttonIdentifier} and id: {id}");
                break;
        }
    }

    private int GetEmailVideosCompletedToday()
    {
        int completedToday = 0;
        foreach (var email in globalControl.emails)
        {
            if (email.SentOnDate(globalControl.dateTime) && email.Completed(globalControl))
            {
                completedToday++;
            }
        }

        return completedToday;
    }

    public void ExitEmailsPage()
    {
        globalControl.StartCoroutine(PopOut(emailsPage, false));
        emailsPageShowing = false;
    }

    private void MeetingVideoPlayer_loopPointReached(VideoPlayer source)
    {
        source.Pause();
        globalControl.Invoke("EndMeeting", 1);
    }



    private IEnumerator DisplayMeetingDialogBox(string message, string buttonName)
    {
        if (meetingArea.confirmMeetingEndDialog.gameObject.activeInHierarchy)
        {
            globalControl.StartCoroutine(PopOut(meetingArea.confirmMeetingEndDialog));
        }
        if (meetingArea.meetingDialogBox.gameObject.activeInHierarchy)
        {
            yield return globalControl.StartCoroutine(PopOut(meetingArea.meetingDialogBox));
        }
        if (buttonName == currentMeetingButtonName)
        {
            currentMeetingButtonName = null;
            yield break;
        }
        currentMeetingButtonName = buttonName;
        globalControl.StartCoroutine(PopIn(meetingArea.meetingDialogBox));
        var button = GameObject.Find(buttonName).transform;
        meetingArea.meetingDialogBox.transform.position = button.position;
        meetingArea.meetingDialogBox.GetComponent<RectTransform>().anchoredPosition += Vector2.up * 18.75f + Vector2.right * 18.75f;
        meetingArea.meetingDialogBox.transform.Find("DialogText").GetComponent<TextMeshProUGUI>().text = message;
        yield return null;
    }

    private void ViewEmailContents(int id)
    {
        var email = emailManager.emails.FirstOrDefault(x => x.index == id);
        globalControl.StartCoroutine(PopIn(emailManager.emailBodyText.transform.parent.gameObject));
        emailManager.emailBodyText.transform.parent.gameObject.SetActive(true);
        emailManager.emailBodyText.text = email.emailMessage;
        emailManager.emailBodyText.transform.parent.Find("EmailHeaderText").GetComponent<TextMeshProUGUI>().text = email.emailTitle;
        emailManager.emailBodyText.transform.parent.Find("EmailSendDateTimeText").GetComponent<TextMeshProUGUI>().text = $"{email.sendTime} {email.sendDate}";
        emailManager.emailBodyText.transform.parent.Find("EmailSenderText").GetComponent<TextMeshProUGUI>().text = email.emailSender;
        selectedEmail = email;
        globalControl.read[email] = true;

        if (email.attachmentFlag)
        {
            emailManager.emailAttachmentButton.gameObject.SetActive(true);
            Image emailAttachmentImage = emailManager.emailAttachmentButton.gameObject.GetComponent<Image>();
            if (email.videoId != null)
            {
                var videoInfo = globalControl.videoInfos.FirstOrDefault(x => x.videoId == email.videoId);
                var path = Path.Combine(videoInfo.dir, "Thumbnail");
                emailAttachmentImage.sprite = Resources.Load<Sprite>(path);

                emailManager.emailAttachmentButton.enabled = email.SentOnDate(globalControl.dateTime);
            }
            else if (email.imageDir!= null)
            {
                var sprite = Resources.Load<Sprite>(email.imageDir);
                emailAttachmentImage.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"Email attachment not found for email with index: {email.index}");
            }

        }
        else
        {
            emailManager.emailAttachmentButton.gameObject.SetActive(false);
        }
    }

    public void PlaySound(AudioClip audioClip)
    {
        if (soundOn)
        {
            GameObject audioObject = new GameObject();
            audioObject.AddComponent<AudioSource>();
            AudioSource audioSource = audioObject.GetComponent<AudioSource>();
            audioSource.PlayOneShot(audioClip);
        }
    }

    internal void ZoomMouseDown()
    {
        var centre3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
        var zoomCentre = postProcessCam.ScreenToWorldPoint(centre3);
        Vector3[] videoCorners = new Vector3[4];
        videoRawImage.GetComponent<RectTransform>().GetWorldCorners(videoCorners);

        var relative = zoomCentre - videoCorners[0];
        var normalized = new Vector2(relative.x / (videoCorners[2].x - videoCorners[0].x), relative.y / (videoCorners[2].y - videoCorners[0].y));

        if (normalized.x < 0 || normalized.y < 0 || normalized.x > 1 || normalized.y > 1) return;
        var mouseDelta = prevMousePosition - Input.mousePosition;
        prevMousePosition = Input.mousePosition;
        ChangeVideoPosition(mouseDelta/2);
    }

    private void OnSliderValueChanged(Slider slider, string sliderName, int id)
    {
        // Code that should run when a slider value is changed
        switch (sliderName)
        {
            case "DeepFakeScene.ContrastSlider":
                // When the contrast slider value is changed do this
                var volume = GameObject.Find("PostProcessCam").GetComponent<Volume>();
                if (volume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
                {
                    colorAdjustments.contrast.overrideState = true;
                    colorAdjustments.contrast.value = slider.value * 200 - 100;
                }
                break;
            case "DeepFakeScene.SaturationSlider":
                // When the saturation slider value is changed
                var volume2 = GameObject.Find("PostProcessCam").GetComponent<Volume>();
                if (volume2.profile.TryGet<ColorAdjustments>(out var colorAdjustments2))
                {
                    colorAdjustments2.saturation.overrideState = true;
                    colorAdjustments2.saturation.value = slider.value * 200 - 100;
                }
                break;
            case "DeepFakeScene.ExposureSlider":
                // When the saturation slider value is changed
                var volume3 = GameObject.Find("PostProcessCam").GetComponent<Volume>();
                if (volume3.profile.TryGet<ColorAdjustments>(out var colorAdjustments3))
                {
                    colorAdjustments3.postExposure.overrideState = true;
                    colorAdjustments3.postExposure.value = slider.value * 3;
                }
                break;
            case "DeepFakeScene.ZoomSlider":
                Debug.Log($"Slider {sliderName} value changed to {slider.value}");
                SetVideoZoom(slider.value);
                break;
            case "DeepFakeScene.VideoScrubber":
                settingValue = true;
                break;
            default:
                // Unknown slider value changed
                Debug.LogWarning($"Unknown slider value changed with name: {sliderName} and id: {id}");
                break;
        }
    }

    private void OnToggleValueChanged(Toggle toggle, string toggleName, int id)
    {
        //Code that is run for all toggles that use the slider style type (every toggle for now)
        if (toggle.transition == Selectable.Transition.None)
        {
            var backdrop = toggle.transform.GetChild(0).GetComponent<RectTransform>();
            var handle = backdrop.GetChild(0).GetComponent<RectTransform>();
            if (toggle.isOn)
            {
                globalControl.StartCoroutine(ToggleSliderOn(backdrop, handle));
            }
            else
            {
                globalControl.StartCoroutine(ToggleSliderOff(backdrop, handle));
            }
        }
        // Code that should run when a toggle is changed
        switch (toggleName)
        {
            case "DeepFakeScene.AudioVisualiserToggle":
                PlaySound(normalClick);
                // when audio visualiser toggle value is changed
                if (toggle.isOn)
                {
                    // turn on the audio visualiser tool
                    audioVisualImage.GetComponent<Image>().sprite = Resources.Load<Sprite>(Path.Combine(globalControl.currentVideoInfo.dir, "AudioVisual"));
                    audioVisualImage.gameObject.SetActive(true);
                }
                else
                {
                    // turn off the audio visualiser tool
                    audioVisualImage.gameObject.SetActive(false);
                }
                break;

            case "MainMenuScene.OptionsSoundToggle":
                PlaySound(normalClick);
                soundOn = !soundOn;
                GameObject openingMusicObject = GameObject.Find("OpeningMusicAudioSource");
                AudioSource openingMusicAudioSource = openingMusicObject.GetComponent<AudioSource>();
                if (!soundOn)
                {
                    // sound is now off, turn off opening music
                    openingMusicAudioSource.Stop();
                }
                else
                {
                    // sound is now on, turn on opening music
                    openingMusicAudioSource.clip = openingMusic;
                    openingMusicAudioSource.Play();
                }
                break;
            case "HomePageScene.OptionsSoundToggle":
                PlaySound(normalClick);
                soundOn = !soundOn;
                if (!managerCall)
                {
                    // user hasn't accepted the incoming call yet
                    GameObject incomingCallObj = GameObject.Find("IncomingCall");
                    if (incomingCallObj != null)
                    {
                        AudioSource managerCallAudio = incomingCallObj.GetComponent<AudioSource>();
                        if (soundOn)
                        {
                            managerCallAudio.Play();
                        }
                        else
                        {
                            managerCallAudio.Stop();
                        }
                    }
                }
                break;
            case "HomePageScene.OptionsTextQuickLoadToggle":
                PlaySound(normalClick);
                globalControl.quickTextSkip = !globalControl.quickTextSkip;
                break;
            case "DeepFakeScene.FaceMappingSwitchToggle":
                ShowHideFaceMesh(toggle.isOn);
                break;
            case "DeepFakeScene.MetadataSwitchToggle":
                if (toggle.isOn)
                {
                    // turn on the audio visualiser tool
                    metadataImage.GetComponent<Image>().sprite = Resources.Load<Sprite>(Path.Combine(globalControl.currentVideoInfo.dir, "Metadata"));
                    metadataImage.gameObject.SetActive(true);
                }
                else
                {
                    // turn off the audio visualiser tool
                    metadataImage.gameObject.SetActive(false);
                }
                break;
            default:
                Debug.LogWarning($"Unknown toggle value changed with name: {toggleName} and id: {id}");
                break;
        }
    }

    private IEnumerator ToggleSliderOn(RectTransform backdrop, RectTransform handle)
    {
        var backdropImage = backdrop.GetComponent<Image>();
        for (int i = 0;i < ToggleSliderTransitionFrames; i++)
        {
            handle.anchoredPosition += Vector2.right * (backdrop.sizeDelta.x - handle.sizeDelta.x - ToggleSliderPadding) / ToggleSliderTransitionFrames;
            backdropImage.color = Color.grey * (7 - i) / 7 + ToggleBackgroundOnColor * (i) / 7;
            yield return null;
        }
        yield return null;
    }
    private IEnumerator ToggleSliderOff(RectTransform backdrop, RectTransform handle)
    {
        var backdropImage = backdrop.GetComponent<Image>();
        for (int i = 0; i < ToggleSliderTransitionFrames; i++)
        {
            handle.anchoredPosition += Vector2.left * (backdrop.sizeDelta.x - handle.sizeDelta.x - ToggleSliderPadding) / ToggleSliderTransitionFrames;
            backdropImage.color = Color.grey * (i) / 7 + ToggleBackgroundOnColor * (7 - i) / 7;
            yield return null;
        }
        yield return null;
    }

    internal void ChangeVideoZoom(float zoom)
    {
        var centre3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
        var zoomCentre = postProcessCam.ScreenToWorldPoint(centre3);
        Vector3[] videoCorners = new Vector3[4];
        videoRawImage.GetComponent<RectTransform>().GetWorldCorners(videoCorners);

        var relative = zoomCentre - videoCorners[0];
        var normalized = new Vector2(relative.x / (videoCorners[2].x - videoCorners[0].x), relative.y / (videoCorners[2].y - videoCorners[0].y));

        if (normalized.x < 0 || normalized.y < 0 || normalized.x > 1 || normalized.y > 1) return;

        var oldZoom = videoZoom;
        videoZoom += zoom * videoZoom * 0.1f;

        if (videoZoom > 8) videoZoom = 8;
        if (videoZoom < 1) videoZoom = 1;

        if (oldZoom == videoZoom) return;

        SetVideoZoom(videoZoom, normalized, videoZoom - oldZoom);
    }

    internal void ChangeVideoPosition(Vector2 rawDelta)
    {
        var uvRectCentreOffset = videoRawImage.uvRect.position;
        uvRectCentreOffset += rawDelta / videoZoom * 0.01f;
        uvRectCentreOffset = new Vector2(Mathf.Clamp(uvRectCentreOffset.x, 0, 1 - (1 / videoZoom)), Mathf.Clamp(uvRectCentreOffset.y, 0, 1 - (1 / videoZoom)));
        videoRawImage.uvRect = new Rect(uvRectCentreOffset, 1 / videoZoom * Vector2.one);
    }

    //Zooming needs more stuff to make it feel more fluid but this is at least better than zooming into the corner
    private void SetVideoZoom(float zoom, Vector2? normalized = null, float delta = 0)
    {
        var uvRectCentreOffset = videoRawImage.uvRect.position;
        if (normalized != null)
        {
            uvRectCentreOffset = normalized.Value - Vector2.one * (0.5f / zoom);
            uvRectCentreOffset = new Vector2(Mathf.Clamp(uvRectCentreOffset.x, 0, 1 - (1 / zoom)), Mathf.Clamp(uvRectCentreOffset.y, 0, 1 - (1 / zoom)));

            Debug.Log(normalized);
        }

        videoRawImage.uvRect = new Rect(uvRectCentreOffset, 1 / videoZoom * Vector2.one);
    }
    
    public IEnumerator PopOut(GameObject element, bool disableElementAtEnd = true)
    {
        int iterations = PopInOutTransitionFrames;
        var initScale = element.transform.localScale;
        for (int i = 0; i < iterations; i++)
        {
            element.transform.localScale = initScale * (1 - (float)i / iterations);
            yield return null;
        }

        if (disableElementAtEnd)
        {
            element.SetActive(false);
        }
        else
        {
            element.transform.localScale = Vector2.zero;
        }
    }

    public IEnumerator PopIn(GameObject element)
    {
        element.SetActive(true);
        int iterations = PopInOutTransitionFrames;
        for (int i = 0; i <= iterations; i++)
        {
            element.transform.localScale = Vector3.one * i / iterations;
            yield return null;
        }
    }

    internal void PausePlayVideo()
    {
        GameObject playPauseButton = GameObject.Find("PlayPauseButton");
        if (videoPlayer.isPaused)
        {
            playPauseButton.GetComponent<Image>().sprite = pauseImage;
        }
        else
        {
            playPauseButton.GetComponent<Image>().sprite = playImage;
        }
        if (videoPlayer.isPaused)
        {
            videoPlayer.Play();
            facemeshVideoPlayer.Play();
        }
        else
        { 
            videoPlayer.Pause();
            facemeshVideoPlayer.Pause(); 
            if (facemeshVideoPlayer.frame != videoPlayer.frame)
            {
                facemeshVideoPlayer.frame = videoPlayer.frame;
            }
        }
    }

    public void ShowHideFaceMesh(bool show)
    {
        if (show)
        {
            facemeshVideoPlayer.transform.SetAsLastSibling();
        }
        else
        {
            videoPlayer.transform.SetAsLastSibling();
        }
    }

    public IEnumerator VideoScrubberCoroutine()
    {
        bool wasPlaying = false;
        yield return new WaitUntil(() => videoPlayer.isPrepared);
        while (true)
        {
            while (settingValue)
            {
                if (videoPlayer.isPlaying) 
                { 
                    videoPlayer.Pause(); 
                    wasPlaying = true;
                }

                videoPlayer.frame = (long) (videoScrubber.value * videoPlayer.frameCount);

                if (!Input.GetKey(KeyCode.Mouse0)) break;

                yield return new WaitForFixedUpdate();
            }

            if (wasPlaying)
            {
                videoPlayer.Play();
                wasPlaying = false;
            }
            if (settingValue)
            {
                yield return new WaitForSeconds(0.1f); //this is kinda ugly but it fixes some weirdness with the slider
            }
            settingValue = false;

            videoScrubber.SetValueWithoutNotify((float)videoPlayer.frame / videoPlayer.frameCount);

            yield return null;
        }
    }
}

