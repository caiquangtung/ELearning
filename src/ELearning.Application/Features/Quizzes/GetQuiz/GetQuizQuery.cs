using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.GetQuiz;

public sealed record GetQuizQuery(Guid Id) : IRequest<Result<QuizDetailDto>>;
