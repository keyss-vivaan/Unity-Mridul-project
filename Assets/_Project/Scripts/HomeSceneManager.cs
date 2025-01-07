// // using System.Collections;
// // using System.Collections.Generic;
// // using UnityEngine;
// // using UnityEngine.SceneManagement;

// // public class HomeSceneManager : MonoBehaviour
// // {
//     // public void LoadARScene()
//     // {
//     //     SceneManager.LoadScene("MagicPlanScene");
//     // }

// //     public void LoadMyScene()
// //     {
// //         SceneManager.LoadScene("StateMachineScene");
// //     }
// // }

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class HomeSceneManager : MonoBehaviour
// {
//     public void Load3dmodel()
//     {
//         SceneManager.LoadScene("Hello");
//     }

//     public void LoadARScene()
//     {
//         SceneManager.LoadScene("StateMachineScene");
//     }

// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeSceneManager : MonoBehaviour
{
    private bool dataReceived = false;

    

    // Method to handle incoming message from Flutter
    public void UpdateRoomDimensions(string jsonData)
    {
        SingletonExample.Instance.JsonData = jsonData;
    }
    

    void Start()
    {
        // Check if data was received; otherwise, load the default scene
        //if (!dataReceived)
        //{
           // Loading();
        //}
        SceneManager.LoadScene("Loading 1");
        
    }

    public void Load3dmodel()
    {
        SceneManager.LoadScene("Hello");
    }

    // public void LoadARScene()
    // {
    //     SceneManager.LoadScene("StateMachineScene");
    // }

    public void Loading()
    {
        SceneManager.LoadScene("Loading 1");
    }
}

