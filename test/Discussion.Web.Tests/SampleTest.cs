using Discussion.Core.Models;
using Discussion.Web.ViewModels;
using Moq;
using Xunit;

namespace Discussion.Web.Tests
{
    public class SampleTest
    {
        [Fact]
        public void ExampleTest()
        {
            Assert.True(true);
        }
        
        [Fact]
        public void TestVirtualMoq()
        {
            var mock = new Mock<TopicViewModel>();

            mock.SetupGet(x => x.Topic).Returns(new Topic(){Title = "mocked"});
            
            var mockObject = mock.Object;
            Assert.NotNull(mockObject.Topic);
            Assert.Equal("mocked", mockObject.Topic.Title);
        }
        
        [Fact]
        public void TestNestedMoq()
        {
            var mock = new Mock<TopicViewModel.NestedClass>();

            mock.SetupGet(x => x.Title).Returns("mocked");
            
            var mockObject = mock.Object;
            Assert.Equal("mocked", mockObject.Title);
        }


    }
}
