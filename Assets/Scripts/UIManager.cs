using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject damageTextPrefab;
    public GameObject healthTextPrefab;
    public Canvas gameCanvas;

    private PlayerInput playerInput;

    private void Awake()
    {
        // Find and assign the main Canvas
        gameCanvas = FindObjectOfType<Canvas>();
        if (gameCanvas == null)
            Debug.LogError("No Canvas found in the scene.");

        // Get the PlayerInput component (attach it to this GameObject in the Inspector)
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
            Debug.LogError("PlayerInput component missing from UIManager GameObject.");
    }

    private void OnEnable()
    {
        CharacterEvents.characterDamaged += CharacterTookDamage;
        CharacterEvents.characterHealed += CharacterHealed;
    }

    private void OnDisable()
    {
        CharacterEvents.characterDamaged -= CharacterTookDamage;
        CharacterEvents.characterHealed -= CharacterHealed;
    }

    public void CharacterTookDamage(GameObject character, int damageReceived)
    {
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);
        TMP_Text tmpText = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform).GetComponent<TMP_Text>();
        tmpText.text = damageReceived.ToString();
    }

    public void CharacterHealed(GameObject character, int healthRestored)
    {
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);
        TMP_Text tmpText = Instantiate(healthTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform).GetComponent<TMP_Text>();
        tmpText.text = healthRestored.ToString();
    }

    // Called by Input System when ESC is pressed (make sure this is connected in the PlayerInput component)
    public void OnExitGame(InputAction.CallbackContext context)
    {
        // Only trigger on key press (not release)
        if (!context.performed) return;

        Debug.Log("ESC Pressed - Exit triggered");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"{name}: {GetType()}: OnExitGame");
#endif

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        Application.Quit();
#elif UNITY_WEBGL
        SceneManager.LoadScene("QuitScene");
#endif
    }
}
