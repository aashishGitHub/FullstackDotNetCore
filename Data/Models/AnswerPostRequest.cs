using System;
using System.ComponentModel.DataAnnotations;
namespace FullstackDotNetCore.Data.Models
{
	public class AnswerPostRequest
	{
        [Required]
        public int? QuestionId { get; set; }
        [Required]
        public string Content { get; set; }
	}
}