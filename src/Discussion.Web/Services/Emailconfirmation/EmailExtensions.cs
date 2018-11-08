using Discussion.Core.Data;
using Discussion.Core.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discussion.Web.Services.EmailConfirmation
{
    public static class EmailExtensions
    {
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

    }
}
