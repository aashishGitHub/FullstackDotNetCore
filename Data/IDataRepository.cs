using System;
using FullstackDotNetCore.Data.Models;
namespace FullstackDotNetCore.Data
{
    public interface IDataRepository
    {
        IEnumerable<QuestionGetManyResponse> GetQuestions();
        IEnumerable<QuestionGetManyResponse>
        GetQuestionsBySearch(string search);
        IEnumerable<QuestionGetManyResponse>
          GetUnansweredQuestions();
        QuestionGetSingleResponse
          GetQuestion(int questionId);
        bool QuestionExists(int questionId);
        AnswerGetResponse GetAnswer(int answerId);
    }
}