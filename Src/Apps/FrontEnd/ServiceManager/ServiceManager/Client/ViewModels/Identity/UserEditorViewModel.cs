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
        private readonly IEventAggregator _aggregator;
        private readonly Action _stateChanged;
        private readonly IUserManagement _userManagement;

        public UserEditorViewModel(IUserManagement userManagement, IEventAggregator aggregator, UserData? user, UserClaim[] claims, Action stateChanged)
        {
            User = user;
            _userManagement = userManagement;
            _aggregator = aggregator;
            _stateChanged = stateChanged;

            EditorModels = (from userClaim in Claims.AllClaims
                            select new ClaimEditorModel(userClaim, claims.Any(c => c.Name == userClaim))
                ).ToImmutableList();
        }

        public bool IsRunning { get; set; }

        public ImmutableList<ClaimEditorModel> EditorModels { get; }

        public UserData? User { get; }

        public string OldPassword { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;

        public bool HideOldPasswordBox { get; set; }

        public bool HideButtons { get; set; }

        public async Task TryDeleteUser()
        {
            try
            {
                if (User == null)
                    _aggregator.PublishError("Benutzer nicht angegeben");
                else if (await _aggregator.IsSuccess(() => _userManagement.DeleteUser(new DeleteUserCommand(User.Id))))
                    _aggregator.PublishSuccess($"Benutzer {User.Name} efolgreich gelöscht");
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }

        public async Task TryUpdatePassword()
        {
            try
            {
                if (User == null)
                {
                    _aggregator.PublishError("Benutzer nicht gefunden");

                    return;
                }

                IsRunning = true;
                var command = new SetNewPasswordCommand(User.Id, OldPassword, NewPassword);
                _stateChanged();

                if (await _aggregator.IsSuccess(() => _userManagement.SetNewPassword(command)))
                    _aggregator.PublishSuccess($"Passwort von {User.Name} efolgreich geändert");
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

        public Task TryCommitClaims()
            => TryCommitClaims(null);

        public async Task TryCommitClaims(string? id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id) && User == null)
                {
                    _aggregator.PublishError("Benutzer nicht gefunden");

                    return;
                }

                IsRunning = true;
                _stateChanged();

                var claims = EditorModels.Aggregate(ImmutableArray<string>.Empty, (array, model) => model.SetClaim(array));
                if (await _aggregator.IsSuccess(() => _userManagement.SetClaims(new SetClaimsCommand(id ?? User!.Id, claims.ToImmutableList())))) _aggregator.PublishSuccess($"Berechtigunen erfolgreich für {User?.Name} geändert");
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