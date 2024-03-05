using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Image = Amazon.Rekognition.Model.Image;

namespace Rekognition_Emotion
{
    public partial class Form1 : Form
    {
        private AmazonRekognitionClient rekognitionClient;
        public Form1()
        {
            InitializeComponent();
            rekognitionClient = new AmazonRekognitionClient(Amazon.RegionEndpoint.EUWest1);
        }
        private void btnSelectImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG files (*.png)|*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox.ImageLocation = openFileDialog.FileName;
                AnalyzeEmotions(openFileDialog.FileName);
            }
        }

        private void btnLoadFromUrl_Click(object sender, EventArgs e)
        {
            string imageUrl = txtImageUrl.Text;
            using (WebClient webClient = new WebClient())
            {
                byte[] imageBytes = webClient.DownloadData(imageUrl);
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    pictureBox.Image = System.Drawing.Image.FromStream(ms);
                }

                AnalyzeEmotions(imageBytes);
            }
        }

        private void AnalyzeEmotions(string imagePath)
        {
            using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                byte[] imageBytes = new byte[fs.Length];
                fs.Read(imageBytes, 0, (int)fs.Length);
                AnalyzeEmotions(imageBytes);
            }
        }

        private void AnalyzeEmotions(byte[] imageBytes)
        {
            DetectFacesRequest detectFacesRequest = new DetectFacesRequest()
            {
                Image = new Image() { Bytes = new MemoryStream(imageBytes) },
                Attributes = new List<string> { "ALL" }
            };

            try
            {
                DetectFacesResponse detectFacesResponse = rekognitionClient.DetectFaces(detectFacesRequest);
                foreach (FaceDetail face in detectFacesResponse.FaceDetails)
                {
                    if (face.Emotions.Count > 0)
                    {
                        var dominantEmotion = face.Emotions.OrderByDescending(e => e.Confidence).FirstOrDefault();

                        if (dominantEmotion != null)
                        {
                            lblResults.Invoke(new Action(() => {
                                lblResults.Text = $"{dominantEmotion.Type} : {dominantEmotion.Confidence:F2}%";
                            }));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error detecting faces: {ex.Message}");
            }
        }
    }
}
