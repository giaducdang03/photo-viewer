using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoViewer
{
    public class CapturedImage : FileInfo
    {
        public BitmapSource Image { get; set; }
    }
}
