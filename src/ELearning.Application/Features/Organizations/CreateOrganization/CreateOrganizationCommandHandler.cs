using ELearning.Application.Common;
using ELearning.Application.Features.Organizations.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.OrganizationAggregate;
using MediatR;

namespace ELearning.Application.Features.Organizations.CreateOrganization;

public class CreateOrganizationCommandHandler(
    IOrganizationRepository organizationRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOrganizationCommand, Result<OrganizationDto>>
{
    public async Task<Result<OrganizationDto>> Handle(
        CreateOrganizationCommand request,
        CancellationToken ct)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Slugify(request.Name)
            : SlugHelper.Slugify(request.Slug);

        if (await organizationRepository.SlugExistsAsync(slug, ct))
            return Result.Failure<OrganizationDto>(
                Error.Conflict("Organization", $"Slug '{slug}' is already taken."));

        var org = Organization.Create(request.Name, slug);
        organizationRepository.Add(org);
        await unitOfWork.SaveChangesAsync(ct);

        return new OrganizationDto(org.Id, org.Name, org.Slug, org.Status.ToString());
    }
}
