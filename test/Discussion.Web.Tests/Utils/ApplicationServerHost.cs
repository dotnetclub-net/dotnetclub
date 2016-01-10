using System;

namespace Discussion.Web.Tests
{
    public class ApplicationServerHost: IDisposable
    {
        static ApplicationServerHost()
        {
            // bootstrap
        }

        public ApplicationServerHost()
        {
            // initialize every time
        }


        void IDisposable.Dispose()
        {
            // recyle resources
        }
    }

}
