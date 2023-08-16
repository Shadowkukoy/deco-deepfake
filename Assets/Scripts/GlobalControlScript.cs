using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalControlScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            UIManager.AssignButtonListeners(canvas);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Code that should be run when a scene is loaded
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null) 
        {
            UIManager.AssignButtonListeners(canvas);
        }
        
        switch (scene.name)
        {
            case "DeepFakeScene":
                //Code that should be run when the deepfake scene is loaded.
                break;
            default:
                Debug.LogWarning($"Unknown scene loaded with name {scene.name}");
                break;
        }
    }
}
