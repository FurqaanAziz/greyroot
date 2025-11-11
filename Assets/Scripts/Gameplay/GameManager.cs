using CardGame;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame
{
    public class GameManager : MonoBehaviour
    {
        public GridManager gridManager;
        public GameObject menuPanel;
        public GameObject gameplayPanel;
        public GameObject gameCompletedPanel;
        public Button startButton;
        public Button homeButton;
        public Button nextButton;
        public Button closeButton;
        public Button loadButton;
        public TMP_InputField rowsInputField, columnsInputField;
        public int rows = 2;
        public int columns = 2;
        public TMP_Text warning;

        private Queue<Card> cardClickQueue = new Queue<Card>();
        private bool isComparing = false;

        private int matchedPairs = 0;

        private List<int> matchedCardIds = new List<int>();

        public TMP_Text scoreText;

        public TMP_Text comboText;
        public GameObject comboObject;

        private int matchesFound = 0;

        private int comboStreak = 0;
        private int comboMultiplier = 1;

        public AudioClip cardFlippedAudio;
        public AudioClip cardMatchedAudio;
        public AudioClip cardMismatchedAudio;
        public AudioClip gameCompletedAudio;

        private AudioSource audioSource;

        private bool isGameCompleted = false;
        public bool IsGameCompleted() => isGameCompleted;

        void Start()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(StartGame);
            }
            if (homeButton != null)
            {
                homeButton.onClick.AddListener(Home);
            }
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseGame);
            }
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(NextGame);
            }
            if (loadButton != null)
            {
                loadButton.onClick.AddListener(LoadGame);
            }
            audioSource = GetComponent<AudioSource>();
        }
        public void StartGame()
        {
            if (string.IsNullOrEmpty(rowsInputField.text) || string.IsNullOrEmpty(columnsInputField.text))
            {
                warning.text = "Rows and columns fields cannot be empty!";
                warning.gameObject.SetActive(true);
                Invoke("WarningTextDeactivate", 3f);
                return;
            }

            rows = int.Parse(rowsInputField.text);
            columns = int.Parse(columnsInputField.text);

            if (!IsGridValid(rows, columns))
            {
                if (rows >= 10 || columns >= 10)
                {
                    warning.text = ($"Grid exceeding limit of 10 x 10");
                }
                else if (rows <= 10 || columns <= 10)
                {
                    warning.text = ($"Invalid grid: {rows} x {columns} = {rows * columns} cards. Result Must be even.");
                }
                else
                {
                    warning.text = ($"Grid exceeding limit of 10 x 10");
                }
                warning.gameObject.SetActive(true);
                Invoke("WarningTextDeactivate", 3f);
                return;
            }

            gridManager.SetupGridLayout(rows, columns);
            gridManager.CreateGrid(rows, columns);

            menuPanel.SetActive(false);
            gameplayPanel.SetActive(true);
        }
        private bool IsGridValid(int rows, int columns)
        {
            if (rows < 2 || rows > 10 || columns < 2 || columns > 10)
            {
                warning.text = "Rows and columns must be between 2x2 and 10x10";
                return false;
            }

            return (rows * columns) % 2 == 0;
        }
        private void WarningTextDeactivate()
        {
            warning.gameObject.SetActive(false);
        }
        public void Home()
        {
            SaveGame();
            ResetGame();
        }
        public void CloseGame()
        {
            Application.Quit();
        }

        #region check cards
        public void CardClicked(Card clickedCard)
        {
            if (clickedCard == null || clickedCard.isFaceUp || cardClickQueue.Contains(clickedCard))
                return;
            PlayAudio(cardFlippedAudio);
            cardClickQueue.Enqueue(clickedCard);
            TryProcessQueue();
        }
        private void TryProcessQueue()
        {
            if (isComparing || cardClickQueue.Count < 2)
                return;

            Card first = cardClickQueue.Dequeue();
            Card second = cardClickQueue.Dequeue();

            StartCoroutine(CheckCards(first, second));
        }
        private IEnumerator CheckCards(Card first, Card second)
        {
            isComparing = true;


            if (!first.isFaceUp) first.Flip();
            if (!second.isFaceUp) second.Flip();


            yield return new WaitForSeconds(0.5f);

            if (first.id == second.id)
            {
                comboStreak++;
                comboMultiplier = comboStreak >= 2 ? comboStreak : 1;

                matchesFound += comboMultiplier;

                matchedPairs++;
                matchedCardIds.Add(first.id);

                PlayAudio(cardMatchedAudio);
                UpdateScoreText();
                CheckForGameCompletion();
                ShowComboText(comboMultiplier > 1 ? $"Combo x{comboMultiplier}!" : "");
                first.Notify(first, CardEvent.Matched);
                second.Notify(second, CardEvent.Matched);
            }
            else
            {
                comboStreak = 0;
                comboMultiplier = 1;
                ShowComboText("");
                PlayAudio(cardMismatchedAudio);
                first.Notify(first, CardEvent.Mismatched);
                second.Notify(second, CardEvent.Mismatched);
                yield return new WaitForSeconds(0.1f);

                first.Flip();
                second.Flip();
            }

            isComparing = false;
            TryProcessQueue();
        }

        #endregion
        private void CheckForGameCompletion()
        {
            int totalPairs = FindObjectsOfType<Card>().Length / 2;
            if (matchedPairs == totalPairs)
            {
                isGameCompleted = true;
                ShowGameCompletedPanel();
            }
        }
        private void ShowGameCompletedPanel()
        {
            if (gameCompletedPanel != null)
            {
                gameCompletedPanel.SetActive(true);
                PlayAudio(gameCompletedAudio);
            }
        }
        private void UpdateScoreText()
        {
            if (scoreText != null)
            {
                scoreText.text = comboMultiplier > 1
                    ? $"Matched: {matchesFound} (x{comboMultiplier} Combo!)"
                    : $"Matched: {matchesFound}";
            }
        }
        private void ShowComboText(string message)
        {
            if (comboText != null)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    comboObject.SetActive(true);
                    comboText.text = message;
                    CancelInvoke(nameof(ClearComboText));
                    Invoke(nameof(ClearComboText), 1.5f);
                }
                else
                {
                    comboObject.SetActive(false);
                }
            }
        }
        private void ClearComboText()
        {
            if (comboText != null)
            {
                comboText.text = "";
                comboObject.SetActive(false);
            }
        }
        public void ResetGame()
        {
            matchesFound = 0;
            matchedPairs = 0;
            comboStreak = 0;
            comboMultiplier = 1;
            matchedCardIds.Clear();
            cardClickQueue.Clear();
            isComparing = false;
            isGameCompleted = false;

            UpdateScoreText();
            ShowComboText("");
            if (gameCompletedPanel != null)
                gameCompletedPanel.SetActive(false);

            GridManager gridManager = FindObjectOfType<GridManager>();
            if (gridManager != null)
            {
                gridManager.ResetGrid();
            }
            menuPanel.SetActive(true);
            gameCompletedPanel.SetActive(false);
            gameplayPanel.SetActive(false);
        }
        private void PlayAudio(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        public int GetScore() => matchedPairs;

        public List<int> GetMatchedCardIds() => matchedCardIds;

        public void RestoreGameState(SaveData data)
        {
            matchedPairs = data.score;
            matchesFound = data.score;

            comboStreak = 0;
            comboMultiplier = 1;

            matchedCardIds.Clear();
            foreach (var cardData in data.cards)
                if (cardData.isFaceUp)
                    matchedCardIds.Add(cardData.id);

            UpdateScoreText();          
            CheckForGameCompletion();  
        }

        public void SaveGame()
        {
            SaveLoadManager.SaveGame(gridManager, this);
        }
        public void LoadGame()
        {
            menuPanel.SetActive(false);
            gameplayPanel.SetActive(true);
            SaveLoadManager.LoadGame(gridManager, this);
        }

        private void OnApplicationQuit()
        {
            StartCoroutine(SaveBeforeQuit());
        }

        private IEnumerator SaveBeforeQuit()
        {
            SaveLoadManager.SaveGame(gridManager, this);
            yield return new WaitForSeconds(0.1f); 
        }


        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGame();
            }
        }

        public void NextGame()
        {
            if (gameCompletedPanel != null)
                gameCompletedPanel.SetActive(false);

            int newRows, newCols;
            do
            {
                newRows = Random.Range(2, 11);
                newCols = Random.Range(2, 11);
            } while ((newRows * newCols) % 2 != 0);

            rows = newRows;
            columns = newCols;

            matchesFound = 0;
            matchedPairs = 0;
            comboStreak = 0;
            comboMultiplier = 1;
            matchedCardIds.Clear();
            cardClickQueue.Clear();
            isComparing = false;
            isGameCompleted = false;

            UpdateScoreText();
            ShowComboText("");
            menuPanel.SetActive(false);
            gameplayPanel.SetActive(true);

            gridManager.SetupGridLayout(rows, columns);
            gridManager.CreateGrid(rows, columns);

        }
    }
}
