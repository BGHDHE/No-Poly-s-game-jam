using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button optionsButton;


    private void Awake() {
        resumeButton.onClick.AddListener(() => {
            GameDirector.Instance.TogglePauseGame();
        });
        mainMenuButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        optionsButton.onClick.AddListener(() => {
            Hide();
            OptionsUI.Instance.Show(Show);
        });
    }

    private void Start() {
        GameDirector.Instance.OnGamePaused += GameDirector_OnGamePaused;
        GameDirector.Instance.OnGameUnpaused += GameDirector_OnGameUnpaused;

        Hide();
    }

    private void GameDirector_OnGameUnpaused(object sender, System.EventArgs e) {
        Hide();
    }

    private void GameDirector_OnGamePaused(object sender, System.EventArgs e) {
        Show();
    }

    private void Show() {
        gameObject.SetActive(true);

        resumeButton.Select();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
