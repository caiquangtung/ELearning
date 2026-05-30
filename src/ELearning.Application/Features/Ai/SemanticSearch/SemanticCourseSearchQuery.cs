using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.SemanticSearch;

public sealed record SemanticCourseSearchQuery(string Query, int Limit)
    : IRequest<Result<SemanticCourseSearchDto>>;
