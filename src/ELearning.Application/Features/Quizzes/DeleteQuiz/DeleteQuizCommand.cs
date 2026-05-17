using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.DeleteQuiz;

public sealed record DeleteQuizCommand(Guid Id) : IRequest<Result>;
