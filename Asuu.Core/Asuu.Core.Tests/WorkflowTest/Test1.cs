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

namespace Asuu.Core.Tests.WorkflowTests
{
    [TestClass]
    public class Test1
    {
        /// <summary>
        /// Temp for prototyping
        /// </summary>
        internal class ModInfo
        {   
            public string FileName;
            public DateTime LastWriteDate;
            public string FullPath;
        }

        internal class WorkflowInfo 
        {
            public WorkflowSteps _initialStep = WorkflowSteps.GET_HISTORY;
            public Queue<WorkflowSteps> workflowSteps;

            public string Destination = "/ShooterGame/Content/Mods";
            public string Source = @"E:\SteamLibrary\steamapps\common\ARK\ShooterGame\Content\Mods";
            public string History = @$"{Directory.GetCurrentDirectory()}\fileHistory.json";

            public FileInfo[] SourceFiles;
            public FileInfo[] DestinationFiles;
            public FileInfo[] HistoryList;
            
            public DirectoryInfo directoryInfo;
        }

        internal WorkflowInfo workflow = new WorkflowInfo();

        [TestInitialize]
        public void OnStart()
        {
            // this is for testing only real implementation wouldnt be out side the object.
            workflow.workflowSteps = new Queue<WorkflowSteps>();
            
            workflow.workflowSteps.Enqueue(workflow._initialStep);

            workflow.directoryInfo = new DirectoryInfo(workflow.Source);

            workflow.SourceFiles = workflow.directoryInfo.GetFiles("*.mod", SearchOption.TopDirectoryOnly);
        }
        [TestMethod]
        public void ProcessTest()
        {   
            do
            {
                Process();
            }
            while (workflow.workflowSteps.Peek() != WorkflowSteps.COMPLETE);
            Assert.IsTrue(workflow.workflowSteps.Peek() == WorkflowSteps.COMPLETE);
        }

        private void Process()
        {
            var _nextStep = workflow.workflowSteps.Dequeue() switch
            {
                WorkflowSteps.GET_SOURCE_LIST => GetSource(),
                WorkflowSteps.GET_HISTORY => GetHistory(),
                WorkflowSteps.CONNECT => Connect(),
                WorkflowSteps.GET_DESTINATION_LIST => GetDestination(),
                WorkflowSteps.COMPARE => Compare(),
                WorkflowSteps.ADD_NEW => Insert(),
                WorkflowSteps.UPDATE_CURRENT => Update(),
                WorkflowSteps.UNKNOWN => throw new ArgumentException("unknown arugment")
            };

             workflow.workflowSteps.Enqueue(_nextStep);
        }

        private WorkflowSteps Update()
        {
            throw new NotImplementedException();
        }

        private WorkflowSteps Insert()
        {
            throw new NotImplementedException();
        }

        private WorkflowSteps GetSource()
        {
           

            return WorkflowSteps.COMPLETE;
        }

        private WorkflowSteps GetHistory()
        {
            if (File.Exists(workflow.History))
            {
                using var reader = File.OpenText(workflow.History);                
                var serializer = new JsonSerializer();
                var mods = (ModInfo[])serializer.Deserialize(reader, typeof(ModInfo[]));

                return WorkflowSteps.GET_SOURCE_LIST;
            }

            return WorkflowSteps.GET_SOURCE_LIST;
        }
        
        /// <summary>
        /// Creates history of each mod based on time last written to. 
        /// Updates after completion.
        /// </summary>
        [TestMethod]
        public void CreateFileHistory()
        {
            workflow.HistoryList = workflow
                .directoryInfo
                .GetFiles("*.mod", SearchOption.TopDirectoryOnly);

            var tempList = new List<(string, string, string)>();

            foreach (var file in workflow.HistoryList)
            {
                tempList.Add((file.Name, file.LastWriteTime.ToString(), file.@FullName));
            }
            // Should be an object. Being lazy. Want to quickly make this work.
            var jsonFile = JsonConvert.SerializeObject(tempList)
                .Replace("Item1", "FileName")
                .Replace("Item2", "LastWriteDate")
                .Replace("Item3", "FullPath");

            using var streamWriter = new StreamWriter(workflow.History);
            streamWriter.Write(jsonFile);
            streamWriter.Close();
        }


        private WorkflowSteps Connect()
        {
            throw new NotImplementedException();
        }

        private WorkflowSteps GetDestination()
        {
            throw new NotImplementedException();
        }

        private WorkflowSteps Compare()
        {
            throw new NotImplementedException();
        }
    }
}
