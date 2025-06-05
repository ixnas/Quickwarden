using Quickwarden.Application;

namespace Quickwarden.Tests;

public class ClearClipboardTests
{
    private readonly ApplicationFixture _fixture = new();
    private ApplicationController _applicationController;

    public ClearClipboardTests()
    {
        _applicationController = _fixture.CreateApplicationController();
    }
}
