using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HFG
{
    public class Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public void EnableTile() => canvasGroup.blocksRaycasts = true;
        public void DisableTile() => canvasGroup.blocksRaycasts = false;

        Canvas canvas;
        CanvasGroup canvasGroup;
        Outline outline;
        RectTransform rectTransform;

        bool isDragging = false;

        private BoardGraphNode node;
        public BoardGraphNode GetNode() => node;
        public int GetPosition() => node.Position;

        private void Start()
        {
            canvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
            outline = GetComponent<Outline>();
        }

        public void SetNode(BoardGraphNode node)
        {
            this.node = node;
            //Debug.Log($"tile:{this} -->set--> node:{this.node}");

            // later can be animated
            transform.SetParent(node.transform);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ResetDrag();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetDrag();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            BoardSetupUI.Instance.DisableAllTiles();
            outline.effectColor = Color.red;
            isDragging = true;
        }
        public void OnDrag(PointerEventData eventData)
        {
            transform.position += (Vector3)(eventData.delta / canvas.scaleFactor);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            BoardSetupUI.Instance.EnableAllTiles();
            outline.effectColor = Color.black;
            isDragging = false;
        }
        public void ResetDrag() => rectTransform.anchoredPosition = Vector2.zero;
    }
}