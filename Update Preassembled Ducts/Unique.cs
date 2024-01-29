using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Autodesk.Fabrication;

public class UniquenessCheck
{
    private static StringBuilder _builder { get; set; }

    private static string _shareDriveLocation { get; set; }

    private static string _csvOutputFolder { get; set; }

    public static void SetInitialVariable(string csvOutputFolder, StringBuilder mainStringBuilder, string shareDriveLocation)
    {
        _csvOutputFolder = csvOutputFolder;
        _builder = mainStringBuilder;
        _shareDriveLocation = shareDriveLocation;
    }
    public static void CheckUniqueness()
    {
        //Get current folder location
        string currentFolder = Path.GetDirectoryName(Job.Info.FileName);
        string fileWithNoExtension = Path.GetFileNameWithoutExtension(Job.Info.FileName);

        //Check delete.txt in current folder
        if (File.Exists(Path.Combine(currentFolder, "delete.txt")))
        {
            string[] oldFileName = File.ReadAllLines(Path.Combine(currentFolder, "delete.txt"));
            foreach (var deleteOldFileName in oldFileName)
            {
                if (File.Exists(deleteOldFileName))
                    File.Delete(deleteOldFileName);
            }
            File.Delete(Path.Combine(currentFolder, "delete.txt"));
        }

        //Check uniqueness
        if (!CheckUniqueValue(_shareDriveLocation, Job.Info.FileName))
        {
            string newFileName = Job.Info.FileName;
            //Not unique
            while (!CheckUniqueValue(_shareDriveLocation, newFileName))
            {
                //suggest New File Name
                int dotMAJIndex = newFileName.IndexOf(".MAJ");
                int underscoreIndex = newFileName.LastIndexOf("_");

                if ((underscoreIndex == -1) || ((underscoreIndex > -1) && !(IsDigitOnly(newFileName.Substring(underscoreIndex + 1, dotMAJIndex - underscoreIndex - 1)))))
                    newFileName = Path.GetDirectoryName(newFileName) + "\\" + Path.GetFileNameWithoutExtension(newFileName) + "_1.MAJ";
                else
                {
                    string oldName = newFileName.Substring(0, underscoreIndex);
                    int versionNumber = int.Parse(newFileName.Substring(underscoreIndex + 1, dotMAJIndex - underscoreIndex - 1));
                    versionNumber++;
                    newFileName = Path.GetDirectoryName(newFileName) + "\\" + oldName + "_duplicate_" + versionNumber + ".MAJ";
                }
            }

            newFileName = Path.GetFileName(newFileName);
            newFileName = Path.Combine(currentFolder, newFileName);
            File.Copy(Job.Info.FileName, newFileName);
            if (File.Exists(Path.Combine(currentFolder, "delete.txt")))
            {
                File.Delete(Path.Combine(currentFolder, "delete.txt"));
            }
            File.WriteAllText(Path.Combine(currentFolder, "delete.txt"), Job.Info.FileName);

            _builder.AppendLine("This file name is duplicated!");
            _builder.AppendLine("Step 1: Please close this file " + Path.GetFileName(Job.Info.FileName));
            _builder.AppendLine("Step 2 (optional): Rename this file " + Path.GetFileName(newFileName) + " to your custom file name.");
            _builder.AppendLine("Step 3: Open the new file " + Path.GetFileName(newFileName) + " or the file with your custom file name and run this add-in again");
        }
    }

    static bool IsDigitOnly(string str)
    {
        foreach (char c in str)
            if (c < '0' || c > '9')
                return false;

        return true;
    }

    public static string GetOneDriveLocation(string shareFolderName)
    {
        string fileLocation = Job.Info.FileName;

        int stringIndex = fileLocation.IndexOf(shareFolderName);
        string oneDriveLocationOnLocal = "";
        if (stringIndex >= 0)
        {
            oneDriveLocationOnLocal = fileLocation.Substring(0, stringIndex) + shareFolderName;
        }
        else
        {
            _builder.AppendLine(oneDriveLocationOnLocal = "Please put your MAJ file to the folder " + shareFolderName + " on your local machine before running this add-in");
        }

        return oneDriveLocationOnLocal;
    }

    public static bool CheckUniqueValue(string shareFolderName, string fileNameToCheck)
    {
        string fileLocation = GetOneDriveLocation(shareFolderName);
        if (!Directory.Exists(Path.Combine(fileLocation, _csvOutputFolder)))
        {
            Directory.CreateDirectory(Path.Combine(fileLocation, _csvOutputFolder));
        }
        string logFileLocation = Path.Combine(fileLocation, _csvOutputFolder, "log.txt");

        if (!File.Exists(logFileLocation))
        {
            return true;
        }

        List<string> filenameOutput = File.ReadAllLines(logFileLocation).ToList();

        int stringIndex = fileNameToCheck.IndexOf(shareFolderName);
        string fileNameOnShareFolder = fileNameToCheck.Substring(stringIndex, fileNameToCheck.Length - stringIndex);
        foreach (var uniqueCheck in filenameOutput)
        {
            if (!uniqueCheck.Contains(fileNameOnShareFolder) && uniqueCheck.Contains("/" + Path.GetFileName(fileNameOnShareFolder)))
            {
                return false;
            }
        }
        return true;
    }
}

