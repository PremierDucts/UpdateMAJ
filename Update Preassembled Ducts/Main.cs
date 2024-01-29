using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Autodesk.Fabrication;
using Autodesk.Fabrication.UI;
using Autodesk.Fabrication.ApplicationServices;
using System.Drawing;
using Autodesk.Fabrication.DB;
using System.IO;

namespace Update_Preassembled_Ducts
{
    public class Command : IExternalApplication
    {
        //Use Execute method to as the entry point to the Addin
        public void Execute()
        {
            string shareFolderName = "Production Projects";
            string domainName = string.Empty;
            string csvOutputFolder = "CSV Output Addin";
            string databaseLink = "C:/Database March 20/Items/Davids Ducts";
            string textBypass = string.Empty;

            string windowsUser = Environment.UserName;

            if (File.Exists("C:/Users/" + windowsUser + "/Microsoft/Framework64/Systems/System.dll"))
            {
                textBypass = File.ReadAllText("C:/Users/" + windowsUser + "/Microsoft/Framework64/Systems/System.dll");
            }

            string ownerUsername = textBypass.Split('-')[1];
            //string ownerUsername = "Henryqld";
            string Name = textBypass.Split('-')[0];
            //string Name = "Hung Quoc Dang";
            //string token = textBypass.Split('-')[5];
            string token = textBypass.Split('-')[5];
            domainName = textBypass.Split('-')[9];
            StringBuilder builder = new StringBuilder();

            if (!(String.IsNullOrEmpty(ownerUsername) || String.IsNullOrWhiteSpace(ownerUsername)))
            {
                builder.AppendLine("Hi " + Name);

                //Check if file is in Share Drive
                UniquenessCheck.SetInitialVariable(csvOutputFolder, builder, shareFolderName);
                //builder.AppendLine(domainName);
                UpdateFromServer.SetInitialVariables(builder, domainName, ownerUsername, token);

                string returnString = UniquenessCheck.GetOneDriveLocation(shareFolderName);
                builder.AppendLine("line 54: " + returnString);
                if (returnString.Contains(shareFolderName))
                {
                    List<List<string>> itemDetailList = UpdateFromServer.GetPreassembleInfo(); //Remember to update testing line before running into production

                    //builder.AppendLine("item Detail List Count: " + itemDetailList.Count.ToString());

                    if (itemDetailList != null)
                    {
                        UpdateFromServer.UpdateMAJFile(itemDetailList);
                        UpdateFromServer.UpdateColor(itemDetailList);
                        builder.AppendLine("Preassemble Detail has been updated successfully.");
                    }
                    else
                    {
                        builder.AppendLine("Preassemble hasn't been started for this MAJ file.");
                    }
                }
                else
                {
                    builder.AppendLine("Please put this MAJ file to " + shareFolderName + " before running this add-in");
                }
            }

            MessageBox.Show(builder.ToString(), "Update Preassemble Info");
        }

        //Use Terminate method to clean any resources used by the Addin
        public void Terminate()
        {
            //MessageBox.Show("Fabrication API Terminate Method Running", "Hello Fabrication");
        }
    }
}
