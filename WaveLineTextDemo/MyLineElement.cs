using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WaveLineTextDemo
{
    class MyLineElement : FrameworkElement
    {
        public MyLineElement()
        {
            _visualShape = new VisualCollection(this);
        }
        internal void DrawLine(Point startPoint, Point endPoint)
        {
            List<Point> points = ForgePoints(startPoint, endPoint);
            DrawLine(points);
        }
        private const int SeparatorPiexl = 4;
        private const int AbundancePiexl = 3;
        private List<Point> ForgePoints(Point startPoint, Point endPoint)
        {
            var points = new List<Point>();

            var lineVector = endPoint - startPoint;
            var lineDistance = lineVector.Length;
            var lineAngle = Math.Atan2(-(endPoint.Y - startPoint.Y), endPoint.X - startPoint.X);

            points.Add(startPoint);
            int index = 0;
            bool isAbundanceUpward = true;
            while (index * SeparatorPiexl < lineDistance)
            {
                index++;
                //计算出间隔长度（模拟点到起始点）
                var separatorDistance = index * SeparatorPiexl;
                var abundancePiexl = AbundancePiexl;
                var distanceToStartPoint = Math.Sqrt(Math.Pow(separatorDistance, 2) + Math.Pow(abundancePiexl, 2));
                //计算出模拟点、起始点，与直线的角度
                var separatorAngle = Math.Atan2(AbundancePiexl, separatorDistance);
                separatorAngle = isAbundanceUpward ? separatorAngle : -separatorAngle;
                isAbundanceUpward = !isAbundanceUpward;
                //得到模拟点的水平角度
                var mockPointAngle = lineAngle + separatorAngle;
                //计算出模拟点坐标
                var verticalDistance = distanceToStartPoint * Math.Sin(mockPointAngle);
                var horizontalDistance = distanceToStartPoint * Math.Cos(mockPointAngle);
                var mockPoint = new Point(startPoint.X + horizontalDistance, startPoint.Y - verticalDistance);
                points.Add(mockPoint);
            }
            points.Add(endPoint);
            return points;
        }

        private void DrawLine(List<Point> points)
        {
            _visualShape.Clear();

            var geometryTest = new StreamGeometry();
            using (var ctx = geometryTest.Open())
            {
                ctx.BeginFigure(points[0], true, false);
                if (points.Count % 2 == 0)
                {
                    //绘制二阶贝塞尔函数，需要保证为偶数点
                    ctx.PolyQuadraticBezierTo(points, true, true);
                }
                else
                {
                    //绘制二阶贝塞尔函数，需要保证为偶数点
                    points.Insert(0, points[0]);
                    ctx.PolyQuadraticBezierTo(points, true, true);
                }

                ctx.Close();
            }

            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawGeometry(FillBrush, StrokePen, geometryTest);
            }
            _visualShape.Add(visual);
        }

        #region 内部方法

        [Obsolete]
        protected override void OnRender(DrawingContext drawingContext)
        {
            //弃用，改为_visualShape填充实现
            //drawingContext.DrawGeometry(FillBrush, StrokePen, BaseGeometry);
        }

        protected override int VisualChildrenCount => _visualShape.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _visualShape.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _visualShape[index];
        }

        #endregion

        #region 曲线属性

        private readonly VisualCollection _visualShape;
        protected Brush FillBrush { get; set; } = Brushes.Transparent;
        public Brush LineBrush { get; set; } = Brushes.DarkSeaGreen;
        protected double BorderThickness { get; set; } = 1.0;
        private Pen _defaultPen = null;
        protected Pen StrokePen
        {
            get
            {
                if (_defaultPen == null)
                {
                    _defaultPen = new Pen(LineBrush, BorderThickness);
                }
                return _defaultPen;
            }
            set => _defaultPen = value;
        }

        #endregion
    }
}
