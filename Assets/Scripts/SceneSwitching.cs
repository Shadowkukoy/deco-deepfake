using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitching : MonoBehaviour
{
    public void toVidPlayer()
    {
        SceneManager.LoadScene(sceneName: "DeepFakeScene");
    }

    public void toMainMenu() {
        SceneManager.LoadScene(sceneName: "Main Menu");
    }

    public void toAboutPage()
    {
        SceneManager.LoadScene(sceneName: "About Page");
    }

}
