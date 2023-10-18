using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.Rendering;
using System.Threading;
using Deepfakes.Typography.TypeWriter;
using Newtonsoft.Json.Linq;
using TMPro;
using System.IO;

public class GlobalControlScript : MonoBehaviour
{
    public const int DayStartTime = 8;
    private readonly DateTime StartDate = new DateTime(2025,5,13);

    public UIManager uiManager;
    public GameObject aboutPagePrefab;
    public GameObject emailsPagePrefab;
    public GameObject settingsPagePrefab;
    public TextAsset videoInfosJsonFile;
    public TextAsset meetingsFile;
    public List<VideoInfo> videoInfos;
    public List<Meeting> meetings;
    public DateTime dateTime;
    public EmailManager emailManager;
    public VideoInfo currentVideoInfo;
    internal VideoClip normalVideo;
    internal VideoClip facemeshVideo;

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);
        
        uiManager = new UIManager();
        uiManager.globalControl = this; //this is disgusting

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        aboutPagePrefab = Resources.Load<GameObject>("Prefabs/AboutUsPagePrefab");
        settingsPagePrefab = Resources.Load<GameObject>("Prefabs/OptionsPagePrefab");
        emailsPagePrefab = Resources.Load<GameObject>("Prefabs/EmailsPagePrefab");

        var videoInfosJArray = JArray.Parse(videoInfosJsonFile.text);
        foreach (var videoInfoJObject in videoInfosJArray)
        {
            var videoInfo = JsonUtility.FromJson<VideoInfo>(videoInfoJObject.ToString());

            videoInfos.Add(videoInfo);
        }

        var meetingsJArray = JArray.Parse(meetingsFile.text);
        foreach (var meetingJObject in meetingsJArray)
        {
            var meeting = JsonUtility.FromJson<Meeting>(meetingJObject.ToString());

            meetings.Add(meeting);
        }

        dateTime = StartDate;
        dateTime = dateTime.AddHours(DayStartTime);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
        //Code that should be run when a scene is loaded
        GameObject canvas = GameObject.Find("Canvas");

        uiManager.aboutUsPage = Instantiate(aboutPagePrefab, canvas.transform);
        uiManager.optionsPage = Instantiate(settingsPagePrefab, canvas.transform);

        if (canvas != null)
        {
            uiManager.canvas = canvas.GetComponent<Canvas>();
            uiManager.AssignButtonListeners(canvas);
            uiManager.AssignSliderListeners(canvas);
            uiManager.AssignToggleListeners(canvas); //This technically causes a bug if any scene other than deepfake scene is loaded!
        }
        uiManager.aboutUsTypeWriter = uiManager.aboutUsPage.transform.GetChild(0).GetChild(0).GetComponent<TypeWriter>();
        uiManager.aboutUsPage.SetActive(false);
        uiManager.optionsPage.SetActive(false);
        uiManager.emailsPageShowing = false;
        InstantiateEmailsPage();
        switch (scene.name)
        {
            case "DeepFakeScene":
                uiManager.videoPlayer = GameObject.Find("Video").GetComponent<VideoPlayer>();
                uiManager.facemeshVideoPlayer = GameObject.Find("FacemeshVideo").GetComponent<VideoPlayer>();
                if (currentVideoInfo != null)
                {
                    normalVideo = Resources.Load<VideoClip>(Path.Combine(currentVideoInfo.dir, "Video"));
                    facemeshVideo = Resources.Load<VideoClip>(Path.Combine(currentVideoInfo.dir, "Facemesh"));
                    uiManager.videoPlayer.clip = normalVideo;
                    uiManager.facemeshVideoPlayer.clip = facemeshVideo;
                    uiManager.facemeshVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                }
                uiManager.videoRawImage = uiManager.videoPlayer.gameObject.GetComponent<RawImage>();
                uiManager.postProcessCam = GameObject.Find("PostProcessCam").GetComponent<Camera>();
                uiManager.noPostCam = GameObject.Find("NoPostCam").GetComponent<Camera>();
                uiManager.videoScrubber = GameObject.Find("VideoScrubber").GetComponent<Slider>();
                StartCoroutine(uiManager.VideoScrubberCoroutine());
                GameObject videoCanvas = GameObject.Find("VideoCanvas");
                uiManager.videoCanvas = videoCanvas.GetComponent<Canvas>();
                uiManager.metadataImage = GameObject.Find("MetaDataImage").GetComponent<UnityEngine.UI.Image>();
                uiManager.metadataImage.gameObject.SetActive(false);
                uiManager.audioVisualImage = GameObject.Find("AudioVisualImage").GetComponent<UnityEngine.UI.Image>();
                uiManager.audioVisualImage.gameObject.SetActive(false);
                uiManager.yesNoVideoArea = GameObject.Find("YesNoVideoArea");
                uiManager.deepFakeSceneTypeWriter = uiManager.yesNoVideoArea.transform.GetChild(0).GetComponent<TypeWriter>();
                StartCoroutine(VideoStuffCoroutine());
                uiManager.deepFakeSceneTypeWriter.LoadNextText(uiManager.deepFakeSceneTypeWriter.gameObject);
                break;
            case "HomePageScene":
                uiManager.PlaySound(uiManager.windowsBootSound);
                uiManager.incomingCall = GameObject.Find("IncomingCall");
                uiManager.incomingCall.SetActive(false);

                uiManager.meetingArea = GameObject.Find("MeetingArea").GetComponent<MeetingAreaScript>();
                uiManager.meetingArea.gameObject.SetActive(false);
                uiManager.meetingArea.confirmMeetingEndDialog.SetActive(false);
                uiManager.meetingArea.meetingDialogBox.SetActive(false);

                if (!uiManager.managerCall)
                {
                    Invoke("ShowManagerCall", 5);
                }

                break;
            case "MainMenuScene":
                if (UIManager.soundOn)
                {
                    AudioSource openingMusicAudioSource = GameObject.Find("OpeningMusicAudioSource").GetComponent<AudioSource>();
                    openingMusicAudioSource.clip = uiManager.openingMusic;
                    openingMusicAudioSource.Play();
                }
                uiManager.disclaimer = GameObject.Find("Disclaimer");
                if (!uiManager.disclaimerAgreed)
                {
                    uiManager.disclaimer.SetActive(true);
                }
                else
                {
                    uiManager.disclaimer.SetActive(false);
                }
                break;
            default:
                Debug.LogWarning($"Unknown scene loaded with name {scene.name}");
                break;
        }
    }
    private void InstantiateEmailsPage()
    {
        uiManager.emailsPage = Instantiate(emailsPagePrefab, uiManager.canvas.transform);
        emailManager = uiManager.emailsPage.GetComponent<EmailManager>();
        uiManager.emailManager = emailManager;
        emailManager.uiManager = uiManager;
        emailManager.globalControl = this;
        emailManager.emailPrefab = Resources.Load<GameObject>("Prefabs/EmailPrefab");
        uiManager.AssignButtonListeners(uiManager.emailsPage);
        uiManager.emailsPage.SetActive(false);

    }
    public void ShowEmailsPage()
    {
        StartCoroutine(uiManager.UnNuke(uiManager.emailsPage));
        emailManager.RefreshEmail();
        uiManager.emailsPage.transform.SetAsLastSibling();
    }
    public void Update()
    {

    }

    public IEnumerator VideoStuffCoroutine()
    {
        while (true)
        {
            yield return null;

            ZoomControls();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                uiManager.PausePlayVideo();
            }
        }
    }

    private void ShowManagerCall()
    {
        StartCoroutine(uiManager.UnNuke(uiManager.incomingCall));
        if (uiManager.timesRejected >= 3) 
        {
            uiManager.incomingCall.transform.Find("CallerPhoto").GetComponent<Image>().color = Color.red;
            Destroy(uiManager.incomingCall.transform.Find("RejectCallButton").gameObject);
            var acceptCallButton = uiManager.incomingCall.transform.Find("AcceptCallButton");
            var newAcceptCallButton = Instantiate(acceptCallButton.gameObject, uiManager.incomingCall.transform);
            newAcceptCallButton.GetComponent<RectTransform>().anchoredPosition += (43.2f * 2) * Vector2.left;
            newAcceptCallButton.gameObject.name = "AcceptCallButton";
            uiManager.AssignButtonListeners(newAcceptCallButton);
        }
        if (UIManager.soundOn)
        {
            uiManager.incomingCall.GetComponent<AudioSource>().clip = uiManager.ringtone;
            uiManager.incomingCall.GetComponent<AudioSource>().Play();
        }
        else
        {
            uiManager.incomingCall.GetComponent<AudioSource>().Stop();
        }
        if (uiManager.incomingCall != null)
        {
            StartCoroutine(uiManager.UnNuke(uiManager.incomingCall));
            if (UIManager.soundOn)
            {
                uiManager.incomingCall.GetComponent<AudioSource>().clip = uiManager.ringtone;
                uiManager.incomingCall.GetComponent<AudioSource>().Play();
            }
            else
            {
                uiManager.incomingCall.GetComponent<AudioSource>().Stop();
            }
        }  
    }

    private void ZoomControls()
    {
        var scrollY = Input.mouseScrollDelta.y;
        if (scrollY != 0)
        {
            uiManager.ChangeVideoZoom(scrollY);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            uiManager.prevMousePosition = Input.mousePosition;
        }
        if (Input.GetKey(KeyCode.Mouse0))
        {
            uiManager.ZoomMouseDown();
        }

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            uiManager.ChangeVideoPosition(Vector2.up);
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            uiManager.ChangeVideoPosition(Vector2.down);
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            uiManager.ChangeVideoPosition(Vector2.right);
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            uiManager.ChangeVideoPosition(Vector2.left);
        }
    }
}
