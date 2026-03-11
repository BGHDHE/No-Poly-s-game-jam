using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public static OptionsUI Instance { get; private set; }


    [SerializeField] private Button soundEffectsButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button moveUpButton;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private TextMeshProUGUI soundEffectsText;
    [SerializeField] private TextMeshProUGUI musicText;
    [SerializeField] private TextMeshProUGUI moveUpText;
    [SerializeField] private TextMeshProUGUI moveDownText;
    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private TextMeshProUGUI moveRightText;
    [SerializeField] private TextMeshProUGUI pauseText;
    [SerializeField] private TextMeshProUGUI skipText;
    [SerializeField] private Transform pressToRebindKeyTransform;


    private Action onCloseButtonAction;


    private void Awake() {
        Instance = this;

        soundEffectsButton.onClick.AddListener(() => {
            //SoundManager.Instance.ChangeVolume();
            UpdateVisual();
        });
        musicButton.onClick.AddListener(() => {
            MusicManager.Instance.ChangeVolume();
            UpdateVisual();
        });
        closeButton.onClick.AddListener(() => {
            Hide();
            onCloseButtonAction();
        });

        moveUpButton.onClick.AddListener(() => { RebindBinding(PlayerInputSystem.Binding.Move_Up); });
        moveDownButton.onClick.AddListener(() => { RebindBinding(PlayerInputSystem.Binding.Move_Down); });
        moveLeftButton.onClick.AddListener(() => { RebindBinding(PlayerInputSystem.Binding.Move_Left); });
        moveRightButton.onClick.AddListener(() => { RebindBinding(PlayerInputSystem.Binding.Move_Right); });    
        pauseButton.onClick.AddListener(() => { RebindBinding(PlayerInputSystem.Binding.Pause); });
        skipButton.onClick.AddListener(() => { RebindBinding(PlayerInputSystem.Binding.Skip); });
        
    }

    private void Start() {
        GameDirector.Instance.OnGameUnpaused += GameDirector_OnGameUnpaused;

        UpdateVisual();

        HidePressToRebindKey();
        Hide();
    }

    private void GameDirector_OnGameUnpaused(object sender, System.EventArgs e) {
        Hide();
    }

    private void UpdateVisual() {
        //soundEffectsText.text = "Sound Effects: " + Mathf.Round(SoundManager.Instance.GetVolume() * 10f);
        musicText.text = "Music: " + Mathf.Round(MusicManager.Instance.GetVolume() * 10f);

        moveUpText.text = PlayerInputSystem.Instance.GetBindingText(PlayerInputSystem.Binding.Move_Up);
        moveDownText.text = PlayerInputSystem.Instance.GetBindingText(PlayerInputSystem.Binding.Move_Down);
        moveLeftText.text = PlayerInputSystem.Instance.GetBindingText(PlayerInputSystem.Binding.Move_Left);
        moveRightText.text = PlayerInputSystem.Instance.GetBindingText(PlayerInputSystem.Binding.Move_Right);
        pauseText.text = PlayerInputSystem.Instance.GetBindingText(PlayerInputSystem.Binding.Pause);
        skipText.text = PlayerInputSystem.Instance.GetBindingText(PlayerInputSystem.Binding.Skip);
    }

    public void Show(Action onCloseButtonAction) {
        this.onCloseButtonAction = onCloseButtonAction;

        gameObject.SetActive(true);

        soundEffectsButton.Select();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void ShowPressToRebindKey() {
        pressToRebindKeyTransform.gameObject.SetActive(true);
    }

    private void HidePressToRebindKey() {
        pressToRebindKeyTransform.gameObject.SetActive(false);
    }

    private void RebindBinding(PlayerInputSystem.Binding binding) {
        ShowPressToRebindKey();
        PlayerInputSystem.Instance.RebindBinding(binding, () => {
            HidePressToRebindKey();
            UpdateVisual();
        });
    }
}
