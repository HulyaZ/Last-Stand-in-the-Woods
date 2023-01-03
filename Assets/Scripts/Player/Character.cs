using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Character : MonoBehaviour  // Character collision-trigger 
{
    const float plMaxHealth = 100;
    float plCurrentHealth;

    [SerializeField] AudioSource FXAudio;
    [SerializeField] AudioSource WalkAudio;
    [SerializeField] AudioClip woodPickSFX;
    [SerializeField] AudioClip[] FXClips;

    [SerializeField] Combat_Bat batScript;

    [SerializeField] GameObject playerHealthBar;
    [SerializeField] Image healthBarFill;

    [SerializeField] Collider batCollider;

    public bool inEnemyRange = false;

    int sceneOnDeath = 3; // index of the scene to be loaded on death

    float sceneTransTimer = 5f;
    float healthResetTimer = 8f;

    public static System.Action woodPicked;
    public static System.Action inStokingRange;
    public static System.Action<bool> inStoveRange;
    public static event System.Action<bool> PlayerKilled;

    private void Awake()
    {
        FXAudio.clip = FXClips[0];
        plCurrentHealth = plMaxHealth;
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            playerHealthBar.SetActive(false);
        }
      
        if (inEnemyRange)
        {
            playerHealthBar.SetActive(true);
            healthBarFill.fillAmount = plCurrentHealth / plMaxHealth;
        }

        if (!inEnemyRange)
        {
            playerHealthBar.SetActive(false);          
        }

        if (notHitTimer < healthResetTimer)
        {
            notHitTimer += Time.deltaTime;

            if (notHitTimer >= healthResetTimer)
            {
                plCurrentHealth = plMaxHealth;
                healthBarFill.fillAmount = plCurrentHealth / plMaxHealth;
            }
        }

    }

    float notHitTimer = 0;
    public void GetDamage(float damage)
    {
        notHitTimer = 0;

        FXAudio.PlayOneShot(FXClips[0]); // audio take damage 

        plCurrentHealth -= damage;

        if (plCurrentHealth <= 0)
        {
            WalkAudio.clip = FXClips[1]; // audio death
            WalkAudio.Play();

            PlayerKilled?.Invoke(false);
            PlayerDieAttacked();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZombieRange"))
        {
            inEnemyRange = true;

            notHitTimer = 0;           
            playerHealthBar.SetActive(true);
        }

        if (other.CompareTag("Wood"))
        {
            WoodPickUpAudio();
            woodPicked?.Invoke();

            Destroy(other.gameObject);
        }

        if (other.CompareTag("Stove"))
        {
            inStokingRange?.Invoke();
        }

        if (other.CompareTag("Shed"))
        {
            inStoveRange?.Invoke(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Shed"))
        {
            CancelInvoke("RangeInvoker");
            inStoveRange?.Invoke(false);
        }

        if (other.CompareTag("ZombieRange"))
        {
            inEnemyRange = false;
            notHitTimer = 0;
            playerHealthBar.SetActive(false);
        }
    }

    #region Initials / Utils
    IEnumerator PlayerDieAttacked()
    {
        yield return new WaitForSecondsRealtime(sceneTransTimer);
        SceneManager.LoadScene(sceneOnDeath);
    }

    void WoodPickUpAudio()
    {
        WalkAudio.PlayOneShot(woodPickSFX);
    }
    void PlayWeaponPickedSound()
    {
        FXAudio.PlayOneShot(FXClips[2]);
    }

    private void OnEnable()
    {
        IInteractable.weaponEquipped += PlayWeaponPickedSound;
        PlayerInventory.itemSwitched += PlayWeaponPickedSound;
        EnemyDamage.playerHit += GetDamage;
    }

    private void OnDisable()
    {
        playerHealthBar.SetActive(false);

        IInteractable.weaponEquipped -= PlayWeaponPickedSound;
        PlayerInventory.itemSwitched -= PlayWeaponPickedSound;
        EnemyDamage.playerHit -= GetDamage;
    }

    #endregion

    #region AnimationEvents

    void BatCollisionActivate()
    {
        batCollider.enabled = true;
    }

    void BatCollisionDeactivate()
    {
        batCollider.enabled = false;
    }

    void PlayAudioBat()
    {
        batScript.PlayBatSound();
    }

    #endregion
}
