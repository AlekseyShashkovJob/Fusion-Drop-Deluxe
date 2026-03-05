using UnityEngine;
using View.Button;

namespace View.UI.Game
{
    public class PausedScreen : UIScreen
    {
		[SerializeField] private UIScreen _optionsScreen;
        [SerializeField] private CustomButton _continue;
		[SerializeField] private CustomButton _settings;
        [SerializeField] private CustomButton _restart;

        private void OnEnable()
        {
            _continue.AddListener(ContinueGame);
			_settings.AddListener(OpenOptions);
            _restart.AddListener(Restart);
        }

        private void OnDisable()
        {
            _continue.RemoveListener(ContinueGame);
			_settings.RemoveListener(OpenOptions);
            _restart.RemoveListener(Restart);
        }

        public override void StartScreen()
        {
            base.StartScreen();

            Time.timeScale = 0.0f;
        }

        private void ContinueGame()
        {
            Time.timeScale = 1.0f;
            CloseScreen();
        }
		
		private void OpenOptions()
        {
            _optionsScreen.StartScreen();
        }

        private void Restart()
        {
            // Time.timeScale = 1.0f;
            GameCore.GameManager.Instance.RestartGame();
            CloseScreen();
        }
    }
}