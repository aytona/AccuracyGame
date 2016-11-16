using UnityEngine;

public class SoundManager : MonoBehaviour {

    public AudioClip m_launchSFX;
    public AudioClip m_targetSFX;

    private AudioSource m_source;

    void Start() {
        m_source = GetComponent<AudioSource>();
    }

    public void PlayLaunchSFX() {
        m_source.clip = m_launchSFX;
        m_source.Play();
    }

    public void PlayTargetSFX() {
        m_source.clip = m_targetSFX;
        m_source.Play();
    }
}
