using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend.Domain.Entities
{
    public enum QuestionType
    {
        MultipleChoice,
        OpenEnded
    }

    public class InterviewQuestion
    {
        [Key]
        public Guid Id { get; set; }
        public Guid InterviewSessionId { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public string CorrectAnswer { get; set; }
        public string UserAnswer { get; set; }
        public bool? IsCorrect { get; set; }
        public DateTime? AskedAt { get; set; }
        public DateTime? AnsweredAt { get; set; }

        // Navigation Property
        public InterviewSession InterviewSession { get; set; }
    }
}
