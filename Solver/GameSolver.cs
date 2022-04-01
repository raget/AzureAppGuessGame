namespace Solver;

public class GameSolver
{
    private const string QuestionPath = "/api/Question";
    private const string AnswerPath = "/api/Answer";

    private readonly HttpClient _httpClient;

    public GameSolver(Uri gameUri, HttpClientHandler? httpClientHandler = null)
    {
        httpClientHandler ??= new HttpClientHandler();
        _httpClient = new HttpClient(httpClientHandler) { BaseAddress = gameUri };
    }

    public async Task<string> Guess()
    {
        var questionRequest = await _httpClient.GetAsync(QuestionPath);
        var questionContent = await questionRequest.Content.ReadAsStringAsync();
        var numbers = questionContent
            .Split("*")
            .Select(int.Parse)
            .ToArray();

        var answer = numbers[0] * numbers[1];
        var answerContent = new StringContent(answer.ToString());

        var result = await _httpClient.PostAsync(AnswerPath, answerContent);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadAsStringAsync();
        }

        throw new Exception("Non-successful return code returned");
    }
}