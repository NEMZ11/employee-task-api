using EmployeeTaskApi.Api.Services;

namespace EmployeeTaskApi.Tests;

public sealed class PasswordServiceTests
{
    [Fact]
    public void Hash_VerifiesOriginalPassword()
    {
        var service = new PasswordService();

        var hash = service.Hash("CorrectHorseBatteryStaple!");

        Assert.True(service.Verify("CorrectHorseBatteryStaple!", hash));
        Assert.False(service.Verify("wrong-password", hash));
    }
}
