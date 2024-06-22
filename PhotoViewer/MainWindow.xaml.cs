using AForge.Video;
using AForge.Video.DirectShow;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;

        private ObservableCollection<CapturedImage> capturedImages = new ObservableCollection<CapturedImage>();
        private ObservableCollection<FileInfo> capturedItems = new ObservableCollection<FileInfo>();
        public MainWindow()
        {
            InitializeComponent();

            capturedItems = new ObservableCollection<FileInfo>();
            lsvFile.ItemsSource = capturedItems;

            CheckFolderPath();

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void CheckFolderPath()
        {
            if (txtFolderPath.Text.Trim().Length > 0)
            {
                btnControlWebCam.IsEnabled = true;
            }
            else
            {
                btnControlWebCam.IsEnabled = false;
                btnCapture.IsEnabled = false;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
        }


        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog
            {
                Title = "Select Folder",
                InitialDirectory = "D:\\"
            };

            if (folderDialog.ShowDialog() == true)
            {
                var selectedFolder = folderDialog.FolderName;
                txtFolderPath.Text = selectedFolder;
                MessageBox.Show($"You choose {txtFolderPath.Text}!", "Select folder", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshListView();
                CheckFolderPath();
            }

        }

        private void btnControlWebCam_Click(object sender, RoutedEventArgs e)
        {
            if (videoSource == null && videoDevices.Count > 0)
            {
                webcamImage.Visibility = Visibility.Visible;
                btnCapture.IsEnabled = true;
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.NewFrame += VideoSource_NewFrame;
                videoSource.Start();
                btnControlWebCam.Content = "Stop Webcam";
            }
            else if (videoSource != null)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
                videoSource = null;
                btnCapture.IsEnabled = false;
                webcamImage.Visibility = Visibility.Hidden;
                btnControlWebCam.Content = "Start Webcam";
            }
        }

        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                BitmapSource bitmapSource = (BitmapSource)webcamImage.Source;

                if (bitmapSource != null)
                {
                    // Convert BitmapSource to Bitmap
                    Bitmap bitmap = BitmapFromSource(bitmapSource);

                    // Save the captured image to the selected folder
                    string selectedFolder = txtFolderPath.Text;
                    if (!string.IsNullOrWhiteSpace(selectedFolder))
                    {
                        string fileName = $"CapturedImage_{DateTime.Now:yyyyMMddHHmmss}.png";
                        string filePath = System.IO.Path.Combine(selectedFolder, fileName);

                        bitmap.Save(filePath, ImageFormat.Png);

                        CapturedImage capturedImage = new CapturedImage
                        {
                            Type = "📄",
                            Name = fileName,
                            Path = filePath,
                            Image = bitmapSource
                        };

                        // Add the captured image to the list
                        capturedImages.Add(capturedImage);

                        // Update the ListView
                        RefreshListView();

                        MessageBox.Show($"Image captured and saved at:\n{filePath}", "Capture Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Please select a folder to save the image.", "Capture Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("No image to capture.", "Capture Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Webcam is not running.", "Capture Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            webcamImage.Dispatcher.Invoke(() =>
            {
                // Convert AForge VideoFrame to Bitmap
                Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
                webcamImage.Source = BitmapToImageSource(bitmap);
            });
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Bmp);
                memoryStream.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }
        private BitmapSource LoadImage(string imagePath)
        {
            // Implement the logic to load the image and return the BitmapSource
            // For example:
            BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));
            return bitmapImage;
        }


        private void RefreshListView()
        {
            string selectedFolder = txtFolderPath.Text;

            if (!string.IsNullOrEmpty(selectedFolder) && Directory.Exists(selectedFolder))
            {
                try
                {
                    // Get all files and directories in the selected folder
                    string[] items = Directory.GetFileSystemEntries(selectedFolder);

                    // Clear the capturedItems collection
                    capturedItems.Clear();

                    foreach (string itemPath in items)
                    {
                        // If it's an image file, add it to the capturedItems list
                        if (IsImageFile(itemPath))
                        {
                            BitmapSource imageSource = LoadImage(itemPath);
                            capturedItems.Add(new CapturedImage
                            {
                                Type = "📄",
                                Name = System.IO.Path.GetFileName(itemPath),
                                Path = itemPath,
                                Image = imageSource
                            });
                        }
                        else
                        {
                            capturedItems.Add(new FileInfo
                            {
                                Type = (Directory.Exists(itemPath)) ? "📂" : "📄",
                                Name = System.IO.Path.GetFileName(itemPath),
                                Path = itemPath
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while retrieving files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool IsImageFile(string filePath)
        {
            // Add more image file extensions as needed
            string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };
            string extension = System.IO.Path.GetExtension(filePath).ToLower();
            return imageExtensions.Contains(extension);
        }

        private void lsvFile_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lsvFile.SelectedItem != null)
            {
                FileInfo selectedFile = (FileInfo)lsvFile.SelectedItem;

                if (selectedFile.Type == "📂")
                {
                    txtFolderPath.Text = selectedFile.Path;

                    // Refresh the ListView for the selected folder
                    RefreshListView();
                }
                else if (selectedFile.Type == "📄")
                {
                    if (IsImageFile(selectedFile.Path))
                    {
                        // Display the image in a new window
                        DisplayImage(selectedFile.Path);
                    }
                    else
                    {
                        // Open the file with the default associated application
                        System.Diagnostics.Process.Start("explorer.exe", selectedFile.Path);
                    }
                }
            }
        }

        private void DisplayImage(string imagePath)
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));

            ImageDisplayWindow imageDisplayWindow = new ImageDisplayWindow();
            imageDisplayWindow.SetImageSource(bitmapImage);
            imageDisplayWindow.Show();
        }
    }
}