using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WaveLineTextDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Point? _startPoint = null;
        private void ContainerCanvas_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(ContainerCanvas);
            if (_startPoint == null)
            {
                _startPoint = position;
            }
            else
            {
                //删除预览
                if (_previewLineElement != null)
                {
                    ContainerCanvas.Children.Remove(_previewLineElement);
                    _previewLineElement = null;
                    _lastMovedPoint = null;
                }
                //确定结束点，绘制波浪线
                var myLineElement = new MyLineElement();
                myLineElement.DrawLine((Point)_startPoint, position);
                ContainerCanvas.Children.Add(myLineElement);
                _startPoint = null;
            }
        }

        private MyLineElement _previewLineElement = null;
        private Point? _lastMovedPoint = null;

        /// <summary>
        /// 波浪线绘制预览
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContainerCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(ContainerCanvas);
            if (_startPoint != null && (_lastMovedPoint == null || _lastMovedPoint != null & (position - (Point)_lastMovedPoint).Length >= 2))
            {
                _lastMovedPoint = position;
                if (_previewLineElement != null)
                {
                    ContainerCanvas.Children.Remove(_previewLineElement);
                }
                var myLineElement = new MyLineElement();
                myLineElement.DrawLine((Point)_startPoint, position);
                ContainerCanvas.Children.Add(myLineElement);
                _previewLineElement = myLineElement;
            }
        }
    }
}
