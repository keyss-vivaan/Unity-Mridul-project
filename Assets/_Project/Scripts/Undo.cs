using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Undo : MonoBehaviour
{
    public void Goback()
    {
        SceneManager.LoadScene("RefactoredScene");
    }

    public void LoadMyScene()
    {
        SceneManager.LoadScene("StateMachineScene");
    }
    public void QuitUnity()
    {
        // Quits the application
        Application.Quit();
    }
}