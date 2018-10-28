using Discussion.Core.Data;
using Discussion.Core.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discussion.Web.Services.Emailconfirmation
{
    public static class EmailExtensions
    {
        private const string DotnetTokenTag = "dotnetclub";
        public static string SplicedMailTemplate(string callBack)
        {
            StringBuilder body = new StringBuilder();
            body.Append("<!DOCTYPE html>");
            body.Append("<html><head>");
            body.Append(@"<meta http-equiv='Content-Type' content='text/html;charset = UTF-8'>");
            body.Append("<title>dotnetclub用户确认</title>");
            body.Append("</head>");
            body.Append("<body style='background:#fff;'>");
            body.Append("<div style='margin: 30px; background:#fff;border:1px solid #ccc;'>");
            body.Append("<h1 style='margin:0; padding:10px; font-size:14px; background:#333;color:#fff;'>");
            body.Append("dotnetclub用户确认");
            body.Append("</h1>");
            body.Append("<div style='padding:30px;'>");
            body.Append("<p> 感谢您绑定dotnetclub邮箱，点击下面按钮以验证您的邮箱 </p>");
            body.Append($@"<a href='{callBack}'{"style='display:inline-block;padding:10px 15px;background:#67C23A;border-radius:5px;color:#fff;text-decoration:none;'"}'>验证电子邮箱</a>");
            body.Append("<p>如果你无法通过上面按钮验证电子邮箱，请点击下面的链接或者把它复制到浏览器地址栏。</p>");
            body.Append($"<a href='#'>{callBack}</a>");
            body.Append("<p style='font-size:12px;'>感谢您的使用！<br/>dotnetclub</p>");
            body.Append("</div> </div> </body> </html>");
            return body.ToString();
        }
        public static async Task<bool> ConfirmEmailByProtectorAsync<TUser>(this UserManager<TUser> userManager, IRepository<EmailBindOptions> emailRepository, TUser user, string token)  //这里传入的Manager和Repository 应该通过DI方式注入进来
            where TUser:class,IUser
        {
            bool confirmResult = false;
            if(user == null || string.IsNullOrEmpty(token))
                throw new ArgumentException(nameof(user));
            string[] tokens = token.Split('|');
            if(tokens.Length != 2)
                throw new FormatException("token format error");
            string userId = tokens[0];
            string tokentag = tokens[1];
            if(!tokentag.Equals(DotnetTokenTag)||!userId.Equals(user.Id.ToString()))
                throw new NotFiniteNumberException("tokentag is wrong");
            user.IsActivation = true;
            var userResult = await userManager.UpdateAsync(user);
            var tokenOption = emailRepository.All().FirstOrDefault(t => t.CallbackToken.Equals(token));
            tokenOption.IsActivation = true;
            emailRepository.Update(tokenOption);
            if (userResult.Succeeded)
                confirmResult = true;
            return confirmResult;
        }
    }
}
