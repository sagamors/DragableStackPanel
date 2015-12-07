using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;

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
        private bool _isDown;
        private bool _isDragging;
        private Point _startPoint;
        private Point _startPointOnDragSource;
        private FrameworkElement _realDragSource;
        private Popup _popup;
        private FrameworkElement _tempElement;
        private DoubleAnimation _animation;
        private static int positionIndex = -1;

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
        }

        private void DragableStack_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDown)
            {
                if ((_isDragging == false && ((Math.Abs(e.GetPosition(this).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(e.GetPosition(this).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance))))
                {
                    _isDragging = true;
                    _popup.IsOpen = true;
                    var index = this.Children.IndexOf(_realDragSource);
                    if (index == -1) return;
                    this.Children.RemoveAt(index);
                    _popup.Child = _realDragSource;
                }
            }

            if (!_isDragging) return;

            var point = e.GetPosition(this);

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

            if (positionIndex != -1)
            {
                Children.RemoveAt(positionIndex);
            }
     
            Children.Insert(newPositionIndex, _tempElement = new Border()
            {
                Width = _realDragSource.Width,
                Height = 0
            });
            _animation.To = _realDragSource.Height;
            _tempElement.BeginAnimation(HeightProperty, _animation);
            positionIndex = newPositionIndex;
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
           // base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (positionIndex != -1 && _isDragging)
            {
                Children.RemoveAt(positionIndex);
                var temoObj = _popup.Child;
                _popup.Child = null;
                Children.Insert(positionIndex, temoObj);
                positionIndex = -1;
            }
            _isDown = false;
            _isDragging = false;
            this.ReleaseMouseCapture();
            base.OnPreviewMouseLeftButtonUp(e);
        }

        private void Animation_Completed(object sender, EventArgs e)
        {
          
        }
    }
}
