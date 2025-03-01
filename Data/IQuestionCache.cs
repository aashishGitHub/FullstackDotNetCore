using FullstackDotNetCore.Data.Models;
namespace FullstackDotNetCore.Data
{
  public interface IQuestionCache
  {
    QuestionGetSingleResponse Get(int questionId);
    void Remove(int questionId);
    void Set(QuestionGetSingleResponse question);
} }