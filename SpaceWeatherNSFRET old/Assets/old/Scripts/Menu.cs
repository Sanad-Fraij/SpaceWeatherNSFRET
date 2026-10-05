using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for UI Slider

public class NewBehaviourScript : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider volumeSlider; // Drag your slider here in the Inspector

    void Start()
    {
        // Optional: Initialize the slider's value to current volume
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    // Volume control function (slider passes a float automatically)
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        Debug.Log("Volume set to: " + volume);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(0);
    }

    public void PlayGame2()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
