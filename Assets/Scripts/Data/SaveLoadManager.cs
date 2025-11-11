using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CardGame
{
    [Serializable]
    public class CardData
    {
        public int id;
        public bool isFaceUp;
        public string prefabName;
    }

    [Serializable]
    public class SaveData
    {
        public int rows;
        public int columns;
        public int score;
        public bool isGameCompleted;
        public List<CardData> cards = new List<CardData>();
    }
     

    public class SaveLoadManager : MonoBehaviour
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "cardgame_save.json");

        public static void SaveGame(GridManager gridManager, GameManager gameManager)
        {
            SaveData data = new SaveData
            {
                rows = gameManager.rows,
                columns = gameManager.columns,
                score = gameManager.GetScore(),
                isGameCompleted = gameManager.IsGameCompleted()
            };

            List<int> matchedIds = gameManager.GetMatchedCardIds();

            foreach (Transform child in gridManager.gridContainer.transform)
            {
                Card card = child.GetComponent<Card>();
                if (card != null)
                {
                    bool isMatched = matchedIds.Contains(card.id);
                    data.cards.Add(new CardData
                    {
                        id = card.id,
                        isFaceUp = card.isFaceUp, 
                        prefabName = card.gameObject.name.Replace("(Clone)", "").Trim()
                    });
                }
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }

        public static void LoadGame(GridManager gridManager, GameManager gameManager)
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("No save file found.");
                return;
            }

                string json = File.ReadAllText(SavePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            gridManager.RestoreGrid(data, gameManager);
            gameManager.RestoreGameState(data);

        }
    }
}
