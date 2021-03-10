using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WaveLineDemo2
{
    class NewLineElement : FrameworkElement
    {
        public NewLineElement()
        {
            _visualShape = new VisualCollection(this);
        }
        
        internal void DrawLine(Point startPoint, Point endPoint)
        {
            var p1 = startPoint;
            var p2 = endPoint;

            var distance = (p1 - p2).Length;

            var angle = CalculateAngle(p1, p2);
            var waveLength = WaveLength;
            var waveHeight = WaveHeight;
            var howManyWaves = distance / waveLength;
            //var waveInterval = distance / howManyWaves;
            var waveInterval = waveLength;
            var maxBcpLength =
                Math.Sqrt(waveInterval / 4.0 * (waveInterval / 4.0) + waveHeight / 2.0 * (waveHeight / 2.0));

            var curveSquaring = CurveSquaring;
            var bcpLength = maxBcpLength * curveSquaring;
            var bcpInclination = CalculateAngle(new Point(0, 0), new Point(waveInterval / 4.0, waveHeight / 2.0));

            var wigglePoints = new List<(Point bcpOut, Point bcpIn, Point anchor)>();
            //添加起始点
            wigglePoints.Add((new Point(), new Point(), p1));

            //添加一系列数据集
            var prevFlexPt = p1;
            var polarity = 1;
            for (var waveIndex = 0; waveIndex < howManyWaves * 2; waveIndex++)
            {
                var bcpOutAngle = angle + bcpInclination * polarity;
                var bcpOut = new Point(prevFlexPt.X + Math.Cos(bcpOutAngle) * bcpLength,
                    prevFlexPt.Y + Math.Sin(bcpOutAngle) * bcpLength);
                var flexPt = new Point(prevFlexPt.X + Math.Cos(angle) * waveInterval / 2.0,
                    prevFlexPt.Y + Math.Sin(angle) * waveInterval / 2.0);
                var bcpInAngle = angle + (Math.PI - bcpInclination) * polarity;
                var bcpIn = new Point(flexPt.X + Math.Cos(bcpInAngle) * bcpLength,
                    flexPt.Y + Math.Sin(bcpInAngle) * bcpLength);

                wigglePoints.Add((bcpOut, bcpIn, flexPt));

                polarity *= -1;
                prevFlexPt = flexPt;
            }
            //纠正尾头过长/过短问题
            if (wigglePoints.Count > 1)
            {
                var wigglePoint = wigglePoints[wigglePoints.Count - 1];
                if (wigglePoint.bcpOut.X > endPoint.X)
                {
                    wigglePoints.Remove(wigglePoint);
                    wigglePoints.Add((p2, p2, p2));
                }
                else if (wigglePoint.bcpIn.X > endPoint.X)
                {
                    wigglePoints.Remove(wigglePoint);
                    wigglePoints.Add((wigglePoint.bcpOut, p2, p2));
                }
                else if (wigglePoint.anchor.X > endPoint.X)
                {
                    wigglePoints.Remove(wigglePoint);
                    wigglePoints.Add((wigglePoint.bcpOut, wigglePoint.bcpIn, p2));
                }
            }

            var streamGeometry = new StreamGeometry();
            using (var streamGeometryContext = streamGeometry.Open())
            {
                streamGeometryContext.BeginFigure(wigglePoints[0].anchor, true, false);

                for (var i = 1; i < wigglePoints.Count; i += 1)
                {
                    var (bcpOut, bcpIn, anchor) = wigglePoints[i];

                    streamGeometryContext.BezierTo(bcpOut, bcpIn, anchor, true, false);
                }
            }

            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawGeometry(FillBrush, StrokePen, streamGeometry);
            }
            _visualShape.Add(visual);
        }

        private static double CalculateAngle(Point p1, Point p2)
        {
            return Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
        }
        /// <summary>线条宽度</summary>
        public double BorderThickness { get; set; } = 1.5;

        public double WaveLength { get; set; } = 12.0;

        public double WaveHeight { get; set; } = 10.0;

        public double CurveSquaring { get; set; } = 0.75;
        

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
