using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIManager
{
    //Code that assigns the method 'OnButtonPress' to the pressing of any children gameobjects.
    //This should be run on canvas whenever a new scene is loaded, and on any new instantiated UI element which contains buttons

    public VideoPlayer videoPlayer;
    public RawImage videoRawImage;
    private float videoZoom;
    public Slider zoomSlider;


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
 
    private void OnButtonPress(string button, int id)
    {
        //Code that should be run when a button is pressed!
        //button: the name of the scene and name of the button GameObject in the format Scene.ButtonName
        //id: a number which can optionally be assigned to be passed through when the button is pressed (could be useful if multiple buttons have the same name).
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
                Debug.Log($"{button} test3");
                break;
            default:
                //unknown button pressed
                Debug.LogWarning($"Unknown button with name: {button} and id: {id}");
                break;
        }
    }
    private void OnSliderValueChanged(Slider slider, string sliderName, int id)
    {
        // Code that should run when a slider value is changed
        switch (sliderName)
        {
            case "DeepFakeScene.LightingSlider":
                // When the lighting slider value is changed
                Debug.Log($"Slider {sliderName} value changed to {slider.value}");
                break;
            case "DeepFakeScene.ZoomSlider":
                Debug.Log($"Slider {sliderName} value changed to {slider.value}");
                SetVideoZoom(slider.value, Vector2.zero);
                break;
            default:
                // Unknown slider value changed
                Debug.LogWarning($"Unknown slider value changed with name: {slider} and id: {id}");
                break;
        }
    }
    internal void ChangeVideoZoom(float zoom)
    {
        videoZoom += zoom;

        if (videoZoom > 4) videoZoom = 4;
        if (videoZoom < 1) videoZoom = 1;

        SetVideoZoom(videoZoom, Input.mousePosition);
    }

    private void SetVideoZoom(float zoom, Vector2 centre)
    {
        var videoRect = videoRawImage.GetComponent<RectTransform>().rect;

        videoRawImage.uvRect = new Rect(videoRawImage.uvRect.position, 1 / videoZoom * Vector2.one);

        zoomSlider.value = zoom;
    }
}

