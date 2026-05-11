using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.PublishQuiz;

public sealed record PublishQuizCommand(Guid Id) : IRequest<Result<QuizDto>>;
