using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.DataTypes
{
    public class ImageViewerTypes
    {
        public enum Arrows
        {
            LEFT,
            UP,
            RIGHT,
            DOWN
        }
        public enum Paging
        {
            NEXT,
            PREVIOUS
        }
        public enum Zooming
        {
            IN,
            OUT
        }
    }


    public static class PagingHelper
    {
        public static ImageViewerTypes.Paging NEXT => ImageViewerTypes.Paging.NEXT;
        public static ImageViewerTypes.Paging PREVIOUS => ImageViewerTypes.Paging.PREVIOUS;
    }
    public static class ZoomingHelper
    {
        public static ImageViewerTypes.Zooming IN => ImageViewerTypes.Zooming.IN;
        public static ImageViewerTypes.Zooming OUT => ImageViewerTypes.Zooming.OUT;
    }
    public static class ArrowsHelper
    {
        public static ImageViewerTypes.Arrows LEFT => ImageViewerTypes.Arrows.LEFT;
        public static ImageViewerTypes.Arrows UP => ImageViewerTypes.Arrows.UP;
        public static ImageViewerTypes.Arrows RIGHT => ImageViewerTypes.Arrows.RIGHT;
        public static ImageViewerTypes.Arrows DOWN => ImageViewerTypes.Arrows.DOWN;
    }
}
