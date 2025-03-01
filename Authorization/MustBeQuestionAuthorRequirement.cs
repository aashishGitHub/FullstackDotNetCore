using Microsoft.AspNetCore.Authorization;
namespace FullstackDotNetCore.Authorization
{
    public class MustBeQuestionAuthorRequirement :
      IAuthorizationRequirement
    {
        public MustBeQuestionAuthorRequirement()
        {
        }
    }
}