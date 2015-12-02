using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
    public class DragableStack : StackPanel
    {
        private bool _isDown;
        private bool _isDragging;
        private Point _startPoint;
        private UIElement _realDragSource;
        private Popup _popup;
        private UIElement _dummyDragSource = new UIElement();
        private UIElement _dummyDragSource2 = new UIElement();
        static DragableStack()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragableStack), new FrameworkPropertyMetadata(typeof(DragableStack)));
        }

        public DragableStack()
        {
            //PreviewMouseLeftButtonDown += SpPreviewMouseLeftButtonDown;
            _popup = new Popup();
            _popup.Placement=PlacementMode.Relative;
            _popup.PlacementTarget = this;
            Children.Add(_popup);
            this.MouseMove += DragableStack_MouseMove;
            this.Drop += sp_Drop;
        }

        private void DragableStack_MouseMove(object sender, MouseEventArgs e)
        {
            Debug.Write("Mouse move" + _isDown);

            if (_isDown)
            {
                //&& ((Math.Abs(e.GetPosition(this).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                //    (Math.Abs(e.GetPosition(this).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
                if ((_isDragging == false) )
                {
                    Debug.Write("_isDragging true");
                    _isDragging = true;
                    _popup.IsOpen = true;
                 //   _realDragSource = e.Source as UIElement;
                
                    var index = this.Children.IndexOf(_realDragSource);
                    this.Children.RemoveAt(index);

/*                    Border border = new Border();
                    border.Background = Brushes.IndianRed;
                    border.Height = 100;
                    border.Width = 100;
                    _popup.Child = new Button() { Content = _realDragSource };*/
                    //this.Children.Insert(index, border);
                    DragDrop.DoDragDrop(_dummyDragSource, new DataObject("UIElement", _realDragSource, true), DragDropEffects.Move);
                    Mouse.SetCursor(Cursors.None);
                }
            }

            if (_isDragging)
            {
                var point = e.GetPosition(this);
                _popup.HorizontalOffset = point.X + 10;
                _popup.VerticalOffset = point.Y + 10;
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.Source != this)
            {
                _realDragSource = e.Source as UIElement;
                _isDown = true;
                _startPoint = e.GetPosition(this);
                Debug.Write("startPoint");
                this.CaptureMouse();
            }
            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            var pointCurrentPositionPointy = Mouse.GetPosition(this);
           var result =  VisualTreeHelper.HitTest(this, pointCurrentPositionPointy);
            if (result.VisualHit!=null)
            {
                
            }
            _isDown = false;
            _isDragging = false;
           this.ReleaseMouseCapture();
            base.OnPreviewMouseLeftButtonUp(e);

        }

        private void sp_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("UIElement"))
            {
                UIElement droptarget = e.Source as UIElement;
                int droptargetIndex = -1, i = 0;
                foreach (UIElement element in this.Children)
                {
                    if (element.Equals(droptarget))
                    {
                        droptargetIndex = i;
                        break;
                    }
                    i++;
                }
                if (droptargetIndex != -1)
                {
                    this.Children.Remove(_realDragSource);
                    this.Children.Insert(droptargetIndex, _realDragSource);
                }

                _isDown = false;
                _isDragging = false;
                _realDragSource.ReleaseMouseCapture();
            }
        }
    }
}
