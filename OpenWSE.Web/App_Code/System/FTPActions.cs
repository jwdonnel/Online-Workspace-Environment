using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;

/// <summary>
/// Summary description for FTPActions
/// </summary>
[Serializable]
public class FTPActions {

    #region Variables

    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _ftpLocation = string.Empty;

    #endregion


    public FTPActions(string ftpLocation, string username, string password) {
        _ftpLocation = ftpLocation;
        if (_ftpLocation[_ftpLocation.Length - 1] != '/') {
            _ftpLocation += "/";
        }

        _username = username;
        _password = password;
    }
    public static string GetFTPPrefix(string loc) {
        string ftpPrefix = "ftp://";
        if (loc.ToLower().StartsWith("ftps:/")) {
            ftpPrefix = "ftps://";
        }

        return ftpPrefix;
    }

    /// <summary>
    /// Builds the FtpWebRequest
    /// </summary>
    /// <param name="ftpLocation">Ftp Location</param>
    /// <param name="method">Ftp Method to be called</param>
    /// <returns>New FtpWebRequest object</returns>
    public FtpWebRequest BuildRequest(string ftpLocation, string method) {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpLocation);
        request.Method = method;
        request.Credentials = new NetworkCredential(_username, _password);

        return request;
    }


    #region FTP Actions

    /// <summary>
    /// Attempts to connect to the Ftp location
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <returns>Whether or not the connection was successful</returns>
    public bool TryConnect(out string errorMessage) {
        errorMessage = string.Empty;

        try {
            bool success = false;

            string tempLoc = _ftpLocation;
            if (tempLoc[tempLoc.Length - 1] == '/') {
                tempLoc = tempLoc.Remove(tempLoc.Length - 1);
            }

            FtpWebRequest request = BuildRequest(tempLoc, WebRequestMethods.Ftp.ListDirectory);
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse()) {
                if (!string.IsNullOrEmpty(response.WelcomeMessage)) {
                    success = true;
                }
            }

            return success;
        }
        catch (Exception e) {
            errorMessage = e.Message;
        }

        return false;
    }

    /// <summary>
    /// Returns the contents of a file
    /// </summary>
    /// <param name="fileLoc">File location</param>
    /// <param name="errorMessage">Response message</param>
    /// <returns></returns>
    public byte[] GetFileContents(string fileLoc, out string errorMessage) {
        List<byte> fileContents = new List<byte>();
        errorMessage = string.Empty;

        try {
            fileLoc = GetLocationWithoutMainFtpLocation(fileLoc);

            FtpWebRequest request = BuildRequest(_ftpLocation + fileLoc, WebRequestMethods.Ftp.DownloadFile);
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse()) {
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream())) {
                    fileContents = FTPActions.ReadToEnd(streamReader.BaseStream).ToList();
                }
            }
        }
        catch (Exception e) {
            errorMessage = e.Message;
        }

        return fileContents.ToArray();
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    /// <param name="fileLoc">File location</param>
    /// <returns>Whether or not the file was successfully deleted</returns>
    public bool DeleteFile(string fileLoc) {
        try {
            fileLoc = GetLocationWithoutMainFtpLocation(fileLoc);

            FtpWebRequest request = BuildRequest(_ftpLocation + fileLoc, WebRequestMethods.Ftp.DeleteFile);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }
        catch { return false; }

        return true;
    }

    /// <summary>
    /// Delete a folder
    /// </summary>
    /// <param name="folderLoc">Folder location</param>
    /// <returns>Whether or not the folder was successfully deleted</returns>
    public bool DeleteFolder(string folderLoc) {
        try {
            folderLoc = GetLocationWithoutMainFtpLocation(folderLoc);

            if (folderLoc[folderLoc.Length - 1] == '/') {
                folderLoc = folderLoc.Remove(folderLoc.Length - 1);
            }

            RecursiveDelete(_ftpLocation + folderLoc);
        }
        catch { return false; }

        return true;
    }
    private void RecursiveDelete(string folderLoc) {
        List<string> dirList = GetDirList(folderLoc);

        string tempLoc = folderLoc;
        if (tempLoc[tempLoc.Length - 1] != '/') {
            tempLoc += "/";
        }

        foreach (string dir in dirList) {
            FileInfo fi = new FileInfo(dir);
            if (string.IsNullOrEmpty(fi.Extension)) {
                RecursiveDelete(tempLoc + dir);
            }
            else {
                DeleteFile(tempLoc + dir);
            }
        }

        FtpWebRequest request = BuildRequest(folderLoc, WebRequestMethods.Ftp.RemoveDirectory);
        FtpWebResponse response = (FtpWebResponse)request.GetResponse();
        response.Close();
    }

    /// <summary>
    /// Renames a File or Folder
    /// </summary>
    /// <param name="fileFolderLoc">File or Folder location</param>
    /// <param name="newName">The new name for the File or Folder</param>
    /// <returns>Whether or not the folder was successfully deleted</returns>
    public bool RenameFileOrFolder(string fileFolderLoc, string newName) {
        try {
            FtpWebRequest request = BuildRequest(fileFolderLoc, WebRequestMethods.Ftp.Rename);
            request.RenameTo = newName;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }
        catch { return false; }

        return true;
    }

    /// <summary>
    /// Uploads a new file to the FTP location
    /// </summary>
    /// <param name="fileLoc">File location</param>
    /// <param name="fileContents">File Contents</param>
    /// <returns>Whether or not the file was successfully uploaded</returns>
    public bool UploadFiles(string fileLoc, byte[] fileContents) {
        try {
            fileLoc = GetLocationWithoutMainFtpLocation(fileLoc);

            FtpWebRequest request = BuildRequest(_ftpLocation + fileLoc, WebRequestMethods.Ftp.UploadFile);
            request.ContentLength = fileContents.Length;

            using (Stream requestStream = request.GetRequestStream()) {
                requestStream.Write(fileContents, 0, fileContents.Length);
            }

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }
        catch { return false; }

        return true;
    }

    /// <summary>
    /// Creates a new folder
    /// </summary>
    /// <param name="folderLoc">Folder name and location</param>
    /// <returns>Whether or not the folder was successfully created</returns>
    public bool CreateNewFolder(string folderLoc) {
        try {
            folderLoc = GetLocationWithoutMainFtpLocation(folderLoc);

            FtpWebRequest request = BuildRequest(_ftpLocation + folderLoc, WebRequestMethods.Ftp.MakeDirectory);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }
        catch { return false; }

        return true;
    }

    #endregion


    #region Build Directory List

    /// <summary>
    /// Builds the FileExplorerList used for the File Explorer
    /// </summary>
    /// <param name="ftpLocation"></param>
    /// <returns></returns>
    public List<FileExplorerList> GetListOfFilesAndDir(string ftpLocation) {
        List<FileExplorerList> PageList = new List<FileExplorerList>();

        try {
            List<string> directories = GetDirList(ftpLocation);

            string tempLoc = ftpLocation;
            if (tempLoc[tempLoc.Length - 1] != '/') {
                tempLoc += "/";
            }

            foreach (string dir in directories) {
                FileInfo fi = new FileInfo(dir);
                if (string.IsNullOrEmpty(fi.Extension)) {
                    PageList.Add(new FileExplorerList(Guid.NewGuid().ToString(), tempLoc + dir, null, new DirectoryInfo(tempLoc.Replace("ftp:/", "").Replace("ftps:/", "") + dir)));
                    GetListOfSubFilesAndDir(tempLoc + dir, ref PageList);
                }
                else {
                    if ((HelperMethods.IsValidFileFolderFormat(fi.Extension)) || (HelperMethods.IsImageFileType(fi.Extension))) {
                        PageList.Add(new FileExplorerList(Guid.NewGuid().ToString(), dir, new FileInfo(tempLoc.Replace("ftp:/", "").Replace("ftps:/", "") + dir), null));
                    }
                }
            }
        }
        catch { }

        return PageList;
    }
    private void GetListOfSubFilesAndDir(string ftpLocation, ref List<FileExplorerList> PageList) {
        try {
            List<string> directories = GetDirList(ftpLocation);

            string tempLoc = ftpLocation;
            if (tempLoc[tempLoc.Length - 1] != '/') {
                tempLoc += "/";
            }

            foreach (string dir in directories) {
                FileInfo fi = new FileInfo(dir);
                if (string.IsNullOrEmpty(fi.Extension)) {
                    PageList.Add(new FileExplorerList(Guid.NewGuid().ToString(), tempLoc + dir, null, new DirectoryInfo(tempLoc.Replace("ftp:/", "").Replace("ftps:/", "") + dir)));
                    GetListOfSubFilesAndDir(tempLoc + dir, ref PageList);
                }
                else {
                    if ((HelperMethods.IsValidFileFolderFormat(fi.Extension)) || (HelperMethods.IsImageFileType(fi.Extension))) {
                        PageList.Add(new FileExplorerList(Guid.NewGuid().ToString(), dir, new FileInfo(tempLoc.Replace("ftp:/", "").Replace("ftps:/", "") + dir), null));
                    }
                }
            }
        }
        catch { }
    }

    /// <summary>
    /// Gets a list of Files and Directories for the given ftp location
    /// </summary>
    /// <param name="ftpLocation">Ftp location</param>
    /// <returns>List of files and directories</returns>
    public List<string> GetDirList(string ftpLocation) {
        string tempLoc = ftpLocation;
        if (tempLoc[tempLoc.Length - 1] != '/') {
            tempLoc += "/";
        }

        FtpWebRequest request = BuildRequest(tempLoc, WebRequestMethods.Ftp.ListDirectory);
        List<string> directories = new List<string>();

        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse()) {
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream())) {
                string line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line)) {
                    if (line != "." && line != ".." && !line.Contains("/.")) {
                        directories.Add(line);
                    }
                    line = streamReader.ReadLine();
                }
            }
        }

        return directories;
    }

    #endregion


    private string GetLocationWithoutMainFtpLocation(string loc) {
        if (loc.StartsWith(_ftpLocation)) {
            return loc.Replace(_ftpLocation, string.Empty);
        }

        return loc;
    }

    private static byte[] ReadToEnd(Stream stream) {
        long originalPosition = 0;

        if (stream.CanSeek) {
            originalPosition = stream.Position;
            stream.Position = 0;
        }

        try {
            byte[] readBuffer = new byte[4096];

            int totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0) {
                totalBytesRead += bytesRead;

                if (totalBytesRead == readBuffer.Length) {
                    int nextByte = stream.ReadByte();
                    if (nextByte != -1) {
                        byte[] temp = new byte[readBuffer.Length * 2];
                        Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                        Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                        readBuffer = temp;
                        totalBytesRead++;
                    }
                }
            }

            byte[] buffer = readBuffer;
            if (readBuffer.Length != totalBytesRead) {
                buffer = new byte[totalBytesRead];
                Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
            }
            return buffer;
        }
        finally {
            if (stream.CanSeek) {
                stream.Position = originalPosition;
            }
        }
    }

}


[Serializable]
public class FileExplorerList {
    private string _id;
    private string _filename;
    private FileInfo _fi;
    private DirectoryInfo _di;

    public FileExplorerList(string id, string filename, FileInfo fi, DirectoryInfo di) {
        _id = id;
        _filename = filename;
        _fi = fi;
        _di = di;
    }

    public string ID {
        get { return _id; }
    }

    public string Filename {
        get { return _filename; }
    }

    public FileInfo File_Info {
        get { return _fi; }
    }

    public DirectoryInfo Directory_Info {
        get { return _di; }
    }
}
