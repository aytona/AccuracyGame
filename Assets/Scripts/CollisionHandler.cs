using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionHandler : MonoBehaviour {

    private Vector3 m_initPosition;
    private Rigidbody m_rb;

    private InitialImpulse m_initImpulse;
    private LaunchImpulse m_launchImpulse;

    private SoundManager m_soundManager;

    void Start() {
        m_initPosition = transform.position;
        m_rb = GetComponent<Rigidbody>();

        m_initImpulse = GetComponent<InitialImpulse>();
        m_launchImpulse = GetComponent<LaunchImpulse>();

        m_soundManager = FindObjectOfType<SoundManager>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "initTarget") {
            Launch();
        }
        if (other.gameObject.tag == "launchTarget") {
            StartCoroutine(LevelTransition());
        }
        if (other.gameObject.tag == "rampTarget") {
            StartCoroutine(LevelFinish());
        }
    }

    private void Launch() {
        m_initImpulse.enabled = false;
        m_rb.velocity = Vector3.zero;
        m_launchImpulse.enabled = true;
    }

    private IEnumerator LevelTransition() {
        m_soundManager.PlayLaunchSFX();
        m_launchImpulse.enabled = false;
        m_rb.velocity = Vector3.zero;
        m_rb.isKinematic = true;
        yield return new WaitForSeconds(m_soundManager.m_launchSFX.length + 1f);
        transform.position = m_initPosition;
        m_rb.isKinematic = false;

    }

    private IEnumerator LevelFinish() {
        m_soundManager.PlayTargetSFX();
        m_rb.velocity = Vector3.zero;
        yield return new WaitForSeconds(m_soundManager.m_targetSFX.length + 1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
