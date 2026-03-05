using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace View.Button
{
    [RequireComponent(typeof(Image))]
    public abstract class CustomButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action OnClick;

        private bool _isPointerDown;
        private bool _interactable = true;

        private Image _image;

        protected virtual void Awake()
        {
            _image = GetComponent<Image>();
        }

        public bool Interactable
        {
            get => _interactable;
            set
            {
                _interactable = value;
                if (_image != null)
                {
                    _image.color = _interactable ? Color.white : new Color(1f, 1f, 1f, 0.4f);
                }
            }
        }

        public void AddListener(Action listener)
        {
            OnClick += listener;
        }

        public void RemoveListener(Action listener)
        {
            OnClick -= listener;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactable) return;
            _isPointerDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_interactable) return;

            if (_isPointerDown)
            {
                _isPointerDown = false;
                OnClick?.Invoke();
            }
        }
    }
}