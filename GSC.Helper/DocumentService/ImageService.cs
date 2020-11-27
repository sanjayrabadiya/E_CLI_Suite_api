
using GSC.Helper;
using Serilog;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace GSC.Shared.DocumentService
{
    public interface IImageService
    {
        string ImageSave(FileModel file, string path, FolderType folderType);
    }

    public class ImageService : IImageService
    {
        public string ImageSave(FileModel file, string path, FolderType imageType)
        {
            if (string.IsNullOrEmpty(path)) return null;


            file.Base64 = file.Base64.Split("base64,")[1];

            var fileBytes = Convert.FromBase64String(file.Base64);

            string[] paths = {path, imageType.ToString(), "Original"};
            var fullPath = Path.Combine(paths);

            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
            var strGuid = Guid.NewGuid() + "." + file.Extension;
            var fileName = Path.Combine(imageType.ToString(), "Original", strGuid);

            var imagePath = Path.Combine(path, fileName);
            File.WriteAllBytes(imagePath, fileBytes);

            var thumbFileBytes = CreateThumbByte(fileBytes);

            string[] thumbPaths = {path, imageType.ToString(), "Thumbnail"};
            var thumbFullPath = Path.Combine(thumbPaths);
            if (!Directory.Exists(thumbFullPath)) Directory.CreateDirectory(thumbFullPath);
            var thumbFileName = Path.Combine(imageType.ToString(), "Thumbnail", strGuid);
            var thumbPath = Path.Combine(path, thumbFileName);
            File.WriteAllBytes(thumbPath, thumbFileBytes);
            return fileName;
        }

        private byte[] CreateThumbByte(byte[] file)
        {
            try
            {
                var frontImg = ByteArrayToImage(file);
                return ImageToByteArray(NistCompliant(frontImg, new Size(180, 225),
                    InterpolationMode.HighQualityBilinear));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
            }

            return null;
        }

        private Image ByteArrayToImage(byte[] byteArrayIn)
        {
            var ms2 = new MemoryStream(byteArrayIn);
            ms2.Position = 0;
            var returnImage = Image.FromStream(ms2);
            return returnImage;
        }

        private byte[] ImageToByteArray(Image imageIn)
        {
            byte[] imageArray;
            var ms1 = new MemoryStream();
            ms1.Position = 0;
            imageIn.Save(ms1, ImageFormat.Jpeg);
            imageArray = ms1.ToArray();
            return imageArray;
        }

        private static Image NistCompliant(Image original, Size renderSize, InterpolationMode mode)
        {
            if (original == null || renderSize.Width == 0 || renderSize.Height == 0 ||
                renderSize.Width >= original.Width || renderSize.Height >= original.Height) return original;
            double num = original.Width / original.Height;
            var num2 = Math.Min(renderSize.Width, renderSize.Height);
            double num3;
            if (num >= 1.0)
                num3 = original.Width / num2;
            else
                num3 = original.Height / num2;
            if (1.0 / num3 < 0.01)
                throw new DivideByZeroException("NistCompliant size must be at least 1% of the original");
            var width = (int) (original.Width / num3);
            var height = (int) (original.Height / num3);
            var image = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.InterpolationMode = mode;
                graphics.DrawImage(original, new Rectangle(0, 0, image.Width, image.Height),
                    0, 0, original.Width, original.Height, GraphicsUnit.Pixel);
            }

            return image;
        }
    }
}