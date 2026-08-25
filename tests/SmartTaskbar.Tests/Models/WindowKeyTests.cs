using SmartTaskbar.Core.Models;

namespace SmartTaskbar.Tests.Models;

public class WindowKeyTests
{
    [Fact]
    public void SameExeAndTitle_DifferentCase_ProduceSameKey()
    {
        var a = WindowKey.Create(@"C:\Chrome\chrome.exe", "chrome", "GitHub");
        var b = WindowKey.Create(@"c:\chrome\chrome.exe", "chrome", "github");

        Assert.Equal(a, b);
    }

    [Fact]
    public void SameExe_DifferentTitle_ProducesDifferentKeys()
    {
        var localhost = WindowKey.Create(@"C:\Chrome\chrome.exe", "chrome", "localhost:5000");
        var youtube = WindowKey.Create(@"C:\Chrome\chrome.exe", "chrome", "YouTube");

        Assert.NotEqual(localhost, youtube);
    }

    [Fact]
    public void MissingExecutablePath_FallsBackToProcessName()
    {
        var key = WindowKey.Create(string.Empty, "notepad", "sem-titulo.txt");

        Assert.StartsWith("notepad|", key);
    }
}
