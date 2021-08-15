using GSC.Helper;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.IO;
using System.Linq;


namespace GSC.Shared.DocumentService
{
    public class DocumentService
    {
        //static readonly string fileServer = "http://localhost:63980/";
        public static readonly string DefulatProfilePic = "default-profile-pic.png";

        public static readonly string DefulatLogo = "default-logo.png";
        //public static class Folders
        //{
        //    public static string Profile = "volunteer/profile-pic";
        //    public static string Documents = "volunteer/documents";
        //    public static string CompanyLogo = "company-logo";
        //}

        //public static class Paths
        //{
        //    public static string Profile = fileServer + Folders.Profile + "/";
        //    public static string Documents = fileServer + Folders.Documents + "/";
        //    public static string CompanyLogo = fileServer + Folders.CompanyLogo + "/";
        //}

        //public string DocumentSave(IFormFile file, string path)
        //{
        //    if (string.IsNullOrEmpty(path)) return null;
        //    string fileName = Path.GetFileName(file.FileName);
        //    byte[] fileBytes;
        //    using (var ms = new MemoryStream())
        //    {

        //        file.CopyTo(ms);
        //        fileBytes = ms.ToArray();
        //    }

        //    string stringId = Guid.NewGuid().ToString();
        //    fileName = stringId + Path.GetExtension(fileName);
        //    var docPath = Path.Combine(path, "Volunteer", fileName);
        //    File.WriteAllBytes(docPath, fileBytes);
        //    return fileName;
        //}
        public static string SaveDocument(FileModel file, string path, FolderType folderType, string categoryName)
        {
            string[] paths = { path, folderType.ToString(), categoryName };
            var fullPath = Path.Combine(paths);

            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);

            file.Base64 = file.Base64.Split("base64,")[1];

            var strGuid = Guid.NewGuid() + "." + file.Extension;
            var fileName = Path.Combine(folderType.ToString(), categoryName, strGuid);

            var imageBytes = Convert.FromBase64String(file.Base64);
            var documentPath = Path.Combine(path, fileName);
            File.WriteAllBytes(documentPath, imageBytes);

            return fileName;
        }

        public static string SaveUploadDocument(FileModel file, string basePath, string companyCode, FolderType folderType, string categoryName)
        {
            string[] paths = { basePath, companyCode, folderType.ToString(), categoryName };
            var fullPath = Path.Combine(paths);

            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);

            file.Base64 = file.Base64.Split("base64,")[1];

            var strGuid = Guid.NewGuid() + "." + file.Extension;
            var filepath = Path.Combine(companyCode, folderType.ToString(), categoryName, strGuid);

            var imageBytes = Convert.FromBase64String(file.Base64);
            var documentPath = Path.Combine(basePath, filepath);
            File.WriteAllBytes(documentPath, imageBytes);

            return filepath;
        }

        public static string SaveUploadDocument(FileModel file, string basePath, string companyCode,string studyCode, FolderType folderType, string categoryName)
        {
            string[] paths = { basePath, companyCode, studyCode, folderType.ToString(), categoryName };
            var fullPath = Path.Combine(paths);

            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);

            file.Base64 = file.Base64.Split("base64,")[1];

            var strGuid = Guid.NewGuid() + "." + file.Extension;
            var filepath = Path.Combine(companyCode, studyCode,folderType.ToString(), categoryName, strGuid);

            var imageBytes = Convert.FromBase64String(file.Base64);
            var documentPath = Path.Combine(basePath, filepath);
            File.WriteAllBytes(documentPath, imageBytes);

            return filepath;
        }
        //public static string SaveProjectDocument(FileModel file, string path, FolderType folderType)
        //{
        //    string[] paths = { path, folderType.ToString() };
        //    var fullPath = Path.Combine(paths);

        //    if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);

        //    file.Base64 = file.Base64.Split("base64,")[1];

        //    var strGuid = Guid.NewGuid() + "." + file.Extension;
        //    var fileName = Path.Combine(folderType.ToString(), strGuid);

        //    var imageBytes = Convert.FromBase64String(file.Base64);
        //    var documentPath = Path.Combine(path, fileName);
        //    File.WriteAllBytes(documentPath, imageBytes);

        //    return fileName;
        //}

        public static string SaveWorkplaceDocument(FileModel file, string path, string Filename)
        {
            string[] paths = { path };
            var fullPath = Path.Combine(paths);

            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);

            file.Base64 = file.Base64.Split("base64,")[1];

            var strGuid = Filename + "_" + DateTime.Now.Ticks + "." + file.Extension;
            var fileName = Path.Combine(strGuid);

            var imageBytes = Convert.FromBase64String(file.Base64);
            var documentPath = Path.Combine(path, fileName);
            File.WriteAllBytes(documentPath, imageBytes);

            return fileName;
        }

        public static string SaveETMFDocument(FileModel file, string path, FolderType folderType, string Version)
        {
            string[] paths = { path, folderType.ToString() };
            var fullPath = Path.Combine(paths);

            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);

            file.Base64 = file.Base64.Split("base64,")[1];

            var strGuid = Version + "_" + Guid.NewGuid() + "." + file.Extension;
            var fileName = Path.Combine(folderType.ToString(), strGuid);

            var imageBytes = Convert.FromBase64String(file.Base64);
            var documentPath = Path.Combine(path, fileName);
            File.WriteAllBytes(documentPath, imageBytes);

            return fileName;
        }

        //public static string SaveEconsentFile(FileModel file, string path, FolderType folderType, string Rootname)
        //{
        //    string[] paths = { path, folderType.ToString(), Rootname };
        //    var fullPath = Path.Combine(paths);

        //    if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);

        //    file.Base64 = file.Base64.Split("base64,")[1];

        //    var strGuid = Guid.NewGuid() + "." + file.Extension;
        //    var fileName = Path.Combine(folderType.ToString(), Rootname, strGuid);

        //    var imageBytes = Convert.FromBase64String(file.Base64);
        //    var documentPath = Path.Combine(path, fileName);
        //    File.WriteAllBytes(documentPath, imageBytes);
            
        //    return fileName;
        //}

        //public static string SaveEconsentSectionReferenceFile(FileModel file, string path, FolderType folderType, string Rootname)
        //{
        //    string[] paths = { path, folderType.ToString(), Rootname };
        //    var fullPath = Path.Combine(paths);

        //    if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);

        //    file.Base64 = file.Base64.Split("base64,")[1];

        //    var strGuid = Guid.NewGuid() + "." + file.Extension;
        //    var fileName = Path.Combine(folderType.ToString(), Rootname, strGuid);

        //    var imageBytes = Convert.FromBase64String(file.Base64);
        //    var documentPath = Path.Combine(path, fileName);
        //    File.WriteAllBytes(documentPath, imageBytes);

        //    return fileName;
        //}


        public static string SaveMedraFile(FileModel file, string path, FolderType folderType, string Language, string Version, string Rootname)
        {
            string[] paths = { path, folderType.ToString(), Language, Version, Rootname };
            var fullPath = Path.Combine(paths);

            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);

            file.Base64 = file.Base64.Split("base64,")[1];

            var strGuid = Guid.NewGuid() + "." + file.Extension;
            var fileName = Path.Combine(folderType.ToString(), Language, Version, Rootname, strGuid);

            var imageBytes = Convert.FromBase64String(file.Base64);
            var documentPath = Path.Combine(path, fileName);
            File.WriteAllBytes(documentPath, imageBytes);
            UnZipFile(documentPath);

            return fileName;
        }

        private static void UnZipFile(string fullpath)
        {
            ArrayList pathList = new ArrayList();

            if (File.Exists(fullpath))
            {
                string baseDirectory = Path.GetDirectoryName(fullpath) + "\\Unzip\\MedAscii";
                string baseDirectorySeq = Path.GetDirectoryName(fullpath) + "\\Unzip\\SeqAscii";

                if (Directory.Exists(Path.GetDirectoryName(fullpath) + "\\Unzip"))
                    Directory.Delete(Path.GetDirectoryName(fullpath) + "\\Unzip", true);

                Directory.CreateDirectory(baseDirectory);
                Directory.CreateDirectory(baseDirectorySeq);

                using (ZipInputStream ZipStream = new ZipInputStream(File.OpenRead(fullpath)))
                {
                    ZipEntry theEntry;
                    while ((theEntry = ZipStream.GetNextEntry()) != null)
                    {
                        if (theEntry.IsFile)
                        {
                            if (theEntry.Name != "")
                            {
                                string strNewFile = "";
                                if (Convert.ToString(theEntry.Name).Split(".").Last() == "asc")
                                    strNewFile = @"" + baseDirectory + "/" + theEntry.Name.Split("/")[1];
                                else
                                    strNewFile = @"" + baseDirectorySeq + "/" + theEntry.Name.Split("/")[1];

                                if (File.Exists(strNewFile))
                                {
                                    //continue;
                                }

                                using (FileStream streamWriter = File.Create(strNewFile))
                                {
                                    pathList.Add(strNewFile);
                                    int size = 2048;
                                    byte[] data = new byte[2048];
                                    while (true)
                                    {
                                        size = ZipStream.Read(data, 0, data.Length);
                                        if (size > 0)
                                            streamWriter.Write(data, 0, size);
                                        else
                                            break;
                                    }
                                    streamWriter.Close();
                                }
                            }
                        }
                    }
                    ZipStream.Close();
                }
            }

        }

        public static string ConvertBase64Image(string imagepath) {

            if (File.Exists(imagepath))
            {
                using (var fs = File.Open(imagepath, FileMode.Open))
                {
                    //FileStream fs = new FileStream(imagepath, FileMode.Open);
                    byte[] byData = new byte[fs.Length];
                    fs.Read(byData, 0, byData.Length);
                    string extension = Path.GetExtension(fs.Name).Replace(".","");
                    fs.Close();
                    var base64 = Convert.ToBase64String(byData);
                    string imgSrc = String.Format("data:image/{0};base64,{1}", extension, base64);
                    return imgSrc;
                }
            }
            return "";
        }
        public static void RemoveFile(string basepath, string filepath)
        {
            if (!String.IsNullOrEmpty(filepath))
            {
                string[] paths = { basepath, filepath };
                var fullPath = Path.Combine(paths);
                if (File.Exists(fullPath))
                {
                    File.Delete(Path.Combine(fullPath));
                }
            }
        }
    }
}