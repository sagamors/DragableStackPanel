using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace DragableStack
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:DragableStack"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:DragableStack;assembly=DragableStack"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:DragableStack/>
    ///
    /// </summary>

    // todo add attached property DragableStackIndex
    // todo fix tremor in animation
    // todo implement horizontal orientation
    public class DragableStack : StackPanel
    {
        private readonly TimeSpan ANIMATION_DURATION = new TimeSpan(0, 0, 0, 0, 200);
        private TimeSpan INTERVAL = new TimeSpan(0,0,0,0,200);
        private bool _isDown;
        private bool _isDragging;
        private Point _startPoint;
        private Point _startPointOnDragSource;
        private FrameworkElement _realDragSource;
        private Popup _popup;
        private FrameworkElement _tempNewPositionElement;
        private FrameworkElement _tempOldPositionElement;

        private DoubleAnimation _animation;
        private int positionIndex = -1;
        private DispatcherTimer _dispatcherTimer;
        private bool _expandedCompled;
        static DragableStack()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragableStack), new FrameworkPropertyMetadata(typeof(DragableStack)));
        }

        public DragableStack()
        {
            _popup = new Popup();
            _popup.AllowsTransparency = true;
            _popup.Placement=PlacementMode.Relative;
            _popup.PlacementTarget = this;
            Children.Add(_popup);
            MouseMove += DragableStack_MouseMove;
            _animation = new DoubleAnimation();
            _animation.Duration = ANIMATION_DURATION;
            _animation.Completed += Animation_Completed;

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = INTERVAL;
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
        }

        private int addedDummyIndex=-1;

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            _dispatcherTimer.Stop();
            if(addedDummyIndex == positionIndex) return;
            else
            {
                if (addedDummyIndex != -1)
                {
                    Children.RemoveAt(addedDummyIndex);
                    addedDummyIndex = -1;
                }
            }
            if (positionIndex==-1 || positionIndex== addedDummyIndex) return;
            addedDummyIndex = positionIndex;

            Children.Insert(positionIndex, _tempNewPositionElement = new Border()
            {
                Width = _realDragSource.Width,
                Height = 0
            });

            _animation.To = _realDragSource.Height;
            _tempNewPositionElement.BeginAnimation(HeightProperty, _animation);
            _expandedCompled = false;
        }

        private void DragableStack_MouseMove(object sender, MouseEventArgs e4)
        {

            Point point = Mouse.GetPosition(this);
            if (_isDown)
            {
                if ((_isDragging == false && ((Math.Abs(point.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(point.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance))))
                {
                    _isDragging = true;
                    _popup.IsOpen = true;
                    var index = this.Children.IndexOf(_realDragSource);
                    if (index == -1) return;
                    this.Children.RemoveAt(index);
                    addedDummyIndex = index;
                    Children.Insert(index, _tempOldPositionElement = new Border()
                    {
                        Width = _realDragSource.Width,
                        Height = _realDragSource.Height
                    });

                    _popup.Child = _realDragSource;
                }
            }

            if (!_isDragging) return;

            _popup.HorizontalOffset = point.X - _startPointOnDragSource.X;
            _popup.VerticalOffset = point.Y - _startPointOnDragSource.Y;
            int newPositionIndex = -1;
            for (int index = 0; index < Children.Count; index++)
            {
                // todo use simple class
                FrameworkElement child = (FrameworkElement) Children[index];
                var pointOnParent = child.TranslatePoint(new Point(), this);
                double yCenter = child.ActualHeight/2 + pointOnParent.Y;

                if (point.Y < yCenter)
                {
                    newPositionIndex = index;
                    break;
                }
                if (index == Children.Count - 1 && point.Y > yCenter)
                {
                    newPositionIndex = index;
                }
            }

            if (newPositionIndex == -1 || positionIndex == newPositionIndex) return;
            positionIndex = newPositionIndex;
            _dispatcherTimer.Start();
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.Source != this)
            {
                _realDragSource = e.Source as FrameworkElement;
                _isDown = true;
                _startPoint = e.GetPosition(this);
                _startPointOnDragSource = e.GetPosition(_realDragSource);
                this.CaptureMouse();
            }
            e.Handled = false;
            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (positionIndex != -1 && _isDragging)
            {
                if (addedDummyIndex != -1)
                {
                    Children.RemoveAt(addedDummyIndex);
                    addedDummyIndex = -1;
                }
                var temoObj = _popup.Child;
                _popup.Child = null;
                Children.Insert(positionIndex, temoObj);
                positionIndex = -1;
            }
            _isDown = false;
            _isDragging = false;
            _expandedCompled = false;
            this.ReleaseMouseCapture();
            base.OnPreviewMouseLeftButtonUp(e);
   
        }

        private void Animation_Completed(object sender, EventArgs e)
        {
            _expandedCompled = true;
        }
    }
}
