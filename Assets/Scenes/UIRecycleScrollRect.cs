using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class RecycleScrollRectEvent : UnityEvent<PointerEventData> { }

public class UIRecycleScrollRect : ScrollRect
{
    public RecycleScrollRectEvent OnBeginDragEvent = new RecycleScrollRectEvent();
    public RecycleScrollRectEvent OnEndDragEvent = new RecycleScrollRectEvent();
    public RecycleScrollRectEvent OnStopMovingEvent = new RecycleScrollRectEvent();

    public MovementType InMovementType;
    public bool needElasticReturn;
    public Vector2 clampedPosition;

    private bool isDragging = false;
    private bool isWaitingToStop = false;
    private Vector2 startCursor = Vector2.zero;

    protected override void Awake()
    {
        base.Awake();

        if (viewport == null)
            viewport = transform.Find("Viewport").GetComponent<RectTransform>();

        if (content == null)
            content = viewport.Find("Content").GetComponent<RectTransform>();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        OnBeginDragEvent?.Invoke(eventData);

        if(InMovementType != MovementType.Elastic)
        {
            base.OnBeginDrag(eventData);
            return;
        }

        isDragging = true;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!IsActive())
            return;

        UpdateBounds();

        startCursor = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out startCursor);
        m_ContentStartPosition = content.anchoredPosition;

        base.OnBeginDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        isWaitingToStop = true;
        isDragging = false;
        base.OnEndDrag(eventData);
        OnEndDragEvent?.Invoke(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if(InMovementType != MovementType.Elastic)
        {
            base.OnDrag(eventData);
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!IsActive())
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out var localCursor))
            return;

        UpdateBounds();

        var cursorDelta = localCursor - startCursor;
        var position = m_ContentStartPosition + cursorDelta;

        var offset = CalculateOffset(position - content.anchoredPosition);
        position += offset;
        var viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);

        if(needElasticReturn)
        {
            if (offset.x != 0)
                position.x = position.x - RubberDelta(offset.x, viewBounds.size.x);
            if (offset.y != 0)
                position.y = position.y - RubberDelta(offset.y, viewBounds.size.y);
        }

        SetContentAnchoredPosition(position);
    }

    private float RubberDelta(float overStretching, float viewSize)
    {
        return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
    }

    protected override void LateUpdate()
    {
        if (isWaitingToStop && velocity.magnitude < 0.01f)
        {
            OnMovementStop();
            isWaitingToStop = false;
        }

        if (InMovementType != MovementType.Elastic)
        {
            base.LateUpdate();
            return;
        }

        if (!content)
            return;

        EnsureLayoutHasRebuilt();
        UpdateBounds();
        var deltaTime = Time.unscaledDeltaTime;
        var offset = CalculateOffset(Vector2.zero);
        if (!isDragging && (offset != Vector2.zero || velocity != Vector2.zero))
        {
            var position = content.anchoredPosition;
            var vel = velocity;

            for (var axis = 0; axis < 2; axis++)
            {
                if (offset[axis] != 0)
                {
                    var speed = velocity[axis];
                    position[axis] = Mathf.SmoothDamp(content.anchoredPosition[axis], content.anchoredPosition[axis] + offset[axis], ref speed, elasticity, Mathf.Infinity, deltaTime);
                    if (Mathf.Abs(speed) < 1)
                        speed = 0;
                    vel[axis] = speed;
                }
                else if (inertia)
                {
                    vel[axis] *= Mathf.Pow(decelerationRate, deltaTime);
                    if (Mathf.Abs(velocity[axis]) < 1)
                        vel[axis] = 0;
                    position[axis] += velocity[axis] * deltaTime;
                }
                else
                {
                    vel[axis] = 0;
                }
            }

            velocity = vel;

            SetContentAnchoredPosition(position);
        }

        base.LateUpdate();
    }

    private void OnMovementStop()
    {
        isWaitingToStop = false;
        OnStopMovingEvent?.Invoke(null);
    }

    private void EnsureLayoutHasRebuilt()
    {
        if (!CanvasUpdateRegistry.IsRebuildingLayout())
            Canvas.ForceUpdateCanvases();
    }

    private Vector2 CalculateOffset(Vector2 delta)
    {
        var mViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
        return InternalCalculateOffset(ref mViewBounds, ref delta);
    }

    internal Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Vector2 delta)
    {
        var offset = Vector2.zero;
        if (!needElasticReturn)
            return offset;

        var min = new Vector2(content.anchoredPosition.x - content.rect.width / 2, (content.anchoredPosition.y - clampedPosition.y) - content.rect.height / 2);
        var max = new Vector2((content.anchoredPosition.x - clampedPosition.x) + content.rect.width / 2, content.anchoredPosition.y + content.rect.height / 2);

        if (horizontal)
        {
            min.x += delta.x;
            max.x += delta.x;
            if (min.x > viewBounds.min.x)
                offset.x = viewBounds.min.x - min.x;
            else if (max.x < viewBounds.max.x)
                offset.x = viewBounds.max.x - max.x;
        }

        if (vertical)
        {
            min.y += delta.y;
            max.y += delta.y;

            if (max.y < viewBounds.max.y)
                offset.y = viewBounds.max.y - max.y;
            else if (min.y > viewBounds.min.y)
                offset.y = viewBounds.min.y - min.y;
        }

        return offset;
    }
}
