#region usings
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using FluentFTP;
#endregion

namespace Asuu.Core.Ftp.Services
{
    public class FtpService
    {
        #region internals
        internal class ConfigInfo
		{
			public string hostIp;

			public string userId;

			public string password;
		}

		readonly ConfigInfo _config;
        #endregion

        public FtpService (string ip, string user, string secret)
		{
			_config = new ConfigInfo
			{
				hostIp = ip ?? throw new ArgumentNullException(nameof(ip)),
				userId = user ?? throw new ArgumentNullException(nameof(user)),
				password = secret ?? throw new ArgumentNullException(nameof(secret))
			};
		}

		#region requests		
		public List<(string,DateTime,string)> GetRemoteList(string location, string filter)
		{
			using var session = GetSession();
			session.SetWorkingDirectory(location);
			session.Connect();

			try 
			{
				var remoteList = session
					.GetListing()
					.Where(item => item.Type == FtpFileSystemObjectType.File)
					.Select(item => (item.Name, item.Modified, item.FullName))
					.ToList();
				
				return remoteList;
			}
			catch (FtpException)
			{
				return new List<(string,DateTime,string)>();
			}
			finally
			{
				session.Dispose();
			}
		}
		#endregion

		#region upload
		/// <summary>
		/// Upload only newer files
		/// This will not work, need to add more info on destination as its per file and multiple sub directories.
		/// May be easier to just do a blind upload until I figure out a way to address this.
		/// </summary>
		public void UploadFiles (string source, string destination, List<( string, DateTime, string )> files)
		{
			using var session = GetSession();
			session.SetWorkingDirectory(destination);
			session.Connect();	
			
			try 
			{
				files.ForEach(file => 
				{ 
					if (session.FileExists($"{destination}{file.Item1}"))
					{
						var modifiedDate = session.GetModifiedTime($"{destination}{file.Item1}");
						var remoteFileOlder = modifiedDate.CompareTo(file.Item2);
						
						if(remoteFileOlder != 1)
						{
							session.UploadFile(file.Item3, $"{destination}{file.Item1}", FtpRemoteExists.Overwrite);
						}
					}
					else 
					{
						session.UploadFile(file.Item3, $"{destination}{file.Item1}", FtpRemoteExists.Overwrite);
					}
				});
			}
			catch(FtpException)
			{
				throw;
			}
			finally
			{
				session.Dispose();
			}
		}
		#endregion

		internal FtpClient GetSession() 
		{
			return new FtpClient(_config.hostIp, _config.userId, _config.password);
		} 
	}
}
