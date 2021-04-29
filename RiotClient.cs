using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace aramboost31
{
	// Token: 0x02000010 RID: 16
	internal class RiotClient
	{
		// Token: 0x06000040 RID: 64 RVA: 0x00004544 File Offset: 0x00002744
		public RiotClient(string appPort, string authToken, int processId)
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

		// Token: 0x06000041 RID: 65 RVA: 0x00004638 File Offset: 0x00002838
		public bool IsAlive()
		{
			return Process.GetProcesses().Any((Process x) => x.Id == this.processId);
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00004660 File Offset: 0x00002860
		public async Task<bool> LoginAsync(string username, string password)
		{
			string body = "{\"clientId\":\"riot-client\",\"trustLevels\":[\"always_trusted\"]}";
			await this.HttpPostJson("/rso-auth/v2/authorizations", body);
			string body2 = string.Concat(new string[]
			{
				"{\"username\":\"",
				username,
				"\",\"password\":\"",
				password,
				"\",\"persistLogin\":false}"
			});
			HttpResponseMessage httpResponseMessage = await this.HttpPut("/rso-auth/v1/session/credentials", body2);
			HttpResponseMessage response = httpResponseMessage;
			httpResponseMessage = null;
			bool result;
			if (!response.IsSuccessStatusCode)
			{
				result = false;
			}
			else
			{
				Thread.Sleep(1000);
				HttpResponseMessage httpResponseMessage2 = await this.HttpPostJson("/product-launcher/v1/products/league_of_legends/patchlines/live", null);
				result = true;
			}
			return result;
		}

		// Token: 0x06000043 RID: 67 RVA: 0x000046B8 File Offset: 0x000028B8
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

		// Token: 0x06000044 RID: 68 RVA: 0x00004710 File Offset: 0x00002910
		public async Task<HttpResponseMessage> HttpDelete(string apiUrl)
		{
			return await this.httpClient.DeleteAsync(this.clientUri + apiUrl);
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00004760 File Offset: 0x00002960
		public async Task<HttpResponseMessage> HttpPut(string apiUrl, string body)
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

		// Token: 0x04000066 RID: 102
		public readonly string appPort;

		// Token: 0x04000067 RID: 103
		public readonly string authToken;

		// Token: 0x04000068 RID: 104
		public readonly int processId;

		// Token: 0x04000069 RID: 105
		public readonly string clientUri;

		// Token: 0x0400006A RID: 106
		private HttpClient httpClient = null;

		// Token: 0x0400006B RID: 107
		private HttpClientHandler clientHandler = new HttpClientHandler();
	}
}
