using System;
using System.Collections.Generic;
using Inventor;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using BxDRobotExporter;

namespace ExportProcess
{
    public class RobotSaver
    {
        #region State Variables
        private TempReader tempReader;
        private TempWriter tempWriter;
        private JointResolver jointResolver;
        private Inventor.Application currentApplication;
        private List<byte> fileData = new List<byte>();
        private readonly byte majVersion = 1, minVersion = 0, patVersion = 0, intVersion = 0;
        private byte[] majVersionBytes, minVersionBytes, patVersionBytes, intVersionBytes;
        private readonly string filePath = "C:\\Users\\" + System.Environment.UserName + "\\Documents\\Synthesis\\Robots\\";
        private List<JointData> jointDataList = new List<JointData>();
        #endregion
        /// <summary>
        /// Constructor the the robot saver objet that handles the exportation process.
        /// </summary>
        /// <param name="currentApplication"></param>
        /// <param name="jDataList"></param>
        public RobotSaver(Inventor.Application currentApplication, ArrayList jDataList)
        {
            string test = (filePath + currentApplication.ActiveDocument.DisplayName.Substring
                   (0, currentApplication.ActiveDocument.DisplayName.Length - 3) + "robot");
            if (System.IO.File.Exists(filePath + currentApplication.ActiveDocument.DisplayName.Substring
                   (0, currentApplication.ActiveDocument.DisplayName.Length - 3) + "robot"))
            {
                //Replaces prior versions of the current robot that is being exported
                DialogResult dialogResult = MessageBox.Show("This file already exists, would you like to replace it?", "Error", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes) System.IO.File.Delete(filePath + currentApplication.ActiveDocument.DisplayName.Substring
                  (0, currentApplication.ActiveDocument.DisplayName.Length - 3) + "robot");
                if (dialogResult == DialogResult.No) throw new Exception();
                //Directory where robots will be saved for Synthesis's uses
            }
            foreach (JointData joint in jDataList)
            {
                jointDataList.Add(joint);
            }

            if (!(Directory.Exists("C:\\Users\\" + System.Environment.UserName + "\\Documents\\Synthesis\\Robots\\"))) Directory.CreateDirectory("C:\\Users\\" + System.Environment.UserName + "\\Documents\\Synthesis\\Robots\\");
            //creation of byte versions of the version numbers
            majVersionBytes = BitConverter.GetBytes(majVersion);
            minVersionBytes = BitConverter.GetBytes(minVersion);
            patVersionBytes = BitConverter.GetBytes(patVersion);
            intVersionBytes = BitConverter.GetBytes(intVersion);
            tempReader = new TempReader((AssemblyDocument)currentApplication.ActiveDocument);
            tempWriter = new TempWriter(currentApplication, ((AssemblyDocument)currentApplication.ActiveDocument).Thumbnail);
            jointResolver = new JointResolver(currentApplication, tempReader.GetSTLDict(), jointDataList);
            this.currentApplication = currentApplication;
        }

        private bool Manager()
        {
            try
            {
                List<byte> versionBytes = new List<byte>();
                versionBytes.AddRange(majVersionBytes);
                versionBytes.AddRange(minVersionBytes);
                versionBytes.AddRange(patVersionBytes);
                versionBytes.AddRange(intVersionBytes);
                fileData.AddRange(versionBytes);
                for (int bit = 0; bit != 72; bit++)
                {
                    fileData.Add(BitConverter.GetBytes(' ')[0]);
                }
                tempWriter.Save();
                foreach (byte fileSec in tempReader.ReadFiles())
                {
                    fileData.Add(fileSec);
                }
                byte[] jointBytes;
                jointBytes = jointResolver.readJoints();
                if (jointBytes != null) fileData.AddRange(jointBytes);
                Assembler();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.StackTrace);
                return false;
            }
        }

        private void Assembler()
        {
            string path = "C:\\Users\\" + System.Environment.UserName + "\\Documents\\Synthesis\\Robots\\" +
                currentApplication.ActiveDocument.DisplayName.Substring(0, currentApplication.ActiveDocument.DisplayName.Length - 3) + "robot";
            bool space = true;

            using (BinaryWriter robotWriter = new BinaryWriter(new FileStream("C:\\Users\\" + System.Environment.UserName + "\\Documents\\Synthesis\\Robots\\" +
                currentApplication.ActiveDocument.DisplayName.Substring(0, currentApplication.ActiveDocument.DisplayName.Length - 3) + "robot", FileMode.Append)))
            {
                foreach (byte fileSection in fileData)
                {
                    robotWriter.Write(fileSection);
                }
            }
        }
        public void BeginExport()
        {
            MessageBox.Show("Conversion " + ((Manager()) ? "successful." : "failed."));
        }
    }
}
