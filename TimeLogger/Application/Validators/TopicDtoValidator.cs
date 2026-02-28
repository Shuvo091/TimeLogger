using FluentValidation;
using SkillAllocationTracker.Application.DTOs;
using System;

namespace SkillAllocationTracker.Application.Validators
{
    public class TopicDtoValidator : AbstractValidator<TopicDto>
    {
        public TopicDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Percentage).InclusiveBetween(1, 100);
        }
    }
}