using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardGame
{
    public enum CardEvent
    {
        Flipped,
        Matched,
        Mismatched
    }

    public class Card : MonoBehaviour, IPointerClickHandler, ISubject
    {
        #region Inspector & Public Fields

        public int id;
        public bool isFaceUp = false;
        public Sprite faceSprite;
        public Sprite backSprite;

        #endregion

        #region Private Fields

        private Image cardImage;
        private Coroutine flipCoroutine;
        private Coroutine flashCoroutine;

        private List<IObserver> observers = new List<IObserver>();

        public CardEvent currentEvent;

        #endregion

        #region Observer Pattern

        public void Attach(IObserver observer) => observers.Add(observer);

        public void Notify(Card card, CardEvent cardEvent)
        {
            foreach (var observer in observers)
            {
                observer.OnNotify(card, cardEvent);
            }
        }

        #endregion

        #region Unity Callbacks

        void Start()
        {
            cardImage = GetComponent<Image>();
            if (cardImage != null)
                cardImage.sprite = backSprite;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isFaceUp)
            {
                Flip();
                FindObjectOfType<GameManager>().CardClicked(this);
            }
        }

        #endregion

        #region Flip Logic

        public void Flip()
        {
            if (currentEvent == CardEvent.Mismatched)
                return;

            if (flipCoroutine != null)
                StopCoroutine(flipCoroutine);

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

        public void FlipForLoad()
        {
            if (flipCoroutine != null)
                StopCoroutine(flipCoroutine);

            flipCoroutine = StartCoroutine(FlipCardForLoad());
        }

        private IEnumerator FlipCardForLoad()
        {
            float duration = 0.8f;
            float elapsedTime = 0f;
            Quaternion originalRotation = transform.localRotation;

            while (elapsedTime < duration / 2)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / (duration / 2);
                transform.localRotation = Quaternion.Slerp(originalRotation, originalRotation * Quaternion.Euler(0, 90, 0), t);
                yield return null;
            }

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

            transform.localRotation = originalRotation;
            Notify(this, CardEvent.Flipped);
        }

        #endregion

        #region Card UI

        public void InitializeCardSprite()
        {
            if (cardImage == null)
                cardImage = GetComponent<Image>();

            cardImage.sprite = isFaceUp ? faceSprite : backSprite;
        }

        #endregion

        #region Match Flash

        public void PlayMatchFlash()
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashColor());
        }

        private IEnumerator FlashColor()
        {
            if (cardImage == null)
                cardImage = GetComponent<Image>();

            Color original = cardImage.color;
            Color flash = new Color(0.6f, 1f, 0.6f, 1f);
            float duration = 0.25f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                cardImage.color = Color.Lerp(original, flash, t / duration);
                yield return null;
            }

            t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                cardImage.color = Color.Lerp(flash, original, t / duration);
                yield return null;
            }

            cardImage.color = original;

            yield return new WaitForSeconds(0.5f);

            cardImage.enabled = false;
        }

        #endregion
    }
}
