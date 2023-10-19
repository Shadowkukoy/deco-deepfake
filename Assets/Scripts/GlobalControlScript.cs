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
using static UnityEngine.ParticleSystem;
using System.Linq;

public class GlobalControlScript : MonoBehaviour
{
    public const int DayStartTime = 8;
    private readonly DateTime StartDate = new DateTime(2025,5,13);
    private bool bootSoundPlayed = false;

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
    public Dictionary<VideoInfo, bool> videosCorrect;
    public bool quickTextSkip = false;
    public List<Email> emails;
    public List<Article> articles;
    public Dictionary<Email,bool> read;
    public TextAsset inboxJsonFile;
    public TextAsset articlesJsonFile;
    private bool showEmailsOnLoad;
    public readonly DateTime TwistDate = new DateTime(2025, 5, 15);

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);
        
        uiManager = new UIManager();
        uiManager.globalControl = this; //this is disgusting

        videosCorrect = new Dictionary<VideoInfo, bool>();

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

        var articlesJArray = JArray.Parse(articlesJsonFile.text);
        foreach (var articleJObject in articlesJArray)
        {
            var article = JsonUtility.FromJson<Article>(articleJObject.ToString());

            articles.Add(article);
        }

        dateTime = StartDate;
        dateTime = dateTime.AddHours(DayStartTime);
        read = new Dictionary<Email, bool>();
        ReadAllMail();

        showEmailsOnLoad = false;
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
                }
                uiManager.videoRawImage = uiManager.videoPlayer.gameObject.GetComponent<RawImage>();
                uiManager.postProcessCam = GameObject.Find("PostProcessCam").GetComponent<Camera>();
                uiManager.noPostCam = GameObject.Find("NoPostCam").GetComponent<Camera>();
                uiManager.videoScrubber = GameObject.Find("VideoScrubber").GetComponent<Slider>();
                StartCoroutine(uiManager.VideoScrubberCoroutine());
                GameObject videoCanvas = GameObject.Find("VideoCanvas");
                uiManager.videoCanvas = videoCanvas.GetComponent<Canvas>();
                uiManager.metadataImage = GameObject.Find("MetaDataImage").GetComponent<Image>();
                uiManager.metadataImage.gameObject.SetActive(false);
                uiManager.audioVisualImage = GameObject.Find("AudioVisualImage").GetComponent<Image>();
                uiManager.audioVisualImage.gameObject.SetActive(false);
                uiManager.yesNoVideoArea = GameObject.Find("YesNoVideoArea");
                uiManager.deepFakeSceneTypeWriter = uiManager.yesNoVideoArea.transform.GetChild(0).GetComponent<TypeWriter>();
                StartCoroutine(VideoStuffCoroutine());
                uiManager.deepFakeSceneTypeWriter.LoadNextText(uiManager.deepFakeSceneTypeWriter.gameObject);
                uiManager.deepFakeSceneTypeWriter.CompleteTextRevealed += DeepFakeSceneTypeWriter_CompleteTextRevealed;
                showEmailsOnLoad = true;
                break;
            case "HomePageScene":
                InstantiateEmailsPage();
                if (!bootSoundPlayed)
                {
                    uiManager.PlaySound(uiManager.windowsBootSound);
                    bootSoundPlayed = true;
                }
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
                if (showEmailsOnLoad)
                {
                    ShowEmailsPage();
                    showEmailsOnLoad = false;
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

    private void DeepFakeSceneTypeWriter_CompleteTextRevealed()
    {
        var yesButton = uiManager.canvas.transform.Find("YesNoVideoArea/YesButton").gameObject;
        var noButton = uiManager.canvas.transform.Find("YesNoVideoArea/NoButton").gameObject;

        StartCoroutine(uiManager.PopIn(yesButton));
        StartCoroutine(uiManager.PopIn(noButton));

        uiManager.AssignButtonListeners(yesButton);
        uiManager.AssignButtonListeners(noButton);
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
        uiManager.emailsPage.transform.localScale = Vector2.zero;

    }
    public void ShowEmailsPage()
    {
        StartCoroutine(uiManager.PopIn(uiManager.emailsPage));
        emailManager.RefreshEmail();
        uiManager.emailsPage.transform.SetAsLastSibling();


    }
    private void ReadAllMail()
    {
        JArray emailsJArray = JArray.Parse(inboxJsonFile.text);
        foreach (var emailJObject in emailsJArray)
        {
            var email = JsonUtility.FromJson<Email>(emailJObject.ToString());
            emails.Add(email);
            read[email] = false;
        }
    }

    public void EndMeeting()
    {
        dateTime = dateTime.AddHours(uiManager.meeting.advanceTimeHours);
        emailManager.RefreshEmail();
        StartCoroutine(uiManager.PopOut(uiManager.meetingArea.gameObject));
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

    public void NextDay()
    {
        dateTime = dateTime.Date.AddDays(1).AddHours(DayStartTime);
        uiManager.managerCall = false;
        StartCoroutine(EndDayCoroutine());
        uiManager.ExitEmailsPage();
    }

    private IEnumerator EndDayCoroutine()
    {
        //Fade to black
        yield return StartCoroutine(FadeToBlack());
        var endOfDayArticle = uiManager.canvas.transform.Find("Black/EndOfDayArticle");
        Article articleToShow = null;
        foreach (var article in articles) {
            if (DateTime.Parse(article.articleDate).Date != dateTime.Date) continue;
            var requirementsFailed = false;
            if (article.correctRequired != null)
            {
                foreach (var videoId in article.correctRequired.Split(','))
                {
                    var videoCorrect = videosCorrect.FirstOrDefault(x => x.Key.videoId == videoId);
                    if (!videoCorrect.Equals(default(KeyValuePair<VideoInfo, bool>)) && !videoCorrect.Value)
                    {
                        requirementsFailed = true;
                    }
                }
                if (requirementsFailed) { continue; }
            }
            requirementsFailed = false;

            if (article.incorrectRequired != null)
            {
                foreach (var videoId in article.incorrectRequired.Split(','))
                {
                    var videoCorrect = videosCorrect.FirstOrDefault(x => x.Key.videoId == videoId);
                    if (!videoCorrect.Equals(default(KeyValuePair<VideoInfo, bool>)) && videoCorrect.Value)
                    {
                        requirementsFailed = true;
                    }
                }
                if (requirementsFailed) { continue; }
            }

            if (!requirementsFailed) 
            { 
                articleToShow = article; break; 
            }
        }
        Debug.Log(dateTime);
        endOfDayArticle.GetComponent<Image>().sprite = Resources.Load<Sprite>(articleToShow.articleDir);
        yield return StartCoroutine(uiManager.PopIn(endOfDayArticle.gameObject));
        endOfDayArticle.transform.SetAsLastSibling();

        yield return new WaitForSeconds(1);
        var endOfDayArticlePrompt = endOfDayArticle.transform.Find("EndOfDayArticlePrompt");
        var nextDayText = uiManager.canvas.transform.Find("Black/NextDayText");
        yield return StartCoroutine(uiManager.PopIn(endOfDayArticlePrompt.gameObject));
        yield return new WaitUntil(() => Input.anyKey);
        yield return StartCoroutine(uiManager.PopOut(endOfDayArticlePrompt.gameObject));
        yield return StartCoroutine(uiManager.PopOut(endOfDayArticle.gameObject));

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(uiManager.PopIn(nextDayText.gameObject));
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(uiManager.PopOut(nextDayText.gameObject));


        if (dateTime.Date == TwistDate.AddDays(1))
        {
            yield return new WaitForSeconds(1);

            if (videosCorrect[videoInfos.FirstOrDefault(x => x.videoId == "ICantBelieveItsNotBen")])
            {
                Invoke("ShowManagerCall", 3);
            }
            else
            {
                var sound = uiManager.PlaySoundWithReturn(uiManager.vibration);
                yield return new WaitForSeconds(3);
                Destroy(sound);
                yield return new WaitForSeconds(1);
                if (videosCorrect[videoInfos.FirstOrDefault(x => x.videoId == "VoteTwice")])
                {
                    if (videosCorrect[videoInfos.FirstOrDefault(x => x.videoId == "PeterDuttonIncomeAssistance")])
                    {
                        sound = uiManager.PlaySoundWithReturn(uiManager.badDay3);
                    }
                    else
                    {
                        sound = uiManager.PlaySoundWithReturn(uiManager.badDay3Dutton);
                    }
                }
                else
                {
                    if (videosCorrect[videoInfos.FirstOrDefault(x => x.videoId == "PeterDuttonIncomeAssistance")])
                    {
                        sound = uiManager.PlaySoundWithReturn(uiManager.badDay3Albo);
                    }
                    else
                    {
                        sound = uiManager.PlaySoundWithReturn(uiManager.badDay3AlboDutton);
                    }
                }

                var audioSource = sound.GetComponent<AudioSource>();
                yield return new WaitWhile(() => audioSource.isPlaying);
            }

            StartCoroutine(uiManager.PopIn(uiManager.canvas.transform.Find("Black/EndOfDemo").gameObject));

            yield return new WaitForSeconds(3);

            StartCoroutine(uiManager.PopIn(endOfDayArticlePrompt.gameObject));

            yield return new WaitUntil(() => Input.anyKey);
            Application.Quit();
        }
        else
        {
            Invoke("ShowManagerCall", 3);
        }

        yield return StartCoroutine(FadeOutFromBlack());

    }

    private IEnumerator FadeToBlack()
    {
        uiManager.black = uiManager.canvas.transform.Find("Black");
        uiManager.black.gameObject.SetActive(true);
        uiManager.blackImage = uiManager.black.GetComponent<Image>();
        uiManager.black.SetAsLastSibling();
        uiManager.blackImage.color = new Color(0, 0, 0, 0);
        for (int i = 0; i < 100; i++)
        {
            uiManager.blackImage.color += Color.black / 100f;
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator FadeOutFromBlack()
    {
        uiManager.black = uiManager.canvas.transform.Find("Black");
        uiManager.black.gameObject.SetActive(true);
        uiManager.blackImage = uiManager.black.GetComponent<Image>();
        uiManager.black.SetAsLastSibling();
        uiManager.blackImage.color = new Color(0, 0, 0, 1);
        for (int i = 0; i < 25; i++)
        {
            uiManager.blackImage.color -= Color.black / 100f;
            yield return new WaitForFixedUpdate();
        }
        uiManager.black.gameObject.SetActive(false);

    }

    private void ShowManagerCall()
    {
        StartCoroutine(uiManager.PopIn(uiManager.incomingCall));
        uiManager.incomingCall.transform.SetAsLastSibling();
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
            StartCoroutine(uiManager.PopIn(uiManager.incomingCall));
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
