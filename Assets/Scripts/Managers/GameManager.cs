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
        #region Inspector References

        public GridManager gridManager;

        [Header("Panels")]
        public GameObject menuPanel;
        public GameObject gameplayPanel;
        public GameObject gameCompletedPanel;

        [Header("Buttons")]
        public Button startButton;
        public Button homeButton;
        public Button nextButton;
        public Button closeButton;
        public Button loadButton;

        [Header("Grid Inputs")]
        public TMP_InputField rowsInputField;
        public TMP_InputField columnsInputField;
        public TMP_Text warning;

        [Header("UI Texts")]
        public TMP_Text scoreText;
        public TMP_Text movesText;
        public TMP_Text comboText;
        public GameObject comboObject;

        [Header("Audio Clips")]
        public AudioClip cardFlippedAudio;
        public AudioClip cardMatchedAudio;
        public AudioClip cardMismatchedAudio;
        public AudioClip gameCompletedAudio;

        #endregion

        #region Private Fields

        private Queue<Card> cardClickQueue = new Queue<Card>();
        private List<int> matchedCardIds = new List<int>();

        private AudioSource audioSource;

        private int rows = 2;
        private int columns = 2;

        private int matchedPairs = 0;
        private int matchesFound = 0;
        private int moves = 0;

        private int comboStreak = 0;
        private int comboMultiplier = 1;

        private bool isComparing = false;
        private bool isGameCompleted = false;

        #endregion

        #region Public Getters

        public bool IsGameCompleted() => isGameCompleted;
        public int GetMatchesFound() => matchesFound;
        public int GetComboStreak() => comboStreak;
        public int GetComboMultiplier() => comboMultiplier;
        public int GetMoves() => moves;
        public int GetScore() => matchedPairs;
        public List<int> GetMatchedCardIds() => matchedCardIds;
        public int GetRows() => rows;
        public int GetColumns() => columns;

        #endregion

        #region Public Setters

        public void SetRows(int value) => rows = value;
        public void SetColumns(int value) => columns = value;

        #endregion

        #region Unity Methods

        void Start()
        {
            if (startButton) startButton.onClick.AddListener(StartGame);
            if (homeButton) homeButton.onClick.AddListener(Home);
            if (closeButton) closeButton.onClick.AddListener(CloseGame);
            if (nextButton) nextButton.onClick.AddListener(NextGame);
            if (loadButton) loadButton.onClick.AddListener(LoadGame);

            audioSource = GetComponent<AudioSource>();

            CheckForSavedGame();
        }

        private void OnApplicationQuit()
        {
            if (matchedPairs > 0 && !isGameCompleted)
                StartCoroutine(SaveBeforeQuit());
        }

        private IEnumerator SaveBeforeQuit()
        {
            SaveLoadManager.SaveGame(gridManager, this);
            yield return new WaitForSeconds(0.1f);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && !isGameCompleted)
            {
                SaveGame();
            }
        }

        #endregion

        #region Game Flow

        public void StartGame()
        {
            GenerateRandomGrid(out rows, out columns);

            gridManager.SetupGridLayout(rows, columns);
            gridManager.CreateGrid(rows, columns);

            menuPanel.SetActive(false);
            gameplayPanel.SetActive(true);

            StartCoroutine(RevealAllCardsOnce());
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

        public void NextGame()
        {
            if (gameCompletedPanel)
                gameCompletedPanel.SetActive(false);

            GenerateRandomGrid(out rows, out columns);
            ResetGameStateForNewGrid();

            gridManager.SetupGridLayout(rows, columns);
            gridManager.CreateGrid(rows, columns);

            StartCoroutine(RevealAllCardsOnce());
        }

        #endregion

        #region Card Interaction & Comparison

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

            moves++;
            UpdateMovesText();

            if (first.id == second.id)
            {
                first.PlayMatchFlash();
                second.PlayMatchFlash();

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

        #region Game State Management

        private void ResetGame()
        {
            matchesFound = 0;
            matchedPairs = 0;
            comboStreak = 0;
            comboMultiplier = 1;
            moves = 0;

            matchedCardIds.Clear();
            cardClickQueue.Clear();

            isComparing = false;
            isGameCompleted = false;

            UpdateMovesText();
            UpdateScoreText();
            ShowComboText("");

            if (gameCompletedPanel)
                gameCompletedPanel.SetActive(false);

            GridManager gm = FindObjectOfType<GridManager>();
            if (gm != null)
                gm.ResetGrid();

            menuPanel.SetActive(true);
            gameCompletedPanel.SetActive(false);
            gameplayPanel.SetActive(false);
        }

        private void ResetGameStateForNewGrid()
        {
            matchesFound = 0;
            matchedPairs = 0;
            comboStreak = 0;
            comboMultiplier = 1;
            moves = 0;

            matchedCardIds.Clear();
            cardClickQueue.Clear();

            isComparing = false;
            isGameCompleted = false;

            UpdateScoreText();
            UpdateMovesText();
            ShowComboText("");
        }

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
            if (gameCompletedPanel)
            {
                gameCompletedPanel.SetActive(true);
                PlayAudio(gameCompletedAudio);
            }
        }

        #endregion

        #region UI Updating

        private void UpdateScoreText()
        {
            if (!scoreText) return;

            scoreText.text = comboMultiplier > 1
                ? $"Matched: {matchesFound} (x{comboMultiplier} Combo!)"
                : $"Matched: {matchesFound}";
        }

        private void UpdateMovesText()
        {
            if (movesText)
                movesText.text = $"Moves: {moves}";
        }

        private void ShowComboText(string message)
        {
            if (!comboText) return;

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

        private void ClearComboText()
        {
            if (comboText)
            {
                comboText.text = "";
                comboObject.SetActive(false);
            }
        }

        #endregion

        #region Audio

        private void PlayAudio(AudioClip clip)
        {
            if (audioSource && clip)
                audioSource.PlayOneShot(clip);
        }

        #endregion

        #region Save & Load

        public void SaveGame()
        {
            if (matchedPairs <= 0)
                return;

            SaveLoadManager.SaveGame(gridManager, this);
        }

        public void LoadGame()
        {
            menuPanel.SetActive(false);
            gameplayPanel.SetActive(true);

            SaveLoadManager.LoadGame(gridManager, this);
        }

        public void RestoreGameState(SaveData data)
        {
            matchedPairs = data.score;
            matchesFound = data.score;
            moves = data.moves;

            comboStreak = data.comboStreak;
            comboMultiplier = Mathf.Max(1, data.comboMultiplier);

            matchedCardIds.Clear();

            foreach (var cardData in data.cards)
                if (cardData.isFaceUp)
                    matchedCardIds.Add(cardData.id);

            UpdateScoreText();
            UpdateMovesText();
            CheckForGameCompletion();
        }

        #endregion

        #region Grid Generation & Reveal

        private void GenerateRandomGrid(out int rows, out int columns, int min = 2, int max = 6)
        {
            do
            {
                rows = Random.Range(min, max + 1);
                columns = Random.Range(min, rows + 1);
            }
            while ((rows * columns) % 2 != 0);
        }

        private IEnumerator RevealAllCardsOnce()
        {
            isComparing = true;

            Card[] allCards = FindObjectsOfType<Card>();

            foreach (var card in allCards)
                if (card && !card.isFaceUp)
                    card.Flip();

            yield return new WaitForSeconds(1.5f);

            foreach (var card in allCards)
                if (card && card.isFaceUp)
                    card.Flip();

            yield return new WaitForSeconds(0.5f);

            isComparing = false;
        }

        #endregion

        #region Save / Load UI

        private void CheckForSavedGame()
        {
            if (loadButton != null)
            {
                string savePath = System.IO.Path.Combine(Application.persistentDataPath, "cardgame_save.json");
                loadButton.gameObject.SetActive(System.IO.File.Exists(savePath));
            }
        }

        #endregion
    }
}
