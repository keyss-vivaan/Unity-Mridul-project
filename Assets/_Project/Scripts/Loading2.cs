using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loadingtwo : MonoBehaviour
{      

    void Start()
    {
        // Check if data was received; otherwise, load the default scene
         StartCoroutine(TimeoutFunction(1f, () =>
        {
            Debug.Log("Timeout reached!");
            Load3dmodel();
        }));
    }

    IEnumerator TimeoutFunction(float timeoutDuration, System.Action callback)
    {
        // Wait for the specified duration
        yield return new WaitForSeconds(timeoutDuration);
        
        // Execute the callback function
        callback?.Invoke();
    }
        
    

     public void Load3dmodel()
    {
        SceneManager.LoadScene("Hello");
    }

}
