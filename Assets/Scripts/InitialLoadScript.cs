using UnityEngine;
public class InitialLoadScript : MonoBehaviour
{
    //The gameobject with this script should be added to every scene to allow for
    //debugging from that scene (and definitely in the starting scene in the build)

    public GameObject globalControlPrefab;
    void Awake()
    {
        if (GameObject.Find("GlobalControl") == null) 
        { 
            var globalControl = Instantiate(globalControlPrefab);
            globalControl.name = "GlobalControl";
        }
        Destroy(gameObject);
    }
}
