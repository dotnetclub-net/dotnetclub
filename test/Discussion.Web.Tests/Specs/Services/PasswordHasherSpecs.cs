using System;
using Discussion.Web.Services;
using Xunit;

namespace Discussion.Web.Tests.Specs.Services
{
    public class PasswordHasherSpecs
    {
        [Theory]
        [InlineData("password0")]
        [InlineData("%^$&^%:\"p")]
        [InlineData("YW3 weyi34fru")]
        public void should_hash_passwords(string password)
        {
            var hashed1st = Hash(password);
            var hashed2nd = Hash(password);
            var hashed3rd = Hash(password);
            
            Assert.NotEqual(password, hashed1st);
            Assert.NotEqual(password, hashed2nd);
            Assert.NotEqual(password, hashed3rd);
        }

        [Theory]
        [InlineData("234p&*word0")]
        [InlineData("GJ%^$&ZTL^%:\"p")]
        [InlineData("YW3<))(i34fru")]
        public void should_hash_password_differently_for_same_password(string password)
        {
            var hashed1st = Hash(password);
            var hashed2nd = Hash(password);
            var hashed3rd = Hash(password);
            
            Assert.NotEqual(hashed1st, hashed2nd);
            Assert.NotEqual(hashed1st, hashed3rd);
            Assert.NotEqual(hashed2nd, hashed3rd);
        }
        
        [Theory]
        [InlineData("word0pass")]
        [InlineData("%p^&NMa2")]
        [InlineData("YW3 34fruweyi")]
        public void should_be_able_to_verify_hashed_password(string password)
        {
            var hashed = Hash(password);
            var hashedBytes = Convert.FromBase64String(hashed);
            
            Assert.True(PasswordHasher.VerifyHashedPassword(hashedBytes, password));
        }
        

        private static string Hash(string password)
        {
            return Convert.ToBase64String(PasswordHasher.HashPassword(password));
        }
    }
}