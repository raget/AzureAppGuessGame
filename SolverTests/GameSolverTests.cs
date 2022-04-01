using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Solver;

public class GameSolverTests
{
    const int mockServerPort = 3013;
    const string questionPath = "/api/Question";
    const string answerPath = "/api/Answer";
    const string testQuestion = "5*6";
    private const string messageWithCode = "Congratulations!";

    private WireMockServer _server;

    [SetUp]
    public void Setup()
    {
        _server = WireMockServer.Start(mockServerPort);
        _server.Given(
                Request.Create()
                    .UsingGet()
                    .WithPath(questionPath))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithBody(testQuestion)
            );
        _server.Given(
                Request.Create()
                    .UsingPost()
                    .WithPath(answerPath))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithBody("Congratulations! Your code is: ...")
            );
    }

    [TearDown]
    public void TearDown()
    {
        _server.Stop();
    }

    [TestCase("http://localhost:3013")]
    // [TestCase("http://localhost:7071")]
    // [TestCase("https://ysoftgame.azurewebsites.net")]
    public async Task GuessCorrectAnswerAgainstRunningServers(string url)
    {
        var gameSolver = new GameSolver(new Uri(url));

        var result = await gameSolver.Guess();

        Assert.True(result.StartsWith(messageWithCode));
        Console.WriteLine(result);
    }

    [Test]
    public async Task GuessCorrectAnswerAgainstHttpClientHandlerMock()
    {
        var mockedHandler = A.Fake<HttpClientHandlerMock>(opt => opt.CallsBaseMethods());

        A.CallTo(() => mockedHandler.SendAsync(HttpMethod.Get, A<string>._))
            .ReturnsLazily(() => new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(testQuestion) });

        A.CallTo(() => mockedHandler.SendAsync(HttpMethod.Post, A<string>._))
            .Returns(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(messageWithCode) });

        var gameSolver = new GameSolver(new Uri("http://testuri"), mockedHandler);

        var result = await gameSolver.Guess();

        Console.WriteLine(result);
        Assert.True(result.StartsWith("Congratulations!"));
    }

    public abstract class HttpClientHandlerMock : HttpClientHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(SendAsync(request.Method, request.RequestUri.AbsolutePath));
        }

        public abstract HttpResponseMessage SendAsync(HttpMethod method, string absolutePath);
    }
}