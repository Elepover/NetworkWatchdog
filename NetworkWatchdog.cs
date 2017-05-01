using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using System.Net;

namespace NetworkWatchdog
{
	/// <summary>
	/// Captive 请求获取到的连接状态
	/// </summary>
	public enum NetworkAvailability
	{
		/// <summary>
		/// OK, 服务器返回了 HTTP 204 状态码。
		/// </summary>
		OK = 0,
		/// <summary>
		/// 无法连接到服务器。
		/// </summary>
		UnableToConnect = 1,
		/// <summary>
		/// 已成功连接到服务器，但被重定向，可能需要登录。
		/// </summary>
		Redirected = 2,
		/// <summary>
		/// TTL 传输中过期。
		/// </summary>
		TTLExpired = 3,
		/// <summary>
		/// 服务器响应超时。
		/// </summary>
		Timeout = 4,
		/// <summary>
		/// 未知，服务器可能返回了非 301-303 307 204 的状态码。
		/// </summary>
		Unknown = 5
	}

	/// <summary>
	/// 一个基于 Android Captive Portal Detection 原理的网络连接监测器
	/// </summary>
	public class NetworkWatchdog
	{
		//Private Vars
		private const string URL_SUFFIX = "/generate_204";
		private long pInterval;
		private string pCaptive;
		private NetworkAvailability pLastResult = NetworkAvailability.Unknown;
		private bool pCurrentRunning;
		private bool pInitialized = false;
		private Thread pThr;

		private string pMethod = "HEAD";
		private void InitializeCheck()
		{
			if (!pInitialized)
				throw new System.InvalidOperationException("没有初始化。");
		}

		private void CheckNetAvi()
		{
			try {
				Redo:
				//Create request and send.
				HttpWebRequest req = HttpWebRequest.Create(pCaptive + "/generate_204");
				var _with1 = req;
				_with1.Method = pMethod;
				_with1.AllowAutoRedirect = false;
				_with1.Timeout = pInterval - 500;
				_with1.UserAgent = "Mozilla/5.0 (Windows NT;) NetworkWathdog/0.0.0.0";
				HttpWebResponse res = req.GetResponse();
				NetworkAvailability resCode = default(NetworkAvailability);
				Console.WriteLine("[NetworkWatchdog]: Current status code: " + res.StatusCode + " - " + res.StatusDescription);

				if (res.StatusCode == HttpStatusCode.NoContent) {
					resCode = NetworkAvailability.OK;
				} else if (res.StatusCode == 301) {
					resCode = NetworkAvailability.Redirected;
				} else if (res.StatusCode == 302) {
					resCode = NetworkAvailability.Redirected;
				} else if (res.StatusCode == 303) {
					resCode = NetworkAvailability.Redirected;
				} else if (res.StatusCode == 307) {
					resCode = NetworkAvailability.Redirected;
				} else {
					resCode = NetworkAvailability.Unknown;
				}

				if (!(resCode == pLastResult)) {
					Console.WriteLine("[NetworkWatchdog]: Raising NetworkAvailabilityChanged event.");
					pLastResult = resCode;
					if (NetworkAvailabilityChanged != null) {
						NetworkAvailabilityChanged(this, pLastResult);
					}
				}

				Thread.Sleep(pInterval);
				goto Redo;

			} catch (Exception thrEnd) {
				return;
			} catch (Exception ex) {
				if (NetworkWatchdogErrorOccurred != null) {
					NetworkWatchdogErrorOccurred(this, ex);
				}
				Thread.Sleep(pInterval);
				goto Redo;
			}
		}

		//Events
		/// <summary>
		/// 网络连接可用性改变
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged;
		public delegate void NetworkAvailabilityChangedEventHandler(object sender, NetworkAvailability e);
		/// <summary>
		/// 监控器遇到错误
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public event NetworkWatchdogErrorOccurredEventHandler NetworkWatchdogErrorOccurred;
		public delegate void NetworkWatchdogErrorOccurredEventHandler(object sender, Exception e);

		/// <summary>
		/// 监测网络连接的时间间隔。
		/// </summary>
		/// <returns></returns>
		public long CheckInterval {
			get {
				InitializeCheck();
				return pInterval;
			}
		}
		/// <summary>
		/// 设置的 Captive Portal Server
		/// </summary>
		/// <returns></returns>
		public string CaptivePortal {
			get {
				InitializeCheck();
				return pCaptive;
			}
		}

		/// <summary>
		/// 初始化一个新的 NetworkWatchdog
		/// </summary>
		/// <param name="interval">监测时间间隔，单位为 ms，最低为 1000ms。与此同时，响应超时将被设为 interval - 500</param>
		/// <param name="captive"></param>
		public NetworkWatchdog(long interval = 10000, string captive = "https://google.cn", string method = "HEAD")
		{
			if (interval < 1000)
				throw new System.ArgumentOutOfRangeException("监测时间间隔必须大于 1000ms.");
			pInterval = interval;
			pMethod = method;
			pCaptive = captive;
			pCurrentRunning = false;
			pThr = new Thread(CheckNetAvi);
			//实际上这个初始化过程没有任何必要
			pInitialized = true;
		}
		/// <summary>
		/// 根据当前设置，开始异步检查网络连接可用性。
		/// </summary>
		public void BeginCheckingNetworkAvailability()
		{
			InitializeCheck();
			if (pThr.IsAlive)
				throw new ThreadStateException("检测器已经在运行。");
			pThr = new Thread(CheckNetAvi);
			pThr.Start();
		}

		public void StopCheckingNetworkAvailability()
		{
			InitializeCheck();
			if (pThr.IsAlive == false)
				throw new ThreadStateException("检测器没有运行。");
			pThr.Abort();
		}
	}
}
