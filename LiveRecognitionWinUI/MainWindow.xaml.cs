using Luxand;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LiveRecognitionWinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        // Camera
        String cameraName = "";
        Luxand.Camera? camera;

        // Tracker
        Luxand.Tracker? tracker;

        // Tracker memory file
        String TrackerMemoryFile = System.IO.Path.Combine(AppContext.BaseDirectory, "tracker_memory.dat");

        // Worker for frames processing
        BackgroundWorker? worker;

        // Current mouse position
        Windows.Foundation.Point? mousePosition;

        // Currently selected face ID
        int selectedFace = -1;

        // If the application is closed
        bool isClosed = false;

        // If the FaceSDK library is activated
        static bool isActivated = false;

        // Person structure to hold name textblock and face ellipse
        public struct Person
        {
            public int faceId;
            public Ellipse? ellipse;
            public TextBlock? nameTextBlock;
            public Person()
            { 
                faceId = -1;
                ellipse = null;
                nameTextBlock = null;
            }
        }

        // Person list to hold detected faces
        public Dictionary<long, Person> persons = new Dictionary<long, Person>();

        public MainWindow()
        {
            InitializeComponent();

            // Disable the button while camera is not started
            this.CheckImageBtn.IsEnabled = false;

            if (worker != null)
                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        private void ShowErrorMessage(string title, string message, bool exitOnClick = true)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot,
            };

            if (exitOnClick)
            {
                dialog.CloseButtonClick += (s, e) =>
                {
                    App.Current.Exit();
                };
            }

            _ = dialog.ShowAsync();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize camera or other resources here if needed
            if (!isActivated && FSDK.FSDKE_OK != FSDK.ActivateLibrary("Insert the License Key here"))
            {
                ShowErrorMessage("FaceSDK activation error.", "Error activating FaceSDK library. Check the license key.");
                return;
            }
            isActivated = true;

            FSDK.InitializeLibrary();
            FSDK.InitializeCapturing();

            string[] cameraList = FSDK.GetCameraList();
            if (cameraList.Length == 0)
            {
                // No cameras found, show an error message and exit
                ShowErrorMessage("No cameras found", "Please attach a camera.");
                return;
            }

            cameraName = cameraList[0];
            Luxand.FSDK.VideoFormatInfo[] formatList = Luxand.Camera.GetVideoFormatList(cameraName);

            // Choose the best camera from the list
            int index = 0;
            int formatIndex = 0;
            int max_height = 0, max_width = 0;
            foreach (var format in formatList)
            {
                if (format.Width > max_width && format.Height > max_height)
                {
                    max_width = format.Width;
                    max_height = format.Height;
                    formatIndex = index;
                }
                index++;
            }

            Luxand.Camera.SetVideoFormat(cameraName, formatList[formatIndex]);
        }

        private void StartCamera(object sender, RoutedEventArgs e)
        {
            // Creating Camera object
            try
            {
                camera = new Luxand.Camera(cameraName);
            } catch (Exception ex)
            {
                ShowErrorMessage("Camera error", $"Error opening camera: {ex.Message}");
                return;
            }

            // Creating Tracker object
            try
            {
                tracker = Luxand.Tracker.LoadMemoryFromFile(TrackerMemoryFile);
            }
            catch (Exception)
            {
                tracker = new Luxand.Tracker();
            }

            // set realtime face detection parameters
            tracker.SetParameter("DetectionVersion", "2");
            tracker.SetMultipleParameters("FaceDetection2PatchSize=256; Threshold=0.8; Threshold2=0.9; SmoothAttributeLiveness=false; LivenessFramesCount=1; DetectLiveness=true", out var errorPosition);

            this.StartCameraBtn.IsEnabled = false;

            // Initialize iBeta liveness addon
            if (FSDK.SetParameter("LivenessModel", "external:dataDir=" + AppContext.BaseDirectory) != FSDK.FSDKE_OK)
            {
                ShowErrorMessage("IBeta Liveness error", "Error initializing IBeta Liveness plugin");
            }

            // Enable the button to check image
            this.CheckImageBtn.IsEnabled = true;

            // Creating and running background worker to process frames
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = false;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(ProcessFrames);
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        public void DrawFrame(CImage frame)
        {
            if (frame == null) return;

            byte[] imageBuffer = frame.SaveImageToBuffer(FSDK.FSDK_IMAGEMODE.FSDK_IMAGE_COLOR_32BIT);

            if (imageBuffer == null || imageBuffer.Length == 0)
            {
                return; // Skip if the image buffer is empty
            }

            for (int i = 0; i < imageBuffer.Length; i += 4)
            {
                // Convert BGRA to RGBA
                byte temp = imageBuffer[i];
                imageBuffer[i] = imageBuffer[i + 2];
                imageBuffer[i + 2] = temp;
            }

            this.DispatcherQueue.TryEnqueue(() =>
            {
                WriteableBitmap bitmap = new WriteableBitmap(frame.Width, frame.Height);
                bitmap.PixelBuffer.AsStream().Write(imageBuffer);
                this.CameraImage.Source = bitmap;
            });
        }

        public void ProcessPersonsAndDrawFrame()
        {
            if (camera == null || tracker == null) { return; }

            CImage frame = camera.GrabFrame();
            if (frame == null) { return; }

            // Drawing the current frame
            DrawFrame(frame);

            // Process the frame with the tracker
            tracker.FeedFrame(frame, out long[] faceIds);

            this.DispatcherQueue.TryEnqueue(() =>
            {
                // Remove faces that are no longer detected
                foreach (var p in persons)
                {
                    var person = p.Value;
                    if (faceIds == null || faceIds.Length == 0 || !faceIds.Contains(person.faceId))
                    {
                        this.canvas.Children.Remove(person.ellipse);
                        this.canvas.Children.Remove(person.nameTextBlock);
                    }
                }

                if (faceIds == null || tracker == null) { return; }

                // Getting video and image dimensions
                double videoWidth = frame.Width;
                double videoHeight = frame.Height;
                double imageWidth = this.CameraImage.ActualWidth;
                double imageHeight = this.CameraImage.ActualHeight;

                // Calculate scale and offsets (letterbox/pillarbox)
                double scale = Math.Min(imageWidth / videoWidth, imageHeight / videoHeight);
                double offsetX = (imageWidth - videoWidth * scale) / 2.0;
                double offsetY = (imageHeight - videoHeight * scale) / 2.0;

                // Setting Canvas size to match Image size
                this.canvas.Width = imageWidth;
                this.canvas.Height = imageHeight;

                // Setting up brushes for different colors
                var green = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 255, 0));
                var blue = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 255));
                var red = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0));
                var yellow = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 0));

                foreach (var faceId in faceIds)
                {
                    // Get the face position and create an ellipse for it
                    FSDK.TFace face;
                    try { face = tracker.GetFace(0, faceId); } catch { continue; }

                    // Load face name
                    string facename = "";
                    try { facename = tracker.GetName(faceId); } catch { }

                    var center = face.center();

                    // Get liveness
                    float liveness = 0.0f;
                    string livenessError = "";
                    string livenessText = "";

                    try {
                        tracker.GetFacialAttribute(faceId, "Liveness", out string livenessAttribute);
                        if (livenessAttribute != null && livenessAttribute != "")
                        {
                            FSDK.GetValueConfidence(livenessAttribute, "Liveness", out liveness);
                        }
                        tracker.GetFacialAttribute(faceId, "LivenessError", out string livenessErrorString);
                        livenessError = livenessErrorString.Substring("LivenessError=".Length);
                        livenessError = livenessError.Remove(livenessError.Length - 2);
                    } catch { }

                    // Check if the face is already tracked, update its position if it is
                    Person person = new();
                    if (persons.ContainsKey(faceId))
                    {
                        person = persons[faceId];
                    }

                    Ellipse? ellipse = person.ellipse;
                    TextBlock? nameTextBlock = person.nameTextBlock;

                    double w = face.width();
                    double h = face.height();
                    double left = center.x - w / 2.0;
                    double top = center.y - h / 2.0;

                    // Converting coordinates and sizes to Canvas coordinates
                    double scaledLeft = left * scale + offsetX;
                    double scaledTop = top * scale + offsetY;
                    double scaledW = w * scale;
                    double scaledH = h * scale;

                    selectedFace = -1; // Reset selected face

                    var color = green;
                    if (livenessError != "")
                    {
                        color = yellow;
                        livenessText = $"Liveness: {livenessError}";
                    }
                    else
                    {
                        if (liveness > 0.0f)
                        {
                            if (liveness < 0.5f)
                            {
                                color = red;
                            }
                            else
                            {
                                livenessText = $"Liveness: {liveness * 100.0f:0.00}%";
                            }
                        }
                    }

                    string labelText = facename;
                    if (facename != "" && livenessText != "")
                    {
                        labelText += $" ({livenessText})";
                    }

                    // Correcting mousePosition for scaling
                    if (mousePosition != null)
                    {
                        double mx = mousePosition.Value.X;
                        double my = mousePosition.Value.Y;
                        if (mx >= scaledLeft && mx <= scaledLeft + scaledW &&
                            my >= scaledTop && my <= scaledTop + scaledH)
                        {
                            color = blue;
                            selectedFace = (int)faceId;
                        }
                    }

                    if (person.faceId == -1)
                    {
                        ellipse = new Ellipse
                        {
                            Width = scaledW,
                            Height = scaledH,
                            Stroke = color,
                            StrokeThickness = 2,
                            Fill = null
                        };

                        nameTextBlock = new TextBlock
                        {
                            Text = labelText,
                            Width = scaledW * 4,
                            Height = 20,
                            FontSize = 14,
                            Foreground = color,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Top
                        };
                        persons[faceId] = new Person
                        {
                            faceId = (int)faceId,
                            ellipse = ellipse,
                            nameTextBlock = nameTextBlock
                        };
                    }

                    if (ellipse != null)
                    {
                        if (this.canvas.Children.Contains(ellipse) == false)
                        {
                            this.canvas.Children.Add(ellipse);
                        }
                        ellipse.Width = scaledW;
                        ellipse.Height = scaledH;
                        ellipse.Stroke = color;
                        Canvas.SetLeft(ellipse, scaledLeft);
                        Canvas.SetTop(ellipse, scaledTop);
                    }

                    if (nameTextBlock != null)
                    {
                        if (this.canvas.Children.Contains(nameTextBlock) == false)
                        {
                            this.canvas.Children.Add(nameTextBlock);
                        }
                        nameTextBlock.Text = labelText;
                        nameTextBlock.Foreground = color;
                        Canvas.SetLeft(nameTextBlock, scaledLeft);
                        Canvas.SetTop(nameTextBlock, scaledTop + scaledH);
                    }
                }
            });
        }

        public void ProcessFrames(object? sender, DoWorkEventArgs e)
        {
            if (camera == null)
            {
                ShowErrorMessage("Error", "Camera object is not initialized");
                return;
            }

            if (tracker == null)
            {
                ShowErrorMessage("Error", "Tracker object is not initialized");
                return;
            }

            while (!isClosed)
            {
                ProcessPersonsAndDrawFrame();
            }
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            isClosed = true;

            if (worker != null && worker.IsBusy)
            {
                worker.CancelAsync();
            }
            else
            {
                CleanupResources();
            }
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            CleanupResources();
        }

        private void CleanupResources()
        {
            if (tracker != null)
            {
                tracker.SaveMemoryToFile(TrackerMemoryFile);
                tracker.Dispose();
                tracker = null;
            }

            if (camera != null)
            {
                camera.Close();
                camera.Dispose();
                camera = null;
            }

            if (worker != null)
            {
                worker.Dispose();
                worker = null;
            }

            FSDK.FinalizeCapturing();
            FSDK.FinalizeLibrary();
        }

        private void CameraImage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (selectedFace != -1 && persons.ContainsKey(selectedFace))
            {
                var person = persons[selectedFace];
                if (person.nameTextBlock != null)
                {
                    string name = "";
                    if (tracker != null)
                    {
                        try { name = tracker.GetName(selectedFace); } catch { }
                    }
                    // Show a dialog to enter the person's name
                    var dialog = new ContentDialog
                    {
                        Title = "Enter Name",
                        Content = new TextBox { Text = name, Width = 200 },
                        PrimaryButtonText = "OK",
                        CloseButtonText = "Cancel",
                        XamlRoot = this.Content.XamlRoot,
                    };
                    dialog.PrimaryButtonClick += (s, args) =>
                    {
                        var textBox = (TextBox)dialog.Content;

                        // Upate the tracker with the new name
                        if (tracker != null) { 
                            tracker.SetName(selectedFace, textBox.Text);
                            if (textBox.Text == "") 
                            {
                                tracker.PurgeID(selectedFace);
                            }
                        }
                    };
                    _ = dialog.ShowAsync();
                }
            }
        }

        private void CameraImage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            mousePosition = e.GetCurrentPoint(this.CameraImage).Position;
        }

        private async void CheckImageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (tracker == null)
            {
                return; // Tracker is not initialized
            }

            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            // Initializing the picker with the current window handle
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            StorageFile file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return; // User cancelled the picker
            }

            string filePath = file.Path;

            CImage image;
            try
            {
                image = new CImage(filePath);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error opening image file", $"Could not open image file: {ex.Message}");
                return;
            }

            FSDK.TFace face = image.DetectFace2();
            if (face.empty())
            {
                // No faces found in the image
                ShowErrorMessage("No faces found", "The image does not contain any detectable faces.");
                return;
            }

            byte[] faceTemplate = new byte[FSDK.TemplateSize];
            try
            {
                faceTemplate = image.GetFaceTemplateInRegion2(face);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error extracting face template", $"Could not extract face template: {ex.Message}");
                return;
            }

            float matchThreshold = 0.1f;
            FSDK.IDSimilarity[] id_similarity = tracker.MatchFaces(ref faceTemplate, matchThreshold);
            if (id_similarity == null || id_similarity.Length == 0)
            {
                ShowErrorMessage("No matches found", $"No faces found above threshold {matchThreshold}");
                return;
            }

            string res = "The possible person(s):\n";
            foreach (var idSim in id_similarity)
            {
                res += $"\nID: {idSim.ID}";
                string name = "";
                try
                {
                    name = tracker.GetName(idSim.ID);
                }
                catch { }
                if (name.Length != 0)
                    res += $" ({name})";
                res += $"; Similarity: {idSim.Similarity}";
            }

            // Show the results in a message box
            var dialog = new ContentDialog
            {
                Title = "Face Recognition Results",
                Content = res,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot,
            };
            await dialog.ShowAsync();
        }
    }
}
