using StudentInsights.Domain.Common;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Domain.Entities;

public class Goal : BaseEntity
{
    private Goal()
    {
    } // EF Core

    private Goal(Guid userId, GoalType type, decimal targetValue, DateTime? targetDateUtc, Guid? relatedActivityId)
    {
        UserId = userId;
        Type = type;
        TargetValue = targetValue;
        TargetDateUtc = targetDateUtc;
        RelatedActivityId = relatedActivityId;
    }

    public static Goal Create(User user, GoalType type, decimal targetValue, DateTime? targetDateUtc = null,
        LearningActivity? relatedActivity = null)
    {
        if (user is null)
            throw new DomainException("User is required.");
        if (targetValue <= 0)
            throw new DomainException("Target value must be greater than zero.");

        if (type == GoalType.ProjectDeadline)
        {
            if (relatedActivity is null)
                throw new DomainException("A ProjectDeadline goal must reference a learning activity.");
            if (relatedActivity.UserId != user.Id)
                throw new DomainException("The related activity must belong to the same user.");
        }
        else if (relatedActivity is not null)
        {
            throw new DomainException("Only ProjectDeadline goals can reference a learning activity.");
        }

        return new Goal(user.Id, type, targetValue, targetDateUtc, relatedActivity?.Id);
    }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public GoalType Type { get; private set; }

    /// <summary>Target value: GPA, hours, chapters, etc. depending on Type.</summary>
    public decimal TargetValue { get; private set; }

    /// <summary>Optional deadline.</summary>
    public DateTime? TargetDateUtc { get; private set; }

    /// <summary>
    /// Set only when Type == ProjectDeadline. Kept as an ID-only reference —
    /// not a navigation property — because Goal and LearningActivity belong
    /// to different aggregates; Goal must not reach into Course's object graph.
    /// </summary>
    public Guid? RelatedActivityId { get; private set; }

    public void UpdateTarget(decimal targetValue, DateTime? targetDateUtc)
    {
        if (targetValue <= 0)
            throw new DomainException("Target value must be greater than zero.");
        TargetValue = targetValue;
        TargetDateUtc = targetDateUtc;
        MarkModified();
    }
}