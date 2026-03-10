using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI keyMoveUpText;
    [SerializeField] private TextMeshProUGUI keyMoveDownText;
    [SerializeField] private TextMeshProUGUI keyMoveLeftText;
    [SerializeField] private TextMeshProUGUI keyMoveRightText;
    [SerializeField] private TextMeshProUGUI keyPauseText;
    [SerializeField] private TextMeshProUGUI keySkipText;

    private void Start() {
        PlayerInputSystem.Instance.OnBindingRebind += GameInput_OnBindingRebind;
        GameDirector.Instance.OnStateChanged += GameDirector_OnStateChanged;

        UpdateVisual();

        Show();
    }
    private void GameDirector_OnStateChanged(object sender, System.EventArgs e) {
        if (GameDirector.Instance.IsCountdownToStartActive()) {
            Hide();
        }
    }
    private void GameInput_OnBindingRebind(object sender, System.EventArgs e) {
        UpdateVisual();
    }
    private void UpdateVisual() {
        keyMoveUpText.text = PlayerInputSystem.Instance.GetBindingText(PlayerInputSystem.Binding.Move_Up);
        keyMoveDownText.text = PlayerInputSystem.Instance.GetBindingText(PlayerInputSystem.Binding.Move_Down);
        keyMoveLeftText.text = PlayerInputSystem.Instance.GetBindingText(PlayerInputSystem.Binding.Move_Left);
        keyMoveRightText.text = PlayerInputSystem.Instance.GetBindingText(PlayerInputSystem.Binding.Move_Right);
        keyPauseText.text = PlayerInputSystem.Instance.GetBindingText(PlayerInputSystem.Binding.Pause);
        keySkipText.text = PlayerInputSystem.Instance.GetBindingText(PlayerInputSystem.Binding.Skip);
    }
    private void Show() {
        gameObject.SetActive(true);
    }
    private void Hide() {
        gameObject.SetActive(false);
    }
}
