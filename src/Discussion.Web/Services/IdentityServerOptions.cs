namespace Discussion.Web.Services
{
    public class IdentityServerOptions
    {
        /// <summary>
        /// 是否启用外部身份服务
        /// </summary>
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// 外部身份服务的 Id
        /// </summary>
        public string ProviderId { get; set; }

        /// <summary>
        /// 外部身份服务的网址
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// 外部身份是否要求 HTTPS
        /// </summary>
        public bool RequireHttpsMetadata { get; set; }

        /// <summary>
        /// 本站在身份服务处注册的客户编号
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// 本站在身份服务处注册的客户密钥
        /// </summary>
        public string ClientSecret { get; set; }
        
        /// <summary>
        /// 在验证由身份服务颁发的令牌时所用的目标受众字段值
        /// </summary>
        public string TokenAudience { get; set; }
        
        /// <summary>
        /// 在验证由身份服务颁发的令牌时所用的颁发方名称
        /// </summary>        
        public string TokenIssuer { get; set; }

        /// <summary>
        /// 身份服务注册新用户的 URL
        /// </summary>
        public string RegisterUri { get; set; }

        /// <summary>
        /// 身份服务修改密码的 URL
        /// </summary>
        public string ChangePasswordUri { get; set; }
    }
}