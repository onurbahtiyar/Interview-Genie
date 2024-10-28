using Backend.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities;

public partial class InterviewQuestion
{
    public Guid Id { get; set; }

    public Guid InterviewSessionId { get; set; }

    public string QuestionText { get; set; } = null!;

    public QuestionType QuestionType { get; set; }

    public string[]? Options { get; set; }

    public string CorrectAnswer { get; set; } = null!;

    public string? UserAnswer { get; set; }

    public bool? IsCorrect { get; set; }

    public string? Topic { get; set; }

    public virtual InterviewSession InterviewSession { get; set; } = null!;
}
