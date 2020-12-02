// This is the library this script uses
using UnityEngine;

// We're using this script to handle all our audio needs
public class SoundManager : MonoBehaviour {

    public GameManager gameManager;
    // this value is the max number of audiosources that can exist at any given point in the scene.
    // It can be chnaged to allow more or less
    readonly int maxAudioSources = 30;

    // An array for storing our AudioSources
    AudioSource[] sources;

    public AudioSource sourcePrefab;

    // An array of blast clips for shooting
    public AudioClip[] blastClips;
    int lastBlast;

    // An array of impact clips for taking damage
    public AudioClip[] impactClips;
    int lastImpact;

    public AudioClip explosionClip;

    // Awake runs before Start() so use it to run code that initlaizes variables or sets game states before the game Starts.
    // Most of the time you can run things in start, but sometimes it is important that the code is run before the Start method runs.
    private void Awake() {
        //Here we create AudioSources. AudioSources in Unity are what we use to play sound clips. 
        //By default an AudioSource has no clip. For now we're fine with that.
        sources = new AudioSource[maxAudioSources];

        for (int i = 0; i < maxAudioSources; i++) {
            sources[i] = Instantiate(sourcePrefab, transform);
        }
    }

    // This can be called from any script. It lets us choose what sound to play and where to play it.
    public void PlaySoundAtPosition(Vector2 pos, int typeIndex) {

        if (!gameManager.enableAudio) return;

        //Index Reference
        // 0 = Explosion
        // 1 = Blaster
        // 2 = Impact

        //We get an free AudioSource
        AudioSource source = GetSource();

        //Depending on what index we pass this function, we select a different sound by setting a clip to the AudioSource
     
        if (typeIndex == 1) {
            int clipNum = GetClipIndex(blastClips.Length, lastBlast);
            lastBlast = clipNum;
            source.clip = blastClips[clipNum];
            source.volume = 0.5f;
        } 
        else if (typeIndex == 2) {
            int clipNum = GetClipIndex(impactClips.Length, lastImpact);
            lastImpact = clipNum;
            source.clip = impactClips[clipNum];

        } else {
            source.clip = explosionClip;
        }

        //We adjust the pitch of each sound slightly to add more variety to our sounds and avoid sound fatigue
        source.pitch = Random.Range(0.75f, 1.25f);
        source.transform.position = pos;

        //Play the sound
        source.Play();

    }

    //We use this function to make sure we're playing a different song from our clip arrays to avoid sound fatigue
    int GetClipIndex(int clipNum, int lastPlayed) {
        int num = Random.Range(0, clipNum);
        while (num == lastPlayed) {
            num = Random.Range(0, clipNum);
        }
        return num;
    }

    // This function gets us an AudioSource that is not in use
    AudioSource GetSource() {
        for (int i = 0; i < maxAudioSources; i++) {
            if (!sources[i].isPlaying) {
                return sources[i];
            }
        }

        // If there are no free AudioSource, we print this to the console
        Debug.Log("NOT ENOUGH SOURCES");
        return sources[0];
    }
}
