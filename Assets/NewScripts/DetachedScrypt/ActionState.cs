using Clicker.Models;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Clicker.Scrypts
{
    public abstract class InteractionSctipt : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        protected abstract class ActionState
        {
            public RectTransform rect;
            public float scale;
            public float velocity;
            public ActionState(Transform trans)
            {
                rect = trans as RectTransform;
                scale = GameObject.Find("Canvas").GetComponent<Canvas>().scaleFactor;
            }

            public abstract IEnumerator Undo();
            public abstract void Update(Vector2 deltaCoord);
        }
        protected class ScrollState : ActionState
        {
            private float height;
            public ScrollState(Transform trans) : base(trans)
            { 
                height = rect.rect.height - (rect.parent as RectTransform).rect.height;
                if (height < 0) height = 0;
                if ((rect.parent.parent.parent as RectTransform).pivot.x == 1)
                    height += 113;
            }

            public override IEnumerator Undo()
            {
                float timer = 0;
                int sign = velocity > 0 ? 1 : -1;
                velocity *= sign;
                float k = 100;
                while (k > 1)
                {
                    timer += Time.deltaTime;
                    k = Mathf.Lerp(velocity, 0, Mathf.Sin(Mathf.PI * timer));
                    float y = Mathf.Clamp(rect.anchoredPosition.y + sign * k, 0, height);
                    rect.anchoredPosition = new Vector2(0, y);
                    yield return null;
                }
            }

            public override void Update(Vector2 deltaCoord)
            {
                velocity = deltaCoord.y / scale;
                float y = Mathf.Clamp(rect.anchoredPosition.y + velocity, 0, height);
                rect.anchoredPosition = new Vector2(0, y);
            }
        }
        protected class SwapState : ActionState
        {
            RectTransform left, right;
            float shift;
            string thisCategory;
            public SwapState(Transform main, RectTransform left, RectTransform right, string thisCategory, float addShift = 0) : base(main)
            {
                this.left = left;
                this.right = right;
                shift = rect.rect.width + addShift;
                this.thisCategory = thisCategory;
            }
            public override IEnumerator Undo()
            {
                float timer = 0;
                float x = rect.anchoredPosition.x + velocity;
                int xsign = x > 0 ? 1 : -1;
                int vsign;
                if (x * xsign < shift / 4f)
                {
                    vsign = -1;
                    x = 0;
                }
                else
                {
                    vsign = 1;
                    x = shift * xsign;
                }
                if (velocity < 0) velocity *= -1;
                float cur_x = rect.anchoredPosition.x * xsign;

                while (rect.anchoredPosition.x != x)
                {
                    timer += Time.deltaTime;
                    float k = Mathf.Clamp(velocity * Mathf.Cos(Mathf.PI * timer), 10, velocity);
                    cur_x = Mathf.Clamp(cur_x + vsign * k, 0, shift);
                    rect.anchoredPosition = new Vector2(cur_x * xsign, 0);
                    yield return null;
                }
                if (x > 0)
                {
                    SwitchCategory(x);
                    Destroy(rect.GetChild(0).gameObject);
                    Destroy(left.gameObject);
                    right.anchoredPosition = new Vector2(0, 0);
                    right.name = "Content";
                }
                else if (x < 0)
                {
                    SwitchCategory(x);
                    Destroy(rect.GetChild(0).gameObject);
                    Destroy(right.gameObject);
                    left.anchoredPosition = new Vector2(0, 0);
                    left.name = "Content";
                }
                else
                {
                    Destroy(left.gameObject);
                    Destroy(right.gameObject);
                }
                rect.anchoredPosition = new Vector2(0, 0);

            }
            
            private void SwitchCategory(float x)
            {
                string category = thisCategory;
                switch (thisCategory)
                {
                    case "Shop":
                        category = "Busters";
                        break;
                    case "Busters":
                        category = "Shop";
                        break;
                    case "Levels":
                        if (Values.data.isTest)
                            if (x > 0)
                                category = "Packs";
                            else
                                category = "Testing";
                        else
                            category = "Packs";
                        break;
                    case "Packs":
                        if (Values.data.isTest)
                            if (x > 0)
                                category = "Testing";
                            else
                                category = "Levels";
                        else
                            category = "Levels";
                        break;
                    case "Testing":
                        if (x > 0)
                            category = "Levels";
                        else
                            category = "Packs";
                        break;
                }
                Transform categories = rect.parent.parent.Find("Categories");
                foreach (Transform categoryBtn in categories)
                {
                    if (categoryBtn.name.Equals(thisCategory))
                        categoryBtn.GetComponent<UnityEngine.UI.Button>().interactable = true;
                    if (categoryBtn.name.Equals(category))
                        categoryBtn.GetComponent<UnityEngine.UI.Button>().interactable = false;
                }
            }

            public override void Update(Vector2 deltaCoord)
            {
                velocity = deltaCoord.x / scale;
                float x = Mathf.Clamp(rect.anchoredPosition.x + velocity, -shift, shift);
                rect.anchoredPosition = new Vector2(x, 0);
            }
        }
        protected class MovePanelStateX : ActionState
        {
            CanvasGroup scoreShow;
            float shift;
            int side;
            public MovePanelStateX(Transform trans, int side) : base(trans)
            {
                shift = rect.rect.width;
                scoreShow = GameObject.Find("ScoreShower").GetComponent<CanvasGroup>();
                this.side = side;
            }

            public override IEnumerator Undo()
            {
                float timer = 0;
                int sign;
                if ((rect.anchoredPosition.x + velocity) * side > shift * 4 / 5f)
                    sign = side;
                else if ((rect.anchoredPosition.x + velocity) * side < shift / 5f)
                    sign = -side;
                else
                    sign = velocity > 0 ? 1 : -1;
                if (velocity < 0) velocity *= -1;

                float x = rect.anchoredPosition.x;
                while (rect.anchoredPosition.x != 0 && rect.anchoredPosition.x * side != shift)
                {
                    timer += Time.deltaTime;
                    float k = Mathf.Clamp(velocity * Mathf.Cos(Mathf.PI * timer), 15, velocity);
                    x = Mathf.Clamp((x + k * sign) * side, 0, shift) * side;
                    rect.anchoredPosition = new Vector2(x, 0);
                    if (side == -1)
                        scoreShow.alpha = 1 - Mathf.Abs(x / shift);
                    yield return null;
                }
            }

            public override void Update(Vector2 deltaCoord)
            {
                velocity = deltaCoord.x / scale;
                float x = Mathf.Clamp((rect.anchoredPosition.x + velocity) * side, 0, shift) * side;
                if (side == -1)
                    scoreShow.alpha = 1 - Mathf.Abs(x / shift);
                rect.anchoredPosition = new Vector2(x, 0);
            }
        }

        protected ActionState actionState;

        public abstract void OnBeginDrag(PointerEventData eventData);

        public void OnDrag(PointerEventData eventData)
        {
            actionState.Update(eventData.delta);
        }

        public void OnEndDrag(PointerEventData eventData)
        { 
            StartCoroutine(actionState.Undo());
        }
    }

}
