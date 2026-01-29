using Postgirl.Common;
using Postgirl.Domain.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Postgirl.Presentation.ViewModels.Authentication
{
    public class RequestAuthViewModel : BaseViewModel
    {
        private AuthType _authType = AuthType.None;
        private string _bearerToken = string.Empty;

        public AuthType AuthType
        {
            get => _authType;
            set => SetProperty(ref _authType, value);
        }

        public IEnumerable<AuthType> AvailableAuthTypes =>
            Enum.GetValues(typeof(AuthType)).Cast<AuthType>();

        // --- Bearer token ---

        public string BearerToken
        {
            get => _bearerToken;
            set => SetProperty(ref _bearerToken, value);
        }

        public bool HasBearerToken =>
            !string.IsNullOrWhiteSpace(BearerToken);
    }
}
