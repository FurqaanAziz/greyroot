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
        public TMP_Text movesText;

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
        public int GetMatchesFound() => matchesFound;
        public int GetComboStreak() => comboStreak;
        public int GetComboMultiplier() => comboMultiplier;

        private int moves = 0;
        public int GetMoves() => moves;
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
            GenerateRandomGrid(out rows, out columns);

            gridManager.SetupGridLayout(rows, columns);
            gridManager.CreateGrid(rows, columns);

            
            menuPanel.SetActive(false);
            gameplayPanel.SetActive(true);

            StartCoroutine(RevealAllCardsOnce());
        }
        //private bool IsGridValid(int rows, int columns)
        //{
        //    if (rows < 2 || rows > 10 || columns < 2 || columns > 10)
        //    {
        //        warning.text = "Rows and columns must be between 2x2 and 10x10";
        //        return false;
        //    }

        //    return (rows * columns) % 2 == 0;
        //}
        //private void WarningTextDeactivate()
        //{
        //    warning.gameObject.SetActive(false);
        //}
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
        private void UpdateMovesText()
        {
            if (movesText != null)
                movesText.text = $"Moves: {moves}";
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
            moves = 0;
            comboMultiplier = 1;
            matchedCardIds.Clear();
            cardClickQueue.Clear();
            isComparing = false;
            isGameCompleted = false;

            
            UpdateMovesText();
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

        private void OnApplicationQuit()
        {
            if (matchedPairs > 0)
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

            GenerateRandomGrid(out rows, out columns);

            ResetGameStateForNewGrid();

            gridManager.SetupGridLayout(rows, columns);
            gridManager.CreateGrid(rows, columns);
            StartCoroutine(RevealAllCardsOnce());
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

        private void GenerateRandomGrid(out int rows, out int columns, int min = 2, int max = 6)
        {
            do
            {
                rows = Random.Range(min, max + 1);
                columns = Random.Range(min, rows + 1);

            } while ((rows * columns) % 2 != 0);
        }
        private IEnumerator RevealAllCardsOnce()
        {
            isComparing = true;

            Card[] allCards = FindObjectsOfType<Card>();

            foreach (var card in allCards)
            {
                if (card == null || card.gameObject == null) continue;
                if (!card.isFaceUp)
                    card.Flip();
            }

            yield return new WaitForSeconds(1.5f);

            foreach (var card in allCards)
            {
                if (card == null || card.gameObject == null) continue;
                if (card.isFaceUp)
                    card.Flip();
            }

            yield return new WaitForSeconds(0.5f);
            isComparing = false;
        }
    }
}
