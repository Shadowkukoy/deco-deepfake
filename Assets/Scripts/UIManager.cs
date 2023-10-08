using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using static UnityEngine.EventSystems.EventTrigger;

public class UIManager
{
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
    public bool soundOn = true;
    public int popup = 0;
    public Image metadataImage;
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
    public AudioClip openingMusic = (AudioClip)Resources.Load("Opening 1");
    public AudioClip vibration = (AudioClip)Resources.Load("Vibration");
    public AudioClip ringtone = (AudioClip)Resources.Load("Ringtone");

    public void AssignButtonListeners(GameObject elements)
    {
        foreach (Button button in elements.GetComponentsInChildren<Button>())
        {
            switch (button.name)
            {
                default:
                    button.onClick.AddListener(delegate { OnButtonPress(SceneManager.GetActiveScene().name + "." + button.name, 0); });
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

    private void OnButtonPress(string button, int id)
    {
        //Code that should be run when a button is pressed!
        //button: the name of the scene and name of the button GameObject in the format Scene.ButtonName
        //id: a number which can optionally be assigned to be bruh passed through when the button is pressed (could be useful if multiple buttons have the same name).
        switch (button)
        {
            case "DeepFakeScene.YesButton":
                PlaySound(normalClick);
                //stuff that happens when yes button is pressed
                Debug.Log("test1");
                break;
            case "DeepFakeScene.NoButton":
                PlaySound(normalClick);
                //stuff that happens when no button is pressed
                Debug.Log("test2");
                break;
            case "DeepFakeScene.MetadataButton":
                PlaySound(normalClick);
                // stuff happens when metadata button is pressed
                //Debug.Log($"{button} test3");
                if (!metaState)
                {
                    globalControl.StartCoroutine(UnNuke(metadataImage.gameObject));
                    metaState = true;
                }
                else
                {
                    globalControl.StartCoroutine(Nuke(metadataImage.gameObject));
                    metaState = false;
                }
                break;
            case "DeepFakeScene.BackButton":
                PlaySound(normalClick);
                SceneManager.LoadScene("MainMenuScene");
                break;
            case "MainMenuScene.PlayButton":
                PlaySound(normalClick);
                SceneManager.LoadScene("HomePageScene");
                break;
            case "MainMenuScene.AboutButton":
                PlaySound(normalClick);
                SceneManager.LoadScene("HomePageScene");
                popup = 1;
                break;
            case "MainMenuScene.OptionsButton":
                PlaySound(settingsClick);
                SceneManager.LoadScene("HomePageScene");
                popup = 2;
                break;
            case "MainMenuScene.DisclaimerAgreeButton":
                PlaySound(normalClick);
                GameObject disclaimer = GameObject.Find("Disclaimer");
                disclaimerAgreed = true;
                globalControl.StartCoroutine(Nuke(disclaimer));
                break;
            case "HomePageScene.HomeButton":
                PlaySound(normalClick);
                SceneManager.LoadScene("MainMenuScene");
                popup = 0;
                break;
            case "HomePageScene.PlayButton":
                SceneManager.LoadScene("DeepFakeScene");
                break;
            case "HomePageScene.InfoButton":
                PlaySound(normalClick);
                if (!aboutUsState)
                {
                    globalControl.StartCoroutine(UnNuke(aboutUsPage));
                    aboutUsState = true;
                }
                else
                {
                    globalControl.StartCoroutine(Nuke(aboutUsPage));
                    aboutUsState = false;
                }
                break;
            case "HomePageScene.AboutExitButton":
                PlaySound(normalClick);
                globalControl.StartCoroutine(Nuke(aboutUsPage));
                aboutUsState = false;
                break;
            case "HomePageScene.SettingsButton":
                PlaySound(settingsClick);
                if (!optionsState)
                {
                    globalControl.StartCoroutine(UnNuke(optionsPage));
                    optionsState = true;
                }
                else
                {
                    globalControl.StartCoroutine(Nuke(optionsPage));
                    optionsState = false;
                }
                break;
            case "HomePageScene.OptionsExitButton":
                PlaySound(normalClick);
                globalControl.StartCoroutine(Nuke(optionsPage));
                optionsState = false;
                break;
            case "HomePageScene.AcceptCallButton":
                PlaySound(normalClick);
                managerCall = true;
                globalControl.StartCoroutine(Nuke(incomingCall));
                break;
            case "HomePageScene.RejectCallButton":
                PlaySound(errorClick);
                break;
            case "HomePageScene.EmailButton":
                PlaySound(normalClick);
                break;
            case "HomePageScene.CalendarButton":
                PlaySound(normalClick);
                break;
            case "HomePageScene.WatchGameIntroButton":
                PlaySound(normalClick);
                break;
            case "HomePageScene.OptionsSoundButton":
                PlaySound(normalClick);
                soundOn = !soundOn;
                break;
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
                videoPlayer.time = videoPlayer.time + 5;
                break;
            case "DeepFakeScene.StepBackwardButton":
                PlaySound(normalClick);
                videoPlayer.frame = videoPlayer.frame - 1;
                break;
            case "DeepFakeScene.JumpBackwardButton":
                PlaySound(normalClick);
                videoPlayer.time = videoPlayer.time - 5;
                break;
            default:
                //unknown button pressed
                Debug.LogWarning($"Unknown button with name: {button} and id: {id}");
                break;
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
        // Code that should run when a toggle is changed
        switch (toggleName)
        {
            case "DeepFakeScene.AudioVisualiserToggle":
                // when audio visualiser toggle value is changed
                if (toggle.isOn)
                {
                    // turn on the audio visualiser tool
                    Debug.Log("Audio tool turned on.");
                }
                else
                {
                    // turn off the audio visualiser tool
                    Debug.Log("Audio tool turned off.");
                }
                break;
            default:
                Debug.LogWarning($"Unknown toggle value changed with name: {toggleName} and id: {id}");
                break;
        }
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
    
    public IEnumerator Nuke(GameObject element)
    {
        int iterations = 8;
        var initScale = element.transform.localScale;
        for (int i = 0; i < iterations; i++)
        {
            element.transform.localScale = initScale * (1 - (float)i / iterations);
            yield return null;
        }
        element.SetActive(false);
    }

    public IEnumerator UnNuke(GameObject element)
    {
        element.SetActive(true);
        int iterations = 8;
        for (int i = 0; i < iterations; i++)
        {
            element.transform.localScale = Vector3.one * (float)i / iterations;
            yield return null;
        }
    }

    internal void PausePlayVideo()
    {
        if (videoPlayer.isPaused) videoPlayer.Play();
        else videoPlayer.Pause();
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

