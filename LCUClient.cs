using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace aramboost31
{
	// Token: 0x02000007 RID: 7
	internal class LCUClient
	{
		// Token: 0x0600001B RID: 27 RVA: 0x000036C4 File Offset: 0x000018C4
		public LCUClient(string appPort, string authToken, int processId)
		{
			this.appPort = appPort;
			this.authToken = authToken;
			this.processId = processId;
			this.clientHandler.ServerCertificateCustomValidationCallback = ((HttpRequestMessage sender, X509Certificate2 cert, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true);
			this.httpClient = new HttpClient(this.clientHandler);
			this.clientUri = "https://127.0.0.1:" + appPort;
			byte[] bytes = Encoding.ASCII.GetBytes("riot:" + authToken);
			this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));
			ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true));
		}

		// Token: 0x0600001C RID: 28 RVA: 0x000037B8 File Offset: 0x000019B8
		public bool IsAlive()
		{
			return Process.GetProcesses().Any((Process x) => x.Id == this.processId);
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000037E0 File Offset: 0x000019E0
		public void SetHeadless()
		{
		}



		public async Task<HttpResponseMessage> HttpGet(string apiUrl)
		{
			return await this.httpClient.GetAsync(this.clientUri + apiUrl);
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00003978 File Offset: 0x00001B78
		public async Task<HttpResponseMessage> HttpPostJson(string apiUrl, string body)
		{
			bool flag = body == "" || body == null;
			HttpResponseMessage result;
			if (flag)
			{
				HttpResponseMessage httpResponseMessage = await this.httpClient.PostAsync(this.clientUri + apiUrl, null);
				result = httpResponseMessage;
			}
			else
			{
				using (StringContent stringContent = new StringContent(body, Encoding.UTF8, "application/json"))
				{
					HttpResponseMessage httpResponseMessage2 = await this.httpClient.PostAsync(this.clientUri + apiUrl, stringContent);
					result = httpResponseMessage2;
				}
			}
			return result;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x000039D0 File Offset: 0x00001BD0
		public async Task<HttpResponseMessage> HttpPutJson(string apiUrl, string body)
		{
			bool flag = body == "" || body == null;
			HttpResponseMessage result;
			if (flag)
			{
				HttpResponseMessage httpResponseMessage = await this.httpClient.PutAsync(this.clientUri + apiUrl, null);
				result = httpResponseMessage;
			}
			else
			{
				using (StringContent stringContent = new StringContent(body, Encoding.UTF8, "application/json"))
				{
					HttpResponseMessage httpResponseMessage2 = await this.httpClient.PutAsync(this.clientUri + apiUrl, stringContent);
					result = httpResponseMessage2;
				}
			}
			return result;
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00003A28 File Offset: 0x00001C28
		public async Task<HttpResponseMessage> HttpPostForm(string apiUrl, IEnumerable<KeyValuePair<string, string>> formData)
		{
			HttpResponseMessage result;
			using (FormUrlEncodedContent content = new FormUrlEncodedContent(formData))
			{
				HttpResponseMessage httpResponseMessage = await this.httpClient.PostAsync(this.clientUri + apiUrl, content);
				result = httpResponseMessage;
			}
			return result;
		}

		public async Task<HttpResponseMessage> HttpPatchForm(string apiUrl, Uri requestUri, HttpContent iContent)
		{
			var method = new HttpMethod("PATCH");
			var request = new HttpRequestMessage(method, apiUrl)
			{
				Content = iContent
			};

			HttpResponseMessage response = new HttpResponseMessage();
			try
			{
				response = await this.httpClient.SendAsync(request);
			}
			catch (TaskCanceledException e)
			{
				Debug.WriteLine("ERROR: " + e.ToString());
			}

			return response;
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00003A80 File Offset: 0x00001C80
		public async Task<HttpResponseMessage> HttpPutForm(string apiUrl, IEnumerable<KeyValuePair<string, string>> formData)
		{
			HttpResponseMessage result;
			using (FormUrlEncodedContent content = new FormUrlEncodedContent(formData))
			{
				HttpResponseMessage httpResponseMessage = await this.httpClient.PutAsync(this.clientUri + apiUrl, content);
				result = httpResponseMessage;
			}
			return result;
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00003AD8 File Offset: 0x00001CD8
		public async Task<HttpResponseMessage> HttpPostForm(string apiUrl, KeyValuePair<string, string> formData)
		{
			HttpResponseMessage result;
			using (FormUrlEncodedContent content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
			{
				formData
			}))
			{
				HttpResponseMessage httpResponseMessage = await this.httpClient.PostAsync(this.clientUri + apiUrl, content);
				result = httpResponseMessage;
			}
			return result;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00003B30 File Offset: 0x00001D30
		public async Task<HttpResponseMessage> HttpDelete(string apiUrl)
		{
			return await this.httpClient.DeleteAsync(this.clientUri + apiUrl);
		}

		// Token: 0x04000027 RID: 39
		public readonly string appPort;

		// Token: 0x04000028 RID: 40
		public readonly string authToken;

		// Token: 0x04000029 RID: 41
		public readonly int processId;

		// Token: 0x0400002A RID: 42
		public readonly string clientUri;

		// Token: 0x0400002B RID: 43
		private HttpClient httpClient = null;

		// Token: 0x0400002C RID: 44
		private HttpClientHandler clientHandler = new HttpClientHandler();
	}
}
