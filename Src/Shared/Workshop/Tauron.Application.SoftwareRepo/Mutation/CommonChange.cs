using Tauron.Application.SoftwareRepo.Data;
using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.SoftwareRepo.Mutation;

public record CommonChange(ApplicationList ApplicationList) : MutatingChange;