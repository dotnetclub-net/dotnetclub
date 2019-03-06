using System;

namespace Discussion.Web.Services.UserManagement.Exceptions
{
    public class FeatureDisabledException : InvalidOperationException
    {
        public FeatureDisabledException() : base("当前站点已设置为关闭该功能")
        {

        }
    }
}