using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace DDCInteligence.Utilities.Adorners
{
    public class RubberBandAdorner : Adorner
    {
        public RubberBandAdorner(UIElement element) : base(element) { }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var adornedElementRect = new Rect(AdornedElement.RenderSize);
            var rectBrush = new SolidColorBrush(Colors.Green);
            var renderPen = new Pen(rectBrush, 2);

            adornedElementRect.Inflate(1, 1);
            drawingContext.DrawRoundedRectangle(null, renderPen, adornedElementRect, 4, 4);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }
    }
}
