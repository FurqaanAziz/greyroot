using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

namespace CardGame
{
    public class GridManager : MonoBehaviour
    {
        public List<GameObject> cardPrefabs;
        
        public float spacing = 10f;
        public float aspectRatio = 1.0f;

        private Dictionary<string, int> prefabIDs = new Dictionary<string, int>();

        public Transform gridContainer;

        private GridLayoutGroup gridLayoutGroup;

        public void CreateGrid(int rows, int columns)
        {
            foreach (Transform child in gridContainer)
            {
                Destroy(child.gameObject);
            }

            int totalCards = rows * columns;
            int totalPairs = totalCards / 2;

            List<int> availableCardIDs = new List<int>();

            List<GameObject> shuffledCardPrefabs = cardPrefabs.OrderBy(x => Guid.NewGuid()).ToList();
            foreach (var prefab in shuffledCardPrefabs)
            {
                if (!prefabIDs.ContainsKey(prefab.name))
                {
                    prefabIDs[prefab.name] = prefabIDs.Count;
                }

                int id = prefabIDs[prefab.name];

                availableCardIDs.Add(id);
                availableCardIDs.Add(id);

                if (availableCardIDs.Count >= totalCards) break;
            }

            while (availableCardIDs.Count < totalCards)
            {
                foreach (var prefab in shuffledCardPrefabs)
                {
                    int id = prefabIDs[prefab.name];
                    availableCardIDs.Add(id);
                    availableCardIDs.Add(id);
                    if (availableCardIDs.Count >= totalCards) break;
                }
            }

            Shuffle(availableCardIDs);

            for (int i = 0; i < totalCards; i++)
            {
                int cardId = availableCardIDs[i];

                string prefabName = prefabIDs.FirstOrDefault(x => x.Value == cardId).Key;
                if (prefabName != null)
                {
                    GameObject cardInstance = Instantiate(cardPrefabs.Find(p => p.name == prefabName), gridContainer);
                    Card card = cardInstance.GetComponent<Card>();
                    card.id = cardId;
                    
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(gridContainer.GetComponent<RectTransform>());
        }

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
            {
                cellWidth = cellHeight * aspectRatio;
            }
            else
            {
                cellHeight = cellWidth / aspectRatio;
            }
            gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
            gridLayoutGroup.spacing = new Vector2(spacing, spacing);
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = columns;
        }

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
            {
                Destroy(child.gameObject);
            }
            prefabIDs.Clear();
        }
    }
}
