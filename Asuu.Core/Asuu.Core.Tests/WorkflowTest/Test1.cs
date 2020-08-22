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

        internal class ModComparer : IEqualityComparer<ModInfo>
        {
            public bool Equals([AllowNull] ModInfo x, [AllowNull] ModInfo y)
            {
                //Check whether the compared objects reference the same data.
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                return x.FileName == y.FileName;
            }

            public int GetHashCode([DisallowNull] ModInfo obj)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(obj, null)) return 0;

                //Get hash code for the Name field if it is not null.
                int hasObjName = obj.FileName == null ? 0 : obj.FileName.GetHashCode();

                //Calculate the hash code for the product.
                return hasObjName;
            }
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

            public ModInfo[] SourceMods;
            public ModInfo[] ModChangeHistory;
            public List<ModInfo> UpdatedMods;

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
                WorkflowSteps.UNKNOWN => throw new ArgumentException("unknown arugment"),
                WorkflowSteps.COMPLETE => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
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
            workflow.SourceFiles = workflow.directoryInfo.GetFiles("*.mod", SearchOption.TopDirectoryOnly);

            var tempList = new List<ModInfo>();

            foreach (var file in workflow.SourceFiles)
            {
                tempList.Add(new ModInfo 
                {
                    FileName = file.Name,
                    LastWriteDate = file.LastWriteTime,
                    FullPath = file.@FullName
                });
            }

            workflow.SourceMods = tempList.ToArray();

            return WorkflowSteps.COMPARE;
        }

        private WorkflowSteps GetHistory()
        {
            if (File.Exists(workflow.History))
            {
                using var reader = File.OpenText(workflow.History);                
                var serializer = new JsonSerializer();
                workflow.ModChangeHistory = (ModInfo[])serializer.Deserialize(reader, typeof(ModInfo[]));
                reader.Close();
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
            var sourceMods = workflow.SourceMods.ToList();
            var changeHistory = workflow.ModChangeHistory.ToList();
            var modsToChange = new List<ModInfo>();
            var modCompare = new ModComparer();
            workflow.UpdatedMods = new List<ModInfo>();

            sourceMods.ForEach(mod =>
            {
                var updatedMod = changeHistory
                    .Select(change => change)
                    .Where(change => change.FileName == mod.FileName && DateTime
                    .Compare(mod.LastWriteDate, change.LastWriteDate) == 1);

                if(updatedMod.ToList().Count > 0)
                {
                    updatedMod.ToList()[0].LastWriteDate = mod.LastWriteDate;
                }

                modsToChange.AddRange(updatedMod);

                if (!changeHistory.Contains(mod, modCompare))
                {
                    // new mod
                    modsToChange.Add(mod);
                }
            });

            workflow.UpdatedMods.AddRange(modsToChange);

            return WorkflowSteps.COMPLETE;
        }
    }
}
