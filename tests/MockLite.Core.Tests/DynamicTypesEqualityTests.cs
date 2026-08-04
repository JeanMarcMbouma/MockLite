namespace BbQ.MockLite.Tests
{
    public class DynamicTypesEqualityTests
    {
        interface ITest
        {
            void MethodA(object parameter);
        }

        [Fact]
        public void DynamicTypes_ShouldBeEqual()
        {
            // Arrange
            var mock = new Mock<ITest>();
            // Act
            mock.Object.MethodA(new { Name = "John", Age = 30 });
            mock.Object.MethodA(new { Name = "John", Age = 30 });
            // Assert
            mock.Verify(m => m.MethodA(It.Matches<object>(o => o.Equals(new { Name = "John", Age = 30 }))), Times.Exactly(2));
        }

        [Fact]
        public void DynamicTypes_Setup_ShouldBeEqual()
        {
            // Arrange
            var mock = new Mock<ITest>();
            var data = new { Name = "John", Age = 30 };
            object? capturedParameter = null;

            mock.OnCall<object>(m => m.MethodA(It.Matches<object>(o => o.Equals(data))), (param) => capturedParameter = param);
            // Act
            mock.Object.MethodA(new { Name = "John", Age = 30 });
            // Assert
            Assert.Equal(data, capturedParameter);
        }

    }
}
