using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discussion.Core.Models;
using Discussion.Web.Services.UserManagement.Avatar;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Discussion.Web.Services.UserManagement
{
    public class KeyCloakUserUpdater
    {
        private readonly HttpMessageInvoker _httpInvoker;
        private readonly ExternalIdentityServiceOptions _externalIdpOptions;
        private readonly string _keyCloakBaseUrl;
        private readonly ILogger<KeyCloakUserUpdater> _logger;
        private readonly IAvatarUrlService _avatarUrlService;

        public KeyCloakUserUpdater(IOptions<ExternalIdentityServiceOptions> smsSendingOptions, HttpMessageInvoker httpInvoker, ILogger<KeyCloakUserUpdater> logger, IAvatarUrlService avatarUrlService)
        {
            _httpInvoker = httpInvoker;
            _logger = logger;
            _avatarUrlService = avatarUrlService;
            _externalIdpOptions = smsSendingOptions.Value;
            
            var idpAuthorityUrl = new Uri(_externalIdpOptions.Authority);
            _keyCloakBaseUrl = idpAuthorityUrl.GetComponents(UriComponents.Scheme | UriComponents.Host | UriComponents.Port, UriFormat.Unescaped);
        }
        
        public async Task UpdateUserInfo(User discussionUser)
        {
            var accessToken = await GetAccessToken();
            if (accessToken == null)
            {
                _logger.LogWarning("用户信息同步至 IdP 失败：{@UserUpdateRequest}", new { UserId = discussionUser.Id, discussionUser.UserName, Result = "无法获取 KeyCloak 访问令牌" });
            }
            
            const string realmPathToken = "/realms/";
            var realmName = _externalIdpOptions.Authority.Substring(_externalIdpOptions.Authority.LastIndexOf(realmPathToken, StringComparison.Ordinal) + realmPathToken.Length).TrimEnd('/');
            var updateUrl = $"{_keyCloakBaseUrl}/auth/admin/realms/{realmName}/users/{discussionUser.OpenId}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, updateUrl)
            {
                Content = new StringContent(ComposeKeyCloakUserRepresentation(discussionUser), Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var httpResponseMessage = await _httpInvoker.SendAsync(requestMessage, CancellationToken.None);
            httpResponseMessage.EnsureSuccessStatusCode();
            _logger.LogWarning("用户信息同步至 IdP 成功：{@UserUpdateRequest}", new { UserId = discussionUser.Id, discussionUser.UserName, Result = $"收到 {httpResponseMessage.StatusCode} 响应代码" });
        }


        private async Task<string> GetAccessToken()
        {
            var tokenUrl = $"{_keyCloakBaseUrl}/auth/realms/master/protocol/openid-connect/token";
            var credentialParts = _externalIdpOptions?.KeyCloakAdminCredential?.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (credentialParts == null || credentialParts.Length < 2)
            {
                _logger.LogWarning("获取 IdP 访问令牌失败：{@AccessTokenRequest}", new { Url = tokenUrl, Reason = "未配置 KeyCloak 管理员凭据" });
                return null;
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("username", credentialParts[0]),
                    new KeyValuePair<string, string>("password", credentialParts[1]),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("client_id", "admin-cli")
                })
            };

            var httpResponseMessage = await _httpInvoker.SendAsync(requestMessage, CancellationToken.None);
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                _logger.LogWarning("获取 IdP 访问令牌失败：{@AccessTokenRequest}", new { Url = tokenUrl, Reason = $"从 KeyCloak 返回了错误的响应代码 {httpResponseMessage.StatusCode}" });
                return null;
            }
            var responseJson = await httpResponseMessage.Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<JObject>(responseJson).GetValue("access_token").ToString();
        }

        private string ComposeKeyCloakUserRepresentation(User discussionUser)
        {
            var attrs = new Dictionary<string, Object>
            {
                {"nickname", discussionUser.DisplayName},
                {"name", discussionUser.DisplayName}
            };
            
            if (discussionUser.VerifiedPhoneNumber != null)
            {
                attrs["phone_number"] = discussionUser.VerifiedPhoneNumber.PhoneNumber;
                attrs["phone_number_verified"] = true;
            }
            
            if (discussionUser.AvatarFile != null)
            {
                attrs["picture"] = _avatarUrlService.GetAvatarUrl(discussionUser);
            }

            var dic = new Dictionary<string, Object>()
            {
                {"email", discussionUser.EmailAddress},
                {"emailVerified", discussionUser.EmailAddressConfirmed},
                {"attributes", attrs}
            };

            return JsonConvert.SerializeObject(dic);
        }

    }
}