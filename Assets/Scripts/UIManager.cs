using System;
using System.Collections;
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
    public Image metadataImage;
    public Vector3 prevMousePosition;
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
                //stuff that happens when yes button is pressed
                Debug.Log("test1");
                break;
            case "DeepFakeScene.NoButton":
                //stuff that happens when no button is pressed
                Debug.Log("test2");
                break;
            case "DeepFakeScene.MetadataButton":
                // stuff happens when metadata button is pressed
                //Debug.Log($"{button} test3");
                if (!metaState)
                {
                    metadataImage.gameObject.SetActive(true);
                    metaState = true;
                }
                else
                {
                    metadataImage.gameObject.SetActive(false);
                    metaState = false;
                }
                break;
            case "DeepFakeScene.BackButton":
                SceneManager.LoadScene("MainMenuScene");
                break;
            case "MainMenuScene.PlayButton":
                SceneManager.LoadScene("HomePageScene");
                AudioSource audio = GameObject.Find("WindowsBootSound").GetComponent<AudioSource>();
                audio.Play();
                break;
            case "MainMenuScene.AboutButton":
                SceneManager.LoadScene("AboutPageScene");
                break;
            case "AboutPageScene.BackButton":
                SceneManager.LoadScene("MainMenuScene");
                break;
            case "HomePageScene.HomeButton":
                SceneManager.LoadScene("MainMenuScene");
                break;
            case "HomePageScene.PlayButton":
                SceneManager.LoadScene("DeepFakeScene");
                break;
            case "DeepFakeScene.PlayPauseButton":
                PausePlayVideo();
                break;
            case "DeepFakeScene.StepForwardButton":
                videoPlayer.frame = videoPlayer.frame + 1;
                break;
            case "DeepFakeScene.JumpForwardButton":
                videoPlayer.time = videoPlayer.time + 5;
                break;
            case "DeepFakeScene.StepBackwardButton":
                videoPlayer.frame = videoPlayer.frame - 1;
                break;
            case "DeepFakeScene.JumpBackwardButton":
                videoPlayer.time = videoPlayer.time - 5;
                break;
            default:
                //unknown button pressed
                Debug.LogWarning($"Unknown button with name: {button} and id: {id}");
                break;
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

