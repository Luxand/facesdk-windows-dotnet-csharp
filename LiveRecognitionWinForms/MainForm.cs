using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Luxand;
using System.Diagnostics.PerformanceData;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.IO;
using System.Drawing.Drawing2D;

namespace LiveRecognition
{
    public partial class MainForm : Form
    {
        String cameraName;
        bool needClose = false;

        Luxand.Tracker tracker = null;
        String TrackerMemoryFile = "tracker90.dat";

        // Mouse coordinates in the pictureBox1 coordinate system
        int mouseX = 0;
        int mouseY = 0;

        // Mouse coordinates in the original image coordinate system
        int mouseImgX = 0;
        int mouseImgY = 0;

        // ID of the face selected by the user
        int selectedFaceId = -1;

        // WinAPI procedure to release HBITMAP handles returned by FSDKCam.GrabFrame
        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr hObject);

        // If the FaceSDK library is activated
        static bool isActivated = false;
        // Prefix of the liveness caption: "iBeta Liveness" once the iBeta plugin is initialized, otherwise the built-in "Liveness"
        string livenessLabel = "Liveness";

        // Store the current frame for drawing
        private Image currentFrame = null;

        public MainForm()
        {
            InitializeComponent();
            // Subscribe to Paint event
            pictureBox1.Paint += pictureBox1_Paint;
            // Stretch pictureBox1 to fill the entire window
            pictureBox1.Dock = DockStyle.Fill;
            // Allow the window to be freely resizable
            this.MinimumSize = new Size(640, 480);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!isActivated && FSDK.FSDKE_OK != FSDK.ActivateLibrary("INSERT THE LICENSE KEY HERE"))
            {
                MessageBox.Show("Please insert the license key in the FSDK.ActivateLibrary()", "Error activating FaceSDK", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            isActivated = true;

            FSDK.InitializeLibrary();
            FSDK.InitializeCapturing();

            string[] cameraList = FSDK.GetCameraList();

            if (cameraList.Length == 0) {
                MessageBox.Show("Please attach a camera", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            needClose = true;
        }

        private void CheckFaceBtn_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "image files (*.jpg;*.png)|*.jpg;*.png";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                //Get the path of specified file
                filePath = openFileDialog.FileName;
            }

            Luxand.CImage image;
            try
            {
                image = new Luxand.CImage(filePath);
            }
            catch (Exception ex2)
            {
                MessageBox.Show(ex2.Message, "Error opening image file.");
                return;
            }

            FSDK.TFace face;
            try
            {
                face = image.DetectFace();
            }
            catch
            {
                // No faces found in the image
                MessageBox.Show("No faces found", "The image does not contain any detectable faces.");
                return;
            }

            byte[] faceTemplate = new byte[FSDK.TemplateSize];
            try
            {
                faceTemplate = image.GetFaceTemplateInRegion(face);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error extracting face template", $"Could not extract face template: {ex.Message}");
                return;
            }

            float matchThreshold = 0.1f;
            FSDK.IDSimilarity[] id_similarity = tracker.MatchFaces(faceTemplate, matchThreshold);
            if (id_similarity == null || id_similarity.Length == 0)
            {
                MessageBox.Show("No matches found", $"No faces found above threshold {matchThreshold}");
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

            MessageBox.Show(res, "Face Recognition Results");
        }

        // Converts mouse coordinates from pictureBox1 to image coordinates
        private void UpdateMouseImageCoords()
        {
            if (currentFrame == null) { mouseImgX = 0; mouseImgY = 0; return; }
            int imgW = currentFrame.Width;
            int imgH = currentFrame.Height;
            int boxW = pictureBox1.Width;
            int boxH = pictureBox1.Height;
            float ratio = Math.Min((float)boxW / imgW, (float)boxH / imgH);
            int drawW = (int)(imgW * ratio);
            int drawH = (int)(imgH * ratio);
            int offsetX = (boxW - drawW) / 2;
            int offsetY = (boxH - drawH) / 2;
            // If the mouse is outside the image, coordinates are invalid
            if (mouseX < offsetX || mouseX >= offsetX + drawW || mouseY < offsetY || mouseY >= offsetY + drawH)
            {
                mouseImgX = -1;
                mouseImgY = -1;
            }
            else
            {
                mouseImgX = (int)((mouseX - offsetX) / ratio);
                mouseImgY = (int)((mouseY - offsetY) / ratio);
            }
        }

        private void StartCameraBtn_Click(object sender, EventArgs e)
        {
            this.StartBtn.Enabled = false;

            // Initialize camera
            Luxand.Camera camera = null;
            try
            {
                camera = new Luxand.Camera(cameraName);
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error opening camera");
                Application.Exit();
            }

            // Creating tracker
            try
            {
                tracker = Luxand.Tracker.LoadMemoryFromFile(TrackerMemoryFile);
            }
            catch
            {
                tracker = new Luxand.Tracker();
            }

            if (tracker == null)
            {
                MessageBox.Show("Error creating tracker", "Error");
                Application.Exit();
            }

            // set realtime face detection parameters
            tracker.SetMultipleParameters("FaceDetectionPatchSize=128; FaceDetectionThreshold=0.4;", out var errorPosition);

            tracker.SetParameter("DetectLiveness", "true");
            tracker.SetParameter("LivenessFramesCount", "1");
            tracker.SetParameter("SmoothAttributeLiveness", "false");

            // Initialize iBeta liveness addon
            int livenessModelResult = FSDK.SetParameter("LivenessModel", "external:dataDir=" + AppContext.BaseDirectory);
            livenessLabel = livenessModelResult == FSDK.FSDKE_OK ? "iBeta Liveness" : "Liveness";
            if (livenessModelResult != FSDK.FSDKE_OK)
            {
                string reason = livenessModelResult == FSDK.FSDKE_PLUGIN_NO_PERMISSION
                    ? "The license key does not permit the iBeta liveness plugin."
                    : "Check that the iBeta license is installed (INSTALL_LICENSE) and the plugin binaries and data are next to the application.";
                MessageBox.Show($"Error initializing iBeta liveness plugin ({livenessModelResult}).\n{reason}\nThe built-in liveness will be used instead.",
                    "iBeta liveness error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            this.CheckFaceBtn.Enabled = true;

            while (!needClose)
            {
                CImage frame = camera.GrabFrame();
                if (frame == null)
                {
                    Application.DoEvents();
                    continue;
                }

                Image frameImage = frame.ToCLRImage();

                // Process the frame with the tracker
                tracker.FeedFrame(frame, out long[] faceIds);

                // Make UI controls accessible (to find if the user clicked on a face)
                Application.DoEvents();

                if (faceIds != null)
                {
                    // Everything is drawn onto the full-resolution frame, which is then scaled
                    // down to fit pictureBox1, so the outline and the label are sized against
                    // the frame rather than in fixed pixels.
                    float strokeWidth = Math.Max(2.0f, frameImage.Height * 0.004f);
                    float fontSize = Math.Max(10.0f, frameImage.Height * 0.03f);

                    Graphics gr = Graphics.FromImage(frameImage);
                    gr.SmoothingMode = SmoothingMode.AntiAlias;

                    using (gr)
                    using (var labelFont = new System.Drawing.Font("Arial", fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (var format = new StringFormat { Alignment = StringAlignment.Center })
                    foreach (var faceId in faceIds)
                    {
                        // Get the face and create an ellipse for it
                        FSDK.TFace face;
                        try { face = tracker.GetFace(0, faceId); } catch { continue; }

                        // Load face name
                        string facename = "";
                        try { facename = tracker.GetName(faceId); } catch { }

                        float w = face.width;
                        float h = face.height;

                        int left = face.left;
                        int top = face.top;
                        int right = face.right;
                        int bottom = face.bottom;

                        // Get liveness
                        float liveness = 0.0f;
                        string livenessError = "";
                        string livenessText = "";

                        try
                        {
                            tracker.GetFacialAttribute(faceId, "Liveness", out string livenessAttribute);
                            if (livenessAttribute != null && livenessAttribute != "")
                            {
                                FSDK.GetValueConfidence(livenessAttribute, "Liveness", out liveness);
                            }
                            tracker.GetFacialAttribute(faceId, "LivenessError", out string livenessErrorString);
                            livenessError = livenessErrorString.Substring("LivenessError=".Length);
                            livenessError = livenessError.Remove(livenessError.Length - 2);
                        }
                        catch { }

                        Color penColor = Color.LightGreen;

                        if (livenessError != "")
                        {
                            // If there is an error, we cannot determine liveness
                            penColor = Color.Yellow;
                            livenessText = $"{livenessLabel}: {livenessError}";
                        }
                        else
                        {
                            if (liveness > 0.0f)
                            {
                                if (liveness < 0.5f)
                                {
                                    // If liveness is low, mark the face as suspicious
                                    penColor = Color.Red;
                                }
                                else
                                {
                                    livenessText = $"{livenessLabel}: {liveness * 100.0f:0.00}%";
                                }
                            }
                        }

                        string labelText = facename;
                        if (facename != "" && livenessText != "")
                        {
                            labelText += $" ({livenessText})";
                        }

                        selectedFaceId = -1;

                        // Check if the mouse is inside the face bounding box considering scaling
                        UpdateMouseImageCoords();

                        if (mouseImgX >= 0 && mouseImgY >= 0 &&
                            mouseImgX >= left && mouseImgX <= right &&
                            mouseImgY >= top && mouseImgY <= bottom)
                        {
                            // Highlight the face if the mouse is over it
                            penColor = Color.Blue;
                            selectedFaceId = (int)faceId;
                        }

                        using (var pen = new Pen(penColor, strokeWidth))
                            gr.DrawEllipse(pen, left, top, w, h);

                        // Draw name
                        if (labelText != "")
                        {
                            using (var brush = new SolidBrush(penColor))
                                gr.DrawString(labelText, labelFont, brush,
                                    face.center.x, bottom, format);
                        }
                    }
                }

                // Save the current frame and redraw pictureBox1
                if (currentFrame != null)
                {
                    var old = currentFrame;
                    currentFrame = null;
                    old.Dispose();
                }

                currentFrame = frameImage;
                pictureBox1.Invalidate();
                GC.Collect();
            }
       
            tracker.SaveMemoryToFile(TrackerMemoryFile);
            camera.Close();

            FSDK.FinalizeCapturing();
            FSDK.FinalizeLibrary();
        }

        // Draw the frame with aspect ratio preserved
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (currentFrame == null)
                return;

            var pb = pictureBox1;
            var g = e.Graphics;
            g.Clear(pb.BackColor);

            int imgW = currentFrame.Width;
            int imgH = currentFrame.Height;
            int boxW = pb.Width;
            int boxH = pb.Height;

            float ratio = Math.Min((float)boxW / imgW, (float)boxH / imgH);
            int drawW = (int)(imgW * ratio);
            int drawH = (int)(imgH * ratio);
            int offsetX = (boxW - drawW) / 2;
            int offsetY = (boxH - drawH) / 2;

            g.DrawImage(currentFrame, new Rectangle(offsetX, offsetY, drawW, drawH));
        }


        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (selectedFaceId >= 0)
            {
                // If the user clicked on a face, show the name input dialog
                InputName inputName = new InputName();
                if (DialogResult.OK == inputName.ShowDialog())
                {
                    string userName = inputName.userName;
                    tracker.SetName(selectedFaceId, userName);

                    if (userName == null || userName.Length <= 0)
                    {
                        tracker.PurgeID(selectedFaceId);
                    }
                }
                selectedFaceId = -1; // Reset selected face ID
            }
        }
        
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            mouseX = 0;
            mouseY = 0;
        }
    }
}
