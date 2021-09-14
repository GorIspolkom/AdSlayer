using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Clicker.Scrypts
{
    public class SwapScript : InteractionSctipt, IPointerClickHandler
    {
        [SerializeField] RectTransform rect;
        [SerializeField] ADPublish ad;
        

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y))
                actionState = new MovePanelStateX(rect, eventData.delta.x > 0 ? 1 : -1);   
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            RectTransform rect = ad.transform as RectTransform;
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, eventData.position, Camera.main))
            ad.ClickAction();
        } 
    }
}