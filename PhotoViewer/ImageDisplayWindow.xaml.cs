using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace PhotoViewer
{
    /// <summary>
    /// Interaction logic for ImageDisplayWindow.xaml
    /// </summary>
    public partial class ImageDisplayWindow : Window, IDisposable
    {
        private bool disposed = false;

        public ImageDisplayWindow()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    imageDisplay.Source = null;
                }

                // Dispose unmanaged resources

                disposed = true;
            }
        }

        ~ImageDisplayWindow()
        {
            Dispose(false);
        }

        public void SetImageSource(BitmapSource bitmapSource)
        {
            imageDisplay.Source = bitmapSource;
        }

        private void OnGoBackButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
