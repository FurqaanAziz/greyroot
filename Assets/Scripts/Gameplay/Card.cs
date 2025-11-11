using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardGame
{
    public class Card : MonoBehaviour, IPointerClickHandler
    {
        public bool isFaceUp = false;
        public Sprite faceSprite;
        public Sprite backSprite;
        private Image cardImage;
        private Coroutine flipCoroutine;

        void Start()
        {
            cardImage = GetComponent<Image>();

            if (cardImage == null)
            {
                return;
            }

            cardImage.sprite = backSprite;
        }
        public void Flip()
        {
            if (flipCoroutine != null)
            {
                StopCoroutine(flipCoroutine);
            }
            flipCoroutine = StartCoroutine(FlipCard());
        }
        private IEnumerator FlipCard()
        {
            float duration = 0.5f;
            float elapsedTime = 0f;
            Quaternion originalRotation = transform.localRotation;

            while (elapsedTime < duration / 2)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / (duration / 2);
                transform.localRotation = Quaternion.Slerp(originalRotation, originalRotation * Quaternion.Euler(0, 90, 0), t);
                yield return null;
            }

            isFaceUp = !isFaceUp;
            cardImage.sprite = isFaceUp ? faceSprite : backSprite;

            transform.localRotation = originalRotation * Quaternion.Euler(0, 90, 0);

            elapsedTime = 0f;
            while (elapsedTime < duration / 2)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / (duration / 2);
                transform.localRotation = Quaternion.Slerp(originalRotation * Quaternion.Euler(0, 90, 0), originalRotation, t);
                yield return null;
            }
            transform.localRotation = Quaternion.identity;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isFaceUp)
            {
                Flip();
            }
        }
    }
}
