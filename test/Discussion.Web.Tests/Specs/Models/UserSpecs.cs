using System;
using Discussion.Core.Models;
using Discussion.Core.Time;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.Specs.Models
{
    public class UserSpecs
    {
        private readonly IClock _clock;
        public UserSpecs()
        {
            var mockClock = new Mock<IClock>();
            mockClock.SetupGet(t => t.Now).Returns(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero));
            _clock = mockClock.Object;
        }
        
        [Fact]
        public void should_modify_phone_number_if_not_verified()
        {
            var user = new User();

            Assert.True(user.CanModifyPhoneNumberNow(_clock));
        }
        
        [Fact]
        public void should_not_modify_phone_number_if_verified_3_days_ago()
        {
            var user = CreateVerifiedUser(DateTime.UtcNow.AddDays(-3), null);

            Assert.False(user.CanModifyPhoneNumberNow(_clock));
        }

        [Fact]
        public void should_not_modify_phone_number_if_updated_3_days_ago()
        {
            var user = CreateVerifiedUser(DateTime.UtcNow.AddDays(-8), DateTime.UtcNow.AddDays(-3));

            Assert.False(user.CanModifyPhoneNumberNow(_clock));
        }

        [Fact]
        public void should_modify_phone_number_if_verified_8_days_ago()
        {
            var user = CreateVerifiedUser(DateTime.UtcNow.AddDays(-8), null);

            Assert.True(user.CanModifyPhoneNumberNow(_clock));
        }

        private static User CreateVerifiedUser(DateTime? initialVerifiedAtUtc, DateTime? updatedAtUtc)
        {
            var user = new User();
            user.VerifiedPhoneNumber = new VerifiedPhoneNumber()
            {
                Id = 10
            };
            
            
            if (initialVerifiedAtUtc != null)
            {
                user.VerifiedPhoneNumber.CreatedAtUtc = initialVerifiedAtUtc.Value;
            }

            if (updatedAtUtc != null)
            {
                user.VerifiedPhoneNumber.ModifiedAtUtc = updatedAtUtc.Value;
            }
            
            user.PhoneNumberId = user.VerifiedPhoneNumber.Id;
            return user;
        }
    }
}