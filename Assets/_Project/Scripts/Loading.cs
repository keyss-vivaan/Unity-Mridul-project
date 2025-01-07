using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{      

    //  public void UpdateRoomDimensions(string jsonData)
    // {
    //     SingletonExample.Instance.JsonData = jsonData;
    // }
    void Start()
    {
        // Check if data was received; otherwise, load the default scene
         StartCoroutine(TimeoutFunction(1f, () =>
        {
           // Debug.Log("Timeout reached!");
            if(SingletonExample.Instance.JsonData != "hi"){
                SceneManager.LoadScene("Hello");
            } else {
                LoadARScene();
            }
            
        }));
    }

    IEnumerator TimeoutFunction(float timeoutDuration, System.Action callback)
    {
        // Wait for the specified duration
        yield return new WaitForSeconds(timeoutDuration);
        
        // Execute the callback function
        callback?.Invoke();
    }
        
    

    public void LoadARScene()
    {
        SceneManager.LoadScene("StateMachineScene");
    }

}
