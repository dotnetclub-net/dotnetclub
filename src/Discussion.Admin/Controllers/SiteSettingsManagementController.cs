using System.Linq;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Admin.Controllers
{
    [Authorize]
    public class SiteSettingsManagementController: ControllerBase
    {
        private readonly IRepository<SiteSettings> _settingsRepo;

        public SiteSettingsManagementController(IRepository<SiteSettings> settingsRepo)
        {
            _settingsRepo = settingsRepo;
        }

        [Route("api/settings")]
        [HttpGet]
        public ApiResponse GetSettings()
        {
            var settings = _settingsRepo.All().FirstOrDefault();
            if (settings == null)
            {
                return ApiResponse.NoContent();
            }
            
            return ApiResponse.ActionResult(settings);
        }
        
        [Route("api/settings")]
        [HttpPut]
        public ApiResponse UpdateSettings([FromBody]SiteSettings settings)
        {
            var existingSettings = _settingsRepo.All().FirstOrDefault();
            if (existingSettings == null)
            {
                _settingsRepo.Save(settings);
            }
            else
            {
                existingSettings.RequireUserPhoneNumberVerified = settings.RequireUserPhoneNumberVerified;
                existingSettings.PublicHostName = settings.PublicHostName;
                
                existingSettings.EnableNewUserRegistration = settings.EnableNewUserRegistration;
                existingSettings.EnableNewTopicCreation  = settings.EnableNewTopicCreation ;
                existingSettings.EnableNewReplyCreation  = settings.EnableNewReplyCreation ;
                existingSettings.IsReadonly  = settings.IsReadonly ;
                
                existingSettings.FooterNoticeLeft = settings.FooterNoticeLeft;
                existingSettings.FooterNoticeRight = settings.FooterNoticeRight;
                
                existingSettings.HeaderLink1Text = settings.HeaderLink1Text;
                existingSettings.HeaderLink1Url = settings.HeaderLink1Url;
                
                existingSettings.HeaderLink2Text = settings.HeaderLink2Text;
                existingSettings.HeaderLink2Url = settings.HeaderLink2Url;
                
                existingSettings.HeaderLink3Text = settings.HeaderLink3Text;
                existingSettings.HeaderLink3Url = settings.HeaderLink3Url;
                
                existingSettings.HeaderLink4Text = settings.HeaderLink4Text;
                existingSettings.HeaderLink4Url = settings.HeaderLink4Url;
                
                _settingsRepo.Update(existingSettings);
            }
            
            return ApiResponse.NoContent();
        }
    }
}