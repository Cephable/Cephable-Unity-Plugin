using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cephable.DemoScene
{

    public class PauseMenu : MonoBehaviour
    {
        public GameObject pauseMenuUI;
        public static bool isPaused = false;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }

        public void PauseGame()
        {
            Time.timeScale = 0f; // Freeze gameplay
            AudioListener.pause = true; // Pause all audio
            pauseMenuUI.SetActive(true);
            isPaused = true;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f; // Resume gameplay
            AudioListener.pause = false;
            pauseMenuUI.SetActive(false);
            isPaused = false;
        }
    }
}