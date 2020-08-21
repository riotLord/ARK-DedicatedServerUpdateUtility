#region usings
using System;
using System.Threading;
using System.Threading.Tasks;
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

        #region download examples
        public static void DownloadFile(string source, string destination, string fileName)
		{
			using (var ftp = new FtpClient("127.0.0.1", "ftptest", "ftptest"))
			{
				ftp.Connect();

				// define the progress tracking callback
				Action<FtpProgress> progress = delegate (FtpProgress p) {
					if (p.Progress == 1)
					{
						// all done!
					}
					else
					{
						//percent done = (p.Progress * 100)
					}
				};

				// download a file with progress tracking
				ftp.DownloadFile($@"{source}{fileName}", $"/{destination}{fileName}", FtpLocalExists.Overwrite, FtpVerify.None, progress);

			}
		}
        #region async
        public static async Task DownloadFileAsync(string source, string destination, string fileName)
		{
			var token = new CancellationToken();
			using (var ftp = new FtpClient("127.0.0.1", "ftptest", "ftptest"))
			{
				await ftp.ConnectAsync(token);

				// define the progress tracking callback
				Progress<FtpProgress> progress = new Progress<FtpProgress>(p => {
					if (p.Progress == 1)
					{
						// all done!
					}
					else
					{
						// percent done = (p.Progress * 100)
					}
				});

				// download a file and ensure the local directory is created
				await ftp.DownloadFileAsync($@"{source}{fileName}", $"/{destination}{fileName}", FtpLocalExists.Append, FtpVerify.None, progress, token);

			}
		}
		#endregion
		#endregion
	}
}
