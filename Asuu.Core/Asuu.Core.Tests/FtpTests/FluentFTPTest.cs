#region usings
using Asuu.Core.Tests.Constants;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Dynamic;
using System.IO;
using Newtonsoft.Json;
using Microsoft.VisualBasic.CompilerServices;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using FluentFTP;
#endregion

namespace Asuu.Core.Tests.FtpTests
{
    [TestClass]
    public class Test 
    {

        List<string> FilesToDownload; 
        string localDirectory = @"F:\test\";
        string testFileName = "newlog.log";
        string fakeFileName = "fake.log";
        string remoteFileName = "Gameplay.1.2020.05.23_17.27.15.log";
        string remoteDirectory = "/ShooterGame/Saved/Logs/";
        bool fileExists = false;
        string hostIp ="184.154.49.66";

        string userId = string.Empty;

        string password = string.Empty;        

        /// <summary>
        /// Init
        /// <summary>
        [TestInitialize]
        public void OnStart()
        {
            FilesToDownload = new List<string>();
        }

        /// <summary>
        /// Test connection and read
        /// <summary>
        [TestMethod]
        public void ConnectTest() 
        {
            using var session = new FtpClient(hostIp,userId,password);
            session.Connect();

            if(session.FileExists($"{remoteDirectory}{remoteFileName}"))
            {
                fileExists = true;
            }

            session.Disconnect();
            session.Dispose();

            Assert.IsTrue(fileExists);
        }

        /// <summary>
        /// Download File Test
        /// </summary>
        [TestMethod]
        public void DownloadTest()
        {
            using var session = new FtpClient(hostIp,userId,password);
            session.Connect();

            try 
            {
                if (session.FileExists($"{remoteDirectory}{remoteFileName}"))
                {   
                    session.DownloadFile(@$"{localDirectory}{testFileName}", $"{remoteDirectory}{remoteFileName}", FtpLocalExists.Overwrite, FtpVerify.Retry);
                }                
            }
            catch(Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally 
            {
                session.Disconnect();
                session.Dispose();
            }

            Assert.IsTrue(File.Exists(@$"{localDirectory}{testFileName}"));
        }

        /// <summary>
        /// Upload File Test
        /// </summary>
        [TestMethod]
        public void UploadTest()
        {
            using var session = new FtpClient(hostIp,userId,password);
            session.Connect();

            try 
            {
                if(session.FileExists($"{remoteDirectory}{remoteFileName}"))
                {
                    session.UploadFile(@$"{localDirectory}{testFileName}",
                    $"{remoteDirectory}{fakeFileName}", FtpRemoteExists.Overwrite, true );

                    Assert.IsTrue(session.FileExists($"{remoteDirectory}{fakeFileName}"));
                }
            }
            catch(Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally 
            {
                session.Disconnect();
                session.Dispose();
            }            
        }

        /// <summary>
        /// I dont know if this is needed?
        /// </summary>
        [TestMethod]
        public void RemoteCompare()
        {
            using var session = new FtpClient(hostIp,userId,password);
            session.Connect();

            try 
            {
                var remoteFileDate = session.GetModifiedTime($"{remoteDirectory}{fakeFileName}");
                var localFileDate = File.GetLastWriteTime($"{localDirectory}{testFileName}");

                var remoteDateIsLater = remoteFileDate.CompareTo(localFileDate);

                Assert.IsFalse(remoteDateIsLater == 1);
            }
            catch(Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally 
            {
                session.Disconnect();
                session.Dispose();
            }             
        }
    }
}
