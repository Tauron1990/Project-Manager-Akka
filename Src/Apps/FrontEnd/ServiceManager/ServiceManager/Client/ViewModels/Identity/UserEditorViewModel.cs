using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using ServiceManager.Shared.Identity;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels.Identity
{
    public sealed class UserEditorViewModel
    {
        private readonly IUserManagement  _userManagement;
        private readonly IEventAggregator _aggregator;
        private readonly Action           _stateChanged;

        public bool IsRunning { get; set; }
        
        public ImmutableList<ClaimEditorModel> EditorModels { get; }
        
        public UserData? User { get; }

        public string OldPassword { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
        
        public UserEditorViewModel(IUserManagement userManagement, IEventAggregator aggregator, UserData? user, UserClaim[] claims, Action stateChanged)
        {
            User               = user;
            _userManagement    = userManagement;
            _aggregator        = aggregator;
            _stateChanged = stateChanged;

            EditorModels = (from userClaim in Claims.AllClaims
                            select new ClaimEditorModel(userClaim, claims.Any(c => c.Name == userClaim))
                           ).ToImmutableList();
        }

        public async Task TryUpdatePassword()
        {
            try
            {
                IsRunning = true;
                var command = new SetNewPasswordCommand(User.Id, OldPassword, NewPassword);
                _stateChanged();

                await _aggregator.IsSuccess(() => _userManagement.SetNewPassword(command));
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
            finally
            {
                IsRunning = false;
                _stateChanged();
            }
        }
        
        public async Task TryCommitClaims()
        {
            try
            {
                IsRunning = true;
                _stateChanged();

                var claims = EditorModels.Aggregate(ImmutableArray<string>.Empty, (array, model) => model.SetClaim(array));
                await _aggregator.IsSuccess(() => _userManagement.SetClaims(new SetClaimsCommand(User.Id, claims.ToImmutableList())));
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
            finally
            {
                IsRunning = false;
                _stateChanged();
            }
        }
    }
}