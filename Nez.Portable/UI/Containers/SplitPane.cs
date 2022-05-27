using Microsoft.Xna.Framework;
using System;


namespace Nez.UI
{
    public class SplitPane : Group, IInputListener
    {
        public override float PreferredWidth
        {
            get
            {
                float first = _firstWidget == null
                    ? 0
                    : (_firstWidget is ILayout ? ((ILayout)_firstWidget).PreferredWidth : _firstWidget.width);
                float second = _secondWidget == null
                    ? 0
                    : (_secondWidget is ILayout ? ((ILayout)_secondWidget).PreferredWidth : _secondWidget.width);

                if (_vertical)
                    return Math.Max(first, second);

                return first + _style.Handle.MinWidth + second;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                float first = _firstWidget == null
                    ? 0
                    : (_firstWidget is ILayout ? ((ILayout)_firstWidget).PreferredHeight : _firstWidget.height);
                float second = _secondWidget == null
                    ? 0
                    : (_secondWidget is ILayout ? ((ILayout)_secondWidget).PreferredHeight : _secondWidget.height);

                if (!_vertical)
                    return Math.Max(first, second);

                return first + _style.Handle.MinHeight + second;
            }
        }

        private SplitPaneStyle _style;
        private float _splitAmount = 0.5f;
        private float _minAmount;
        private float _maxAmount = 1;
        private Element _firstWidget;
        private Element _secondWidget;
        private RectangleF _firstWidgetBounds;
        private RectangleF _secondWidgetBounds;
        private RectangleF _handleBounds;
        private bool _vertical;
        private Vector2 _lastPoint;
        private Vector2 _handlePosition;


        public SplitPane(Element firstWidget, Element secondWidget, SplitPaneStyle style, bool vertical = false)
        {
            SetStyle(style);
            SetFirstWidget(firstWidget);
            SetSecondWidget(secondWidget);

            _vertical = vertical;
            SetSize(PreferredWidth, PreferredHeight);
        }


        public SplitPane(Element firstWidget, Element secondWidget, IDrawable handle, bool vertical = false) : this(
            firstWidget, secondWidget, new SplitPaneStyle(handle), vertical)
        { }


        public SplitPane(SplitPaneStyle style, bool vertical = false) : this(null, null, style, vertical)
        { }


        #region IInputListener

        void IInputListener.OnMouseEnter()
        { }


        void IInputListener.OnMouseExit()
        { }


        bool IInputListener.OnMousePressed(Vector2 mousePos)
        {
            if (_handleBounds.Contains(mousePos))
            {
                _lastPoint = mousePos;
                _handlePosition = _handleBounds.Location;
                return true;
            }

            return false;
        }


        void IInputListener.OnMouseMoved(Vector2 mousePos)
        {
            if (_vertical)
            {
                float delta = mousePos.Y - _lastPoint.Y;
                float availHeight = height - _style.Handle.MinHeight;
                float dragY = _handlePosition.Y + delta;
                _handlePosition.Y = dragY;
                dragY = Math.Max(0, dragY);
                dragY = Math.Min(availHeight, dragY);
                _splitAmount = 1 - (dragY / availHeight);
                _splitAmount = Mathf.Clamp(_splitAmount, _minAmount, _maxAmount);

                _lastPoint = mousePos;
            }
            else
            {
                float delta = mousePos.X - _lastPoint.X;
                float availWidth = width - _style.Handle.MinWidth;
                float dragX = _handlePosition.X + delta;
                _handlePosition.X = dragX;
                dragX = Math.Max(0, dragX);
                dragX = Math.Min(availWidth, dragX);
                _splitAmount = dragX / availWidth;
                _splitAmount = Mathf.Clamp(_splitAmount, _minAmount, _maxAmount);

                _lastPoint = mousePos;
            }

            Invalidate();
        }


        void IInputListener.OnMouseUp(Vector2 mousePos)
        { }


        bool IInputListener.OnMouseScrolled(int mouseWheelDelta) => false;

        #endregion


        public override void Layout()
        {
            if (_vertical)
                CalculateVertBoundsAndPositions();
            else
                CalculateHorizBoundsAndPositions();

            if (_firstWidget != null)
            {
                var firstWidgetBounds = _firstWidgetBounds;
                _firstWidget.SetBounds(firstWidgetBounds.X, firstWidgetBounds.Y, firstWidgetBounds.Width,
                    firstWidgetBounds.Height);

                if (_firstWidget is ILayout)
                    ((ILayout)_firstWidget).Validate();
            }

            if (_secondWidget != null)
            {
                var secondWidgetBounds = _secondWidgetBounds;
                _secondWidget.SetBounds(secondWidgetBounds.X, secondWidgetBounds.Y, secondWidgetBounds.Width,
                    secondWidgetBounds.Height);

                if (_secondWidget is ILayout layout)
                    layout.Validate();
            }
        }


        public override void Draw(Batcher batcher, float parentAlpha)
        {
            Validate();

            if (transform)
                ApplyTransform(batcher, ComputeTransform());
            if (_firstWidget != null && _firstWidget.IsVisible())
            {
                var scissor = ScissorStack.CalculateScissors(_stage?.Camera, batcher.TransformMatrix, _firstWidgetBounds);
                if (ScissorStack.PushScissors(scissor))
                {
                    batcher.EnableScissorTest(true);
                    _firstWidget.Draw(batcher, parentAlpha * color.A);
                    batcher.EnableScissorTest(false);
                    ScissorStack.PopScissors();
                }
            }

            if (_secondWidget != null && _secondWidget.IsVisible())
            {
                var scissor = ScissorStack.CalculateScissors(_stage?.Camera, batcher.TransformMatrix,
                    _secondWidgetBounds);
                if (ScissorStack.PushScissors(scissor))
                {
                    batcher.EnableScissorTest(true);
                    _secondWidget.Draw(batcher, parentAlpha * color.A);
                    batcher.EnableScissorTest(false);
                    ScissorStack.PopScissors();
                }
            }

            _style.Handle.Draw(batcher, _handleBounds.X, _handleBounds.Y, _handleBounds.Width, _handleBounds.Height,
                ColorExt.Create(color, (int)(color.A * parentAlpha)));

            if (transform)
                ResetTransform(batcher);
        }

        private void CalculateHorizBoundsAndPositions()
        {
            float availWidth = width - _style.Handle.MinWidth;
            int leftAreaWidth = (int)(availWidth * _splitAmount);
            float rightAreaWidth = availWidth - leftAreaWidth;
            float handleWidth = _style.Handle.MinWidth;

            _firstWidgetBounds = new RectangleF(0, 0, leftAreaWidth, height);
            _secondWidgetBounds = new RectangleF(leftAreaWidth + handleWidth, 0, rightAreaWidth, height);
            _handleBounds = new RectangleF(leftAreaWidth, 0, handleWidth, height);
        }

        private void CalculateVertBoundsAndPositions()
        {
            float availHeight = height - _style.Handle.MinHeight;
            int topAreaHeight = (int)(availHeight * _splitAmount);
            float bottomAreaHeight = availHeight - topAreaHeight;

            _firstWidgetBounds = new RectangleF(0, height - topAreaHeight, width, topAreaHeight);
            _secondWidgetBounds = new RectangleF(0, 0, width, bottomAreaHeight);
            _handleBounds = new RectangleF(0, bottomAreaHeight, width, _style.Handle.MinHeight);
        }


        #region Configuration

        public SplitPane SetStyle(SplitPaneStyle style)
        {
            _style = style;
            SetHandle(_style.Handle);
            return this;
        }


        public SplitPaneStyle GetStyle() => _style;


        public SplitPane SetHandle(IDrawable handle)
        {
            _style.Handle = handle;
            Invalidate();

            return this;
        }


        public SplitPane SetFirstWidget(Element firstWidget)
        {
            if (_firstWidget != null)
                RemoveElement(_firstWidget);

            _firstWidget = firstWidget;
            if (_firstWidget != null)
                AddElement(_firstWidget);
            Invalidate();

            return this;
        }


        public SplitPane SetSecondWidget(Element secondWidget)
        {
            if (_secondWidget != null)
                RemoveElement(_secondWidget);

            _secondWidget = secondWidget;
            if (_secondWidget != null)
                AddElement(_secondWidget);
            Invalidate();

            return this;
        }


        /// <summary>
        /// The split amount between the min and max amount
        /// </summary>
        /// <param name="amount">Amount.</param>
        public SplitPane SetSplitAmount(float amount)
        {
            _splitAmount = Mathf.Clamp(amount, _minAmount, _maxAmount);
            return this;
        }


        public SplitPane SetMinSplitAmount(float amount)
        {
            Insist.IsTrue(amount < 0, "minAmount has to be >= 0");
            _minAmount = amount;
            return this;
        }


        public SplitPane SetMaxSplitAmount(float amount)
        {
            Insist.IsTrue(amount > 0, "maxAmount has to be <= 1");
            _maxAmount = amount;
            return this;
        }

        #endregion
    }


    public class SplitPaneStyle
    {
        public IDrawable Handle;


        public SplitPaneStyle()
        { }


        public SplitPaneStyle(IDrawable handle) => Handle = handle;


        public SplitPaneStyle Clone() => new SplitPaneStyle
        {
            Handle = Handle
        };
    }
}