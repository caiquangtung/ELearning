using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.RemoveQuestion;

public sealed record RemoveQuestionCommand(Guid QuizId, Guid QuestionId) : IRequest<Result>;
