using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BetterScrollView : LayoutGroup
{
    public enum Direction
    {
        Horizontal = 0,
        Vertical = 1,
    }

    [System.Serializable]
    public class Element
    {
        public int index;
        public RectTransform transform;
    }

    [System.Serializable]
    public class EndlessScrollEvent : UnityEvent<int, RectTransform>
    {
    }

    [SerializeField]
    private Vector2 m_CellSize = new Vector2(100, 100);

    [SerializeField]
    private Vector2 m_Spacing = new Vector2(0, 0);

    [SerializeField]
    private Direction m_Direction = Direction.Vertical;

    [SerializeField]
    private LinkedList<Element> m_Elements;

    [SerializeField]
    private RectTransform m_View;

    private RectTransform m_Content;

    [SerializeField]
    private GameObject m_ElementPrefab;

    [SerializeField]
    private int m_ObjectCount;

    [SerializeField]
    private EndlessScrollEvent m_OnIndexChanged = new EndlessScrollEvent();

    RectTransform Content
    {
        get
        {
            if (m_Content == null)
            {
                m_Content = GetComponent<RectTransform>();
            }
            return m_Content;
        }
    }

    RectTransform View
    {
        get
        {
            if (m_View == null)
            {
                var scroll = GetComponentInParent<ScrollRect>();
                m_View = scroll.viewport;
            }
            return m_View;
        }
    }

    public EndlessScrollEvent OnIndexChanged
    {
        get
        {
            return m_OnIndexChanged;
        }
    }

    public int ObjectCount
    {
        get
        {
            return m_ObjectCount;
        }
        set
        {
            m_ObjectCount = value;
        }
    }

    public GameObject ElementPrefab
    {
        get
        {
            return m_ElementPrefab;
        }
        set
        {
            m_ElementPrefab = value;
        }
    }

    public int index
    {
        get
        {
            return CalcuteCurrentIndex();
        }
        set
        {
            SetCurrentIndex(value);
        }
    }

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        CreateElements();
    }

    public override void CalculateLayoutInputVertical()
    {
    }

    public override void SetLayoutHorizontal()
    {
    }

    public override void SetLayoutVertical()
    {
    }

    public int CalcuteCurrentIndex()
    {
        if (m_Elements == null || m_Elements.Count == 0)
        {
            return 0;
        }

        var curNode = m_Elements.First;
        Element lastEle = null;
        float distance_ = float.MaxValue;

        if (m_Direction == Direction.Horizontal)
        {
            float checkborder = View.TransformPoint(View.rect.min).x;
            while (curNode != null)
            {
                float eleWorldCenterX = curNode.Value.transform.TransformPoint(curNode.Value.transform.rect.center).x;
                var dis = Mathf.Abs(eleWorldCenterX - checkborder);
                if (dis > distance_)
                {
                    break;
                }
                distance_ = dis;
                lastEle = curNode.Value;
                curNode = curNode.Next;
            }
            return lastEle.index;
        }
        else
        {
            while (curNode != null)
            {
                float checkborder = View.TransformPoint(View.rect.max).y;
                float eleWorldCenterY = curNode.Value.transform.TransformPoint(curNode.Value.transform.rect.center).y;
                var dis = Mathf.Abs(checkborder - eleWorldCenterY);
                if (dis > distance_)
                {
                    break;
                }
                distance_ = dis;
                lastEle = curNode.Value;
                curNode = curNode.Next;
            }
            return lastEle.index;
        }
    }

    public bool SetCurrentIndex(int index)
    {
        if(index <0 || index >= m_ObjectCount)
        {
            throw (new System.IndexOutOfRangeException());
        }

        if(m_Elements == null || m_Elements.Count==0)
        {
            return false;
        }

        if(m_Direction == Direction.Horizontal)
        {
            float offsetX = m_Padding.left + (m_CellSize.x + m_Spacing.x) * index;
            float maxOffsetX = Content.rect.width - View.rect.width;
            offsetX = offsetX > maxOffsetX ? maxOffsetX : offsetX;
            Vector3 newPosition = Content.localPosition;
            newPosition.x = offsetX;
            Content.localPosition = newPosition;
        }
        else
        {
            float offsetY = m_Padding.top + (m_CellSize.y + m_Spacing.y) * index;
            float maxOffsetY = Content.rect.height - View.rect.height;
            offsetY = offsetY > maxOffsetY ? maxOffsetY : offsetY;
            Vector3 newPosition = Content.localPosition;
            newPosition.y = offsetY;
            Content.localPosition = newPosition;
        }
        return true;
    }

    private bool IsElementShouldMoveUp(RectTransform t)
    {
        return View.TransformPoint(View.rect.min).y - t.TransformPoint(t.rect.max).y > m_CellSize.y;
    }

    private bool IsElementShouldMoveDown(RectTransform t)
    {
        return t.TransformPoint(t.rect.min).y - View.TransformPoint(View.rect.max).y > m_CellSize.y;
    }

    private bool IsElementShouldMoveRight(RectTransform t)
    {
        return View.TransformPoint(View.rect.min).x - t.TransformPoint(t.rect.max).x > m_CellSize.x;
    }

    private bool IsElementShouldMoveLeft(RectTransform t)
    {
        return t.TransformPoint(t.rect.min).x - View.TransformPoint(View.rect.max).x > m_CellSize.x;
    }

    private int CalculateElementsCount()
    {
        Vector2 viewSize = View.rect.size;
        int index_ = m_Direction == Direction.Horizontal ? 0 : 1;
        return Mathf.CeilToInt(viewSize[index_] / m_CellSize[index_]) + 2;
    }

    private void CreateElements()
    {
        int elementCount = CalculateElementsCount();
        if (m_Elements == null)
        {
            m_Elements = new LinkedList<Element>();
        }
        else
        {
            m_Elements.Clear();
        }

        if (m_Direction == Direction.Vertical)
        {
            float contentHeight = (m_CellSize.y + m_Spacing.y) * m_ObjectCount + m_Padding.vertical;
            Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_CellSize.x + m_Padding.horizontal);
            Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
        }
        else
        {
            float contentWidth = (m_CellSize.x + m_Spacing.x) * m_ObjectCount + m_Padding.horizontal;
            Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
            Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_CellSize.y + m_Padding.vertical);
        }

        int childCount = rectChildren.Count;

        for (int i = 0; i < elementCount; i++)
        {
            RectTransform rt = null;
            if (i < childCount)
            {
                rt = this.rectTransform.GetChild(i) as RectTransform;
            }
            else
            {
                GameObject g = Instantiate(m_ElementPrefab, Content);
                rt = g.transform as RectTransform;
            }

            if (m_Direction == Direction.Horizontal)
            {
                Vector2 offset = new Vector2(GetStartOffset(0, (m_CellSize.x+m_Spacing.x)*elementCount), GetStartOffset(1, m_CellSize.y));
                SetChildAlongAxis(rt, 0, offset.x + (m_CellSize.x + m_Spacing.x) * i);
                SetChildAlongAxis(rt, 1, offset.y);
            }
            else
            {
                Vector2 offset = new Vector2(GetStartOffset(0, m_CellSize.x), GetStartOffset(1, (m_CellSize.y + m_Spacing.y) * elementCount));
                SetChildAlongAxis(rt, 0, offset.x);
                SetChildAlongAxis(rt, 1, offset.y + (m_CellSize.y + m_Spacing.y) * i);
            }

            m_Elements.AddLast(new Element()
            {
                index = i,
                transform = rt
            });
        }

        var ele = m_Elements.First;
        while (ele != null)
        {
            OnIndexChanged.Invoke(ele.Value.index, ele.Value.transform);
            ele = ele.Next;
        }
    }

    private void LateUpdate()
    {
        if (m_Elements == null || m_Elements.Count == 0)
        {
            return;
        }

        if (m_Direction == Direction.Vertical)
        {
            var top = m_Elements.First.Value;
            var bottom = m_Elements.Last.Value;

            while (IsElementShouldMoveUp(bottom.transform) && top.index > 0)
            {
                m_Elements.RemoveLast();
                m_Elements.AddFirst(bottom);
                Vector3 newPos = bottom.transform.localPosition;
                newPos.y = top.transform.localPosition.y + m_CellSize.y + m_Spacing.y;
                bottom.transform.localPosition = newPos;
                bottom.index = top.index - 1;
                OnIndexChanged.Invoke(bottom.index, bottom.transform);

                top = m_Elements.First.Value;
                bottom = m_Elements.Last.Value;
            }

            while (IsElementShouldMoveDown(top.transform) && bottom.index < m_ObjectCount - 1)
            {
                m_Elements.RemoveFirst();
                m_Elements.AddLast(top);
                Vector3 newPos = top.transform.localPosition;
                newPos.y = bottom.transform.localPosition.y - m_CellSize.y - m_Spacing.y;
                top.transform.localPosition = newPos;
                top.index = bottom.index + 1;
                OnIndexChanged.Invoke(top.index, top.transform);

                top = m_Elements.First.Value;
                bottom = m_Elements.Last.Value;
            }
        }
        else
        {
            var left = m_Elements.First.Value;
            var right = m_Elements.Last.Value;

            while (IsElementShouldMoveLeft(right.transform) && left.index > 0)
            {
                m_Elements.RemoveLast();
                m_Elements.AddFirst(right);
                Vector3 newPos = right.transform.localPosition;
                newPos.x = left.transform.localPosition.x - m_CellSize.x - m_Spacing.x;
                right.transform.localPosition = newPos;
                right.index = left.index - 1;

                OnIndexChanged.Invoke(right.index, right.transform);

                left = m_Elements.First.Value;
                right = m_Elements.Last.Value;
            }

            while (IsElementShouldMoveRight(left.transform) && right.index < m_ObjectCount - 1)
            {
                m_Elements.RemoveFirst();
                m_Elements.AddLast(left);
                Vector3 newPos = left.transform.localPosition;
                newPos.x = right.transform.localPosition.x + m_CellSize.x + m_Spacing.x;
                left.transform.localPosition = newPos;
                left.index = right.index + 1;

                OnIndexChanged.Invoke(left.index, left.transform);

                left = m_Elements.First.Value;
                right = m_Elements.Last.Value;
            }
        }
    }
}
