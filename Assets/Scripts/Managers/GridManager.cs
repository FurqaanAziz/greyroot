using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame
{
    public class GridManager : MonoBehaviour
    {
        #region Inspector Fields

        public List<GameObject> cardPrefabs;
        public float spacing = 10f;
        public float aspectRatio = 1.0f;
        public Transform gridContainer;

        #endregion

        #region Private Fields

        private Dictionary<string, int> prefabIDs = new Dictionary<string, int>();
        private GridLayoutGroup gridLayoutGroup;

        #endregion

        #region Grid Creation

        public void CreateGrid(int rows, int columns)
        {
            ClearExistingGrid();

            int totalCards = rows * columns;
            int totalPairs = totalCards / 2;

            List<int> availableCardIDs = GenerateCardIDs(totalCards);
            Shuffle(availableCardIDs);

            InstantiateCards(availableCardIDs, totalCards);

            LayoutRebuilder.ForceRebuildLayoutImmediate(gridContainer.GetComponent<RectTransform>());
        }

        private void ClearExistingGrid()
        {
            foreach (Transform child in gridContainer)
                Destroy(child.gameObject);
        }

        private List<int> GenerateCardIDs(int totalCards)
        {
            List<int> availableCardIDs = new List<int>();

            List<GameObject> shuffledCardPrefabs = cardPrefabs.OrderBy(x => Guid.NewGuid()).ToList();

            foreach (var prefab in shuffledCardPrefabs)
            {
                if (!prefabIDs.ContainsKey(prefab.name))
                    prefabIDs[prefab.name] = prefabIDs.Count;

                int id = prefabIDs[prefab.name];

                availableCardIDs.Add(id);
                availableCardIDs.Add(id);

                if (availableCardIDs.Count >= totalCards)
                    break;
            }

            while (availableCardIDs.Count < totalCards)
            {
                foreach (var prefab in shuffledCardPrefabs)
                {
                    int id = prefabIDs[prefab.name];
                    availableCardIDs.Add(id);
                    availableCardIDs.Add(id);

                    if (availableCardIDs.Count >= totalCards)
                        break;
                }
            }

            return availableCardIDs;
        }

        private void InstantiateCards(List<int> ids, int totalCards)
        {
            for (int i = 0; i < totalCards; i++)
            {
                int cardId = ids[i];

                string prefabName = prefabIDs.FirstOrDefault(x => x.Value == cardId).Key;

                if (prefabName != null)
                {
                    GameObject prefab = cardPrefabs.Find(p => p.name == prefabName);

                    GameObject cardInstance = Instantiate(prefab, gridContainer);
                    Card card = cardInstance.GetComponent<Card>();

                    card.id = cardId;
                }
            }
        }

        #endregion

        #region Grid Layout

        public void SetupGridLayout(int rows, int columns)
        {
            gridLayoutGroup = gridContainer.GetComponent<GridLayoutGroup>();

            RectTransform canvasRect = gridContainer.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.sizeDelta;

            int padding = 10;
            gridLayoutGroup.padding = new RectOffset(padding, padding, padding, padding);

            float availableWidth = canvasSize.x - (padding * 2);
            float availableHeight = canvasSize.y - (padding * 2);

            float cellWidth = (availableWidth - (spacing * (columns - 1))) / columns;
            float cellHeight = (availableHeight - (spacing * (rows - 1))) / rows;

            if (columns < rows)
            {
                cellWidth = (availableWidth - (spacing * (columns - 1))) / columns;
                cellHeight = (availableHeight - (spacing * (rows - 1))) / rows;
            }
            else
            {
                cellWidth = (availableWidth - (spacing * (rows - 1))) / rows;
                cellHeight = (availableHeight - (spacing * (columns - 1))) / columns;
            }

            if (cellWidth / cellHeight > aspectRatio)
                cellWidth = cellHeight * aspectRatio;
            else
                cellHeight = cellWidth / aspectRatio;

            gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
            gridLayoutGroup.spacing = new Vector2(spacing, spacing);
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = columns;
        }

        #endregion

        #region Utility

        private void Shuffle(List<int> list)
        {
            System.Random rand = new System.Random();
            int n = list.Count;

            while (n > 1)
            {
                int k = rand.Next(n--);
                int temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
        }

        public void ResetGrid()
        {
            foreach (Transform child in gridContainer)
                Destroy(child.gameObject);

            prefabIDs.Clear();
        }

        #endregion

        #region Save/Load

        public void RestoreGrid(SaveData data, GameManager gameManager)
        {
            if (data == null || data.cards == null)
                return;

            ResetGrid();

            gameManager.SetRows(data.rows);
            gameManager.SetColumns(data.columns);

            SetupGridLayout(gameManager.GetRows(), gameManager.GetColumns());

            var instantiatedCards = new List<Card>(data.cards.Count);

            foreach (var cardData in data.cards)
            {
                var prefab = cardPrefabs.FirstOrDefault(p => p.name == cardData.prefabName);

                var cardObj = Instantiate(prefab, gridContainer);
                var card = cardObj.GetComponent<Card>();

                card.id = cardData.id;
                card.isFaceUp = cardData.isFaceUp;
                card.InitializeCardSprite();

                instantiatedCards.Add(card);

                if (cardData.isFaceUp)
                    gameManager.GetMatchedCardIds().Add(card.id);
            }

            foreach (var card in instantiatedCards)
            {
                if (card.isFaceUp)
                    card.FlipForLoad();
            }

            var gridRect = gridContainer as RectTransform;

            if (gridRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
        }

        #endregion
    }
}
