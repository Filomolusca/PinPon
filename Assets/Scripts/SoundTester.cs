using UnityEngine;

public class SoundTester : MonoBehaviour
{
    [Header("--- Assign in Inspector ---")]
    [Tooltip("Drag the AudioSource from your SoundManager's 'Effects Source' field here.")]
    public AudioSource sourceToTest;

    [Tooltip("Drag any sound effect audio clip here, like your 'Whistle'.")]
    public AudioClip clipToPlay;

    void Update()
    {
        // Press the 'T' key to run the test
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (sourceToTest == null)
            {
                Debug.LogError("SOUND TESTER: The 'sourceToTest' has not been assigned in the Inspector!");
                return;
            }
            if (clipToPlay == null)
            {
                Debug.LogError("SOUND TESTER: The 'clipToPlay' has not been assigned in the Inspector!");
                return;
            }

            Debug.Log("--- SOUND TEST: Forcing a sound to play via key press. ---");
            sourceToTest.PlayOneShot(clipToPlay);
            Debug.Log("--- SOUND TEST: PlayOneShot has been called. Did you hear it? ---");
        }
    }
}
