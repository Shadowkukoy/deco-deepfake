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

public class GlobalControlScript : MonoBehaviour
{
    // Start is called before the first frame update
    public UIManager uiManager;
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);

        uiManager = new UIManager();
        uiManager.globalControl = this; //this is disgusting

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Code that should be run when a scene is loaded
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            uiManager.canvas = canvas.GetComponent<Canvas>();
            uiManager.AssignButtonListeners(canvas);
            uiManager.AssignSliderListeners(canvas);
            uiManager.AssignToggleListeners(canvas); //This technically causes a bug if any scene other than deepfake scene is loaded!
        }
        switch (scene.name)
        {
            case "DeepFakeScene":
                uiManager.videoPlayer = GameObject.Find("Video").GetComponent<VideoPlayer>();
                uiManager.videoRawImage = uiManager.videoPlayer.gameObject.GetComponent<RawImage>();
                uiManager.postProcessCam = GameObject.Find("PostProcessCam").GetComponent<Camera>();
                uiManager.noPostCam = GameObject.Find("NoPostCam").GetComponent<Camera>();
                uiManager.videoScrubber = GameObject.Find("VideoScrubber").GetComponent<Slider>();
                StartCoroutine(uiManager.VideoScrubberCoroutine());
                GameObject videoCanvas = GameObject.Find("VideoCanvas");
                uiManager.videoCanvas = videoCanvas.GetComponent<Canvas>();
                uiManager.metadataImage = GameObject.Find("MetaDataImage").GetComponent<Image>();
                uiManager.metadataImage.gameObject.SetActive(false);
                uiManager.yesNoVideoArea = GameObject.Find("YesNoVideoArea");
                uiManager.deepFakeSceneTypeWriter = uiManager.yesNoVideoArea.transform.GetChild(0).GetComponent<TypeWriter>();
                StartCoroutine(VideoStuffCoroutine());
                uiManager.deepFakeSceneTypeWriter.LoadNextText(uiManager.deepFakeSceneTypeWriter.gameObject);
                break;
            case "HomePageScene":
                uiManager.PlaySound(uiManager.windowsBootSound);
                uiManager.incomingCall = GameObject.Find("IncomingCall");
                uiManager.incomingCall.SetActive(false);
                uiManager.aboutUsPage = GameObject.Find("AboutUsPage");
                uiManager.optionsPage = GameObject.Find("OptionsPage");
                uiManager.aboutUsTypeWriter = uiManager.aboutUsPage.transform.GetChild(0).GetChild(0).GetComponent<TypeWriter>();

                if (!uiManager.managerCall)
                {
                    Invoke("ShowManagerCall", 5);
                }

                if (uiManager.popup == 0)
                {
                    uiManager.aboutUsPage = GameObject.Find("AboutUsPage");
                    uiManager.aboutUsPage.SetActive(false);
                    uiManager.optionsPage = GameObject.Find("OptionsPage");
                    uiManager.optionsPage.SetActive(false);
                }
                else if (uiManager.popup == 1)
                {
                    // keep about us page
                    uiManager.aboutUsPage.SetActive(true);
                    uiManager.aboutUsTypeWriter.LoadNextText(uiManager.aboutUsTypeWriter.gameObject);
                    uiManager.optionsPage.SetActive(false);
                }
                else
                {
                    // keep options page
                    uiManager.aboutUsPage = GameObject.Find("AboutUsPage");
                    uiManager.aboutUsPage.SetActive(false);
                    uiManager.optionsPage = GameObject.Find("OptionsPage");
                    uiManager.optionsPage.SetActive(true);
                }

                break;
            case "MainMenuScene":
                if (uiManager.soundOn)
                {
                    AudioSource openingMusicAudioSource = GameObject.Find("OpeningMusicAudioSource").GetComponent<AudioSource>();
                    openingMusicAudioSource.clip = uiManager.openingMusic;
                    Debug.Log($"Is the audio source null? {openingMusicAudioSource == null}");
                    Debug.Log($"Is the opening music audio clip null? {uiManager.openingMusic == null}");
                    Debug.Log($"Is the audio clip null? {openingMusicAudioSource.clip == null}");
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
        uiManager.incomingCall.SetActive(true);
        AudioClip callClip;
        if (uiManager.soundOn)
        {
            callClip = uiManager.ringtone;
        }
        else
        {
            callClip = uiManager.vibration;
        }
        uiManager.incomingCall.GetComponent<AudioSource>().clip = callClip;
        uiManager.incomingCall.GetComponent<AudioSource>().Play();
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
