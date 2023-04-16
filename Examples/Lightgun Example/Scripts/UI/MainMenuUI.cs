using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiMouseUnity.Example
{
    public class PlayData
    {
        public int[] playerIDs = new int[] { -1, -1, };
    }

    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] string gameSceneName = "Lightgun01";

        [SerializeField] Text clickStartLabel;
        [SerializeField] CanvasGroup playerMenu;
        [SerializeField] CanvasGroup playButton;

        [SerializeField] PlayerSlot[] slots;
        [SerializeField] AudioSource gunShotSound;

        public static PlayData PlayData;

        // Start is called before the first frame update
        void Start()
        {
            PlayData = new PlayData();

            // Just in case these were toyed with
            clickStartLabel.enabled = true;
            playerMenu.alpha = 0;
            slots[0].UpdateStateForDevice(-1);
            slots[1].UpdateStateForDevice(-1);
        }

        private void OnEnable()
        {
            MultiMouseWrapper.OnDeviceFound += OnDeviceFound;
        }

        private void OnDisable()
        {
            MultiMouseWrapper.OnDeviceFound -= OnDeviceFound;
        }

        private void OnDeviceFound(int obj)
        {
            clickStartLabel.enabled = false;
            playerMenu.alpha = 1;
            gunShotSound.Play();
        }

        public void SamButtonClicked()
        {
            for (int i = 0; i < MultiMouseWrapper.Instance.ActiveDeviceCount; i++)
            {
                if (MultiMouseWrapper.Instance.GetMouseButtonUp(i, 0))
                {
                    if (PlayData.playerIDs[0] == -1)
                    {
                        if (PlayData.playerIDs[1] == i)
                        {
                            PlayData.playerIDs[1] = -1;
                            slots[1].UpdateStateForDevice(-1);
                        }
                        PlayData.playerIDs[0] = i;
                        slots[0].UpdateStateForDevice(i);
                    }
                    else if (PlayData.playerIDs[0] == i)
                    {
                        PlayData.playerIDs[0] = -1;
                        slots[0].UpdateStateForDevice(-1);
                    }
                }
            }
            UpdatePlayButton();
        }

        public void JimButtonClicked()
        {
            for (int i = 0; i < MultiMouseWrapper.Instance.ActiveDeviceCount; i++)
            {
                if (MultiMouseWrapper.Instance.GetMouseButtonUp(i, 0))
                {
                    if (PlayData.playerIDs[1] == -1)
                    {
                        if (PlayData.playerIDs[0] == i)
                        {
                            PlayData.playerIDs[0] = -1;
                            slots[0].UpdateStateForDevice(-1);
                        }
                        PlayData.playerIDs[1] = i;
                        slots[1].UpdateStateForDevice(i);
                    }
                    else if (PlayData.playerIDs[1] == i)
                    {
                        PlayData.playerIDs[1] = -1;
                        slots[1].UpdateStateForDevice(-1);
                    }
                }
            }
            UpdatePlayButton();
        }

        public void UpdatePlayButton()
        {
            bool hasPlayers = false;
            for (int i = 0; i < 2; i++)
            {
                if (PlayData.playerIDs[i] != -1)
                {
                    hasPlayers = true;
                    break;
                }
            }

            playButton.alpha = (float)System.Convert.ToDouble(hasPlayers);
        }

        public void Play()
        {
            playerMenu.alpha = 0;
            Invoke(nameof(ChangeScene), 1);
        }

        public void ChangeScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
        }
    }
}