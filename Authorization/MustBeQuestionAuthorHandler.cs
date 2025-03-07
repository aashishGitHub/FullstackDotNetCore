using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using FullstackDotNetCore.Data;

namespace FullstackDotNetCore.Authorization
{
    public class MustBeQuestionAuthorHandler :
      AuthorizationHandler<MustBeQuestionAuthorRequirement>
    {
        private readonly IDataRepository _dataRepository;
        private readonly IHttpContextAccessor
         _httpContextAccessor;
        public MustBeQuestionAuthorHandler(
          IDataRepository dataRepository,
          IHttpContextAccessor httpContextAccessor)
        {
            _dataRepository = dataRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        protected async override Task
          HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MustBeQuestionAuthorRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                context.Fail();
                return;
            }

            var questionId =_httpContextAccessor.HttpContext.Request.RouteValues["questionId"];
            int questionIdAsInt = Convert.ToInt32(questionId);
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var question = await _dataRepository.GetQuestion(questionIdAsInt);
            if (question == null)
            {
                // let it through so the controller can return a 404
                context.Succeed(requirement);
                return;
            }
            if (question.UserId != userId)
            {
                context.Fail();
                return;
            }
            context.Succeed(requirement);
        }
    }
}