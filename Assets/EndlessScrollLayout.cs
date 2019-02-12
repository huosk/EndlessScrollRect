using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EndlessScrollLayout : LayoutGroup
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
    private int m_LineCount = 1;

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
                if (scroll != null)
                {
                    m_View = scroll.viewport;
                }
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
        if (index < 0 || index >= m_ObjectCount)
        {
            throw (new System.IndexOutOfRangeException());
        }

        if (m_Elements == null || m_Elements.Count == 0)
        {
            return false;
        }

        int lineIndex = Mathf.FloorToInt(index / m_LineCount);

        if (m_Direction == Direction.Horizontal)
        {
            float offsetX = m_Padding.left + (m_CellSize.x + m_Spacing.x) * lineIndex;
            float maxOffsetX = Content.rect.width - View.rect.width;
            offsetX = offsetX > maxOffsetX ? maxOffsetX : offsetX;
            Vector3 newPosition = Content.localPosition;
            newPosition.x = -offsetX;
            Content.localPosition = newPosition;
        }
        else
        {
            float offsetY = m_Padding.top + (m_CellSize.y + m_Spacing.y) * lineIndex;
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

    // 计算实现滚动，需要元素的数量
    private int CalculateElementsCount()
    {
        Vector2 viewSize = View.rect.size;
        int index_ = m_Direction == Direction.Horizontal ? 0 : 1;
        return m_LineCount * (Mathf.CeilToInt(viewSize[index_] / m_CellSize[index_]) + 2);
    }

    private void CreateElements()
    {
        int elementCount = CalculateElementsCount();
        m_Elements = new LinkedList<Element>();

        // 每行/每列最大元素的数量
        int eleCountPerLine = Mathf.CeilToInt((float)m_ObjectCount / m_LineCount);

        if (m_Direction == Direction.Vertical)
        {
            float contentHeight = (m_CellSize.y + m_Spacing.y) * eleCountPerLine + m_Padding.vertical;
            float contentWidth = (m_CellSize.x + m_Spacing.x) * m_LineCount + m_Padding.horizontal;
            Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
            Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
        }
        else
        {
            float contentHeight = (m_CellSize.y + m_Spacing.y) * m_LineCount + m_Padding.vertical;
            float contentWidth = (m_CellSize.x + m_Spacing.x) * eleCountPerLine + m_Padding.horizontal;
            Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
            Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
        }

        int childCount = rectChildren.Count;

        for (int i = 0; i < elementCount; i++)
        {
            RectTransform rt = null;
            if (i < childCount)
            {
                rt = this.rectTransform.GetChild(i) as RectTransform;
            }
            else if (m_ElementPrefab != null)
            {
                GameObject g = Instantiate(m_ElementPrefab, Content);
                rt = g.transform as RectTransform;
            }

            if (rt == null)
            {
                return;
            }

            // 沿着指定方向的索引数（水平-列索引、垂直-行索引）
            int r = Mathf.FloorToInt((float)i / m_LineCount);

            // 行内索引(水平-行索引、垂直-列索引)
            int c = i % m_LineCount;

            if (m_Direction == Direction.Horizontal)
            {
                Vector2 offset = new Vector2(GetStartOffset(0, (m_CellSize.x + m_Spacing.x) * eleCountPerLine), GetStartOffset(1, m_CellSize.y));
                SetChildAlongAxis(rt, 0, offset.x + (m_CellSize.x + m_Spacing.x) * r);
                SetChildAlongAxis(rt, 1, offset.y + (m_CellSize.y + m_Spacing.y) * c);
            }
            else
            {
                Vector2 offset = new Vector2(GetStartOffset(0, m_CellSize.x), GetStartOffset(1, (m_CellSize.y + m_Spacing.y) * eleCountPerLine));
                SetChildAlongAxis(rt, 0, offset.x + (m_CellSize.x + m_Spacing.x) * c);
                SetChildAlongAxis(rt, 1, offset.y + (m_CellSize.y + m_Spacing.y) * r);
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
                if (top.index % m_LineCount == 0)
                {
                    // 不在同一行:top在第一列，bottom的位置则应该是上一行的最后一个位置
                    newPos.x = top.transform.localPosition.x + (m_LineCount - 1) * (m_CellSize.y + m_Spacing.y);
                    newPos.y = top.transform.localPosition.y + m_CellSize.y + m_Spacing.y;
                }
                else
                {
                    // 在同一行:bottom位于top的左侧
                    newPos.x = top.transform.localPosition.x - (m_CellSize.x + m_Spacing.x);
                    newPos.y = top.transform.localPosition.y;
                }

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
                if (bottom.index % m_LineCount == (m_LineCount - 1))
                {
                    // 不在同一行:bottom位于最后一列，则top位置应该是下一行的第一列
                    newPos.x = bottom.transform.localPosition.x - (m_LineCount - 1) * (m_CellSize.x + m_Spacing.x);
                    newPos.y = bottom.transform.localPosition.y - (m_CellSize.y + m_Spacing.y);
                }
                else
                {
                    // 在同一行:top位于bottom右侧
                    newPos.x = bottom.transform.localPosition.x + (m_CellSize.x + m_Spacing.x);
                    newPos.y = bottom.transform.localPosition.y;
                }
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
                if (left.index % m_LineCount == 0)
                {
                    // 不在同一列:left位于第一行，则right位置应该是上一列的最后一行
                    newPos.x = left.transform.localPosition.x - (m_CellSize.x + m_Spacing.x);
                    newPos.y = left.transform.localPosition.y - (m_LineCount - 1) * (m_CellSize.y + m_Spacing.y);
                }
                else
                {
                    // 在同一列:right位于left的上方
                    newPos.x = left.transform.localPosition.x;
                    newPos.y = left.transform.localPosition.y + (m_CellSize.y + m_Spacing.y);
                }
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
                if (right.index % m_LineCount == (m_LineCount - 1))
                {
                    // 不在同一列:right位于最后一行，则left应该位于下一列的第一行
                    newPos.x = right.transform.localPosition.x + m_CellSize.x + m_Spacing.x;
                    newPos.y = right.transform.localPosition.y + (m_LineCount - 1) * (m_CellSize.y + m_Spacing.y);
                }
                else
                {
                    // 在同一列：left位于right的下方
                    newPos.x = right.transform.localPosition.x;
                    newPos.y = right.transform.localPosition.y - (m_CellSize.y + m_Spacing.y);
                }
                left.transform.localPosition = newPos;
                left.index = right.index + 1;

                OnIndexChanged.Invoke(left.index, left.transform);

                left = m_Elements.First.Value;
                right = m_Elements.Last.Value;
            }
        }
    }
}
