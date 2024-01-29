using Autodesk.Fabrication;
using Autodesk.Fabrication.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public class UpdateFromServer
{
    private static StringBuilder _builder;
    private static string _domainName;
    private static string _username;
    private static string _token;

    public static void SetInitialVariables(StringBuilder builder, string domainName, string username, string token)
    {
        _builder = builder;
        _domainName = domainName;
        _username = username;
        _token = token;
    }

    public static List<List<string>> GetPreassembleInfo()
    {
        string remoteURI = _domainName + "CamductAddin/downloadPreassembleInfo.php";
        string filename = Job.Info.FileName;
        filename = Path.GetFileNameWithoutExtension(filename);
        //filename = "152 Wharf Level 5"; //For testing purpose only. Comment out this line after finishing the test.

        var request = (HttpWebRequest)WebRequest.Create(remoteURI);
        request.Timeout = 120000;
        var postData = "username=" + Uri.EscapeDataString(_username);
        postData += "&token=" + Uri.EscapeDataString(_token);
        postData += "&filename=" + Uri.EscapeDataString(filename);

        var data = Encoding.ASCII.GetBytes(postData);

        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = data.Length;

        using (var stream = request.GetRequestStream())
        {
            stream.Write(data, 0, data.Length);
        }

        var response = (HttpWebResponse)request.GetResponse();

        var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

        //_builder.AppendLine("Feedback " + responseString);
        if (!responseString.Contains("no data"))
        {
            string tempString = responseString.Replace("\\/", "/");
            tempString = tempString.Replace("[", "");
            tempString = tempString.Replace("]", "");
            tempString = tempString.Replace("{", "");
            tempString = tempString.Replace("}", "");
            tempString = tempString.Replace("\"", "");

            List<string> itemList = tempString.Split(',').ToList();
            List<List<string>> itemDetailList = new List<List<string>>();

            foreach (var item in itemList)
            {
                if (item != null)
                    itemDetailList.Add(item.Split('-').ToList());
            }

            return itemDetailList;
        }
        else
        {
            return null;
        }
    }

    public static void UpdateMAJFile(List<List<string>> itemDetailList)
    {
        List<string> handleListInFeedback = new List<string>();

        foreach (var item in itemDetailList)
            handleListInFeedback.Add(item[0]);

        foreach (var item in Job.Items)
        {
            string handleNumber = item.Handle.ToString("X");

            //Find index of item handle in the feedback list
            var indexNumber = handleListInFeedback.IndexOf(handleNumber);

            if (indexNumber >= 0)
            {
                CustomItemData preassmbleNo = item.CustomData[6];
                CustomDataStringValue preassembleNoData = preassmbleNo as CustomDataStringValue;
                preassembleNoData.Value = itemDetailList[indexNumber][1];

                CustomItemData subGroupNo = item.CustomData[7];
                CustomDataStringValue subGroupData = subGroupNo as CustomDataStringValue;
                subGroupData.Value = itemDetailList[indexNumber][2];
            }
        }
    }

    public static void UpdateColor(List<List<string>> itemDetailList)
    {
        Section standAlone = Database.Sections.FirstOrDefault(x => x.Description == "Stand Alone");
        foreach (Item itm in Job.Items)
        {
            //Job No. - CustomData field
            CustomItemData data = itm.CustomData[1];
            CustomDataStringValue myCustomData = data as CustomDataStringValue;
            string jobNumber = myCustomData.Value.ToString();

            if (CheckJobnoInSubgroupList(jobNumber, itemDetailList))
            {
                itm.Section = standAlone;
            }
        }

        foreach (var myItem in itemDetailList)
        {
            Item itm = Job.Items.FirstOrDefault(x => x.Handle == ulong.Parse(myItem[0], System.Globalization.NumberStyles.HexNumber));

            //_builder.AppendLine(itm.Number.ToString());
            CustomItemData preassmbleNo = itm.CustomData[6];
            CustomDataStringValue preassembleNoData = preassmbleNo as CustomDataStringValue;
            preassembleNoData.Value = myItem[1];

            CustomItemData subGroupNo = itm.CustomData[7];
            CustomDataStringValue subGroupNoData = subGroupNo as CustomDataStringValue;
            subGroupNoData.Value = myItem[2];

            Section originalColor = Database.Sections.FirstOrDefault(x => x.Index == int.Parse(myItem[3]));
            Section lightColor = Database.Sections.FirstOrDefault(x => x.Index == int.Parse(myItem[3]) + 1);

            if (IsEven(int.Parse(preassembleNoData.Value)))
            {
                itm.Section = lightColor;
            }
            else
            {
                itm.Section = originalColor;
            }
        }
    }

    public static bool CheckJobnoInSubgroupList(string jobno, List<List<string>> ListToCheck)
    {
        List<string> subgroupNo = new List<string>();

        foreach (var item in ListToCheck)
        {
            subgroupNo.Add(item[2]);
        }

        var match = subgroupNo.Any(p => p == jobno);

        if (match != null)
        {
            //Do stuff
            return true;
        }

        return false;
    }

    public static bool IsEven(int numberToCheck)
    {
        if ((numberToCheck % 2) != 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
