using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Image = System.Drawing.Image;

public class ImageConverter
{
    public ImageConverter() { }

    #region Image Converter

    public bool NeedRoundCorners = true;
    public bool SaveNewImg(string url, string filePath, int width = 64, int height = 64) {
        if (url.Substring(0, 4) != "http") {
            url = "http://" + url;
        }

        bool cancontinue = false;
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        var httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse();
        Stream stream = httpWebReponse.GetResponseStream();
        if (stream != null) {
            Image imgurl = Image.FromStream(stream, false, true);
            if (HelperMethods.IsImage(httpWebReponse)) {
                if ((imgurl.Size.Height > height) || (imgurl.Size.Width > width)) {
                    Bitmap newImage = new Bitmap(width, height);
                    newImage.SetResolution(imgurl.HorizontalResolution, imgurl.VerticalResolution);

                    using (Graphics gr = Graphics.FromImage(newImage)) {
                        gr.SmoothingMode = SmoothingMode.HighQuality;
                        gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        gr.DrawImage(imgurl, new Rectangle(0, 0, newImage.Width, newImage.Height));
                    }

                    if (NeedRoundCorners) {
                        newImage = RoundCorners(newImage, 7, Color.Transparent);
                    }

                    newImage.Save(filePath);
                }
                else {
                    imgurl.Save(filePath);
                }
                cancontinue = true;
            }
        }

        return cancontinue;
    }
    public bool SaveNewImg(HttpPostedFile file, string filePath, int width = 64, int height = 64) {
        bool cancontinue = false;
        Image imgurl = Image.FromStream(file.InputStream, false, true);
        if (HelperMethods.IsImage(file)) {

            // Get the image's original width and height
            int originalWidth = width;
            int originalHeight = height;

            if (width <= 0 || height <= 0) {
                originalWidth = imgurl.Width;
                originalHeight = imgurl.Height;

                // To preserve the aspect ratio
                float ratioX = (float)1080 / (float)originalWidth;
                float ratioY = (float)1080 / (float)originalHeight;
                float ratio = Math.Min(ratioX, ratioY);

                // New width and height based on aspect ratio
                originalWidth = (int)(originalWidth / ratio);
                originalHeight = (int)(originalHeight / ratio);
            }

            if ((imgurl.Size.Height > originalHeight) || (imgurl.Size.Width > originalWidth)) {
                Bitmap newImage = new Bitmap(originalWidth, originalHeight);
                newImage.SetResolution(imgurl.HorizontalResolution, imgurl.VerticalResolution);

                using (Graphics gr = Graphics.FromImage(newImage)) {
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    gr.DrawImage(imgurl, new Rectangle(0, 0, newImage.Width, newImage.Height));
                }

                if (NeedRoundCorners) {
                    newImage = RoundCorners(newImage, 7, Color.Transparent);
                }

                newImage.Save(filePath);
            }
            else {
                imgurl.Save(filePath);
            }
            cancontinue = true;
        }

        return cancontinue;
    }
    private static Bitmap RoundCorners(Image startImage, int cornerRadius, Color backgroundColor) {
        cornerRadius *= 2;
        var roundedImage = new Bitmap(startImage.Width, startImage.Height);
        Graphics g = Graphics.FromImage(roundedImage);
        g.Clear(backgroundColor);
        g.SmoothingMode = SmoothingMode.HighQuality;
        Brush brush = new TextureBrush(startImage);
        var gp = new GraphicsPath();
        gp.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
        gp.AddArc(0 + roundedImage.Width - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90);
        gp.AddArc(0 + roundedImage.Width - cornerRadius, 0 + roundedImage.Height - cornerRadius, cornerRadius,
                  cornerRadius, 0, 90);
        gp.AddArc(0, 0 + roundedImage.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
        g.FillPath(brush, gp);

        return roundedImage;
    }

    #endregion

}