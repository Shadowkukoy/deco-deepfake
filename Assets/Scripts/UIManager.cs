using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class UIManager
{
    //Code that assigns the method 'OnButtonPress' to the pressing of any children gameobjects.
    //This should be run on canvas whenever a new scene is loaded, and on any new instantiated UI element which contains buttons
    public static void AssignButtonListeners(GameObject elements)
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

    private static void OnButtonPress(string button, int id)
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
            default:
                //unknown button pressed
                Debug.LogWarning($"Unknown button with name: {button} and id: {id}");
                break;
        }
    }
}
