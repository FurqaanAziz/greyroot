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
        public Button startButton;
        public Button homeButton;
        public Button closeButton;
        public TMP_InputField rowsInputField, columnsInputField;
        public int rows = 2;
        public int columns = 2;
        public TMP_Text warning;

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
            if (homeButton != null)
            {
                closeButton.onClick.AddListener(CloseGame);
            }
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
                if (rows <= 10 || columns <= 10)
                {
                    warning.text = ($"Invalid grid: {rows} x {columns} = {rows * columns} cards. Result Must be even.");
                }
                else if (rows >= 10 || columns >= 10)
                {
                    warning.text = ($"Grid exceeding limit of 10 x 10");
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
            gridManager.ResetGrid();
            menuPanel.SetActive(true);
            gameplayPanel.SetActive(false);
        }
        public void CloseGame()
        {
            Application.Quit();
        }
    }
}
