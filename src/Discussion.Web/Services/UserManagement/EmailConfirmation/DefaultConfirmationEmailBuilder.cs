using System.Text;

namespace Discussion.Web.Services.UserManagement.EmailConfirmation
{
    public class DefaultConfirmationEmailBuilder: IConfirmationEmailBuilder
    {
        public string BuildEmailBody(string displayName, string callbackUrl)
        {
            var body = new StringBuilder();
            
            body.Append("<!DOCTYPE html>");
            body.Append("<html><head>");
            body.Append(@"<meta http-equiv='Content-Type' content='text/html;charset = UTF-8'>");
            body.Append("<title>dotnet club 用户邮件地址确认</title>");
            body.Append("</head>");
            body.Append("<body style='background:#fff;'>");
            body.Append("<div style='margin: 30px; background:#fff;border:1px solid #ccc;'>");
            body.Append("<h1 style='margin:0; padding:10px; font-size:14px; background:#333;color:#fff;'>");
            body.Append("dotnet club 用户邮件地址确认");
            body.Append("</h1>");
            body.Append("<div style='padding:30px;'>");
            body.Append($"<p> {displayName} 你好， </p>");
            body.Append("<p> 感谢你注册 dotnet club 并验证邮箱地址，请点击下面按钮以验证您的邮件地址： </p>");
            body.Append($@"<a href='{callbackUrl}'{"style='display:inline-block;padding:10px 15px;background:#67C23A;border-radius:5px;color:#fff;text-decoration:none;'"}'>验证邮件地址</a>");
            body.Append("<p>如果你无法通过点击上面的按钮验证邮件地址，请点击下面的链接或者把它复制到浏览器地址栏并访问。</p>");
            body.Append($"<a href='#'>{callbackUrl}</a>");
            body.Append("<p style='font-size:12px;'>再次感谢你的使用！<br /></p>");
            body.Append("<p style='font-size:12px;'>dotnet club<br /></p>");
            body.Append("<p style='font-size:12px;font-style:italic'>如果你没有注册 dotnet club.net，请忽略本邮件。如果你确信你的邮件地址被他人误用，<a href='https://jinshuju.net/f/PzMDyb' target='_blank'>请转到此处举报</a>。</p>");
            body.Append("</div> </div> </body> </html>");
          
            return body.ToString();
        }
    }
}