﻿using GSC.Helper;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace GSC.Shared.DocumentService
{
    public class DocumentService
    {
        public static readonly string DefulatProfilePic = "default-profile-pic.png";

        public static readonly string DefulatLogo = "default-logo.png";
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

        public static string SaveUploadDocument(FileModel file, string basePath, string companyCode, string studyCode, FolderType folderType, string categoryName)
        {
            string[] paths = { basePath, companyCode, studyCode, folderType.ToString(), categoryName };
            var fullPath = Path.Combine(paths);

            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);

            file.Base64 = file.Base64.Split("base64,")[1];

            var strGuid = Guid.NewGuid() + "." + file.Extension;
            var filepath = Path.Combine(companyCode, studyCode, folderType.ToString(), categoryName, strGuid);

            var imageBytes = Convert.FromBase64String(file.Base64);
            var documentPath = Path.Combine(basePath, filepath);
            File.WriteAllBytes(documentPath, imageBytes);

            return filepath;
        }

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


        public string GetEtmfOldFileName(string path, string Filename)
        {
            string[] paths = { path };
            var fullPath = Path.Combine(paths);
            if (Directory.Exists(fullPath))
            {
                List<EtmfFileNameModel> fileNames = new List<EtmfFileNameModel>();
                var allFiles = Directory.GetFiles(fullPath).Where(x => x.Contains(Filename));
                foreach (var item in allFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(item);
                    var model = new EtmfFileNameModel()
                    {
                        FileCodeName = fileName.Substring(fileName.LastIndexOf('_') + 1),
                        FileName = Path.GetFileName(item),
                        FileCreateDate = new DateTime(Convert.ToInt64(fileName.Substring(fileName.LastIndexOf('_') + 1)), DateTimeKind.Utc)
                    };
                    fileNames.Add(model);
                }

                var oldFile = fileNames.OrderBy(x => x.FileCreateDate).FirstOrDefault();

                return oldFile?.FileName;
            }

            return "";
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
                        if (theEntry.IsFile && theEntry.Name != "")
                        {

                            string strNewFile = "";
                            if (Convert.ToString(theEntry.Name).Split(".").Last() == "asc")
                                strNewFile = @"" + baseDirectory + "/" + theEntry.Name.Split("/")[1];
                            else
                                strNewFile = @"" + baseDirectorySeq + "/" + theEntry.Name.Split("/")[1];

                            using (FileStream streamWriter = File.Create(strNewFile))
                            {
                                pathList.Add(strNewFile);
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    int size = ZipStream.Read(data, 0, data.Length);
                                    if (size > 0)
                                        streamWriter.Write(data, 0, size);
                                    else
                                        break;
                                }
                                streamWriter.Close();
                            }

                        }
                    }
                    ZipStream.Close();
                }
            }

        }

        public static string ConvertBase64Image(string imagepath)
        {

            if (File.Exists(imagepath))
            {
                using (var fs = File.Open(imagepath, FileMode.Open))
                {
                    byte[] byData = new byte[fs.Length];
                    fs.Read(byData, 0, byData.Length);
                    string extension = Path.GetExtension(fs.Name).Replace(".", "");
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