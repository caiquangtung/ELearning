using FluentValidation;

namespace ELearning.Application.Features.Courses.UploadAsset;

public sealed class UploadLessonAssetCommandValidator : AbstractValidator<UploadLessonAssetCommand>
{
    public UploadLessonAssetCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(512);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Content).NotNull();
    }
}

