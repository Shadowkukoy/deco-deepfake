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

public class GlobalControlScript : MonoBehaviour
{
    // Start is called before the first frame update
    public UIManager uiManager;
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);

        uiManager = new UIManager();

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
            uiManager.AssignToggleListeners(canvas);
        }
        switch (scene.name)
        {
            case "DeepFakeScene":
                uiManager.videoPlayer = GameObject.Find("Video").GetComponent<VideoPlayer>();
                uiManager.videoRawImage = GameObject.Find("Video").GetComponent<RawImage>();
                uiManager.zoomSlider = GameObject.Find("ZoomSlider").GetComponent<Slider>();
                uiManager.postProcessCam = GameObject.Find("PostProcessCam").GetComponent<Camera>();
                uiManager.noPostCam = GameObject.Find("NoPostCam").GetComponent<Camera>();
                uiManager.videoScrubber = GameObject.Find("VideoScrubber").GetComponent<Slider>();
                StartCoroutine(uiManager.VideoScrubberCoroutine());
                GameObject videoCanvas = GameObject.Find("VideoCanvas");
                uiManager.videoCanvas = videoCanvas.GetComponent<Canvas>();

                break;
            default:
                Debug.LogWarning($"Unknown scene loaded with name {scene.name}");
                break;
        }
    }

    public void Update()
    {
        ZoomControls();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            uiManager.PausePlayVideo();
        }
    }



    private void ZoomControls()
    {
        var scrollY = Input.mouseScrollDelta.y;
        if (scrollY != 0)
        {
            uiManager.ChangeVideoZoom(scrollY);
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
