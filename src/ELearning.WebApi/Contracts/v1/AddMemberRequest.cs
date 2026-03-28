namespace ELearning.WebApi.Contracts.v1;

public record AddMemberRequest(Guid UserId, string OrgRole, Guid? DepartmentId);
