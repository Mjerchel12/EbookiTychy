//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Moq;
//using Moq.EntityFrameworkCore;
//using Tychy.Components;
//using Xunit;

//public class MonthlyCheckServiceTests
//{
//    [Fact]
//    public void GetNextFirstDayOfMonth_ShouldReturnCorrectDate()
//    {
//        // Arrange
//        var service = CreateService();

//        // Act & Assert - testuj różne miesiące
//        var testDate = new DateTime(2024, 1, 15);
//        var result = InvokePrivateMethod(service, "GetNextFirstDayOfMonth", testDate);

//        Assert.Equal(new DateTime(2024, 2, 1), result);
//    }

//    [Fact]
//    public async Task ExecuteMonthlyTask_ShouldResetFlags()
//    {
//        // Arrange
//        var mockContext = new Mock<AppDbContext>();
//        var readers = new List<Reader>
//        {
//            new Reader { Id = 1, HasUnusedCodeLastMonth = true },
//            new Reader { Id = 2, HasUnusedCodeLastMonth = true }
//        };

//        mockContext.Setup(x => x.Readers).ReturnsDbSet(readers);

//        var service = CreateService();

//        // Act
//        await InvokePrivateMethod(service, "ExecuteMonthlyTask", mockContext.Object);

//        // Assert
//        Assert.All(readers, r => Assert.False(r.HasUnusedCodeLastMonth));
//    }
//}